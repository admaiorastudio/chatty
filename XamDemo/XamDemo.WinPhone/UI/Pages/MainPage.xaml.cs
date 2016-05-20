using System;

using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AdMaiora.XamDemo.UI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Page Methods

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.LoadLayout.Visibility = Visibility.Collapsed;

            this.ContentLayout.Navigate(typeof(UI.SubPages.LoginPage));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Block the main UI, preventing user from tapping any view
        /// </summary>
        public void BlockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Unblock the main UI, allowing user tapping views
        /// </summary>
        public void UnblockUI()
        {
            if (this.LoadLayout != null)
                this.LoadLayout.Visibility = Visibility.Collapsed;
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
            MessageDialog msgbox = new MessageDialog(message, title);

            msgbox.Commands.Clear();

            if (!String.IsNullOrWhiteSpace(ok))
                msgbox.Commands.Add(new UICommand(ok, (s) => whenOk()));

            if (!String.IsNullOrWhiteSpace(cancel))
                msgbox.Commands.Add(new UICommand(cancel, (s) => whenCancel()));

            msgbox.ShowAsync();
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

        public void DismissKeyboard()
        {
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
        }

        #endregion
    }
}
