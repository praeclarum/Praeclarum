using System;
using UIKit;
using Foundation;
using System.Linq;
using StoreKit;

namespace Praeclarum.App
{
	public interface IHasReviewNagging
	{
		ReviewNagging ReviewNagging { get; }
	}

	public class ReviewNagging
	{
		// https://developer.apple.com/documentation/storekit/skstorereviewcontroller/requesting_app_store_reviews

		const string CountKey = "ReviewCountTotal";
		const string LastShownVersionKey = "ReviewLastShownVersion";
		const string LastShownDateKey = "ReviewLastShownDate";

		static readonly TimeSpan MinTimeBetweenPrompts = TimeSpan.FromDays (60);

		readonly string appVersionMajorMinor;
		readonly NSUserDefaults defs;

		int MinNumPositiveActions { get; }

		int NumPositiveActions => (int)defs.IntForKey (CountKey);

		string LastShownVersion => defs.StringForKey (LastShownVersionKey) ?? "";

		DateTime? LastShownDate {
			get {
				var ticks = defs.DoubleForKey (LastShownDateKey);
				if (ticks <= 0 || ticks > long.MaxValue)
					return null;
				return new DateTime ((long)ticks, DateTimeKind.Utc);
			}
		}

		bool ShownForThisVersion => LastShownVersion == appVersionMajorMinor;

		bool EnoughTimeSinceLastShown =>
			LastShownDate is not DateTime lastShown ||
			(DateTime.UtcNow - lastShown) >= MinTimeBetweenPrompts;

		public bool NeedsReview => !ShownForThisVersion && EnoughTimeSinceLastShown;

		public ReviewNagging (int minNumPositiveActions = 3)
		{
			var appVersionFull = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"]?.ToString () ?? "1.0";
			appVersionMajorMinor = string.Join (".", appVersionFull.Split ('.').Take (2));

			defs = NSUserDefaults.StandardUserDefaults;
			MinNumPositiveActions = minNumPositiveActions;
		}

		public void Reset ()
		{
			defs.SetInt (0, CountKey);
			defs.RemoveObject (LastShownVersionKey);
			defs.RemoveObject (LastShownDateKey);
		}

		public void RegisterPositiveAction ()
		{
			try {
				defs.SetInt (NumPositiveActions + 1, CountKey);
				Log.Info ("Num Review Actions = " + NumPositiveActions);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public bool ShouldPresent {
			get {
				var osok = UIDevice.CurrentDevice.CheckSystemVersion (10, 3);
				var shouldPresent = osok && NeedsReview && NumPositiveActions >= MinNumPositiveActions;
				Log.Info ($"Present Review (os={osok}, shownForVersion={ShownForThisVersion}, timeOK={EnoughTimeSinceLastShown}, n={NumPositiveActions}) = {shouldPresent}");
				return shouldPresent;
			}
		}

		public void PresentIfAppropriate ()
		{
			try {
				if (ShouldPresent) {

					defs.SetString (appVersionMajorMinor, LastShownVersionKey);
					defs.SetDouble ((double)DateTime.UtcNow.Ticks, LastShownDateKey);
					defs.SetInt (0, CountKey);

					RequestReview ();
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		static void RequestReview ()
		{
			try {
				if (UIDevice.CurrentDevice.CheckSystemVersion (14, 0)) {
					var scene = UIApplication.SharedApplication.ConnectedScenes
						.OfType<UIWindowScene> ()
						.FirstOrDefault (s => s.ActivationState == UISceneActivationState.ForegroundActive);
					if (scene is not null) {
						SKStoreReviewController.RequestReview (scene);
						return;
					}
				}
				SKStoreReviewController.RequestReview ();
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}
}
