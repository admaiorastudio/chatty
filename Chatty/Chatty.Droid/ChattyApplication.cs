namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Runtime;
    using Android.Widget;
    using Android.OS;
    using Android.Gms.Common;
    using Android.Gms.Gcm.Iid;
    using Android.Gms.Gcm;
    using Android.Support.V4.App;

    using WindowsAzure.Messaging;

    using AdMaiora.AppKit;   

    public class PushEventArgs : EventArgs
    {
        public int Action
        {
            get;
            private set;
        } 

        public string Payload
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public PushEventArgs(int action, string payload)
        {
            this.Action = action;
            this.Payload = payload;
        }

        public PushEventArgs(Exception error)
        {
            this.Error = error;
        }
    }

#if DEBUG
    [Application(Name = "admaiora.chatty.ChattyApplication")]
#else
    [Application(Name = "admaiora.chatty.ChattyApplication", Debuggable = false)]
#endif
    public class ChattyApplication : AppKitApplication
    {
        #region Constants and Fields
        
        private NotificationHub _hub;

        private string _notificationDeviceToken;

        #endregion

        #region Events

        public event EventHandler<EventArgs> PushNotificationRegistered;
        public event EventHandler<PushEventArgs> PushNotificationRegistrationFailed;
        public event EventHandler<PushEventArgs> PushNotificationReceived;

        #endregion

        #region Constructors

        public ChattyApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) 
        {
        }

        #endregion

        #region Properties

        public bool IsNotificationHubConnected
        {
            get;
            private set;
        }

        #endregion

        #region Application Methods

        public override void OnCreate()
        {
            base.OnCreate();

            // Setup Application
            AppController.EnableSettings(new AdMaiora.AppKit.Data.UserSettingsPlatformAndroid());
            AppController.EnableUtilities(new AdMaiora.AppKit.Utils.ExecutorPlatformAndroid());
            AppController.EnableServices(new AdMaiora.AppKit.Services.ServiceClientPlatformAndroid());

            // Setup push notifications
            RegisterForRemoteNotifications(AppController.Globals.GoogleGcmSenderID);
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
        }

        public override void OnRegisteredForRemoteNotifications(InstanceID instanceID, string token)
        {
            RegisterToNotificationsHub(token);            
            AppController.Utility.DebugOutput("Chatty", "GCM, Registration Succeded!");
        }

        public override void OnFailedToRegisterForRemoteNotifications(Exception ex)
        {
            PushNotificationRegistrationFailed?.Invoke(this, new PushEventArgs(ex));            
            AppController.Utility.DebugOutput("Chatty", "GCM, Registration Error: " + ex.ToString());

            //AppController.Utility.ExecuteOnMainThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Long).Show());
        }

        public override void OnReceivedRemoteNotification(Dictionary<string, object> data)
        {
            string title = (string)data["title"];
            string message = (string)data["body"];
            int action = (int)data["action"];
            string payload = (string)data["payload"];

            if(action == 1)
            {
                int lastMessageId = Int32.Parse(payload);
                AppController.Settings.LastMessageId = lastMessageId;
            }

            // Check if the app is in foreground
            // If not show an "heads up" notification, otherwhise notify an event
            if (!this.IsApplicationInForeground)
            {
                // Preparing notification to be shown to the user
                NotificationCompat.Builder mBuilder =
                        new NotificationCompat.Builder(Application.Context.ApplicationContext)
                        .SetSmallIcon(Resource.Drawable.ic_push)
                        .SetContentTitle(title)
                        .SetContentText(message)
                        .SetAutoCancel(true)
                        .SetSound(Android.Net.Uri.Parse(String.Format("android.resource://{0}/{1}",
                            this.PackageName, Resource.Raw.sound_ding)))
                        .SetPriority((int)Android.App.NotificationPriority.High);                        

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    mBuilder.SetVibrate(new long[0]);

                // Setting the intent to be executed when the user tap the notification
                Intent notificationIntent = new Intent(this, typeof(MainActivity));                
                notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
                PendingIntent pendingIntent = PendingIntent.GetActivity(
                    Application.Context.ApplicationContext,
                    0,
                    notificationIntent,
                    PendingIntentFlags.UpdateCurrent);

                if (pendingIntent != null)
                    mBuilder.SetContentIntent(pendingIntent);

                NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
                mNotificationManager.Notify(0, mBuilder.Build());
            }

            PushNotificationReceived?.Invoke(this, new PushEventArgs(action, payload));
        }

        #endregion

        #region Azure Methods

        public void RegisterToNotificationsHub(string deviceToken = null)
        {
            this.IsNotificationHubConnected = false;

            AppController.Utility.ExecuteOnAsyncTask(
                System.Threading.CancellationToken.None,
                () =>
                {
                    // Refresh current device token
                    if (deviceToken != null)
                        _notificationDeviceToken = deviceToken;

                    // Do we have a current device token;
                    if (_notificationDeviceToken == null)
                        return;

                    if (!AppController.IsUserRestorable)
                        return;

                    // We have already created our notification hub
                    if (_hub == null)
                    {
                        _hub = new NotificationHub(
                            AppController.Globals.AzureNHubName,
                            AppController.Globals.AzureNHubConnectionString,
                            Application.Context.ApplicationContext);
                    }

                    try
                    {
                        _hub.UnregisterAll(_notificationDeviceToken);
                    }
                    catch (Exception ex)
                    {
                        AppController.Utility.DebugOutput("Chatty", "Azure HUB, UnregisterAll Error: " + ex.ToString());
                   }

                    try
                    {
                        var tags = new List<string>()
                        {
                            AppController.Settings.LastLoginUsernameUsed
                        };

                        var hubRegistration = _hub.Register(_notificationDeviceToken, tags.ToArray());
                        this.IsNotificationHubConnected = true;

                        PushNotificationRegistered?.Invoke(this, EventArgs.Empty);

                        AppController.Utility.ExecuteOnMainThread(() => Toast.MakeText(this.ApplicationContext, "You are connected!", ToastLength.Long).Show());
                    }
                    catch (Exception ex)
                    {
                        PushNotificationRegistrationFailed?.Invoke(this, new PushEventArgs(ex));                        
                        AppController.Utility.DebugOutput("Chatty", "Azure HUB, Register Error: " + ex.ToString());

                        AppController.Utility.ExecuteOnMainThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Long).Show());
                    }
                });
        }

        #endregion
    }
}
