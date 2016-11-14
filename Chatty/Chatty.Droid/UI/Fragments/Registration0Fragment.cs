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

    #pragma warning disable CS4014
    public class Registration0Fragment : AdMaiora.AppKit.UI.App.Fragment
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;

        #endregion

        #region Widgets

        [Widget]
        private EditText EmailText;        

        #endregion

        #region Constructors

        public Registration0Fragment()
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

            SetContentView(Resource.Layout.FragmentRegistration0, inflater, container);            

            SlideUpToShowKeyboard();

            this.HasOptionsMenu = true;

            #endregion

            this.ActionBar.Show();

            this.EmailText.Text = _email;
            this.EmailText.EditorAction += EmailText_EditorAction;
        }

        public override void OnStart()
        {
            base.OnStart();

            this.EmailText.RequestUserInput();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.DismissKeyboard();
                    this.FragmentManager.PopBackStack();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            this.EmailText.EditorAction -= EmailText_EditorAction;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private void RegisterUser()
        {
            if (ValidateInput())
            {
                DismissKeyboard();

                AppController.Utility.ExecuteDelayedAction(300, default(System.Threading.CancellationToken),
                    () =>
                    {
                        _email = this.EmailText.Text;

                        var f = new Registration1Fragment();
                        f.Arguments = new Bundle();
                        f.Arguments.PutString("Email", _email);
                        this.FragmentManager.BeginTransaction()
                            .AddToBackStack("BeforeRegistration1Fragment")
                            .Replace(Resource.Id.ContentLayout, f, "Registration1Fragment")
                            .Commit();
                    });
            }
        }

        private bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert an email.")
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsEmail, "Your email is not valid!");

            string errorMessage;
            if(!validator.Validate(out errorMessage))
            {
                Toast.MakeText(this.Activity.Application, errorMessage, ToastLength.Long).Show();
                return false;
            }

            return true;
        }

        #endregion

        #region Event Handlers

        private void EmailText_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == Android.Views.InputMethods.ImeAction.Next)
            {
                e.Handled = true;
                RegisterUser();
            }
            else
            {
                e.Handled = false;
            }
        }

        #endregion
    }
}