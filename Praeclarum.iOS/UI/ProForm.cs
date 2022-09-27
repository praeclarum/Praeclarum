#nullable enable

using System;
using Praeclarum.App;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using System.Collections.Generic;
using Internal;

namespace Praeclarum.UI
{
	public class ProForm : PForm
	{
		SubscribeSection buySection;

		ProAboutSection aboutSection;
		ProFeaturesSection featuresSection;

		static SubscriptionPrice[] prices = new SubscriptionPrice[0];

		bool subscribedToPro;

		DateTime subscribedDate;

		public ProForm(IEnumerable<(int Months, string Name)> names)
		{
			var appdel = DocumentAppDelegate.Shared;
			var app = appdel.App;
			var appName = app.Name;
			var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
			prices = names.Select(x => new SubscriptionPrice(bundleId + ".pro." + x.Months + "_month", x.Months, app.ProSymbol + " " + appName + " Pro (" + x.Name + ")")).ToArray();

			Title = "Upgrade to " + appName + " Pro";

			aboutSection = new ProAboutSection(appName);
			featuresSection = new ProFeaturesSection(appName);
			buySection = new SubscribeSection(prices);

			Sections.Add(aboutSection);
			Sections.Add(featuresSection);
			Sections.Add(buySection);
			Sections.Add (new ProRestoreSection ());
#if DEBUG
			Sections.Add (new ProDeleteSection ());
#endif

			subscribedToPro = appdel.Settings.SubscribedToPro;
			subscribedDate = appdel.Settings.SubscribedToProDate;
			aboutSection.SetPatronage();
			buySection.SetPatronage();

			RefreshProDataAsync().ContinueWith(t => {
				if (t.IsFaulted)
					Log.Error(t.Exception);
			});
		}

		static ProForm? visibleForm;

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			visibleForm = this;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			visibleForm = null;
		}

		bool hasCloud = false;

		public async Task<int> RestorePastPurchasesAsync()
		{
			StoreManager.Shared.Restore();

			//var settings = DocumentAppDelegate.Shared.Settings;
			//settings.HasTipped = isPatron;
			//settings.TipDate = endDate;

			return 0;
		}

		async Task DeletePastPurchasesAsync()
		{
			try
			{

				var settings = DocumentAppDelegate.Shared.Settings;
				settings.SubscribedToPro = false;
				settings.SubscribedToProDate = new DateTime (1970, 1, 1);
				settings.SubscribedToProMonths = 0;

			}
			catch (NSErrorException ex)
			{
				Console.WriteLine("ERROR: {0}", ex.Error);
				Log.Error(ex);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		async Task<int> RefreshProDataAsync()
		{
			var ids = prices.Select(x => x.Id).ToArray();
			var prods = await StoreManager.Shared.FetchProductInformationAsync(ids);

			foreach (var price in prices)
			{
				var prod = prods.Products.FirstOrDefault(x => x.ProductIdentifier == price.Id);
				if (prod != null)
				{
					price.Price = prod.PriceLocale.CurrencySymbol + prod.Price.DescriptionWithLocale(prod.PriceLocale);
					price.Product = prod;
				}
				else
				{
					price.Price = "?";
					price.Product = null;
				}
			}

			ReloadSection(buySection);

			var settings = DocumentAppDelegate.Shared.Settings;
			subscribedToPro = settings.SubscribedToPro;
			subscribedDate = settings.SubscribedToProDate;

			aboutSection.SetPatronage();
			buySection.SetPatronage();

			ReloadSection(aboutSection);
			ReloadSection(buySection);

			return subscribedToPro ? 1 : 0;
		}

		void ShowFormError(string title, Exception ex)
		{
			try
			{
				var iex = ex;
				while (iex.InnerException != null)
				{
					iex = iex.InnerException;
				}
				var m = iex.Message;
				var alert = UIAlertController.Create(title, m, UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, a => { }));
				PresentViewController(alert, true, null);
			}
			catch (Exception ex2)
			{
				Log.Error(ex2);
			}
		}
		public static async Task HandlePurchaseFailAsync(StoreKit.SKPaymentTransaction t)
		{
			try
			{
				var p = prices.FirstOrDefault(x => x.Id == t.Payment.ProductIdentifier);
				if (p == null)
					return;

				var m = t.Error != null ? t.Error.LocalizedDescription : "Unknown error";
				var alert = UIAlertController.Create("Tip Failed", m, UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, a => { }));

				visibleForm?.PresentViewController(alert, true, null);
			}
			catch (Exception ex)
			{
				visibleForm?.ShowError(ex);
				Log.Error(ex);
			}
		}
		static async Task AddSubscriptionAsync(string transactionId, DateTime transactionDate, SubscriptionPrice p)
		{
			var settings = DocumentAppDelegate.Shared.Settings;
			settings.SubscribedToPro = true;
			settings.SubscribedToProDate = transactionDate;
			settings.SubscribedToProMonths = p.Months;

			var v = visibleForm;
			if (v != null)
			{
				var m = "Your continued support is very much appreciated.";
				var alert = UIAlertController.Create("Thank you!", m, UIAlertControllerStyle.Alert);
				alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, a => { }));

				v.PresentViewController(alert, true, null);

				await v.RefreshProDataAsync();
			}
		}
		public static async Task HandlePurchaseCompletionAsync(StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault(x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			await AddSubscriptionAsync(t.TransactionIdentifier, (DateTime)t.TransactionDate, p);
		}

		async Task ForceSubscriptionAsync(SubscriptionPrice p)
		{
			var now = DateTime.UtcNow;
			var id = "force" + now.Ticks;
			await AddSubscriptionAsync(id, now, p);
		}

		class ProAboutSection : PFormSection
		{
			public ProAboutSection(string appName)
			{

			}

			public void SetPatronage()
			{
				var form = (ProForm)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				Title = appName + " Pro";
				Hint = $"Pro is awesome.";
				if (form.subscribedToPro)
				{
					Hint += $"\n\n⭐️⭐️⭐️ Thank you for your subscription! ⭐️⭐️⭐️";
				}
			}
		}

		class ProFeaturesSection : PFormSection
		{
			public ProFeaturesSection(string appName)
				: base("Pro Feature List")
			{
			}
			public override bool SelectItem(object item)
			{
				ShowFeatures();
				return false;
			}
			public override bool GetItemNavigates (object item)
			{
				return true;
			}
			void ShowFeatures()
			{
				var appdel = DocumentAppDelegate.Shared;
				var features = appdel.App.GetProFeatures();
				var featuresForm = new PForm("Pro Feature List");
				foreach (var (title, items) in features)
				{
					var s = new PFormSection();
					s.Title = title;
					foreach (var i in items)
					{
						s.Items.Add(i);
					}
					featuresForm.Sections.Add(s);
				}
				if (this.Form.NavigationController is UINavigationController nav)
				{
					nav.PushViewController(featuresForm, true);
				}
			}
		}

		class SubscriptionPrice
		{
			public readonly string Id;
			public readonly string Name;
			public string Price;
			public StoreKit.SKProduct? Product;
			public readonly int Months;
			public SubscriptionPrice(string id, int months, string name)
			{
				Console.WriteLine("Created subscription: " + id);
				Id = id;
				Name = name;
				Price = "";
			}
		}

		class SubscribeSection : PFormSection
		{
			public SubscribeSection(SubscriptionPrice[] prices)
				: base(prices)
			{
				Hint = "Tapping one of the above will subscribe you to iCircuit Pro.";
			}

			public void SetPatronage()
			{
				var form = (ProForm)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				Title = "Pro Subscription Options";
			}

			public override string GetItemTitle(object item)
			{
				var p = (SubscriptionPrice)item;
				return p.Name;
			}

			public override PFormItemDisplay GetItemDisplay(object item)
			{
				return PFormItemDisplay.TitleAndValue;
			}

			public override string GetItemDetails(object item)
			{
				var p = (SubscriptionPrice)item;
				return p.Price;
			}

			public override bool SelectItem(object item)
			{
				BuyAsync(item).ContinueWith(t => {
					if (t.IsFaulted)
						Log.Error(t.Exception);
				});
				return false;
			}

			async Task BuyAsync(object item)
			{
				var form = (ProForm)Form;
				try
				{
					var price = (SubscriptionPrice)item;

					if (price.Product == null)
					{
						var m =
							"The prices have not been loaded. Are you connected to the internet? If so, please wait for the prices to appear.";
						var alert = UIAlertController.Create("Unable to Proceed", m, UIAlertControllerStyle.Alert);
						alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, a => { }));
						Form.PresentViewController(alert, true, null);
						return;
					}
					else
					{
						StoreManager.Shared.Buy(price.Product);
					}
				}
				catch (Exception ex)
				{
					form.ShowFormError("Purchase Failed", ex);
					Log.Error(ex);
				}
			}
		}

		class ProRestoreSection : PFormSection
		{
			public ProRestoreSection()
				: base("Restore Subscriptions")
			{
			}

			public override bool SelectItem(object item)
			{
				RestoreAsync();
				return false;
			}

			async void RestoreAsync()
			{
				var form = (ProForm)Form;
				try
				{
					// We save receipts in iCloud

					var n = await form.RestorePastPurchasesAsync();
					//n = await form.RefreshPatronDataAsync ();

					//Console.WriteLine (n);
					//var m = n > 0 ?
					//	"Your subscriptions have been restored." :
					//	"No past subscriptions found.";
					//var alert = UIAlertController.Create ("Restore Complete", m, UIAlertControllerStyle.Alert);
					//alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, a => { }));
					//form.PresentViewController (alert, true, null);
				}
				catch (Exception ex)
				{
					form.ShowFormError("Restore Failed", ex);
					Log.Error(ex);
				}
			}
		}

		class ProDeleteSection : PFormSection
		{
			public ProDeleteSection()
				: base("Delete Subscriptions")
			{
			}

			public override bool SelectItem(object item)
			{
				DeleteAsync();
				return false;
			}

			async void DeleteAsync()
			{
				var form = (ProForm)Form;
				await form.DeletePastPurchasesAsync();
				await form.RefreshProDataAsync();
			}
		}
	}

	public class ProSection : PFormSection
	{
		public ProSection()
			: base(new Command("Upgrade to iCircuit Pro"))
		{
			var appdel = DocumentAppDelegate.Shared;
			var appName = appdel.App.Name;
			Hint = $"If you love using {appName}, you can upgrade to iCircuit Pro!";
		}

		public static bool ShouldBeAtTop
		{
			get
			{
				return true;
			}
		}

		public override bool GetItemNavigates(object item)
		{
			return true;
		}

		public override bool SelectItem(object item)
		{
			var f = new ProForm(DocumentAppDelegate.Shared.GetProPrices());
			f.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem(UIKit.UIBarButtonSystemItem.Done, (s, e) => {
				if (f != null && f.PresentingViewController != null)
				{
					f.DismissViewController(true, null);
				}
			});
			if (this.Form.NavigationController != null)
			{
				this.Form.NavigationController.PushViewController(f, true);
			}
			return false;
		}

		public override string GetItemTitle(object item)
		{
			var isp = DocumentAppDelegate.Shared.Settings.SubscribedToPro;
			return isp ?
				"You're a Pro!" :
				"Upgrade to iCircuit Pro";
		}
	}


}

