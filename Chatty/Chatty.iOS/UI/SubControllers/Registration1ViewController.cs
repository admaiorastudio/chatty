namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Foundation;
    using UIKit;

    using AdMaiora.AppKit.UI;

    #pragma warning disable CS4014
    public partial class Registration1ViewController : AdMaiora.AppKit.UI.App.UISubViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;
        private string _password;

        // This flag check if we are already calling the login REST service
        private bool _isRegisteringUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Constructors

        public Registration1ViewController()
            : base("Registration1ViewController", null)
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
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #region Designer Stuff

            SlideUpToShowKeyboard();

            #endregion

            this.NavigationController.SetNavigationBarHidden(false, true);

            this.PasswordText.Text = _password;
            this.PasswordText.BecomeFirstResponder();
            this.PasswordText.ShouldReturn += PasswordText_ShouldReturn;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_cts != null)
                _cts.Cancel();
        
            this.PasswordText.ShouldReturn -= PasswordText_ShouldReturn;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        public void RegisterUser()
        {
            if (ValidateInput())
            {
                if (_isRegisteringUser)
                    return;

                _isRegisteringUser = true;

                _password = this.PasswordText.Text;

                // Prevent user form tapping views while logging
                ((MainViewController)this.MainViewController).BlockUI();

                // Create a new cancellation token for this request                
                _cts = new CancellationTokenSource();
                AppController.RegisterUser(_cts,
                    _email,
                    _password,
                    // Service call success                 
                    (data) =>
                    {
                        var c = new RegistrationDoneViewController();
                        this.NavigationController.PopToViewController(this.NavigationController.ViewControllers.Single(x => x is LoginViewController), false);
                        this.NavigationController.PushViewController(c, true);
                    },
                    // Service call error
                    (error) =>
                    {
                        this.PasswordText.BecomeFirstResponder();

                        UIToast.MakeText(error, UIToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        _isRegisteringUser = false;

                        // Allow user to tap views
                        ((MainViewController)this.MainViewController).UnblockUI();
                    });
            }
        }

        public bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert a password.")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsPasswordMin8, "Your password is not valid!");

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
            RegisterUser();

            DismissKeyboard();

            return true;
        }

        #endregion
    }
}


