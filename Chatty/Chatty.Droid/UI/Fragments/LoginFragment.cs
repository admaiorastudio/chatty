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
    using Android.Views.Animations;
    using Android.Animation;
    using Android.Widget;
    using Android.Views.InputMethods;
    using Android.Content.Res;
    using Android.Hardware.Input;

    using AdMaiora.AppKit.UI;

    #pragma warning disable CS4014
    public class LoginFragment : AdMaiora.AppKit.UI.App.Fragment
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;
        private string _password;

        // This flag check if we are already calling the login REST service
        private bool _isLogginUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts0;

        // This flag check if we are already calling the login REST service
        private bool _isConfirmingUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts1;


        #endregion

        #region Widgets

        [Widget]
        private ImageView LogoImage;

        [Widget]
        private RelativeLayout InputLayout;

        [Widget]
        private EditText EmailText;

        [Widget]
        private EditText PasswordText;

        [Widget]
        private Button LoginButton;

        [Widget]
        private Button RegisterButton;

        [Widget]
        private Button VerifyButton;

        #endregion

        #region Constructors

        public LoginFragment()
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

            // Use this to return your custom view for this Fragment
            var view = inflater.InflateWithWidgets(Resource.Layout.FragmentLogin, this, container, false);

            SlideUpToShowKeyboard();

            #endregion            

            this.ActionBar.Hide();
           
            this.EmailText.Text = AppController.Settings.LastLoginUsernameUsed;

            this.PasswordText.Text = String.Empty;            
            this.PasswordText.EditorAction += PasswordText_EditorAction;

            this.LoginButton.Click += LoginButton_Click;

            this.RegisterButton.Click += RegisterButton_Click;

            this.VerifyButton.Visibility = ViewStates.Gone;
            this.VerifyButton.Click += VerifyButton_Click;

            return view;
        }

        public override void OnKeyboardShow()
        {
            base.OnKeyboardShow();

            long duration = 500;

            AnimatorSet set1 = new AnimatorSet();
            set1.SetInterpolator(new AccelerateDecelerateInterpolator());
            set1.SetTarget(this.LogoImage);
            set1.SetDuration(duration);
            set1.PlayTogether(new[] {
                    ObjectAnimator.OfFloat(this.LogoImage, "scaleX", .5f),
                    ObjectAnimator.OfFloat(this.LogoImage, "scaleY", .5f),
                    ObjectAnimator.OfFloat(this.LogoImage, "translationY", ViewBuilder.AsPixels(-38f))
                    });
            set1.Start();

            AnimatorSet set2 = new AnimatorSet();
            set2.SetInterpolator(new AccelerateDecelerateInterpolator());
            set2.SetTarget(this.InputLayout);
            set2.SetDuration(duration);
            set2.PlayTogether(new[] {
                    ObjectAnimator.OfFloat(this.InputLayout, "translationY", ViewBuilder.AsPixels(-130f))
                    });
            set2.Start();
        }

        public override void OnKeyboardHide()
        {
            base.OnKeyboardHide();

            long duration = 300;

            AnimatorSet set = new AnimatorSet();
            set.SetInterpolator(new AccelerateDecelerateInterpolator());
            set.SetTarget(this.LogoImage);
            set.SetDuration(duration);
            set.PlayTogether(new[] {
                    ObjectAnimator.OfFloat(this.LogoImage, "scaleX", 1f),
                    ObjectAnimator.OfFloat(this.LogoImage, "scaleY", 1f),
                    ObjectAnimator.OfFloat(this.LogoImage, "translationY", 0f)
                    });
            set.Start();

            AnimatorSet set2 = new AnimatorSet();
            set2.SetInterpolator(new AccelerateDecelerateInterpolator());
            set2.SetTarget(this.InputLayout);
            set2.SetDuration(duration);
            set2.PlayTogether(new[] {
                    ObjectAnimator.OfFloat(this.InputLayout, "translationY", 0f)
                    });
            set2.Start();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_cts0 != null)
                _cts0.Cancel();

            if (_cts1 != null)
                _cts1.Cancel();

            StopNotifyKeyboardStatus();

            this.PasswordText.EditorAction -= PasswordText_EditorAction;

            this.LoginButton.Click -= LoginButton_Click;

            this.RegisterButton.Click -= RegisterButton_Click;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        private void LoginUser()
        {
            if (ValidateInput())                                                   
            {
                if (_isLogginUser)
                    return;

                _isLogginUser = true;                

                _email = this.EmailText.Text;
                _password = this.PasswordText.Text;

                // Prevent user form tapping views while logging
                ((MainActivity)this.Activity).BlockUI();

                // Create a new cancellation token for this request                
                _cts0 = new CancellationTokenSource();
                AppController.LoginUser(_cts0, _email, _password,
                    // Service call success                 
                    (data) =>
                    {                        
                        AppController.Settings.LastLoginUsernameUsed = _email;
                        AppController.Settings.AuthAccessToken = data.AuthAccessToken;
                        AppController.Settings.AuthExpirationDate = data.AuthExpirationDate.GetValueOrDefault().ToLocalTime();

                        ((ChattyApplication)this.Activity.Application).RegisterToNotificationsHub();

                        var f = new ChatFragment();
                        f.Arguments = new Bundle();
                        f.Arguments.PutString("Email", _email);                        
                        this.FragmentManager.BeginTransaction()
                            .AddToBackStack("BeforeChatFragment")
                            .Replace(Resource.Id.ContentLayout, f, "ChatFragment")
                            .Commit();
                    },
                    // Service call error
                    (error) =>
                    {
                        if (error.Contains("confirm"))
                            this.VerifyButton.Visibility = ViewStates.Visible;

                        Toast.MakeText(this.Activity.Application, error, ToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        _isLogginUser = false;

                        // Allow user to tap views
                        ((MainActivity)this.Activity).UnblockUI();
                    });
            }
        }

        private void VerifyUser()
        {
            if (ValidateInput())
            {
                if (_isConfirmingUser)
                    return;

                _isConfirmingUser = true;

                _email = this.EmailText.Text;
                _password = this.PasswordText.Text;

                // Prevent user form tapping views while logging
                ((MainActivity)this.Activity).BlockUI();

                this.VerifyButton.Visibility = ViewStates.Gone;

                // Create a new cancellation token for this request                
                _cts1 = new CancellationTokenSource();
                AppController.VerifyUser(_cts1, _email, _password,
                    // Service call success                 
                    () =>
                    {
                        Toast.MakeText(this.Activity.Application, "You should receive a new mail!", ToastLength.Long).Show();
                    },
                    // Service call error
                    (error) =>
                    {
                        Toast.MakeText(this.Activity.Application, error, ToastLength.Long).Show();
                    },
                    // Service call finished 
                    () =>
                    {
                        _isConfirmingUser = false;

                        // Allow user to tap views
                        ((MainActivity)this.Activity).UnblockUI();
                    });
            }
        }

        private bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert your email.")
                .AddValidator(() => this.EmailText.Text, WidgetValidator.IsEmail, "Your mail is not valid!")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert your password.")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsPasswordMin8, "Your passowrd is not valid!");

            string errorMessage;
            if (!validator.Validate(out errorMessage))
            {
                Toast.MakeText(this.Activity.Application, errorMessage, ToastLength.Long).Show();

                return false;
            }

            return true;
        }

        #endregion

        #region Event Handlers     

        private void PasswordText_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {            
            if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
            {
                LoginUser();

                e.Handled = true;

                DismissKeyboard();                
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            LoginUser();

            DismissKeyboard();            
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {            
            var f = new Registration0Fragment();            
            this.FragmentManager.BeginTransaction()
                .AddToBackStack("BeforeRegistration0Fragment")
                .Replace(Resource.Id.ContentLayout, f, "Registration0Fragment")
                .Commit();

            DismissKeyboard();
        }

        private void VerifyButton_Click(object sender, EventArgs e)
        {
            VerifyUser();

            DismissKeyboard();
        }

        #endregion
    }
}