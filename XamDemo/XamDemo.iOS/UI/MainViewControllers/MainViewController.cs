using CoreGraphics;
using System;

using UIKit;

namespace AdMaiora.XamDemo
{
    public partial class MainViewController : UIViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private UINavigationController ContentController;

        #endregion

        #region Widgets
        #endregion

        #region Constructors

        public MainViewController()
            : base("MainViewController", null)
        {
        }

        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.ContentController = new UINavigationController();
            this.AddChildViewController(ContentController);
            this.ContentController.SetNavigationBarHidden(true, false);
            this.ContentLayout.AddSubview(this.ContentController.View);
            this.ContentController.View.Frame = new CGRect(0, 0, this.ContentLayout.Bounds.Size.Width, this.ContentLayout.Bounds.Size.Height);

            this.LoadLayout.UserInteractionEnabled = true;
            this.LoadLayout.Hidden = true;            
            
            LoginViewController loginViewController = new LoginViewController();
            ContentController.PushViewController(loginViewController, true);
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
                LoadLayout.Hidden = false;
        }

        /// <summary>
        /// Unblock the main UI, allowing user tapping views
        /// </summary>
        public void UnblockUI()
        {
            if (this.LoadLayout != null)
                LoadLayout.Hidden = true;
        }

        /// <summary>
        /// Show a message dialog
        /// </summary>
        /// <param name="title">Title to show</param>
        /// <param name="message">Message to show</param>
        /// <param name="ok">Positive button text</param>
        /// <param name="cancel">Negative button text</param>
        /// <param name="whenOk">Action delegate when positive button is pressed</param>
        /// <param name="whenCancel">Action delegate when negative button is pressed</param>
        public void ShowMessage(string title, string message, string ok, string cancel, Action whenOk, Action whenCancel)
        {
            UIAlertView alert = new UIAlertView()
            {
                Title = title,
                Message = message
            };

            if (!String.IsNullOrWhiteSpace(ok))
            {
                var index = alert.AddButton(ok);                
                alert.Clicked += (s, e) => { if (index == 0) whenOk(); };
            }

            if (!String.IsNullOrWhiteSpace(cancel))
            {
                var index = alert.AddButton(cancel);
                alert.Canceled += (s, e) => { if (index == 0) whenCancel(); };
            }

            alert.Show();
        }
        
        /// <summary>
        /// Show a message dialog
        /// </summary>
        /// <param name="title">Title to show</param>
        /// <param name="message">Message to show</param>
        /// <param name="ok">Positive button text</param>        
        /// <param name="whenOk">Action delegate when positive button is pressed</param>        
        public void ShowMessage(string title, string message, string ok, Action whenOk)
        {
            ShowMessage(title, message, ok, null, whenOk, null);
        }

        /// <summary>
        /// Show a message dialog
        /// </summary>
        /// <param name="title">Title to show</param>
        /// <param name="message">Message to show</param>
        public void ShowMessage(string title, string message)
        {
            ShowMessage(title, message, "OK", null, () => { /* Do Nothing */ }, null);
        }

        public void DismissKeyboard()
        {
            //if (focused == null)
            //    return;

            //focused.ResignFirstResponder();

            this.View.EndEditing(true);
        }

        #endregion

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}