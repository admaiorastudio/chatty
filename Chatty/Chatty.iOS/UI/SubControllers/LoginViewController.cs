﻿namespace AdMaiora.Chatty
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

    #pragma warning disable CS4014
    public partial class LoginViewController : AdMaiora.AppKit.UI.App.UISubViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;
        private string _password;

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

            SlideUpToShowKeyboard();

            #endregion

            this.NavigationController.SetNavigationBarHidden(true, false);

            StartNotifyKeyboardStatus();

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

            long duration = 500;

            UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut,
                () =>
                {
                    CAAnimationGroup group1 = CAAnimationGroup.CreateAnimation();
                    group1.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut);
                    group1.Duration = duration;
                    group1.Animations = new CAAnimation[]
                    {
                        new CABasicAnimation
                        {
                            KeyPath = "transform.scale",
                            From = NSNumber.FromFloat(1f),
                            To = NSNumber.FromFloat(.5f)
                        },
                        new CABasicAnimation
                        {
                            KeyPath = "position",
                            From = NSValue.FromCGPoint(this.LogoImage.Frame.Location),
                            To = NSValue.FromCGPoint(new CGPoint(this.LogoImage.Frame.Location.X, this.LogoImage.Frame.Location.Y - 38f)) 
                        }
                    };

                    this.LogoImage.Layer.AddAnimation(group1, null);
                },
                () =>
                {

                });
        }

        public override void KeyboardWillHide()
        {
            base.KeyboardWillHide();
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


