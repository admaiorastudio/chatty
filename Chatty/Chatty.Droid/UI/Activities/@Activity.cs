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

    [Activity(
        Label = "YourAppNameHere",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges =
            ConfigChanges.Orientation | ConfigChanges.ScreenSize |
            ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard
    )]
    public class _Activity : Activity
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields
        #endregion

        #region Widgets
        #endregion

        #region Constructors

        public _Activity()
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

            SetContentView(0);

            #endregion
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}