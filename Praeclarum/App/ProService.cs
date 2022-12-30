using System;
using Foundation;
using Praeclarum.UI;
using StoreKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Praeclarum.App
{
	public class ProPrice
	{
		public readonly string Id;
		public readonly string Name;
		public string Price;
		public object? Product;
		public readonly int Months;
		public ProPrice (string id, int months, string name)
		{
			Console.WriteLine ("Created pro subscription: " + id);
			Id = id;
			Name = name;
			Price = "";
			Months = months;
		}
	}

	public class ProService
	{
		public static readonly ProService Shared = new ProService ();

		bool restoredSubs = false;
		(DateTime Time, int Months)? restoredSubDate = null;

		readonly ProPrice[] prices;

		public ProPrice[] Prices => prices;

		public static bool SubscribedToPro
		{
			get
			{
				try
				{
					var settings = DocumentAppDelegate.Shared.Settings;
					return DateTime.UtcNow <= settings.SubscribedToProEndDate ();
				}
				catch
				{
					return false;
				}
			}
		}

		public int SubscribedToProMonths
		{
			get
			{
				var settings = DocumentAppDelegate.Shared.Settings;
				return settings.SubscribedToProMonths;
			}
		}

		private ProService ()
		{
			var appdel = DocumentAppDelegate.Shared;
			var app = appdel.App;
			var appName = app.Name;
			IEnumerable<(int Months, string Name)> names = appdel.GetProPrices ();
			var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
			prices = names.Select (x => new ProPrice (bundleId + ".pro." + x.Months + "_month", x.Months, appName + " Pro (" + x.Name + ")")).ToArray ();
		}

		void SignalProChanged ()
		{
			NSNotificationCenter.DefaultCenter.PostNotificationName (nameof (SubscribedToPro), null);
		}

		public void Restore ()
		{
			restoredSubs = false;
			restoredSubDate = null;
			StoreManager.Shared.Restore ();
		}

		public async Task HandlePurchaseRestoredAsync (NSError? error)
		{
			var settings = DocumentAppDelegate.Shared.Settings;
			if (error is not null)
			{
				// Error, do nothing
			}
			else if (restoredSubs && restoredSubDate is { } subDate)
			{
				settings.SubscribedToProDate = subDate.Time;
				settings.SubscribedToProMonths = subDate.Months;
				SignalProChanged ();
			}
			else
			{
				settings.SubscribedToProDate = DateTime.UtcNow.AddMonths (-1);
				settings.SubscribedToProMonths = 0;
				SignalProChanged ();
			}
		}

		public async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			await AddSubscriptionAsync (t.TransactionIdentifier, (DateTime)t.TransactionDate, t.TransactionState, p);
		}

		async Task AddSubscriptionAsync (string transactionId, DateTime transactionDate, SKPaymentTransactionState transactionState, ProPrice p)
		{
			if (transactionState == SKPaymentTransactionState.Restored)
			{
				restoredSubs = true;
				if (restoredSubDate is { } subDate)
				{
					if (transactionDate > subDate.Time)
					{
						restoredSubDate = (transactionDate, p.Months);
					}
				}
				else
				{
					restoredSubDate = (transactionDate, p.Months);
				}
			}
			else if (transactionState == SKPaymentTransactionState.Purchased)
			{
				var settings = DocumentAppDelegate.Shared.Settings;
				settings.SubscribedToProDate = transactionDate;
				settings.SubscribedToProMonths = p.Months;
				SignalProChanged ();
			}
		}

		public async Task DeletePastPurchasesAsync ()
		{
			try
			{
				var settings = DocumentAppDelegate.Shared.Settings;
				settings.SubscribedToProDate = new DateTime (1970, 1, 1);
				settings.SubscribedToProMonths = 0;
				SignalProChanged ();
			}
			catch (NSErrorException ex)
			{
				Console.WriteLine ("ERROR: {0}", ex.Error);
				Log.Error (ex);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
			}
		}
	}
}

