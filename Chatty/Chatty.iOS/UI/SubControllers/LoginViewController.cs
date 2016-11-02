namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Foundation;
    using UIKit;
    using CoreGraphics;
    using CoreAnimation;

    using AdMaiora.AppKit.UI;

    using Facebook.CoreKit;
    using Facebook.LoginKit;

#pragma warning disable CS4014
    public partial class LoginViewController : AdMaiora.AppKit.UI.App.UISubViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;
        private string _password;

        private LoginManager _lm;

        // This flag check if we are already calling the login REST service
        private bool _isLogginUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts0;

        // This flag check if we are already calling the login REST service
        private bool _isConfirmingUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts1;

        #endregion

        #region Constructors

        public LoginViewController()
            : base("LoginViewController", null)
        {
        }

        #endregion

        #region Properties

        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #region Designer Stuff

            AutoShouldReturnTextFields(new[] { this.EmailText, this.PasswordText });

            SlideUpToShowKeyboard();

            StartNotifyKeyboardStatus();

            #endregion

            this.NavigationController.SetNavigationBarHidden(true, true);            

            this.EmailText.Text = AppController.Settings.LastLoginUsernameUsed;
            this.PasswordText.Text = String.Empty;

            this.PasswordText.ShouldReturn += PasswordText_ShouldReturn;

            this.LoginButton.TouchUpInside += LoginButton_TouchUpInside;

            this.RegisterButton.TouchUpInside += RegisterButton_TouchUpInside;

            this.VerifyButton.Hidden = true;
            this.VerifyButton.TouchUpInside += VerifyButton_TouchUpInside;
        }

        public override void KeyboardWillShow()
        {
            base.KeyboardWillShow();

            double duration = .5f;

            UIView.Animate(duration, 0f,
                UIViewAnimationOptions.CurveEaseInOut, 
                () =>
                {
                    this.LogoImage.Transform =
                        CGAffineTransform.MakeScale(.5f, .5f) *
                        CGAffineTransform.MakeTranslation(0f, -38);

                    this.InputLayout.Transform =
                        CGAffineTransform.MakeTranslation(0f, -110f);
                },
                () =>
                {
                });
        }

        public override void KeyboardWillHide()
        {
            base.KeyboardWillHide();

            double duration = .3f;

            UIView.Animate(duration, 0f,
                UIViewAnimationOptions.CurveEaseInOut,
                () =>
                {
                    this.LogoImage.Transform =
                        CGAffineTransform.MakeScale(1f, 1f) *
                        CGAffineTransform.MakeTranslation(0f, 0f);

                    this.InputLayout.Transform =
                        CGAffineTransform.MakeTranslation(0f, 0f);
                },
                () =>
                {
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_cts0 != null)
                _cts0.Cancel();

            if (_cts1 != null)
                _cts1.Cancel();
           
            StopNotifyKeyboardStatus();

            this.PasswordText.ShouldReturn -= PasswordText_ShouldReturn;

            this.LoginButton.TouchUpInside -= LoginButton_TouchUpInside;

            this.RegisterButton.TouchUpInside -= RegisterButton_TouchUpInside;
                       
            this.VerifyButton.TouchUpInside -= VerifyButton_TouchUpInside;
        }

        #endregion

        #region Facebook Methods

        public void LoginRequestHandler(LoginManagerLoginResult result, NSError error)
        {
            if (error != null)
            {
                // Login Error
                ((MainViewController)this.MainViewController).UnblockUI();
            }
            else if (result.IsCancelled)
            {
                // Login Cancelled
                ((MainViewController)this.MainViewController).UnblockUI();
            }
            else
            {
                if (AccessToken.CurrentAccessToken == null)
                    return;

                GetFacebookData();
            }
        }

        public void DataRequestHandler(GraphRequestConnection connection, NSObject result, NSError err)
        {            
            try
            {
                string fbId= result.ValueForKey(new NSString("id")).ToString();
                string fbToken = AccessToken.CurrentAccessToken.TokenString;
                string fbEmail = result.ValueForKey(new NSString("email")).ToString();

                _email = fbEmail;

                // Create a new cancellation token for this request                
                _cts0 = new CancellationTokenSource();
                AppController.LoginUser(_cts0, fbId, fbEmail, fbToken,
                    // Service call success                 
                    (data) =>
                    {
                        AppController.Settings.LastLoginUsernameUsed = _email;
                        AppController.Settings.AuthAccessToken = data.AuthAccessToken;
                        AppController.Settings.AuthExpirationDate = data.AuthExpirationDate.GetValueOrDefault().ToLocalTime();

                        ((AppDelegate)UIApplication.SharedApplication.Delegate).RegisterToNotificationsHub();

                        var c = new ChatViewController();
                        c.Arguments = new UIBundle();
                        c.Arguments.PutString("Email", _email);
                        this.NavigationController.PushViewController(c, true);
                    },
                    // Service call error
                    (error) =>
                    {
                        UIToast.MakeText(error, UIToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        // Allow user to tap views
                        ((MainViewController)this.MainViewController).UnblockUI();
                    });
            }
            catch (Exception ex)
            {
                ((MainViewController)this.MainViewController).UnblockUI();

                UIToast.MakeText("Error", UIToastLength.Long).Show();                
            }
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private void LoginUser()
        {
            if (ValidateInput())
            {
                if (_isLogginUser)
                    return;

                _isLogginUser = true;

                _email = this.EmailText.Text;
                _password = this.PasswordText.Text;

                // Prevent user form tapping views while logging
                ((MainViewController)this.MainViewController).BlockUI();

                // Create a new cancellation token for this request                
                _cts1 = new CancellationTokenSource();
                AppController.LoginUser(_cts1, _email, _password,
                    // Service call success                 
                    (data) =>
                    {
                        AppController.Settings.LastLoginUsernameUsed = _email;
                        AppController.Settings.AuthAccessToken = data.AuthAccessToken;
                        AppController.Settings.AuthExpirationDate = data.AuthExpirationDate.GetValueOrDefault().ToLocalTime();

                        ((AppDelegate)UIApplication.SharedApplication.Delegate).RegisterToNotificationsHub();

                        var c = new ChatViewController();
                        c.Arguments = new UIBundle();
                        c.Arguments.PutString("Email", _email);
                        this.NavigationController.PushViewController(c, true);
                    },
                    // Service call error
                    (error) =>
                    {
                        if (error.Contains("confirm"))
                            this.VerifyButton.Hidden = false;

                        UIToast.MakeText(error, UIToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        _isLogginUser = false;

                        // Allow user to tap views
                        ((MainViewController)this.MainViewController).UnblockUI();
                    });
            }
        }

        private void VerifyUser()
        {
            if (ValidateInput())
            {
                if (_isLogginUser)
                    return;

                _isLogginUser = true;

                _email = this.EmailText.Text;
                _password = this.PasswordText.Text;

                // Prevent user form tapping views while logging
                ((MainViewController)this.MainViewController).BlockUI();

                this.VerifyButton.Hidden = true;

                // Create a new cancellation token for this request                
                _cts1 = new CancellationTokenSource();
                AppController.VerifyUser(_cts1, _email, _password,
                    // Service call success                 
                    () =>
                    {
                        UIToast.MakeText("You should receive a new mail!", UIToastLength.Long).Show();
                    },
                    // Service call error
                    (error) =>
                    {
                        UIToast.MakeText(error, UIToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        _isLogginUser = false;

                        // Allow user to tap views
                        ((MainViewController)this.MainViewController).UnblockUI();
                    });
            }
        }

        private bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert your email.")
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsEmail, "Your mail is not valid!")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert your password.")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsPasswordMin8, "Your passowrd is not valid!");

            string errorMessage;
            if (!validator.Validate(out errorMessage))
            {
                UIToast.MakeText(errorMessage, UIToastLength.Long).Show();

                return false;
            }

            return true;
        }

        private void LoginInFacebook()
        {
            ((MainViewController)this.MainViewController).BlockUI();

            _lm.LogInWithReadPermissions(new[] { "public_profile", "email" }, this, LoginRequestHandler);
        }

        private void GetFacebookData()
        {
            GraphRequest gr = new GraphRequest("/me", null, "GET");
            gr.Parameters.Add(NSObject.FromObject("fields"), NSObject.FromObject("id,name,email"));
            GraphRequestConnection rc = new GraphRequestConnection();
            rc.AddRequest(gr, DataRequestHandler);
            rc.Start();
        }

        #endregion

        #region Event Handlers

        private bool PasswordText_ShouldReturn(UITextField sender)
        {
            LoginUser();

            DismissKeyboard();

            return true;
        }

        private void LoginButton_TouchUpInside(object sender, EventArgs e)
        {
            LoginUser();

            DismissKeyboard();
        }

        private void FacebookLoginButton_TouchUpInside(object sender, EventArgs e)
        {
            LoginInFacebook();

            DismissKeyboard();
        }

        private void RegisterButton_TouchUpInside(object sender, EventArgs e)
        {
            var c = new Registration0ViewController();            
            this.NavigationController.PushViewController(c, true);

            DismissKeyboard();
        }

        private void VerifyButton_TouchUpInside(object sender, EventArgs e)
        {
            VerifyUser();

            DismissKeyboard();
        }

        #endregion
    }
}


