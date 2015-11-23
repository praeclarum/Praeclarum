using System;
using Praeclarum.App;
using System.Linq;

namespace Praeclarum.UI
{
	public class PatronForm : PForm
	{
		readonly string bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;

		PatronBuySection buySection;

		PatronAboutSection aboutSection;

		readonly PatronSubscriptionPrice[] prices;

		public PatronForm ()
		{
			prices = new[] {
				new PatronSubscriptionPrice(bundleId + ".patrontest2.3month", 3, ""),
				new PatronSubscriptionPrice(bundleId + ".patron_6month", 6, ""),
				new PatronSubscriptionPrice(bundleId + ".patron_12month", 12, "")
			};

			var appdel = DocumentAppDelegate.Shared;
			var appName = appdel.App.Name;
			Title = "Support " + appName;

			aboutSection = new PatronAboutSection (appName);
			buySection = new PatronBuySection (prices);

			Sections.Add (aboutSection);
			Sections.Add (buySection);
			Sections.Add (new PatronRestoreSection ());

			var isPatron = appdel.Settings.IsPatron;
			var endDate = appdel.Settings.PatronEndDate;
			aboutSection.SetPatronage (isPatron, endDate);
			buySection.SetPatronage (isPatron, endDate);

			RefreshPatronData ();
		}

		async void RefreshPatronData ()
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

//			var isPatron = DocumentAppDelegate.Shared.IsPatronageActive;
			var isPatron = false;
			var endDate = DateTime.Now.AddDays (123);
			aboutSection.SetPatronage (isPatron, endDate);
			buySection.SetPatronage (isPatron, endDate);

			ReloadSection (aboutSection);
			ReloadSection (buySection);
		}

		class PatronAboutSection : PFormSection
		{
			public PatronAboutSection (string appName)
			{
				
			}

			public void SetPatronage (bool isPatron, DateTime endDate)
			{
				var appName = DocumentAppDelegate.Shared.App.Name;
				if (isPatron) {
					Title = "Thank you for supporting " + appName + "!";
					Hint = "Your patronage makes continued development possible. Thank you. \ud83d\udc99\n\n" +
						"Patron through " + endDate.ToLongDateString () + ".";

				} else {
					Title = "Thank you for using " + appName;
					Hint = appName + "'s development is supported " +
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
				Hint = "These one-time purchases do not auto-renew.";
			}

			public void SetPatronage (bool isPatron, DateTime endDate)
			{
				var appName = DocumentAppDelegate.Shared.App.Name;
				if (isPatron) {
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
				var price = (PatronSubscriptionPrice)item;
				if (price.Product == null)
					return false;
				StoreManager.Shared.Buy (price.Product);
				return false;
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
				StoreManager.Shared.Restore ();
				return false;
			}
		}
	}

	class BecomeAPatronSection : PFormSection
	{
		public BecomeAPatronSection ()
			: base (new Command ("Become a Patron"))
		{
			var appName = "Calca";
			Hint = appName + "'s development is supported by voluntary patronage from people like you.";
		}

		public override bool GetItemNavigates (object item)
		{
			return true;
		}

		public override bool SelectItem (object item)
		{
			var f = new PatronForm ();
			if (this.Form.NavigationController != null) {
				this.Form.NavigationController.PushViewController (f, true);
			}
			return false;
		}
	}
}

