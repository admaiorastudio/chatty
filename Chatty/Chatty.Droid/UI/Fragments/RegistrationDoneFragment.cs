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

    public class RegistrationDoneFragment : Android.Support.V4.App.Fragment
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

            this.ResizeToShowKeyboard();

            #endregion

            this.GetActionBar().Hide();

            this.GoToLoginButton.Click += GoToLoginButton_Click;       

            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
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