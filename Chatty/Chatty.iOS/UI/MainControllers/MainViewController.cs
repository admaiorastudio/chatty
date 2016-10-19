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

    public partial class MainViewController : AdMaiora.AppKit.UI.App.UIMainViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private bool _userRestored;
        private string _email;

        #endregion

        #region Widgets


        #endregion

        #region Constructors

        public MainViewController()
            : base("MainViewController", null)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            #region Designer Stuff

            SetContentView(this.ContentLayout);
            
            #endregion

            this.LoadLayout.UserInteractionEnabled = true;
            this.LoadLayout.Hidden = true;

            bool isResuming = this.ContentController.ViewControllers.Length > 0;
            if(!isResuming)
            {
                this.ContentController.PushViewController(new ChatViewController(), false);

                _userRestored = this.Arguments.GetBoolean("UserRestored");
                if (_userRestored)
                {
                    _email = this.Arguments.GetString("Email");

                    var c = new ChatViewController();
                    c.Arguments = new UIBundle();
                    c.Arguments.PutString("Email", _email);
                    this.ContentController.PushViewController(c, false);
                }
            }
        }

        public override bool ShouldPopItem()
        {
            UIViewController controller = this.ContentController.TopViewController;
            if (controller is IBackButton)
                return !((IBackButton)controller).ViewWillPop();

            return base.ShouldPopItem();
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Block the main UI, preventing user from tapping any view
        /// </summary>
        public void BlockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Hidden = false;
        }

        /// <summary>
        /// Unblock the main UI, allowing user tapping views
        /// </summary>
        public void UnblockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Hidden = true;
        }

        #endregion

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}


