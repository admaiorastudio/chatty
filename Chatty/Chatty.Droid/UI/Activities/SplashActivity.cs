namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Content.Res;
    using Android.OS;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;

    using AdMaiora.AppKit.UI;
    
    #pragma warning disable CS4014
    [Activity(
        Label = "Chatty",
        MainLauncher = true,        
        Theme = "@style/AppTheme.Splash",
        ScreenOrientation = ScreenOrientation.Portrait,
        NoHistory = true,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.ScreenSize |
            ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard
    )]
    public class SplashActivity : AdMaiora.AppKit.UI.App.AppCompactActivity
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        // This flag check if we are already calling the login REST service
        private bool _isLogginUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Widgets

        [Widget]
        private ImageView LogoImage;

        [Widget]
        private TextView TitleLabel;

        [Widget]
        private TextView VersionLabel;


        #endregion

        #region Constructors and Destructors

        public SplashActivity()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Activity Methods

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            #region Desinger Stuff

            SetContentView(Resource.Layout.ActivitySplash);            

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
                        MakeRoot(typeof(MainActivity));
                });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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
                    Bundle b = new Bundle();
                    b.PutBoolean("UserRestored", true);
                    b.PutString("Email", data.Email);                    
                    MakeRoot(typeof(MainActivity), b);
                },
                // Service call error
                (error) =>
                {
                    Toast.MakeText(this.Application, error, ToastLength.Long).Show();

                    MakeRoot(typeof(MainActivity));
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