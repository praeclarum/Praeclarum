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
					var settings = DocumentAppDelegate.Shared?.Settings;
					return settings is not null && DateTime.UtcNow <= settings.SubscribedToProEndDate ();
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
				var settings = DocumentAppDelegate.Shared?.Settings;
				return settings?.SubscribedToProDate ?? DateTime.MinValue;
			}
		}

		public static string SubscribedToProFromPlatform
		{
			get
			{
				var settings = DocumentAppDelegate.Shared?.Settings;
				return settings?.SubscribedToProFromPlatform ?? "";
			}
		}

		public static int SubscribedToProMonths
		{
			get
			{
				var settings = DocumentAppDelegate.Shared?.Settings;
				return settings?.SubscribedToProMonths ?? 0;
			}
		}

		private ProService ()
		{
			if (DocumentAppDelegate.Shared?.App is { } app)
			{
				var appName = app.Name;
				IEnumerable<ProPriceSpec> names = app.GetProPrices ();
				var bundleId = Foundation.NSBundle.MainBundle.BundleIdentifier;
				prices = names.Select (x => new ProPrice (bundleId + ".pro." + x.Months + "_month", x.Months,
					appName + " Pro (" + x.Name + ")")).ToArray ();
			}
			else
			{
				prices = [];
			}
		}

		void SignalProChanged ()
		{
			DocumentAppDelegate.Shared?.BeginInvokeOnMainThread(() =>
				NSNotificationCenter.DefaultCenter.PostNotificationName (nameof (SubscribedToPro), null));
		}

#if __IOS__ || __MACOS__
		public void Restore ()
		{
			StoreManager.Shared.Restore ();
			RestoreFromCloudKit ();
		}
		public void RestoreFromCloudKit ()
		{
			RestoreFromCloudKitAsync ().ContinueWith (Log.TaskError);
		}

		async Task<CKDatabase?> GetCloudKitDatabaseAsync()
		{
			try
			{
				var containerId = DocumentAppDelegate.Shared?.App.CloudKitContainerId;
				var container = containerId is { } cid ? CKContainer.FromIdentifier(cid) : CKContainer.DefaultContainer;
				var status = await container.GetAccountStatusAsync();
				var hasCloud = status == CKAccountStatus.Available;
				if (!hasCloud)
					return null;
				return container.PrivateCloudDatabase;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		async Task<ProSubscriptionCloudRecord?> GetPlatformCloudKitSubAsync(SubPlatform platform, CKDatabase db)
		{
			try
			{
				var pred = NSPredicate.FromFormat($"Platform == '{platform}'");
				var query = new CKQuery(ProSubscriptionCloudRecord.RecordName, pred);
				var recs = await db.PerformQueryAsync(query, CKRecordZone.DefaultRecordZone().ZoneId);
				//Console.WriteLine("NUM PRO RECS = {0}", recs.Length);
				return
					recs
					.Select(x => new ProSubscriptionCloudRecord(x))
					.OrderByDescending(x => x.PurchaseDate)
					.FirstOrDefault();
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		async Task RestoreFromCloudKitAsync()
		{
			try
			{
				if ((await GetCloudKitDatabaseAsync()) is { } db)
				{
					var otherPlat = GetOtherPlatform();
					var record = await GetPlatformCloudKitSubAsync(GetOtherPlatform(), db);
					if (record is { } sub && prices.FirstOrDefault(x => x.Months == sub.NumMonths) is { } price)
					{
						Console.WriteLine("PRO FOUND OTHER SUB = {0} on {1}", record.PurchaseDate, record.Platform);
						await AddSubscriptionAsync(null, record.PurchaseDate, SKPaymentTransactionState.Restored, price, otherPlat);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		async Task SaveToCloudKitAsync (DateTime date, int months)
		{
			try
			{
				if ((await GetCloudKitDatabaseAsync ()) is { } db)
				{
					var record = await GetPlatformCloudKitSubAsync (GetThisPlatform (), db);

					if (record is not object)
					{
						record = new ProSubscriptionCloudRecord();
						record.Platform = GetThisPlatform().ToString();
					}

					record.PurchaseDate = date;
					record.NumMonths = months;
					await db.SaveRecordAsync(record.Record);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
			}
		}
#else
		public void Restore()
		{
		}
		public void RestoreFromCloudKit ()
		{
		}
		async Task RestoreFromCloudKitAsync()
		{
		}
		async Task SaveToCloudKitAsync ()
		{
		}
#endif

		public Task HandlePurchaseRestoredAsync (NSError? error)
		{
			return Task.CompletedTask;
		}

		public async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var p = prices.FirstOrDefault (x => x.Id == t.Payment.ProductIdentifier);
			if (p == null)
				return;
			DateTime transactionDate = (DateTime)(t.TransactionDate ?? NSDate.Now);
			await AddSubscriptionAsync (t.TransactionIdentifier, transactionDate, t.TransactionState, p, GetThisPlatform());
		}

		public Task HandlePurchaseFailAsync (StoreKit.SKPaymentTransaction t)
		{
			return Task.CompletedTask;
		}

		private static SubPlatform GetThisPlatform ()
		{
#if __IOS__
			return SubPlatform.iOS;
#elif __MACOS__
					return SubPlatform.Mac;
#elif __ANDROID__
					return SubPlatform.Android;
#endif
		}

		private static SubPlatform GetOtherPlatform ()
		{
#if __IOS__
			return SubPlatform.Mac;
#elif __MACOS__
					return SubPlatform.iOS;
#elif __ANDROID__
					return SubPlatform.Android;
#endif
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
					var saveText = Date.ToString ("O") + "\n" + Months;
					SubPlatform thisPlat = GetThisPlatform ();
					var saveName = $"ProSub-{thisPlat}.txt";
#if __IOS__ || __MACOS__
					NSFileManager defaultManager = Foundation.NSFileManager.DefaultManager;
					if (DocumentAppDelegate.Shared?.App.AppGroup is {} appGroup && defaultManager.GetContainerUrl (appGroup) is {} saveDirUrl)
					{
						var saveUrl = saveDirUrl.Append (saveName, false);
						NSData.FromString (saveText, NSStringEncoding.UTF8).Save (saveUrl, true);
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
					var otherPlat = GetOtherPlatform();
					var saveName = $"ProSub-{otherPlat}.txt";
#if __IOS__ || __MACOS__
					NSFileManager defaultManager = Foundation.NSFileManager.DefaultManager;
					if (DocumentAppDelegate.Shared?.App?.AppGroup is {} appGroup && defaultManager.GetContainerUrl(appGroup) is {} saveDirUrl)
					{
						var saveUrlO = saveDirUrl.Append(saveName, false);
						if (saveUrlO is { } saveUrl)
						{
							if (saveUrl.FilePathUrl?.Path is { } path && !defaultManager.FileExists(saveUrl.FilePathUrl.Path))
								return null;
							saveText = NSData.FromUrl(saveUrl).ToString(NSStringEncoding.UTF8);
						}
						else
						{
							return null;
						}
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

		async Task SaveSubscriptionIfNewerAsync(DateTime date, int months, SubPlatform fromPlatform)
		{
			var settings = DocumentAppDelegate.Shared?.Settings;
			if (settings is null || date > settings.SubscribedToProDate)
			{
				await SaveSubscriptionAsync(date, months, fromPlatform);
				SignalProChanged ();
			}
		}

		async Task SaveSubscriptionAsync (DateTime date, int months, SubPlatform fromPlatform)
		{
			if (DocumentAppDelegate.Shared?.Settings is { } settings)
			{
				settings.SubscribedToProDate = date;
				settings.SubscribedToProMonths = months;
				settings.SubscribedToProFromPlatform = fromPlatform.ToString ();
			}

			//var save = new SavedSub (date, months);
			//save.TrySave ();
			if (fromPlatform == GetThisPlatform())
			{
				await SaveToCloudKitAsync(date, months);
			}
		}

		async Task AddSubscriptionAsync (string? transactionId, DateTime transactionDate, SKPaymentTransactionState transactionState, ProPrice p, SubPlatform fromPlatform)
		{
			await SaveSubscriptionIfNewerAsync(transactionDate, p.Months, fromPlatform);
		}

		public async Task DeletePastPurchasesAsync ()
		{
			try
			{
				await SaveSubscriptionAsync (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0, GetThisPlatform());
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

		public const string RecordName = "ProSubscription";

		public ProSubscriptionCloudRecord ()
			: this (new CKRecord (RecordName))
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

		public string Platform
		{
			get
			{
				var v = Record["Platform"];
				return v != null ? v.ToString () : "";
			}
			set
			{
				Record["Platform"] = new NSString (value ?? "");
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

