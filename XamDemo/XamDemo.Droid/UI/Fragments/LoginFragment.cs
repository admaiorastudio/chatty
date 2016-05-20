using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AdMaiora.XamDemo
{
    public class LoginFragment : Android.Support.V4.App.Fragment
    {
        #region Constants and Fields

        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;
        
        #endregion

        #region Widgets

        private EditText UsernameText;
        private EditText PasswordText;
        private Button LoginButton;

        #endregion

        #region Constructors

        public LoginFragment()
        {

        }

        #endregion

        #region Fragment Methods

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.FragmentLogin, container, false);
            
            this.UsernameText = view.FindViewById<EditText>(Resource.Id.UsernameText);
            this.PasswordText = view.FindViewById<EditText>(Resource.Id.PasswordText);

            this.LoginButton = view.FindViewById<Button>(Resource.Id.LoginButton);
            this.LoginButton.Click += LoginButton_Click;

            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            // Cancel the request in case is in progress
            if (_cts != null)
                _cts.Cancel();

            this.LoginButton.Click -= LoginButton_Click;
        }

        #endregion

        #region Event Handlers     

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = this.UsernameText.Text;
            string password = this.PasswordText.Text;

            // Dismiss keyboard
            ((MainActivity)this.Activity).DismissKeyboard(this.View.FindFocus());

            if (String.IsNullOrWhiteSpace(username)
                || String.IsNullOrWhiteSpace(password))
            {
                ((MainActivity)this.Activity)
                    .ShowMessage("Login Failed", "Please eneter valid login credentials!");
            }
            else
            {
                // Prevent user form tapping views while logging
                ((MainActivity)this.Activity).BlockUI();

                // Create a new cancellation token for this request                
                _cts = new CancellationTokenSource();
                await (new RestManager()).LoginUser(_cts, username, password,
                    // Service call succeded, we get an access token
                    (data) =>
                    {
                        // Save current assigned username and access token
                        RestManager.CurrentUser = data.username;
                        RestManager.AccessToken = data.token;

                        // User granted, we proceed to the next screen
                        ChatFragment f = new ChatFragment();
                        this.FragmentManager.BeginTransaction()
                            .Replace(Resource.Id.ContentLayout, f, "ChatFragment")
                            .Commit();
                    },
                    // Service call error or login cancelled
                    (error) =>
                    {
                        if(!_cts.IsCancellationRequested)                        
                            ((MainActivity)this.Activity).ShowMessage("Login Failed", error);                        
                    },
                    // Service call ended
                    () =>
                    {
                        // Allow user to tap views
                        ((MainActivity)this.Activity).UnblockUI();
                    });
            }
        }

        #endregion
    }
}