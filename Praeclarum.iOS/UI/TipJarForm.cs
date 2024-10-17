#nullable enable

using System;
using Praeclarum.App;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using System.Collections.Generic;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Praeclarum.UI
{
	public class TipJarForm : PForm
	{
		PatronBuySection buySection;

		PatronAboutSection aboutSection;

		static TipJarPrice[] prices = new TipJarPrice[0];

		bool hasTipped;

		DateTime tipDate;

		public TipJarForm (IEnumerable<string> names)
		{
			var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
			prices = names.Select (x => new TipJarPrice (bundleId + ".tip." + x.Replace(' ', '_').ToLowerInvariant(), x)).ToArray ();

			var appdel = DocumentAppDelegate.Shared;
			var appName = appdel.App.Name;
			Title = "Support " + appName;

			aboutSection = new PatronAboutSection (appName);
			buySection = new PatronBuySection (prices);

			Sections.Add (aboutSection);
			Sections.Add (buySection);
#if DEBUG
			Sections.Add (new TipRestoreSection ());
			Sections.Add (new TipDeleteSection ());
#endif

			hasTipped = appdel.Settings.HasTipped;
			tipDate = appdel.Settings.TipDate;
			aboutSection.SetPatronage ();
			buySection.SetPatronage ();

			RefreshPatronDataAsync ().ContinueWith (t => {
				if (t.IsFaulted)
					Log.Error (t.Exception);
			});
		}

		static TipJarForm? visibleForm;

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			visibleForm = this;
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			visibleForm = null;
		}

		public async Task<int> RestorePastPurchasesAsync ()
		{
			StoreManager.Shared.Restore ();

			//var settings = DocumentAppDelegate.Shared.Settings;
			//settings.HasTipped = isPatron;
			//settings.TipDate = endDate;

			return 0;
		}

		async Task DeletePastPurchasesAsync ()
		{
			try {

				var settings = DocumentAppDelegate.Shared.Settings;
				settings.HasTipped = false;

			}
			catch (NSErrorException ex) {
				Console.WriteLine ("ERROR: {0}", ex.Error);
				Log.Error (ex);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		async Task<int> RefreshPatronDataAsync ()
		{
			var ids = prices.Select (x => x.Id).ToArray ();
			var prods = await StoreManager.Shared.FetchProductInformationAsync (ids);

			foreach (var price in prices) {
				var prod = prods.Products.FirstOrDefault (x => x.ProductIdentifier == price.Id);
				if (prod != null) {
					price.Price = prod.PriceLocale.CurrencySymbol + prod.Price.DescriptionWithLocale (prod.PriceLocale);
					price.Product = prod;
				}
				else {
					price.Price = "?";
					price.Product = null;
				}
			}

			ReloadSection (buySection);

			var settings = DocumentAppDelegate.Shared.Settings;
			hasTipped = settings.HasTipped;
			tipDate = settings.TipDate;

			aboutSection.SetPatronage ();
			buySection.SetPatronage ();

			ReloadSection (aboutSection);
			ReloadSection (buySection);

			return hasTipped ? 1 : 0;
		}

		public static async Task HandlePurchaseFailAsync (StoreKit.SKPaymentTransaction t)
		{
			try {
				var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
				if (p == null)
					return;

				var m = t.Error != null ? t.Error.LocalizedDescription : "Unknown error";
				var alert = UIAlertController.Create ("Tip Failed", m, UIAlertControllerStyle.Alert);
				alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => { }));

				visibleForm?.PresentViewController (alert, true, null);
			}
			catch (Exception ex) {
				visibleForm?.ShowError (ex);
				Log.Error (ex);
			}
		}
		static async Task AddSubscriptionAsync (string transactionId, DateTime transactionDate, TipJarPrice p)
		{
			var settings = DocumentAppDelegate.Shared.Settings;
			settings.HasTipped = true;
			settings.TipDate = transactionDate;

			var v = visibleForm;
			if (v != null) {
				var m = "Your continued support is very much appreciated.";
				var alert = UIAlertController.Create ("Thank you!", m, UIAlertControllerStyle.Alert);
				alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => { }));

				v.PresentViewController (alert, true, null);

				await v.RefreshPatronDataAsync ();
			}
		}
		public static async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			if (t.TransactionIdentifier is null || t.TransactionDate is null)
				return;
			await AddSubscriptionAsync (t.TransactionIdentifier, (DateTime)t.TransactionDate, p);
		}

		async Task ForceSubscriptionAsync (TipJarPrice p)
		{
			var now = DateTime.UtcNow;
			var id = "force" + now.Ticks;
			await AddSubscriptionAsync (id, now, p);
		}

		class PatronAboutSection : PFormSection
		{
			public PatronAboutSection (string appName)
			{

			}

			public void SetPatronage ()
			{
				var form = (TipJarForm?)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				Title = "Hi, I'm Frank";
				Hint = $"I am the author of " + appName + " and I want to thank you for your purchase. " +
					"I am an independent developer and am able to make my living thanks to people like you. Thank you!\n\n" +
					$"This form is here if you love {appName} and want to help fund its continued development. " +
					"Tips like this help me to pay the bills and spend more time improving the app."
					;
				if (form is not null && form.hasTipped) {
					Hint += $"\n\n⭐️⭐️⭐️ Thank you for your tip on {form.tipDate.ToShortDateString ()}. " +
						"Your support is very much appreciated! ⭐️⭐️⭐️";
				}
			}
		}

		class TipJarPrice
		{
			public readonly string Id;
			public readonly string Name;
			public string Price;
			public StoreKit.SKProduct? Product;
			public TipJarPrice (string id, string name)
			{
				Console.WriteLine ("Created tip: " + id);
				Id = id;
				Name = name + " Tip";
				Price = "";
			}
		}

		class PatronBuySection : PFormSection
		{
			public PatronBuySection (TipJarPrice[] prices)
				: base (prices)
			{
				Hint = "Tapping one of the above will charge you the listed amount.";
			}

			public void SetPatronage ()
			{
				var form = (TipJarForm?)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				if (form is not null && form.hasTipped) {
					Title = "Tip Again";
				}
				else {
					Title = "Tip Jar";
				}
			}

			public override string GetItemTitle (object item)
			{
				var p = (TipJarPrice)item;
				return p.Name;
			}

			public override PFormItemDisplay GetItemDisplay (object item)
			{
				return PFormItemDisplay.TitleAndValue;
			}

			public override string GetItemDetails (object item)
			{
				var p = (TipJarPrice)item;
				return p.Price;
			}

			public override bool SelectItem (object item)
			{
				BuyAsync (item).ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);
				});
				return false;
			}

			async Task BuyAsync (object item)
			{
				var form = (TipJarForm?)Form;
				try {
					var price = (TipJarPrice)item;

					if (price.Product == null) {
						var m =
							"The prices have not been loaded. Are you connected to the internet? If so, please wait for the prices to appear.";
						var alert = UIAlertController.Create ("Unable to Proceed", m, UIAlertControllerStyle.Alert);
						alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => { }));
						Form?.PresentViewController (alert, true, null);
						return;
					}
					else {
						StoreManager.Shared.Buy (price.Product);
					}
				}
				catch (Exception ex) {
					form?.ShowFormError ("Purchase Failed", ex);
					Log.Error (ex);
				}
			}
		}

		class TipRestoreSection : PFormSection
		{
			public TipRestoreSection ()
				: base ("Restore Previous Tips")
			{
			}

			public override bool SelectItem (object item)
			{
				RestoreAsync ();
				return false;
			}

			async void RestoreAsync ()
			{
				var form = (TipJarForm?)Form;
				if (form is null)
				{
					return;
				}
				try {
					// We save receipts in iCloud

					var n = await form.RestorePastPurchasesAsync ();
					//n = await form.RefreshPatronDataAsync ();

					//Console.WriteLine (n);
					//var m = n > 0 ?
					//	"Your subscriptions have been restored." :
					//	"No past subscriptions found.";
					//var alert = UIAlertController.Create ("Restore Complete", m, UIAlertControllerStyle.Alert);
					//alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => { }));
					//form.PresentViewController (alert, true, null);
				}
				catch (Exception ex) {
					form.ShowFormError ("Restore Failed", ex);
					Log.Error (ex);
				}
			}
		}

		class TipDeleteSection : PFormSection
		{
			public TipDeleteSection ()
				: base ("Delete Previous Tips")
			{
			}

			public override bool SelectItem (object item)
			{
				DeleteAsync ();
				return false;
			}

			async void DeleteAsync ()
			{
				if (Form is TipJarForm form)
				{
					await form.DeletePastPurchasesAsync ();
					await form.RefreshPatronDataAsync ();
				}
			}
		}
	}

	public class AddToTipJarSection : PFormSection
	{
		public AddToTipJarSection ()
			: base (new Command ("Tip the Author"))
		{
			var appdel = DocumentAppDelegate.Shared;
			var appName = appdel.App.Name;
			Hint = $"If you love using {appName}, you can tip me to help fund its development. Thank you!";
		}

		public static bool ShouldBeAtTop {
			get {
				return true;
			}
		}

		public override bool GetItemNavigates (object item)
		{
			return true;
		}

		public override bool SelectItem (object item)
		{
			var f = new TipJarForm (DocumentAppDelegate.Shared.GetTipDollars ());
			f.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Done, (s, e) => {
				if (f != null && f.PresentingViewController != null) {
					f.DismissViewController (true, null);
				}
			});
			if (this.Form?.NavigationController is {} nav) {
				nav.PushViewController (f, true);
			}
			return false;
		}

		public override string GetItemTitle (object item)
		{
			var isp = DocumentAppDelegate.Shared.Settings.HasTipped;
			return isp ?
				"Tip the Author Again" :
				"Tip the Author";
		}
	}


}

