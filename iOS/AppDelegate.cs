using System;
using System.Net.Http;
using Foundation;
using UIKit;

namespace OpenTheGate.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		//todo: have this set from file
		private string server = "open-gate-server.herokuapp.com";
		private string port = "";

		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert, new NSSet());

				UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			}
			else
			{
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Alert);
			}

			return true;
		}

		/// <summary>
		/// Called when the token from APNS comes
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="deviceToken">Device token.</param>
		public override async void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			// Get current device token
			var DeviceToken = deviceToken.Description;
			if (!string.IsNullOrWhiteSpace(DeviceToken))
			{
				DeviceToken = DeviceToken.Trim('<').Trim('>').Replace(" ", "");//its very important to remove <, > and spaces from the token
			}

			//notify our server that the device token has changed/been created
			using (var client = new HttpClient())
			{
				var postResponse = await client.PostAsync(string.Format("http://{0}/addToken", server, port), new StringContent(DeviceToken));
				postResponse.EnsureSuccessStatusCode();
				string response = await postResponse.Content.ReadAsStringAsync();
			}

			// Save new device token 
			NSUserDefaults.StandardUserDefaults.SetString(DeviceToken, "PushDeviceToken");
		}

		/// <summary>
		/// Called when the app failed to register with APNS
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="error">Error.</param>
		public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
		{
			new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
		}


		/// <summary>
		/// Opens the gate !
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="userInfo">User info.</param>
		/// <param name="completionHandler">Completion handler.</param>
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			if (userInfo["content-available"].Description.Equals("1"))
			{
				// fetch content
				//completionHandler(UIBackgroundFetchResult.NewData);

				//Xamarin Forms: Device.OpenUri(new Uri("tel:0549398542"));

				var url = new NSUrl("tel:0549398542"); 				if (!UIApplication.SharedApplication.OpenUrl(url))
				{
					var av = new UIAlertView("Error", "Calling the gate failed", null, "OK", null);
					av.Show();
				};
			}
		}

		//I need dedicated iOS app - not Forms - for run in background
		//Enabled the Background Modes capability.
		//Enabled the Remote notifications background mode.

		//run at startup by having the 'UIBackgroundModes' key with 'voip' value in the info.plist file
		//however, the app must run before device went down
		//todo: test if app indeed run at startup

		//TODO: find a new way to debug - insert to notification shadow or show a message box or Console.WriteLine - so i can remove the UI

		//todo: to add a button handler to compensate if the server is redeployed then the token file gets deleted
		//use this: var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

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
	}
}

