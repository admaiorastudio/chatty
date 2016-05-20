using System;
using System.Threading;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AdMaiora.XamDemo.UI.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        #region Constants and Fields

        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Fragment Methods

        public LoginPage()
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
            this.LoginButton.Click += LoginButton_Click;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Cancel the request in case is in progress
            if (_cts != null)
                _cts.Cancel();

            this.LoginButton.Click -= LoginButton_Click;
        }

        #endregion

        #region Event Handlers 

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = this.UsernameText.Text;
            string password = this.PasswordText.Password;

            // Dismiss keyboard
            ((Window.Current.Content as Frame).Content as Pages.MainPage).DismissKeyboard();

            if (String.IsNullOrWhiteSpace(username)
                || String.IsNullOrWhiteSpace(password))
            {
                ((Window.Current.Content as Frame).Content as Pages.MainPage)
                    .ShowMessage("Login Failed", "Please eneter valid login credentials!");
            }
            else
            {
                // Prevent user form tapping views while logging
                ((Window.Current.Content as Frame).Content as Pages.MainPage).BlockUI();

                // Create a new cancellation token for this request                
                _cts = new CancellationTokenSource();
                await (new RestManager()).LoginUser(_cts, username, password,
                    // Service call succeded, we get an access token
                    (data) =>
                    {
                        // Save current assigned username and access token
                        RestManager.CurrentUser = data.username;
                        RestManager.AccessToken = data.token;

                        // User granted, we proceed to the next screen
                        Frame.Navigate(typeof(ChatPage));
                    },
                    // Service call error or login cancelled
                    (error) =>
                    {
                        if (!_cts.IsCancellationRequested)
                            ((Window.Current.Content as Frame).Content as Pages.MainPage).ShowMessage("Login Failed", error);
                    },
                    // Service call ended
                    () =>
                    {
                        // Allow user to tap views
                        ((Window.Current.Content as Frame).Content as Pages.MainPage).UnblockUI();
                    });
            }
        }

        #endregion
    }
}
