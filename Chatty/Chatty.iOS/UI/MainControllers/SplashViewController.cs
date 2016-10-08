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
    public partial class SplashViewController : AdMaiora.AppKit.UI.App.UIMainViewController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        // This flag check if we are already calling the login REST service
        private bool _isLogginUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Constructors

        public SplashViewController()
            : base("SplashViewController", null)
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
            #endregion

            string osVersion = null;
            string appVersion = "1.0.0";
            string appBuild = "1";
            AppController.Utility.GetContextInfo(out osVersion, out appVersion, out appBuild);
            this.VersionLabel.Text = string.Format("v{0} ({1})", appVersion, appBuild);

            DateTime beginTime = DateTime.Now;
            AppController.Utility.ExecuteOnAsyncTask(CancellationToken.None,
                () =>
                {
                    // Do background stuff here...

                    // Check if we must wait at least splash screen timeout
                    TimeSpan loadSpan = DateTime.Now - beginTime;
                    Thread.Sleep(Math.Max(0, AppController.Globals.SplashScreenTimeout - (int)loadSpan.TotalMilliseconds));
                },
                () =>
                {
                    if (!RestoreUser())                   
                        MakeRoot(typeof(MainViewController));                    
                });
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            if (_cts != null)
                _cts.Cancel();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private bool RestoreUser()
        {
            if (!AppController.IsUserRestorable)
                return false;

            if (_isLogginUser)
                return true;

            _isLogginUser = true;

            // Create a new cancellation token for this request                
            _cts = new CancellationTokenSource();
            AppController.RestoreUser(_cts, AppController.Settings.AuthAccessToken,
                // Service call success                 
                (data) =>
                {                    
                    UIBundle b = new UIBundle();
                    b.PutBoolean("UserRestored", true);
                    b.PutString("Email", data.Email);
                    MakeRoot(typeof(MainViewController), b);
                },
                // Service call error
                (error) =>
                {                    
                    MakeRoot(typeof(MainViewController));

                    UIToast.MakeText(error, UIToastLength.Long).Show();
                },
                // Service call finished 
                () =>
                {
                    _isLogginUser = false;
                });

            return true;
        }

        #endregion

        #region Event Handlers
        #endregion
    }
}


