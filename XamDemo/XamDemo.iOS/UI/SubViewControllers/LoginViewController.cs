using System;
using System.Threading;
using UIKit;

namespace AdMaiora.XamDemo
{
	public partial class LoginViewController : UIViewController
	{
        #region Constants and Fields

        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Constructors

        public LoginViewController() 
            : base ("LoginViewController", null)
		{
		}

        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
		{
			base.ViewDidLoad ();            
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.LoginButton.TouchUpInside += LoginButton_Click;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.LoginButton.TouchUpInside -= LoginButton_Click;
        }

        #endregion

        #region Event Handlers     

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = this.UsernameText.Text;
            string password = this.PasswordText.Text;

            ((MainViewController)this.NavigationController.ParentViewController).DismissKeyboard();
        
            if (String.IsNullOrWhiteSpace(username)
                || String.IsNullOrWhiteSpace(password))
            {
                ((MainViewController)this.NavigationController.ParentViewController)
                    .ShowMessage("Login Failed", "Please eneter valid login credentials!");
            }
            else
            {
                // Prevent user form tapping views while logging
                ((MainViewController)this.NavigationController.ParentViewController).BlockUI();

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
                        ChatViewController f = new ChatViewController();
                        (this.ParentViewController as UINavigationController).PushViewController(f, true);
                    },
                    // Service call error or login cancelled
                    (error) =>
                    {
                        if (!_cts.IsCancellationRequested)
                            ((MainViewController)this.NavigationController.ParentViewController).ShowMessage("Login Failed", error);
                    },
                    // Service call ended
                    () =>
                    {
                        // Allow user to tap views
                        ((MainViewController)this.NavigationController.ParentViewController).UnblockUI();
                    });
            }
        }

        #endregion
    }
}


