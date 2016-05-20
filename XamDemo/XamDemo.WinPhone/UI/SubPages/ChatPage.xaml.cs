using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AdMaiora.XamDemo.UI.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        #region Constants and Fields

        private DispatcherTimer _updateTimer;

        private CancellationTokenSource _cts;

        private bool _isLoadingMessages;

        #endregion

        #region Fragment Methods

        public ChatPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SendButton.Click += SendButton_Click;

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(RestManager.RefreshInterval);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            _isLoadingMessages = false;

            LoadMessages();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Tick -= UpdateTimer_Tick;
            }

            if (_cts != null)
                _cts.Cancel();

            this.SendButton.Click -= SendButton_Click;
        }

        #endregion

        #region Methods

        private async Task LoadMessages(bool blockUI = true)
        {
            if (_isLoadingMessages)
                return;

            if (blockUI)
                ((Window.Current.Content as Frame).Content as Pages.MainPage).BlockUI();

            _isLoadingMessages = true;
            _cts = new CancellationTokenSource();
            await (new RestManager()).GetMessages(_cts,
                (data) =>
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);
                    this.ChatView.NavigateToString(html);
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.NavigateToString(html);
                },
                () =>
                {
                    _isLoadingMessages = false;

                    if (blockUI)
                        ((Window.Current.Content as Frame).Content as Pages.MainPage).UnblockUI();
                });
        }

        #endregion

        #region Event Handlers

        private async void UpdateTimer_Tick(object sender, object e)
        {
            _updateTimer.Stop();

            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, 
                async () =>
                 {
                     await LoadMessages(false);
                     _updateTimer.Start();
                 });
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string content = this.MessageText.Text;
            if (String.IsNullOrWhiteSpace(content))
                return;

            ((Window.Current.Content as Frame).Content as Pages.MainPage).DismissKeyboard();

            ((Window.Current.Content as Frame).Content as Pages.MainPage).BlockUI();

            _cts = new CancellationTokenSource();
            await (new RestManager()).SendMessage(_cts, content,
                (data) =>
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);
                    this.ChatView.NavigateToString(html);

                    this.MessageText.Text = String.Empty;
                    //this.MessageText.Focus(FocusState.Programmatic);
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.NavigateToString(html);
                },
                () =>
                {
                    ((Window.Current.Content as Frame).Content as Pages.MainPage).UnblockUI();
                });
        }

        #endregion
        
    }
}
