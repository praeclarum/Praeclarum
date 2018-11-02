using System;
using UIKit;
using Foundation;
using System.Linq;
using StoreKit;

namespace Praeclarum.App
{
	public class ReviewNagging
	{
		// https://developer.apple.com/documentation/storekit/skstorereviewcontroller/requesting_app_store_reviews

		readonly string numPositiveKey;
		readonly string shownKey;
		readonly NSUserDefaults defs;

		int MinNumPositiveActions { get; }

		int NumPositiveActions => (int)defs.IntForKey (numPositiveKey);

		bool Shown => defs.BoolForKey (shownKey);

		public ReviewNagging (int minNumPositiveActions = 5)
		{
			var appVersionFull = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"]?.ToString () ?? "1.0";
			var appVersionMajorMinor = string.Join (".", appVersionFull.Split ('.').Take (2));

			defs = NSUserDefaults.StandardUserDefaults;
			numPositiveKey = "ReviewCount" + appVersionMajorMinor;
			shownKey = "ReviewShown" + appVersionMajorMinor;
			MinNumPositiveActions = minNumPositiveActions;
		}

		public void Reset ()
		{
			defs.SetInt (0, numPositiveKey);
			defs.SetBool (false, shownKey);
		}

		public void RegisterPositiveAction ()
		{
			try {
				defs.SetInt (NumPositiveActions + 1, numPositiveKey);
				Log.Info ("Num Review Actions = " + NumPositiveActions);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public void PresentIfAppropriate ()
		{
			try {

				var osok = UIDevice.CurrentDevice.CheckSystemVersion (10, 3);
				var shouldPresent = osok && !Shown && NumPositiveActions >= MinNumPositiveActions;

				Log.Info ($"Present Review (os={osok}, s={Shown}, n={NumPositiveActions}) = {shouldPresent}");

				if (shouldPresent) {

					defs.SetBool (true, shownKey);

					SKStoreReviewController.RequestReview ();

				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}
	}
}
