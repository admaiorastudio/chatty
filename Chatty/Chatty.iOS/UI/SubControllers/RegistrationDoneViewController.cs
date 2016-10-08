namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Foundation;
    using UIKit;

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    public partial class RegistrationDoneViewController : AdMaiora.AppKit.UI.App.UISubViewController, IBackButton
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields
        #endregion

        #region Constructors

        public RegistrationDoneViewController()
            : base("RegistrationDoneViewController", null)
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

            ResizeToShowKeyboard();

            #endregion

            this.NavigationController.SetNavigationBarHidden(true, false);

            this.GoToLoginButton.TouchUpOutside += GoToLoginButton_TouchUpOutside;
        }

        public bool ViewWillPop()
        {
            return true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.GoToLoginButton.TouchUpOutside -= GoToLoginButton_TouchUpOutside;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods
        #endregion

        #region Event Handlers

        private void GoToLoginButton_TouchUpOutside(object sender, EventArgs e)
        {
            this.NavigationController.PopViewController(true);
        }

        #endregion
    }
}


