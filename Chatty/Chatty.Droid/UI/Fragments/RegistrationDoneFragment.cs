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

    public class RegistrationDoneFragment : AdMaiora.AppKit.UI.App.Fragment
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

        public override void OnCreateView(LayoutInflater inflater, ViewGroup container)
        {
            base.OnCreateView(inflater, container);

            #region Desinger Stuff

            SetContentView(Resource.Layout.FragmentRegistrationDone, inflater, container);

            ResizeToShowKeyboard();

            #endregion

            this.ActionBar.Show();

            this.GoToLoginButton.Click += GoToLoginButton_Click;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.FragmentManager.PopBackStack();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
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