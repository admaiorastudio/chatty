using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

namespace AdMaiora.XamDemo
{    
    public class ChatFragment : Android.Support.V4.App.Fragment
    {
        #region Constants and Fields

        private System.Timers.Timer _updateTimer;

        private CancellationTokenSource _cts;

        private bool _isLoadingMessages;

        #endregion

        #region Widgets

        private WebView ChatView;
        private EditText MessageText;
        private Button SendButton;

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
            var view = inflater.Inflate(Resource.Layout.FragmentChat, container, false);

            this.ChatView = view.FindViewById<WebView>(Resource.Id.ChatView);
            this.ChatView.Settings.JavaScriptEnabled = true;

            this.MessageText = view.FindViewById<EditText>(Resource.Id.MessageText);

            this.SendButton = view.FindViewById<Button>(Resource.Id.SendButton);
            this.SendButton.Click += SendButton_Click;

            _updateTimer = new System.Timers.Timer(RestManager.RefreshInterval * 1000);
            _updateTimer.AutoReset = false;
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Start();
            
            _isLoadingMessages = false;

            LoadMessages();

            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Elapsed -= UpdateTimer_Elapsed;
            }

            if (_cts != null)
                _cts.Cancel();
            
            this.SendButton.Click -= SendButton_Click;
        }

        #endregion

        #region Methods

        private async Task LoadMessages(bool blockUI = true)
        {
            if (_isLoadingMessages)
                return;
            
            if(blockUI)
                ((MainActivity)this.Activity).BlockUI();

            _isLoadingMessages = true;
            _cts = new CancellationTokenSource();
            await (new RestManager()).GetMessages(_cts,
                (data) => 
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);
                    this.ChatView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
                },
                () =>
                {
                    _isLoadingMessages = false;

                    if(blockUI)
                        ((MainActivity)this.Activity).UnblockUI();
                });
        }

        #endregion

        #region Event Handlers

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Activity.RunOnUiThread(
                async () =>
                {
                    await LoadMessages(false);
                    _updateTimer.Start();
                });                    
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            string content = this.MessageText.Text;
            if (String.IsNullOrWhiteSpace(content))
                return;

            ((MainActivity)this.Activity).DismissKeyboard(this.View.FindFocus());
            ((MainActivity)this.Activity).BlockUI();
            
            _cts = new CancellationTokenSource();
            await (new RestManager()).SendMessage(_cts, content,
                (data) =>
                {
                    // Update chat view with content
                    string html = (new RestManager()).CreateHTMLFromMessages(data);
                    this.ChatView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);

                    this.MessageText.Text = String.Empty;
                    this.MessageText.RequestFocus();
                },
                (error) =>
                {
                    if (_cts.IsCancellationRequested)
                        return;

                    string html = (new RestManager()).CreateHTMLFromError(error);
                    this.ChatView.LoadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
                },
                () =>
                {
                    ((MainActivity)this.Activity).UnblockUI();
                });
        }

        #endregion
    }
}