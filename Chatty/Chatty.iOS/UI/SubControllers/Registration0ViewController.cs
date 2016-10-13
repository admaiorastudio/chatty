namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Foundation;
    using UIKit;

    using AdMaiora.AppKit.UI;

    #pragma warning disable CS4014
    public partial class Registration0ViewController : AdMaiora.AppKit.UI.App.UISubViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;

        #endregion

        #region Constructors

        public Registration0ViewController()
            : base("Registration0ViewController", null)
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

            this.NavigationController.SetNavigationBarHidden(false, true);

            this.EmailText.Text = _email;
            this.EmailText.ShouldReturn += EmailText_ShouldReturn;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.EmailText.ShouldReturn -= EmailText_ShouldReturn;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private void RegisterUser()
        {
            if (ValidateInput())
            {
                _email = this.EmailText.Text;

                var c = new Registration1ViewController();
                c.Arguments = new UIBundle();
                c.Arguments.PutString("Email", _email);
                this.NavigationController.PushViewController(c, true);
            }
        }

        private bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert an email.")
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsEmail, "Your email is not valid!");

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

        private bool EmailText_ShouldReturn(UITextField sender)
        {
            DismissKeyboard();

            RegisterUser();

            return true;
        }

        #endregion
    }
}


