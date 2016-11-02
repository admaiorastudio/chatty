namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Foundation;
    using UIKit;

    using AdMaiora.AppKit;
    using AdMaiora.AppKit.UI;

    using WindowsAzure.Messaging;

    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register ("AppDelegate")]
	public class AppDelegate : UIAppKitApplicationDelegate
	{
        #region Constants and Fields

        private SBNotificationHub _hub;

        private NSData _notificationDeviceToken;

        #endregion

        #region Events

        public event EventHandler<EventArgs> PushNotificationRegistered;
        public event EventHandler<PushEventArgs> PushNotificationRegistrationFailed;
        public event EventHandler<PushEventArgs> PushNotificationReceived;

        #endregion

        #region Constructors

        public AppDelegate()
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

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
            // Setup Application
            AppController.EnableSettings(new AdMaiora.AppKit.Data.UserSettingsPlatformiOS());
            AppController.EnableUtilities(new AdMaiora.AppKit.Utils.ExecutorPlatformiOS());
            AppController.EnableServices(new AdMaiora.AppKit.Services.ServiceClientPlatformiOS());

            // Setup push notifications
            RegisterForRemoteNotifications(launchOptions);

            // Setup Facebook
            Facebook.CoreKit.Settings.AppID = "1111406372229015";
            Facebook.CoreKit.Settings.DisplayName = "Chatty";
            Facebook.CoreKit.Profile.EnableUpdatesOnAccessTokenChange(true);
            Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(application, launchOptions);

            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            return RegisterMainLauncher(new SplashViewController(), launchOptions);            
		}

        public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            RegisterToNotificationsHub(deviceToken);
            AppController.Utility.DebugOutput("Chatty", "APN, Registration Succeded!");
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            var ex = new InvalidOperationException(error.Description);
            PushNotificationRegistrationFailed?.Invoke(this, new PushEventArgs(ex));
            AppController.Utility.DebugOutput("Chatty", "APN, Registration Error: " + ex.ToString());

            //AppController.Utility.ExecuteOnMainThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Long).Show());
        }

        public override void ReceivedRemoteNotification(Dictionary<string, object> data)
        {
            string title = (string)data["title"];
            string message = (string)data["body"]; 
            int action = (int)data["action"];
            string payload = (string)data["payload"];

            if (action == 1)
            {
                int lastMessageId = Int32.Parse(payload);
                AppController.Settings.LastMessageId = lastMessageId;
            }

            // Check if the app is in foreground            
            if (!this.IsApplicationInForeground)
            {
                // "Heads-up" notification is handled by iOS so we just need to handle what 
                // to do when the app is "restarted" or "resumed"
                HandleRemoteNotificationLaunching(
                    () =>
                    {
                        this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
                        this.Window.RootViewController = new MainViewController();
                        this.Window.MakeKeyAndVisible();
                    },
                    () =>
                    {
                        var c = this.Window.RootViewController;
                        if (c is AdMaiora.AppKit.UI.App.UIMainViewController)
                            ((AdMaiora.AppKit.UI.App.UIMainViewController)c).RemoteNotification(new UIBundle());
                    });                
            }

            PushNotificationReceived?.Invoke(this, new PushEventArgs(action, payload));
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // We need to handle URLs by passing them to FBSession in order for SSO authentication
            // to work.                                        
            return Facebook.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        #endregion

        #region Azure Methods

        public void RegisterToNotificationsHub(NSData deviceToken = null)
        {
            this.IsNotificationHubConnected = false;

            // Refresh current device token
            if (deviceToken != null)
                _notificationDeviceToken = deviceToken;

            // Do we have a current device token;
            if (_notificationDeviceToken == null)
                return;

            if (!AppController.IsUserRestorable)
                return;

            // We have already registered our notification hub
            if (_hub == null)
            {
                _hub = new SBNotificationHub(
                    AppController.Globals.AzureNHubConnectionString,
                    AppController.Globals.AzureNHubName);
            }

            _hub.UnregisterAllAsync(_notificationDeviceToken,
                (error) =>
                {
                    if (error != null)
                    {
                        AppController.Utility.DebugOutput("Chatty", "Azure HUB, UnregisterAll Error: " + error.Description);
                        return;
                    }

                    NSSet tags = new NSSet(new[]
                    {
                        AppController.Settings.LastLoginUsernameUsed
                    });

                    _hub.RegisterNativeAsync(_notificationDeviceToken, tags,
                        (err) =>
                        {
                            if (err == null)
                            {
                                this.IsNotificationHubConnected = true;

                                PushNotificationRegistered?.Invoke(this, EventArgs.Empty);

                                AppController.Utility.ExecuteOnMainThread(() => UIToast.MakeText("You are connected!", UIToastLength.Long).Show());
                            }
                            else
                            {
                                PushNotificationRegistrationFailed?.Invoke(this, new PushEventArgs(new InvalidOperationException(err.Description)));
                                AppController.Utility.DebugOutput("Chatty", "Azure HUB, Register Error: " + err.Description);

                                AppController.Utility.ExecuteOnMainThread(() => UIToast.MakeText(err.Description, UIToastLength.Long).Show());

                            }
                        });
                });
        }

        #endregion
    }
}


