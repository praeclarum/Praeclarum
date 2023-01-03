﻿#nullable enable

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

		bool isPurchasing = false;

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
			buySection.SetPatronage(isPurchasing);

			BeginRefreshProData();
		}

		static ProForm? visibleForm;

		NSObject? proObserver = null;

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			visibleForm = this;
			NSNotificationCenter.DefaultCenter.AddObserver(new NSString(nameof(ProService.SubscribedToPro)), n =>
			{
				BeginInvokeOnMainThread(() =>
				{
					aboutSection.SetPatronage();
					buySection.SetPatronage(isPurchasing);
					ReloadAll();
				});
			});
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			visibleForm = null;
			if (proObserver is not null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(proObserver);
				proObserver = null;
			}
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
			BeginRefreshProData();
		}

		bool needsPrices = true;

		void BeginRefreshProData()
		{
			BeginInvokeOnMainThread(() =>
			{
				RefreshProDataAsync ().ContinueWith (t => {
					if (t.IsFaulted)
						Log.Error (t.Exception);
				});
			});
		}

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

			aboutSection.SetPatronage();
			buySection.SetPatronage(isPurchasing);

			ReloadSection(aboutSection);
			ReloadSection(buySection);
			ReloadSection(restoreSection);

			return subscribedToPro ? 1 : 0;
		}

		public static async Task HandlePurchaseFailAsync(StoreKit.SKPaymentTransaction t)
		{
			try
			{
				var p = prices.FirstOrDefault(x => x.Id == t.Payment.ProductIdentifier);
				if (p == null)
					return;

				var m = t.Error != null ? t.Error.LocalizedDescription : "Unknown error";
				if (visibleForm is { } f)
				{
					f.isPurchasing = false;
					f.BeginRefreshProData ();
					f.ShowAlert("Pro Subscription Failed", m);
				}
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
				if (visibleForm is { } f)
				{
					f.isPurchasing = false;
					f.BeginRefreshProData();
				}
			}
		}

		private static void ShowThanksAlert ()
		{
			visibleForm?.ShowAlert ("Thank you!", $"You have successfully subscribed to {DocumentAppDelegate.Shared.App.Name} Pro!\n\nPro features are now unlocked.\n\nYour continued support is very much appreciated.");
		}

		public static async Task HandlePurchaseRestoredAsync(NSError? error)
		{
		}

		public static async Task HandlePurchasingAsync(StoreKit.SKPaymentTransaction t)
		{
			if (visibleForm is { } f)
			{
				f.isPurchasing = true;
				f.BeginRefreshProData ();
			}
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
				DocumentApplication app = DocumentAppDelegate.Shared.App;
				var appName = app.Name;
				Title = appName + " Pro";
				Hint = app.ProMarketing;
				if (ProService.SubscribedToPro)
				{
					Hint = $"⭐️⭐️⭐️ Thank you for your Pro subscription! ⭐️⭐️⭐️\n\nYou last renewed on {ProService.SubscribedToProDate.ToShortDateString()} for {ProService.SubscribedToProMonths} months.\n\n" + Hint;
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
				Form?.PushForm(featuresForm);
			}
		}

		class SubscribeSection : PFormSection
		{
			readonly string baseHint = $"Tapping one of the above will subscribe you to {DocumentAppDelegate.Shared.App.Name} Pro. If you are already subscribed to that plan, you will be able to manage your subscription. If you select a new plan, you will be able to re-subscribe using that plan.";

			public SubscribeSection(ProPrice[] prices)
				: base(prices)
			{
				Hint = baseHint;
			}

			public void SetPatronage(bool purchasing)
			{
				Title = "Pro Subscription Options";
				Hint = purchasing ? "Purchasing...\n\n" + baseHint : baseHint;
			}

			public override string GetItemTitle(object item)
			{
				var p = (ProPrice)item;
				var app = DocumentAppDelegate.Shared.App;
				if (p.Months == ProService.SubscribedToProMonths)
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
						Form?.ShowAlert ("Unable to Proceed", m);
						return;
					}
				}
				catch (Exception ex)
				{
					Form?.ShowFormError("Purchase Failed", ex);
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
					Form?.ReloadSection(this);
				}
				catch (Exception ex)
				{
					Form?.ShowFormError("Restore Failed", ex);
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
				if (Form is ProForm form)
				{
					await form.DeletePastPurchasesAsync();
					await form.RefreshProDataAsync();
				}
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
			Form?.PushForm (f);
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

