using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UIKit;

namespace AdMaiora.XamDemo
{
	public partial class ChatViewController : UIViewController
	{
        #region Constants and Fields

        private System.Timers.Timer _updateTimer;

        private CancellationTokenSource _cts;

        private bool _isLoadingMessages;

        #endregion

        #region Constructors

        public ChatViewController ()
            : base ("ChatViewController", null)
		{
		}

        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
		{
			base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.SendButton.TouchUpInside += SendButton_TouchUpInside;

            _updateTimer = new System.Timers.Timer(RestManager.RefreshInterval * 1000);
            _updateTimer.AutoReset = false;
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Start();

            _isLoadingMessages = false;

            LoadMessages();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Elapsed -= UpdateTimer_Elapsed;
            }

            if (_cts != null)
                _cts.Cancel();

            this.SendButton.TouchUpInside -= SendButton_TouchUpInside;
        }

        #endregion

        #region Methods

        private async Task LoadMessages(bool blockUI = true)
        {
            if (_isLoadingMessages)
                return;

            if (blockUI)
                ((MainViewController)this.NavigationController.ParentViewController).BlockUI();

            _isLoadingMessages = true;
            _cts = new CancellationTokenSource();
            await (new RestManager()).GetMessages(_cts,
                (data) =>
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);
                    this.ChatView.LoadHtmlString(html, null);                    
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.LoadHtmlString( html, null);
                },
                () =>
                {
                    _isLoadingMessages = false;

                    if (blockUI)
                        ((MainViewController)this.NavigationController.ParentViewController).UnblockUI();
                });
        }

        #endregion

        #region Event Handlers

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.InvokeOnMainThread(            
                async () =>
                {
                    await LoadMessages(false);
                    _updateTimer.Start();
                });
        }

        private async void SendButton_TouchUpInside(object sender, EventArgs e)
        {
            string content = this.MessageText.Text;
            if (String.IsNullOrWhiteSpace(content))
                return;
            
            ((MainViewController)this.NavigationController.ParentViewController).DismissKeyboard();
            ((MainViewController)this.NavigationController.ParentViewController).BlockUI();

            _cts = new CancellationTokenSource();
            await (new RestManager()).SendMessage(_cts, content,
                (data) =>
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);                                      
                    this.ChatView.LoadHtmlString(html, null);

                    this.MessageText.Text = String.Empty;
                    this.MessageText.BecomeFirstResponder();
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.LoadHtmlString(html, null);
                },
                () =>
                {
                    ((MainViewController)this.NavigationController.ParentViewController).UnblockUI();
                });
        }

        #endregion
    }
}


