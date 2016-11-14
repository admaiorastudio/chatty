namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Foundation;
    using UIKit;
    using AudioToolbox;
    using AVFoundation;

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    using AdMaiora.Chatty.Api;

    #pragma warning disable CS4014
    public partial class ChatViewController : AdMaiora.AppKit.UI.App.UISubViewController
    {
        #region Inner Classes

        private class ChatViewSource : UIItemListViewSource<Message>
        {
            #region Constants and Fields

            private Random _rnd;

            private List<string> _palette;
            private Dictionary<string, string> _colors;

            #endregion

            #region Constructors

            public ChatViewSource(UIViewController controller, IEnumerable<Message> source)
                : base(controller, "ChatViewCell", source)
            {
                _rnd = new Random(DateTime.Now.Second);

                _palette = new List<string>
                {
                    "C3BEF7", "8A4FFF", "273C2C", "626868", "80727B", "62929E",
                    "F79256", "66101F", "DB995A", "654236", "6369D1", "22181C ",
                    "998FC7", "5B2333", "564D4A"

                };

                _colors = new Dictionary<string, string>();
            }

            #endregion

            #region Public Methods

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath, UITableViewCell cellView, Message item)
            {
                var cell = cellView as ChatViewCell;

                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                bool isYours = String.IsNullOrWhiteSpace(item.Sender);
                bool isSending = item.SendDate == DateTime.MinValue;
                bool isSent = item.SendDate != DateTime.MinValue && item.SendDate != DateTime.MaxValue;
                bool isLost = item.SendDate == DateTime.MaxValue;

                if (!isYours && !_colors.ContainsKey(item.Sender))
                    _colors.Add(item.Sender, _palette[_rnd.Next(_palette.Count)]);

                var margins = cell.ContentView.LayoutMarginsGuide;
                cell.CalloutLayout.LeadingAnchor.ConstraintEqualTo(margins.LeadingAnchor, 4).Active = !isYours;
                cell.CalloutLayout.TrailingAnchor.ConstraintEqualTo(margins.TrailingAnchor, 4).Active = isYours;

                cell.SenderLabel.Text = String.Concat(isYours ? "YOU" : item.Sender.Split('@')[0], "   ");

                cell.CalloutLayout.BackgroundColor = 
                    ViewBuilder.ColorFromARGB(isYours ? AppController.Colors.PictonBlue : _colors[item.Sender]);

                cell.CalloutLayout.Alpha = isSent ? 1 : .35f;

                cell.MessageLabel.Text = String.Concat(item.Content, "   ");

                cell.DateLabel.Text = isSent ? String.Format("  sent @ {0:G}", item.SendDate) : String.Empty;

                return cell;
            }

            public bool HasMessage(int messageId)
            {
                return this.SourceItems.Count(x => x.MessageId == messageId) > 0;
            }

            #endregion
        }

        #endregion

        #region Constants and Fields

        private const string ReceiverLock = "ReceiverLock";

        private string _email;
        private string _username;

        private int _lastMessageId;

        private ChatViewSource _source;
            
        // This cancellation token is used to cancel the UI blocking until connection is done
        private CancellationTokenSource _cts0;

        // This flag check if we are already calling the login REST service
        private bool _isSendingMessage;
        // This cancellation token is used to cancel the rest send message request
        private CancellationTokenSource _cts1;

        // This cancellation token is used to cancel the rest refresh messages request
        private CancellationTokenSource _cts2;

        private SystemSound _sound; 

        #endregion

        #region Constructors

        public ChatViewController()
            : base("ChatViewController", null)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _email = this.Arguments.GetString("Email");
            _username = (_email?.Split('@')[0]);

            _source = new ChatViewSource(this, new Message[0]);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #region Designer Stuff

            ResizeToShowKeyboard();

            #endregion

            ((AppDelegate)UIApplication.SharedApplication.Delegate).PushNotificationReceived += Application_PushNotificationReceived;

            this.Title = "Chatty";

            this.NavigationController.SetNavigationBarHidden(false, true);
            
            this.SendButton.TouchUpInside += SendButton_TouchUpInside;

            this.MessageText.Constraints.Single(x => x.GetIdentifier() == "Height").Constant = 30f;                       
            this.MessageText.Changed += MessageText_Changed;

            this.MessageList.RowHeight = UITableView.AutomaticDimension;
            this.MessageList.EstimatedRowHeight = 74;            
            this.MessageList.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            this.MessageList.BackgroundColor = ViewBuilder.ColorFromARGB(AppController.Colors.Snow);
            this.MessageList.TableFooterView = new UIView(CoreGraphics.CGRect.Empty);
            this.MessageList.Source = _source;

            InitSound();

            RefreshMessages();

            WaitConnection();
        }

        public override bool BarButtonItemSelected(int index)
        {
            switch(index)
            {
                case UISubViewController.BarButtonBack:
                    QuitChat();
                    return true;

                default:
                    return base.BarButtonItemSelected(index);
            }
            
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_cts0 != null)
                _cts0.Cancel();

            if (_cts1 != null)
                _cts1.Cancel();

            if (_cts2 != null)
                _cts2.Cancel();

            if(_sound != null)
            {
                _sound.Dispose();
                _sound = null;
            }

            this.SendButton.TouchUpInside -= SendButton_TouchUpInside;

            ((AppDelegate)UIApplication.SharedApplication.Delegate).PushNotificationReceived -= Application_PushNotificationReceived;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private void SendMessage()
        {
            string content = this.MessageText.Text;
            if (!String.IsNullOrWhiteSpace(content))
            {
                if (_isSendingMessage)
                    return;

                _isSendingMessage = true;

                // Add message to the message list 
                Message message = new Message { Sender = null, Content = content, SendDate = DateTime.MinValue };
                _source.AddItem(message);
                this.MessageList.ReloadData();
                this.MessageList.ScrollToRow(
                    NSIndexPath.FromItemSection((nint)(_source.Count - 1), 0),
                        UITableViewScrollPosition.Bottom,
                        false);

                _cts1 = new CancellationTokenSource();
                AppController.SendMessage(_cts1,
                    _email,
                    content,
                    (data) =>
                    {
                        message.MessageId = data.MessageId;
                        message.SendDate = data.SendDate.GetValueOrDefault();
                        this.MessageList.ReloadData();
                    },
                    (error) =>
                    {
                        message.SendDate = DateTime.MaxValue;
                        this.MessageList.ReloadData();

                        UIToast.MakeText(error, UIToastLength.Long).Show();
                    },
                    () =>
                    {
                        _isSendingMessage = false;
                    });

                // Ready to send new message
                this.MessageText.Text = String.Empty;
                AdjustMessageTextHeight();                
            }
        }

        private void RefreshMessages()
        {
            lock (ReceiverLock)
            {
                if (_lastMessageId == 0)
                {
                    if (AppController.Settings.LastMessageId == 0)
                        return;

                    _lastMessageId = AppController.Settings.LastMessageId;
                    AppController.Settings.LastMessageId = 0;
                }

                if (_cts2 != null && !_cts2.IsCancellationRequested)
                    _cts2.Cancel();

                _cts2 = new CancellationTokenSource();

                var cts = _cts2;
                Poco.Message[] newMessages = null;
                AppController.RefreshMessages(
                    _cts2,
                    _lastMessageId,
                    _email,
                    (data) =>
                    {
                        if ((cts?.IsCancellationRequested).GetValueOrDefault(true))
                            return;

                        lock (ReceiverLock)
                        {
                            newMessages = data?.Messages?.ToArray();
                            if(newMessages?.Length > 0) 
                            {
                                var lm = newMessages.Last();
                                _lastMessageId = lm.MessageId;
                            }
                        }
                    },
                    (error) =>
                    {
                        // Do Nothing
                    },
                    () =>
                    {
                        if ((cts?.IsCancellationRequested).GetValueOrDefault(true))
                            return;

                        lock (ReceiverLock)
                        {
                            bool playSound = false;
                            if (newMessages != null)
                            {
                                foreach (var m in newMessages)
                                {
                                    if (!_source.HasMessage(m.MessageId))
                                    {
                                        playSound = true;

                                        // Add message to the message list 
                                        Message message = new Message
                                        {
                                            MessageId = m.MessageId,
                                            Sender = m.Sender,
                                            Content = m.Content,
                                            SendDate = m.SendDate.GetValueOrDefault()
                                        };

                                        _source.AddItem(message);
                                    }
                                }
                            }

                            this.MessageList.ReloadData();
                            this.MessageList.ScrollToRow(
                                NSIndexPath.FromItemSection((nint)(_source.Count - 1), 0),
                                    UITableViewScrollPosition.Bottom,
                                    false);

                            if (playSound)
                                PlaySound();
                        }
                    });
            }
        }

        private void QuitChat()
        {            
            (new UIAlertViewBuilder(new UIAlertView()))
                .SetTitle("Leave the chat?")
                .SetMessage("Press ok to leave the chat now!")
                .AddButton("Ok",
                    (s, ea) =>
                    {
                        AppController.Settings.AuthAccessToken = null;
                        AppController.Settings.AuthExpirationDate = null;

                        this.DismissKeyboard();
                        this.NavigationController.PopViewController(true);
                    })
                .AddButton("Take me back",
                    (s, ea) =>
                    {
                    })
                .Show();
        }

        private void WaitConnection()
        {
            ((MainViewController)this.MainViewController).BlockUI();
            _cts0 = new CancellationTokenSource();
            AppController.Utility.ExecuteOnAsyncTask(_cts0.Token,
                () =>
                {
                    while (!_cts0.IsCancellationRequested)
                    {
                        System.Threading.Tasks.Task.Delay(100, _cts0.Token).Wait();
                        if (((AppDelegate)UIApplication.SharedApplication.Delegate).IsNotificationHubConnected)
                            break;
                    }
                },
                () =>
                {
                    ((MainViewController)this.MainViewController).UnblockUI();
                });
        }

        private void InitSound()
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord);
            _sound = SystemSound.FromFile(NSUrl.FromString(NSBundle.MainBundle.PathForResource("Raws/sound_ding", "wav")));
        }

        private void PlaySound()
        {
            if (_sound != null)
                _sound.PlayAlertSoundAsync();
        }

        private void AdjustMessageTextHeight()
        {
            UITextView t = this.MessageText;

            nfloat textWidth = t.TextContainerInset.InsetRect(t.Frame).Width;
            textWidth -= 2.0f * t.TextContainer.LineFragmentPadding;

            var size = (new NSString(t.Text)).GetBoundingRect(
                (new CoreGraphics.CGSize(textWidth, Double.MaxValue)),
                NSStringDrawingOptions.UsesLineFragmentOrigin,
                new UIStringAttributes() { Font = t.Font },
                null).Size;

            int numberOfLines = (int)Math.Round(size.Height / t.Font.LineHeight);

            if (numberOfLines > 0 && numberOfLines < 4)
                this.MessageText.Constraints.Single(x => x.GetIdentifier() == "Height").Constant = 30f + (numberOfLines - 1) * t.Font.LineHeight;

            if (numberOfLines == 2)
            {
                this.MessageText.SetContentOffset(CoreGraphics.CGPoint.Empty, true);
                this.MessageText.SetNeedsDisplay();
            }

        }

        #endregion

        #region Event Handlers

        private void MessageText_Changed(object sender, EventArgs e)
        {
            AdjustMessageTextHeight();
        }

        private void SendButton_TouchUpInside(object sender, EventArgs e)
        {
            DismissKeyboard();

            SendMessage();
        }

        private void Application_PushNotificationReceived(object sender, PushEventArgs e)
        {
            AppController.Utility.ExecuteOnMainThread(
                () =>
                {
                    switch (e.Action)
                    {
                        case 1:

                            _lastMessageId = Int32.Parse(e.Payload);
                            RefreshMessages();

                            break;

                        case 2:

                            if (e.Payload != _email.Split('@')[0])
                            {
                                PlaySound();

                                UIToast
                                    .MakeText(String.Format("Say welocome to '{0}'", e.Payload), UIToastLength.Long)
                                    .Show();
                            }

                            break;
                    }
                });
        }

        #endregion
    }
}


