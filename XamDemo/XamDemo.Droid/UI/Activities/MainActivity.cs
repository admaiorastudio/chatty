using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Views.InputMethods;

namespace AdMaiora.XamDemo
{
	[Activity (
        Label = "Chatty", 
        MainLauncher = true,
        Theme = "@style/Theme.AppCompat.NoActionBar",
        Icon = "@drawable/icon")]
	public class MainActivity : Android.Support.V7.App.AppCompatActivity
	{
        #region Widgets

        private FrameLayout LoadLayout;

        #endregion

        #region Activity Methods

        protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
           
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ActivityMain);
            
            this.LoadLayout = FindViewById<FrameLayout>(Resource.Id.LoadLayout);
            this.LoadLayout.Focusable = true;
            this.LoadLayout.FocusableInTouchMode = true;
            this.LoadLayout.Clickable = true;
            this.LoadLayout.Visibility = ViewStates.Gone;

            LoginFragment f = new LoginFragment();
            this.SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.ContentLayout, f, "LoginFragment")
                .Commit();
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
    }
}


