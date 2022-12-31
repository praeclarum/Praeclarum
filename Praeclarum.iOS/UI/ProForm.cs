#nullable enable

using System;
using Praeclarum.App;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using System.Collections.Generic;
using StoreKit;
using System.Diagnostics;
using Circuit;
using System.Runtime;

namespace Praeclarum.UI
{
	public class ProForm : PForm
	{
		readonly SubscribeSection buySection;

		readonly ProAboutSection aboutSection;
		readonly ProFeaturesSection featuresSection;
		readonly ProRestoreSection restoreSection;

		readonly ProService service;

		static ProPrice[] prices => ProService.Shared.Prices;

		bool subscribedToPro => ProService.SubscribedToPro;

		public ProForm()
		{
			service = ProService.Shared;

			var appdel = DocumentAppDelegate.Shared;			
			var app = appdel.App;
			var appName = app.Name;			

			Title = appName + " Pro";

			aboutSection = new ProAboutSection(appName);
			featuresSection = new ProFeaturesSection(appName);
			buySection = new SubscribeSection(prices);
			restoreSection = new ProRestoreSection();

			Sections.Add(aboutSection);
			Sections.Add(featuresSection);
			Sections.Add(buySection);
			Sections.Add (restoreSection);
#if DEBUG
			Sections.Add (new ProDeleteSection ());
#endif

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
			service.Restore();			

			return 0;
		}

		async Task DeletePastPurchasesAsync()
		{
			await service.DeletePastPurchasesAsync();
			await RefreshProDataAsync();
		}

		bool needsPrices = true;

		async Task<int> RefreshProDataAsync()
		{
			if (needsPrices)
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
				needsPrices = false;
			}

			ReloadSection(buySection);

			aboutSection.SetPatronage();
			buySection.SetPatronage();

			ReloadSection(aboutSection);
			ReloadSection(buySection);
			ReloadSection(restoreSection);

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
				ShowAlert (title, m);
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
				visibleForm?.ShowAlert("Pro Subscription Failed", m);
			}
			catch (Exception ex)
			{
				visibleForm?.ShowError(ex);
				Log.Error(ex);
			}
		}
		
		static async Task AddSubscriptionAsync(string transactionId, DateTime transactionDate, SKPaymentTransactionState transactionState, ProPrice p)
		{
			if (transactionState == SKPaymentTransactionState.Purchased)
			{
				ShowThanksAlert();
				if (visibleForm is not null)
					await visibleForm.RefreshProDataAsync();
			}
		}

		private static void ShowThanksAlert ()
		{
			visibleForm?.ShowAlert ("Thank you!", $"You have successfully subscribed to {DocumentAppDelegate.Shared.App.Name} Pro!\n\nPro features are now unlocked.\n\nYour continued support is very much appreciated.");
		}

		public static async Task HandlePurchaseRestoredAsync(NSError? error)
		{
			if (error is not null)
			{
				visibleForm?.ShowAlert("Error Restoring Pro Subscription", error.LocalizedDescription);
			}
			else if (!ProService.SubscribedToPro)
			{
				visibleForm?.ShowAlert ("No Subscriptions Found", $"You are not currently subscribed to {DocumentAppDelegate.Shared.App.Name} Pro.\n\nChoose one of the pricing plans to subscribe.");
			}
			else
			{
				ShowThanksAlert();
			}
			if (visibleForm is not null)
				await visibleForm.RefreshProDataAsync();
		}
		public static async Task HandlePurchaseCompletionAsync(StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault(x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			await AddSubscriptionAsync(t.TransactionIdentifier, (DateTime)t.TransactionDate, t.TransactionState, p);
		}

		class ProAboutSection : PFormSection
		{
			public ProAboutSection(string appName)
			{

			}

			public void SetPatronage()
			{
				var form = (ProForm)Form;
				DocumentApplication app = DocumentAppDelegate.Shared.App;
				var appName = app.Name;
				Title = appName + " Pro";
				Hint = app.ProMarketing;
				if (form.subscribedToPro)
				{
					Hint = $"⭐️⭐️⭐️ Thank you for your Pro subscription! ⭐️⭐️⭐️\n\n" + Hint;
				}
				else
				{
					Hint += "\n\nTap one of the plans below to start enjoying the full iCircuit experience.";
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
#if __IOS__
				if (this.Form.NavigationController is UIKit.UINavigationController nav)
				{
					nav.PushViewController(featuresForm, true);
				}
#endif
			}
		}

		

		class SubscribeSection : PFormSection
		{
			public SubscribeSection(ProPrice[] prices)
				: base(prices)
			{
				Hint = $"Tapping one of the above will subscribe you to {DocumentAppDelegate.Shared.App.Name} Pro. If you are already subscribed to that plan, you will be able to manage your subscription. If you select a new plan, you will be able to re-subscribe using that plan.";
			}

			public void SetPatronage()
			{
				var form = (ProForm)Form;
				var appName = DocumentAppDelegate.Shared.App.Name;
				Title = "Pro Subscription Options";
			}

			public override string GetItemTitle(object item)
			{
				var p = (ProPrice)item;
				var app = DocumentAppDelegate.Shared.App;
				if (p.Months == ProService.Shared.SubscribedToProMonths)
				{
					return app.ProSymbol + " " + p.Name;
				}
				else
				{
					return "⬦ " + p.Name;
				}
			}

			public override PFormItemDisplay GetItemDisplay(object item)
			{
				return PFormItemDisplay.TitleAndValue;
			}

			public override string GetItemDetails(object item)
			{
				var p = (ProPrice)item;
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
					var price = (ProPrice)item;

					if (price.Product is StoreKit.SKProduct product)
					{
						StoreManager.Shared.Buy(product);
					}
					else
					{
						var m =
							"The prices have not been loaded. Are you connected to the internet? If so, please wait for the prices to appear.";
						form.ShowAlert ("Unable to Proceed", m);
						return;
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
				: base("Restore Pro Subscription")
			{
			}

			public override bool SelectItem(object item)
			{
				RestoreAsync();
				return false;
			}

			public override bool GetItemEnabled (object item)
			{
				return !StoreManager.Shared.IsRestoring;
			}

			async void RestoreAsync()
			{
				var form = (ProForm)Form;
				try
				{
					if (StoreManager.Shared.IsRestoring)
						return;

					var n = await form.RestorePastPurchasesAsync();
					form.ReloadSection(this);
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
			: base(new Command($"Upgrade to {DocumentAppDelegate.Shared.App.Name} Pro"))
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
			var f = new ProForm();
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
			var isp = ProService.SubscribedToPro;
			var app = DocumentAppDelegate.Shared.App;
			var appName = app.Name;
			return isp ?
				$"{app.ProSymbol} Subscribed to {appName} Pro!" :
				$"Upgrade to {appName} Pro";
		}
	}
}

