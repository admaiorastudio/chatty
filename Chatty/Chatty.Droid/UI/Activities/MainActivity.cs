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

    [Activity(
        Label = "Chatty",
        Theme = "@style/AppTheme",        
        LaunchMode = LaunchMode.SingleTask,       
        WindowSoftInputMode = SoftInput.AdjustNothing,             
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Android.Support.V7.App.AppCompatActivity
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private bool _userRestored;
        private string _email;
        private long _loginDate;

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
            this.SetContentViewWithWidgets(Resource.Layout.ActivityMain);

            #endregion

            SetSupportActionBar(this.Toolbar);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            this.LoadLayout.Focusable = true;
            this.LoadLayout.FocusableInTouchMode = true;
            this.LoadLayout.Clickable = true;
            this.LoadLayout.Visibility = ViewStates.Gone;
            
            this.SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.ContentLayout, new LoginFragment(), "LoginFragment")
                .Commit();

            _userRestored = this.Intent.GetBooleanExtra("UserRestored", false);
            if (_userRestored)
            {
                _email = this.Intent.GetStringExtra("Email");
                _loginDate = this.Intent.GetLongExtra("LoginDate", 0);

                var f = new ChatFragment();
                f.Arguments = new Bundle();
                f.Arguments.PutString("Email", _email);                
                this.SupportFragmentManager.BeginTransaction()
                    .AddToBackStack("BeforeChatFragment")
                    .Replace(Resource.Id.ContentLayout, f, "ChatFragment")
                    .Commit();
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent == null)
                return;
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
            var dialog = (new AlertDialog.Builder(this))
                .SetTitle(title)
                .SetMessage(message)
                .SetCancelable(false);

            if(!String.IsNullOrWhiteSpace(ok))           
                dialog.SetPositiveButton(ok, (s, e) => whenOk());
            
            if(!String.IsNullOrWhiteSpace(cancel))
                dialog.SetNegativeButton(cancel, (s, e) => whenCancel());

            dialog.Show();
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

        public void DismissKeyboard(View focused)
        {
            if (focused == null)
                return;

            InputMethodManager imm = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(focused.WindowToken, 0);
        }

        #endregion        

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}


