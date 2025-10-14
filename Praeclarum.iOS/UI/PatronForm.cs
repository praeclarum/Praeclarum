﻿using System;
using Praeclarum.App;
using System.Linq;
using System.Threading.Tasks;
using CloudKit;
using Foundation;
using UIKit;
using System.Collections.Generic;

namespace Praeclarum.UI
{
	public class PatronForm : PForm
	{
		PatronBuySection buySection;

		PatronAboutSection aboutSection;

		const bool allowForcePurchase = false;

		PatronForceSubscriptionSection forceSection;

		static PatronSubscriptionPrice[] prices = new PatronSubscriptionPrice[0];

		bool isPatron;

		DateTime endDate;

		public PatronForm (IEnumerable<Tuple<int, string>> monthlyPrices)
		{
			var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
			prices = monthlyPrices.Select (x => new PatronSubscriptionPrice (bundleId + ".patron.cons." + x.Item1 + "month", x.Item1, x.Item2)).ToArray ();

			var appdel = DocumentAppDelegate.Shared;
			var appName = appdel.App.Name;
			Title = "Support " + appName;

			aboutSection = new PatronAboutSection (appName);
			buySection = new PatronBuySection (prices);

			Sections.Add (aboutSection);
			Sections.Add (buySection);
			#if DEBUG
			Sections.Add (new PatronRestoreSection ());
			Sections.Add (new PatronDeleteSection ());
			#endif
			forceSection = new PatronForceSubscriptionSection (prices);

			isPatron = appdel.Settings.IsPatron;
			endDate = appdel.Settings.PatronEndDate;
			aboutSection.SetPatronage ();
			buySection.SetPatronage ();

			RefreshPatronDataAsync ().ContinueWith (t => {
				if (t.IsFaulted)
					Log.Error (t.Exception);
			});
		}

		static PatronForm visibleForm;

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

		bool hasCloud = false;

		public static async Task<int> GetPastPurchasesAsync ()
		{
			try
			{
#if __IOS__ && !__MACCATALYST__
				if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR) {
					return 0;
				}
#endif
				var container = CKContainer.DefaultContainer;

				var status = await container.GetAccountStatusAsync ();
				var hasCloud = status == CKAccountStatus.Available;
				if (!hasCloud)
					return 0;

				var db = container.PrivateCloudDatabase;

				var pred = NSPredicate.FromFormat ("TransactionId != 'tttt'");
				var query = new CKQuery ("PatronSubscription", pred);

				var recs = await db.PerformQueryAsync (query, CKRecordZone.DefaultRecordZone ().ZoneId);

				Console.WriteLine ("NUM PATRON RECS = {0}", recs.Length);

				var subs = recs.Select (x => new PatronSubscription (x)).OrderBy (x => x.PurchaseDate).ToArray ();

				var ed = DateTime.UtcNow;

				if (subs.Length > 0)
				{
					ed = subs[0].PurchaseEndDate;
					foreach (var s in subs.Skip (1))
					{
						if (s.PurchaseDate < ed)
						{
							ed = ed.AddMonths (s.NumMonths);
						}
						else
						{
							ed = s.PurchaseEndDate;
						}
					}
				}

				Console.WriteLine ("NEW END DATE = {0}", ed);

				var endDate = ed;
				var isPatron = DateTime.UtcNow < endDate;

				var settings = DocumentAppDelegate.Shared.Settings;
				settings.IsPatron = isPatron;
				settings.PatronEndDate = endDate;

				return subs.Length;
			}
			catch (Exception ex)
			{
				Log.Error (ex);
				return 0;
			}
		}

		async Task DeletePastPurchasesAsync ()
		{
			try {

				var container = CKContainer.DefaultContainer;
				var db = container.PrivateCloudDatabase;

				var pred = NSPredicate.FromFormat ("TransactionId != 'tttt'");
				var query = new CKQuery ("PatronSubscription", pred);

				var recs = await db.PerformQueryAsync (query, CKRecordZone.DefaultRecordZone().ZoneId);

				Console.WriteLine ("NUM RECS = {0}", recs.Length);

				foreach (var r in recs) {
					await db.DeleteRecordAsync (r.Id);
				}

			} catch (Exception ex) {
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
				} else {
					price.Price = "?";
					price.Product = null;
				}
			}

			ReloadSection (buySection);

			var n = 0;
			var error = false;
			try {
				n = await GetPastPurchasesAsync ();	
				var settings = DocumentAppDelegate.Shared.Settings;
				isPatron = settings.IsPatron;
				endDate = settings.PatronEndDate;
			} catch (Exception ex) {
				error = true;
				Log.Error (ex);
			}

			aboutSection.SetPatronage ();
			buySection.SetPatronage ();

			if (n == 0 && !error && allowForcePurchase) {
				if (!Sections.Contains (forceSection)) {
					Sections.Add (forceSection);
				}
			} else {
				Sections.Remove (forceSection);
			}

			ReloadSection (aboutSection);
			ReloadSection (buySection);

			return n;
		}

		async Task<bool> CheckForCloudAsync ()
		{
			try {
				Console.WriteLine ("Check for Cloud");

				var container = CKContainer.DefaultContainer;
				var s = await container.GetAccountStatusAsync ();
				hasCloud = s == CKAccountStatus.Available;
				if (!hasCloud) {
					var alert = UIKit.UIAlertController.Create ("iCloud Required for Subscriptions", "In order to keep your patronage synced between devices, you need to be logged into iCloud.", UIKit.UIAlertControllerStyle.Alert);
					var tcs = new TaskCompletionSource<object> ();
					alert.AddAction (UIKit.UIAlertAction.Create ("OK", UIKit.UIAlertActionStyle.Default, a => {
						tcs.SetResult (null);
					}));
					PresentViewController (alert, true, null);
					await tcs.Task;
				}
			} catch (NSErrorException ex) {
				Console.WriteLine ("ERROR: {0}", ex.Error);
				Log.Error (ex);
				ShowFormError ("Failed to Connect to iCloud", ex);
			} catch (Exception ex) {
				Log.Error (ex);
				ShowFormError ("Failed to Connect to iCloud", ex);
			}
			return hasCloud;
		}
#pragma warning disable 1998
		public static async Task HandlePurchaseFailAsync (StoreKit.SKPaymentTransaction t)
		{
			try {
				var m = t.Error != null ? t.Error.LocalizedDescription : "Unknown error";
				var alert = UIAlertController.Create ("Purchase Failed", m, UIAlertControllerStyle.Alert);
				alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => {}));
				var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
				while (vc.PresentedViewController != null) {
					vc = vc.PresentedViewController;
				}
				vc.PresentViewController (alert, true, null);
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}
#pragma warning restore 1998
		static async Task AddSubscriptionAsync (string transactionId, DateTime transactionDate, PatronSubscriptionPrice p)
		{
			var sub = new PatronSubscription ();
			sub.TransactionId = transactionId;
			sub.PurchaseDate = transactionDate;
			sub.ProductId = p.Id;
			sub.NumMonths = p.NumMonths;

			var db = CKContainer.DefaultContainer.PrivateCloudDatabase;
			await db.SaveRecordAsync (sub.Record);

			var v = visibleForm;
			if (v != null) {
				NSTimer.CreateScheduledTimer (1.0, nst => {
					v.BeginInvokeOnMainThread (async () => {
						for (var i = 0; i < 3; i++) {
							try {
								await v.RefreshPatronDataAsync ();
							} catch (Exception ex) {
								Log.Error (ex);
							}
							try {
								await DocumentAppDelegate.Shared.CurrentDocumentListController.LoadDocs ();
							} catch (Exception ex) {
								Log.Error (ex);
							}
							await Task.Delay (TimeSpan.FromSeconds (2.0));
						}
					});
				});
			}
		}
		public static async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			await AddSubscriptionAsync (t.TransactionIdentifier, (DateTime)t.TransactionDate, p);
		}

		async Task ForceSubscriptionAsync (PatronSubscriptionPrice p)
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
				var form = (PatronForm)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				if (form.isPatron) {
					Title = "Thank you for supporting " + appName + "!";
					Hint = "Your patronage makes continued development possible. Thank you. \ud83d\udc99\n\n" +
						"Patron through " + form.endDate.ToLongDateString () + ".";

				} else {
					Title = "Thank you for using " + appName;
					Hint = "Development of " + appName + " for iOS is supported " +
						"by voluntary patronage from people like you.\n\n" +
						"Please consider becoming a patron " +
						"to make continued development of " + appName + " possible and " +
						"to put it in as many hands as possible.";
				}
			}
		}

		class PatronSubscriptionPrice
		{
			public readonly string Id;
			public readonly int NumMonths;
			public string Price;
			public StoreKit.SKProduct Product;
			public PatronSubscriptionPrice (string id, int numMonths, string price)
			{
				Id = id;
				NumMonths = numMonths;
				Price = price;
			}
		}

		class PatronBuySection : PFormSection
		{
			public PatronBuySection (PatronSubscriptionPrice[] prices)
				: base (prices)
			{
				Hint = "These one-time purchases do not auto-renew and are saved in iCloud.";
			}

			public void SetPatronage ()
			{
				var form = (PatronForm)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				if (form.isPatron) {
					Title = "Extend your Patronage";
				} else {
					Title = "Become a Patron";
				}
			}

			public override string GetItemTitle (object item)
			{
				var p = (PatronSubscriptionPrice)item;
				return p.NumMonths + " Months";
			}

			public override PFormItemDisplay GetItemDisplay (object item)
			{
				return PFormItemDisplay.TitleAndValue;
			}

			public override string GetItemDetails (object item)
			{
				var p = (PatronSubscriptionPrice)item;
				return p.Price;
			}

			public override bool SelectItem (object item)
			{
				BuyAsync (item).ContinueWith (t => {
					if (t.IsFaulted) Log.Error (t.Exception);
				});
				return false;
			}

			async Task BuyAsync (object item)
			{
				var form = (PatronForm)Form;
				try {
					var price = (PatronSubscriptionPrice)item;

					if (price.Product == null) {
						var m =
							"The prices have not been loaded. Are you connected to the internet? If so, please wait for the prices to appear.";
						var alert = UIAlertController.Create ("Unable to Proceed", m, UIAlertControllerStyle.Alert);
						alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => {}));
						Form.PresentViewController (alert, true, null);
						return;
					}
					else {
						if (!await form.CheckForCloudAsync ())
							return;
						StoreManager.Shared.Buy (price.Product);
					}
				} catch (Exception ex) {
					form.ShowFormError ("Purchase Failed", ex);
					Log.Error (ex);
				}
			}
		}

		class PatronForceSubscriptionSection : PFormSection
		{
			PatronSubscriptionPrice[] prices;
			public PatronForceSubscriptionSection (PatronSubscriptionPrice[] prices)
				: base ()
			{
				this.prices = prices;
				Hint = "Did you already donate but are not receiving credit?";
				var c = new Command ("Already Paid", this.SelectPayAsync);
				Items.Add(c);
			}

			async Task SelectPayAsync ()
			{
				var form = (PatronForm)Form;
				try {
					if (!await form.CheckForCloudAsync ())
						return;

					var m = "Which duration did you purchase?";
					var alert = UIAlertController.Create ("", m, UIAlertControllerStyle.ActionSheet);
					foreach (var p in prices) {
						alert.AddAction (UIAlertAction.Create (p.NumMonths + " Months", UIAlertActionStyle.Default, async a => {
							try {
								await form.ForceSubscriptionAsync (p);
							} catch (Exception ex) {
								form.ShowFormError ("Purchase Restoration Failed", ex);
								Log.Error (ex);
							}
						}));
					}
					alert.AddAction (UIAlertAction.Create ("Cancel", UIAlertActionStyle.Cancel, a => {}));
					form.PresentViewController (alert, true, null);
				} catch (Exception ex) {
					form.ShowFormError ("Restore Failed", ex);
					Log.Error (ex);
				}
			}
		}

		class PatronRestoreSection : PFormSection
		{
			public PatronRestoreSection ()
				: base ("Restore Previous Purchases")
			{
			}

			public override bool SelectItem (object item)
			{
				RestoreAsync ();
				return false;
			}

			async void RestoreAsync ()
			{
				var form = (PatronForm)Form;
				try {
					if (!await form.CheckForCloudAsync ())
						return;
					// We save receipts in iCloud
					//				StoreManager.Shared.Restore ();
					var n = await form.RefreshPatronDataAsync ();

					Console.WriteLine (n);
					var m = n > 0 ?
						"Your subscriptions have been restored." :
						"No past subscriptions found for this iCloud account.";
					var alert = UIAlertController.Create ("Restore Complete", m, UIAlertControllerStyle.Alert);
					alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => {}));
					form.PresentViewController (alert, true, null);
				} catch (Exception ex) {
					form.ShowFormError ("Restore Failed", ex);
					Log.Error (ex);
				}
			}
		}

		class PatronDeleteSection : PFormSection
		{
			public PatronDeleteSection ()
				: base ("Delete Previous Purchases")
			{
			}

			public override bool SelectItem (object item)
			{
				DeleteAsync ();
				return false;
			}

			async void DeleteAsync ()
			{
				var form = (PatronForm)Form;
				if (!await form.CheckForCloudAsync ())
					return;
				await form.DeletePastPurchasesAsync ();
				await form.RefreshPatronDataAsync ();
				try {
					await DocumentAppDelegate.Shared.CurrentDocumentListController.LoadDocs ();					
				} catch (Exception ex) {
					Log.Error (ex);
				}
			}
		}
	}

	public class BecomeAPatronSection : PFormSection
	{
		public BecomeAPatronSection ()
			: base (new Command ("Become a Patron"))
		{
			var appName = "Calca";
			Hint = appName + "'s continued development is supported by voluntary patronage from people like you.";
		}

		public override bool GetItemNavigates (object item)
		{
			return true;
		}

		public override bool SelectItem (object item)
		{
			var f = new PatronForm (DocumentAppDelegate.Shared.GetPatronMonthlyPrices ());
			f.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Done, (s, e) => {
				if (f != null && f.PresentingViewController != null) {
					f.DismissViewController (true, null);
				}
			});
			if (this.Form.NavigationController != null) {
				this.Form.NavigationController.PushViewController (f, true);
			}
			return false;
		}

		public override string GetItemTitle (object item)
		{
			var isp = DocumentAppDelegate.Shared.Settings.IsPatron;
			return isp ?
				"Extend Your Patrongage" :
				"Become a Patron";
		}
	}

	public class PatronSubscription
	{
		public readonly CKRecord Record;

		public PatronSubscription ()
			: this (new CKRecord ("PatronSubscription"))
		{				
		}

		public PatronSubscription (CKRecord record)
		{
			Record = record;
		}

		public int NumMonths {
			get {
				var v = Record ["NumMonths"];
				return v != null ? ((NSNumber)v).Int32Value : 0;
			}
			set {
				Record ["NumMonths"] = (NSNumber)value;
			}
		}

		public DateTime PurchaseDate {
			get {
				var v = Record ["PurchaseDate"];
				return v != null ? (DateTime)(NSDate)v : DateTime.MinValue;
			}
			set {
				Record ["PurchaseDate"] = (NSDate)value;
			}
		}

		public DateTime PurchaseEndDate {
			get {
				return PurchaseDate.AddMonths (NumMonths);
			}
		}

		public string TransactionId {
			get {
				var v = Record ["TransactionId"];
				return v != null ? v.ToString () : "";
			}
			set {
				Record ["TransactionId"] = new NSString (value ?? "");
			}
		}

		public string ProductId {
			get {
				var v = Record ["ProductId"];
				return v != null ? v.ToString () : "";
			}
			set {
				Record ["ProductId"] = new NSString (value ?? "");
			}
		}
	}

}

