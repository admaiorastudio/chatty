namespace AdMaiora.Chatty
{
    using System;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;    
    using Android.Views;
    using Android.Widget;
    using Android.OS;           
    using Android.Views.InputMethods;

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    [Activity(
        Label = "Chatty",
        Theme = "@style/AppTheme",        
        LaunchMode = LaunchMode.SingleTask,       
        WindowSoftInputMode = SoftInput.AdjustNothing,             
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AdMaiora.AppKit.UI.App.AppCompactActivity
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private bool _userRestored;
        private string _email;

        #endregion

        #region Widgets

        [Widget]
        private Android.Support.V7.Widget.Toolbar Toolbar;
        
        [Widget]
        private FrameLayout LoadLayout;

        #endregion

        #region Constructors

        public MainActivity()
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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ActivityMain, this.Toolbar);

            this.SupportActionBar.SetDisplayShowHomeEnabled(true);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            #endregion

            this.LoadLayout.Focusable = true;
            this.LoadLayout.FocusableInTouchMode = true;
            this.LoadLayout.Clickable = true;
            this.LoadLayout.Visibility = ViewStates.Gone;

            bool isResuming = this.SupportFragmentManager.FindFragmentById(Resource.Id.ContentLayout) != null;
            if (!isResuming)
            {
                this.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.ContentLayout, new LoginFragment(), "LoginFragment")
                    .Commit();

                _userRestored = this.Arguments.GetBoolean("UserRestored", false);
                if (_userRestored)
                {
                    _email = this.Intent.GetStringExtra("Email");

                    var f = new ChatFragment();
                    f.Arguments = new Bundle();
                    f.Arguments.PutString("Email", _email);
                    this.SupportFragmentManager.BeginTransaction()
                        .AddToBackStack("BeforeChatFragment")
                        .Replace(Resource.Id.ContentLayout, f, "ChatFragment")
                        .Commit();
                }
            }
        }

        public override void OnBackPressed()
        {
            var f = this.SupportFragmentManager.FindFragmentById(Resource.Id.ContentLayout);
            if(f is IBackButton)
            {
                if (((IBackButton)f).OnBackButton())
                    return;
            }

            base.OnBackPressed();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Block the main UI, preventing user from tapping any view
        /// </summary>
        public void BlockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Unblock the main UI, allowing user tapping views
        /// </summary>
        public void UnblockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Visibility = ViewStates.Gone;
        }

        #endregion        

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}


