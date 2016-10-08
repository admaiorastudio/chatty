namespace AdMaiora.Chatty
{
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

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    public class RegistrationDoneFragment : AdMaiora.AppKit.UI.App.Fragment, IBackButton
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields
        #endregion

        #region Widgets

        [Widget]
        private Button GoToLoginButton;

        #endregion

        #region Constructors

        public RegistrationDoneFragment()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Fragment Methods

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region Desinger Stuff

            View view = inflater.InflateWithWidgets(Resource.Layout.FragmentRegistrationDone, this, container, false);

            ResizeToShowKeyboard();

            #endregion

            this.ActionBar.Hide();

            this.GoToLoginButton.Click += GoToLoginButton_Click;       

            return view;
        }

        public bool OnBackButton()
        {
            this.FragmentManager.PopBackStack();
            return true;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            this.GoToLoginButton.Click -= GoToLoginButton_Click;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods
        #endregion

        #region Event Handlers

        private void GoToLoginButton_Click(object sender, EventArgs e)
        {
            this.FragmentManager.PopBackStack();
        }

        #endregion
    }
}