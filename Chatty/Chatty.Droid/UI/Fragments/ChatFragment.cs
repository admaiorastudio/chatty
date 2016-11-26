namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Util;
    using Android.Views;
    using Android.Widget;
    using Android.Graphics;
    using Android.Media;

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    using AdMaiora.Chatty.Api;

	#pragma warning disable CS4014
    public class ChatFragment : AdMaiora.AppKit.UI.App.Fragment
    {
        #region Inner Classes

        class ChatAdapter : ItemRecyclerAdapter<ChatAdapter.ChatViewHolder, Message>
        {
            #region Inner Classes

            public class ChatViewHolder : ItemViewHolder
            {
                [Widget]
                public RelativeLayout CalloutLayout;

                [Widget]
                public TextView SenderLabel;

                [Widget]
                public TextView MessageLabel;

                [Widget]
                public TextView DateLabel;


                public ChatViewHolder(View itemView)
                    : base(itemView)
                {                    
                }
            }

            #endregion

            #region Costants and Fields

            private Random _rnd;

            private List<string> _palette;
            private Dictionary<string, string> _colors;

            #endregion

            #region Constructors

            public ChatAdapter(AdMaiora.AppKit.UI.App.Fragment context, IEnumerable<Message> source) 
                : base(context, Resource.Layout.CellChat, source)
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

            public override void GetView(int postion, ChatViewHolder holder, View view, Message item)
            {
                bool isYours = String.IsNullOrWhiteSpace(item.Sender);
                bool isSending = item.SendDate == DateTime.MinValue;                
                bool isSent = item.SendDate != DateTime.MinValue && item.SendDate != DateTime.MaxValue;
                bool isLost = item.SendDate == DateTime.MaxValue;

                if (!isYours && !_colors.ContainsKey(item.Sender))
                    _colors.Add(item.Sender, _palette[_rnd.Next(_palette.Count)]);

                ((RelativeLayout)view).SetGravity(isYours ? GravityFlags.Right : GravityFlags.Left);

                holder.SenderLabel.Text = String.Concat(isYours ? "YOU" : item.Sender.Split('@')[0], "   ");

                holder.CalloutLayout.Background.SetColorFilter(
                    ViewBuilder.ColorFromARGB(isYours ? AppController.Colors.PictonBlue : _colors[item.Sender]),
                    PorterDuff.Mode.SrcIn);

                holder.CalloutLayout.Alpha =  isSent ? 1 : .35f;

                holder.MessageLabel.Text = String.Concat(item.Content, "   ");

                holder.DateLabel.Text = isSent ? String.Format("  sent @ {0:G}", item.SendDate) : String.Empty;
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

        private ChatAdapter _adapter;

        // This cancellation token is used to cancel the UI blocking until connection is done
        private CancellationTokenSource _cts0;

        // This flag check if we are already calling the send message REST service
        private bool _isSendingMessage;
        // This cancellation token is used to cancel the rest send message request
        private CancellationTokenSource _cts1;
        
        // This cancellation token is used to cancel the rest refresh messages request
        private CancellationTokenSource _cts2;

        private SoundPool _sp;
        private int _dingSoundId;
        
        #endregion

        #region Widgets

        [Widget]
        private ItemRecyclerView MessageList;

        [Widget]
        private RelativeLayout InputLayout;

        [Widget]
        private EditText MessageText;

        [Widget]
        private ImageButton SendButton;

        #endregion

        #region Constructors

        public ChatFragment()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Fragment Methods

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _email = this.Arguments.GetString("Email");
            _username = _email.Split('@')[0];            

            _adapter = new ChatAdapter(this, new Message[0]);            
        }	

        public override void OnCreateView(LayoutInflater inflater, ViewGroup container)
        {
            base.OnCreateView(inflater, container);

            #region Desinger Stuff

            SetContentView(Resource.Layout.FragmentChat, inflater, container);
            
            ResizeToShowKeyboard();

            this.HasOptionsMenu = true;

            #endregion

            ((ChattyApplication)this.Activity.Application).PushNotificationReceived += Application_PushNotificationReceived;

            this.Title = "Chatty";

            this.ActionBar.Show();

            this.MessageList.SetAdapter(_adapter);

            this.SendButton.Click += SendButton_Click;

            InitSound();

            RefreshMessages();

            WaitConnection();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    QuitChat();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }            
        }

        public override bool OnBackButton()
        {
            QuitChat();
            return true;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_cts0 != null)
                _cts0.Cancel();

            if (_cts1 != null)
                _cts1.Cancel();

            if (_cts2 != null)
                _cts2.Cancel();

            if(_sp != null)
            {
                _sp.Release();
                _sp.Dispose();
            }

            this.SendButton.Click -= SendButton_Click;

            ((ChattyApplication)this.Activity.Application).PushNotificationReceived -= Application_PushNotificationReceived;
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
                _adapter.AddItem(message);
                this.MessageList.ReloadData();
                this.MessageList.SmoothScrollToPosition(_adapter.ItemCount);

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

                        Toast.MakeText(this.Activity.Application, error, ToastLength.Long).Show();
                    },
                    () =>
                    {
                        _isSendingMessage = false;
                    });

                // Ready to send new message
                this.MessageText.Text = String.Empty;
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
                        if (cts.IsCancellationRequested)
                            return;

                        lock (ReceiverLock)
                        {                            
                            newMessages = data.Messages?.ToArray();
                            _lastMessageId = (newMessages?.Last().MessageId).GetValueOrDefault(0);
                        }
                    },
                    (error) =>
                    {
                        // Do Nothing
                    },
                    () =>
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        lock (ReceiverLock)
                        {
                            bool playSound = false;
                            if (newMessages != null)
                            {
                                foreach (var m in newMessages)
                                {
                                    if (!_adapter.HasMessage(m.MessageId))
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

                                        _adapter.AddItem(message);
                                    }
                                }
                            }                            

                            this.MessageList.ReloadData();
                            this.MessageList.SmoothScrollToPosition(_adapter.ItemCount);

                            if(playSound)
                                PlaySound();
                        }
                    });
            }
        }

        private void QuitChat()
        {
            (new AlertDialog.Builder(this.Activity))
                .SetTitle("Leave the chat?")
                .SetMessage("Press ok to leave the chat now!")
                .SetPositiveButton("Ok",
                    (s, ea) =>
                    {
                        AppController.Settings.AuthAccessToken = null;
                        AppController.Settings.AuthExpirationDate = null;

                        this.DismissKeyboard();
                        this.FragmentManager.PopBackStack();
                    })
                .SetNegativeButton("Take me back",
                    (s, ea) =>
                    {
                    })
                .Show();
        }

        private void WaitConnection()
        {
            ((MainActivity)this.Activity).BlockUI();
            _cts0 = new CancellationTokenSource();
            AppController.Utility.ExecuteOnAsyncTask(_cts0.Token,
                () =>
                {
                    int awaited = 0;
                    while (!_cts0.IsCancellationRequested)
                    {
                        System.Threading.Tasks.Task.Delay(100, _cts0.Token).Wait();
                        awaited += 100;

                        if (((ChattyApplication)this.Activity.Application).IsNotificationHubConnected)
                            break;

                        if (awaited > 10000)
                            break;
                    }
                },
                () =>
                {
                    ((MainActivity)this.Activity).UnblockUI();

                    if (!((ChattyApplication)this.Activity.Application).IsNotificationHubConnected)
                    {
                        this.MessageText.Enabled = false;
                        this.SendButton.Enabled = false;

                        Toast.MakeText(this.Activity.Application, "Unable to connect to the message hub!", ToastLength.Long).Show();
                    }
                });
        }

        private void InitSound()
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                AudioAttributes aa = new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Game)
                    .SetContentType(AudioContentType.Sonification)
                    .Build();

                _sp = new SoundPool.Builder()
                    .SetAudioAttributes(aa)
                    .Build();
            }
            else
            {
                _sp = new SoundPool(5, Stream.Music, 0);
            }

            _dingSoundId = _sp.Load(this.Activity, Resource.Raw.sound_ding, 1);            
        }

        private void PlaySound()
        {
            if(((ChattyApplication)this.Activity.Application).IsApplicationInForeground)
                _sp.Play(_dingSoundId, 1f, 1f, 1, 0, 1f);
        }

        #endregion

        #region Event Handlers

        private void SendButton_Click(object sender, EventArgs e)
        {
            DismissKeyboard();            

            SendMessage();
        }

        private void Application_PushNotificationReceived(object sender, PushEventArgs e)
        {
            AppController.Utility.ExecuteOnMainThread(
                () =>
                {
                    switch(e.Action)
                    {
                        case 1:

                            _lastMessageId = Int32.Parse(e.Payload);
                            RefreshMessages();

                            break;

                        case 2:

                            if (e.Payload != _email.Split('@')[0])
                            {
                                PlaySound();

                                Toast
                                    .MakeText(this.Activity.Application, String.Format("Say welocome to '{0}'", e.Payload), ToastLength.Long)
                                    .Show();
                            }

                            break;
                    }
                });            
        }

        #endregion
    }
}