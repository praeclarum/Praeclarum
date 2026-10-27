using System;
using System.Linq;
using Foundation;
using UIKit;

namespace Praeclarum.UI
{
	/// <summary>
	/// Delegate for the app's single window scene. DocumentAppDelegate still owns the UI,
	/// so this only forwards the scene's events to it.
	/// </summary>
	[Register ("DocumentSceneDelegate")]
	public class DocumentSceneDelegate : UIWindowSceneDelegate
	{
		static DocumentAppDelegate AppDelegate => DocumentAppDelegate.Shared;

		public override UIWindow Window {
			get => AppDelegate?.Window;
			set {
			}
		}

		public override void WillConnect (UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
		{
			try {
				if (scene is UIWindowScene windowScene) {
					AppDelegate?.ConnectScene (windowScene, connectionOptions);
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override async void DidDisconnect (UIScene scene)
		{
			try {
				if (AppDelegate is { } appDelegate) {
					await appDelegate.DisconnectSceneAsync ();
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override void DidEnterBackground (UIScene scene)
		{
			AppDelegate?.SceneDidEnterBackground ();
		}

		public override void WillEnterForeground (UIScene scene)
		{
			AppDelegate?.SceneWillEnterForeground ();
		}

		public override void OpenUrlContexts (UIScene scene, NSSet<UIOpenUrlContext> urlContexts)
		{
			try {
				var url = urlContexts?.ToArray ().FirstOrDefault ()?.Url;
				if (url is not null) {
					AppDelegate?.HandleOpenUrl (url);
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override async void PerformAction (UIWindowScene windowScene, UIApplicationShortcutItem shortcutItem, Action<bool> completionHandler)
		{
			try {
				if (AppDelegate is { } appDelegate) {
					await appDelegate.HandleShortcutItemAsync (shortcutItem);
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
			try {
				completionHandler (true);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}
}
