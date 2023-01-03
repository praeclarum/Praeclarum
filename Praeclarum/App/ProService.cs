#nullable enable

using System;
using Foundation;
using Praeclarum.UI;
using StoreKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime;
using System.IO;

#if __IOS__ || __MACOS__
using CloudKit;
#endif

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

		public static DateTime SubscribedToProDate
		{
			get
			{
				var settings = DocumentAppDelegate.Shared.Settings;
				return settings.SubscribedToProDate;
			}
		}

		public static int SubscribedToProMonths
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
			IEnumerable<(int Months, string Name)> names = app.GetProPrices ();
			var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
			prices = names.Select (x => new ProPrice (bundleId + ".pro." + x.Months + "_month", x.Months, appName + " Pro (" + x.Name + ")")).ToArray ();
		}

		void SignalProChanged ()
		{
			DocumentAppDelegate.Shared.BeginInvokeOnMainThread(() =>
				NSNotificationCenter.DefaultCenter.PostNotificationName (nameof (SubscribedToPro), null));
		}

		public void Restore ()
		{
#if __IOS__ || __MACOS__
			StoreManager.Shared.Restore ();
#endif
		}

		public async Task HandlePurchaseRestoredAsync (NSError? error)
		{
		}

		public async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			await AddSubscriptionAsync (t.TransactionIdentifier, (DateTime)t.TransactionDate, t.TransactionState, p);
		}

		public async Task HandlePurchaseFailAsync (StoreKit.SKPaymentTransaction t)
		{
		}

		enum SubPlatform
		{
			iOS,
			Mac,
			Android
		}

		class SavedSub
		{
			public readonly DateTime Date;
			public readonly int Months;

			public SavedSub(DateTime date, int months)
			{
				Date = date;
				Months = months;
			}

			public void TrySave()
			{
				try
				{
					var saveText = Date.ToString("O") + "\n" + Months;
#if __IOS__
					var thisPlat = SubPlatform.iOS;
#elif __MACOS__
					var thisPlat = SubPlatform.Mac;
#elif __ANDROID__
					var thisPlat = SubPlatform.Android;
#endif
					var saveName = $"ProSub-{thisPlat}.txt";
#if __IOS__ || __MACOS__
					NSFileManager defaultManager = Foundation.NSFileManager.DefaultManager;
					if (DocumentAppDelegate.Shared.App.AppGroup is string appGroup && defaultManager.GetContainerUrl (appGroup) is NSUrl saveDirUrl)
					{
						var saveUrl = saveDirUrl.Append(saveName, false);
						NSData.FromString(saveText, NSStringEncoding.UTF8).Save(saveUrl, true);
					}
#else
#endif
				}
				catch (Exception ex)
				{
					Log.Error(ex);
				}
			}

			public static SavedSub? TryLoadOther()
			{
				try
				{
					string saveText = "";
#if __IOS__
					var otherPlat = SubPlatform.Mac;
#elif __MACOS__
					var otherPlat = SubPlatform.iOS;
#elif __ANDROID__
					var otherPlat = SubPlatform.Android;
#endif
					var saveName = $"ProSub-{otherPlat}.txt";
#if __IOS__ || __MACOS__
					NSFileManager defaultManager = Foundation.NSFileManager.DefaultManager;
					if (DocumentAppDelegate.Shared.App.AppGroup is string appGroup && defaultManager.GetContainerUrl(appGroup) is NSUrl saveDirUrl)
					{
						var saveUrl = saveDirUrl.Append(saveName, false);
						if (!defaultManager.FileExists(saveUrl.FilePathUrl.Path))
							return null;
						saveText = NSData.FromUrl(saveUrl).ToString(NSStringEncoding.UTF8);
					}

#else
#endif
					var lines = saveText.Split("\n").Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
					if (lines.Length >= 2)
					{
						return new SavedSub(DateTime.Parse(lines[0]), int.Parse(lines[1]));
					}
					return null;
				}
				catch
				{
					return null;
				}
			}
		}

		async Task SaveSubscriptionIfNewerAsync(DateTime date, int months)
		{
			var settings = DocumentAppDelegate.Shared.Settings;
			if (date > settings.SubscribedToProDate)
			{
				await SaveSubscriptionAsync(date, months);
				SignalProChanged ();
			}
		}

		async Task SaveSubscriptionAsync (DateTime date, int months)
		{
			var settings = DocumentAppDelegate.Shared.Settings;
			settings.SubscribedToProDate = date;
			settings.SubscribedToProMonths = months;
			//var save = new SavedSub (date, months);
			//save.TrySave ();
		}

		async Task AddSubscriptionAsync (string transactionId, DateTime transactionDate, SKPaymentTransactionState transactionState, ProPrice p)
		{
			await SaveSubscriptionIfNewerAsync(transactionDate, p.Months);
		}

		public async Task DeletePastPurchasesAsync ()
		{
			try
			{
				await SaveSubscriptionAsync (new DateTime (1970, 1, 1), 0);
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

#if __IOS__ || __MACOS__
	public class ProSubscriptionCloudRecord
	{
		public readonly CKRecord Record;

		public ProSubscriptionCloudRecord ()
			: this (new CKRecord ("ProSubscription"))
		{
		}

		public ProSubscriptionCloudRecord (CKRecord record)
		{
			Record = record;
		}

		public int NumMonths
		{
			get
			{
				var v = Record["NumMonths"];
				return v != null ? ((NSNumber)v).Int32Value : 0;
			}
			set
			{
				Record["NumMonths"] = (NSNumber)value;
			}
		}

		public DateTime PurchaseDate
		{
			get
			{
				var v = Record["PurchaseDate"];
				return v != null ? (DateTime)(NSDate)v : DateTime.MinValue;
			}
			set
			{
				Record["PurchaseDate"] = (NSDate)value;
			}
		}

		public DateTime PurchaseEndDate
		{
			get
			{
				return PurchaseDate.AddMonths (NumMonths);
			}
		}

		public string TransactionId
		{
			get
			{
				var v = Record["TransactionId"];
				return v != null ? v.ToString () : "";
			}
			set
			{
				Record["TransactionId"] = new NSString (value ?? "");
			}
		}

		public string ProductId
		{
			get
			{
				var v = Record["ProductId"];
				return v != null ? v.ToString () : "";
			}
			set
			{
				Record["ProductId"] = new NSString (value ?? "");
			}
		}
	}
#endif
}

