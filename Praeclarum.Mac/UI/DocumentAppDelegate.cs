using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Praeclarum.IO;
using System.IO;
using System.Globalization;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Praeclarum.UI
{
	[Register("DocumentAppDelegate")]
	public abstract class DocumentAppDelegate : NSApplicationDelegate
	{
		public DocumentApplication App { get; protected set; }

		public static DocumentAppDelegate Shared { get; private set; }

		public IDocumentAppSettings Settings
		{
			get;
			private set;
		}

		public override void WillFinishLaunching(NSNotification notification)
		{
			Shared = this;
			App = CreateApplication();
			Settings = CreateSettings();
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			//
			// In-app Purchases
			//
			try
			{
				if (App.IsPatronSupported || App.HasTips || App.HasPro)
				{
					if (Settings.IsPatron)
					{
						Settings.IsPatron = DateTime.UtcNow <= Settings.PatronEndDate;
					}
					StoreManager.Shared.RestoredActions.Add (HandlePurchaseRestoredAsync);
					StoreManager.Shared.CompletionActions.Add (HandlePurchaseCompletionAsync);
					StoreManager.Shared.FailActions.Add (HandlePurchaseFailAsync);
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex);
			}

			//
			// Start in app purchase data
			//
			try
			{
				StoreKit.SKPaymentQueue.DefaultQueue.AddTransactionObserver (StoreManager.Shared);
			}
			catch (Exception ex)
			{
				Log.Error (ex);
			}
		}

		protected virtual IDocumentAppSettings CreateSettings()
		{
			return new DocumentAppSettings(NSUserDefaults.StandardUserDefaults);
		}

		protected abstract DocumentApplication CreateApplication();

		static async Task HandlePurchaseRestoredAsync (NSError? error)
		{
			// await TipJarForm.HandlePurchaseRestoredAsync(error);
			await ProService.Shared.HandlePurchaseRestoredAsync (error);
			await ProForm.HandlePurchaseRestoredAsync (error);
			// await PatronForm.HandlePurchaseRestoredAsync (error);
		}

		static async Task HandlePurchaseCompletionAsync (StoreKit.SKPaymentTransaction t)
		{
			var pid = t.Payment.ProductIdentifier;
			if (pid.Contains (".tip."))
			{
				//await TipJarForm.HandlePurchaseCompletionAsync (t);
			}
			else if (pid.Contains (".pro."))
			{
				await ProService.Shared.HandlePurchaseCompletionAsync (t);
				await ProForm.HandlePurchaseCompletionAsync (t);
			}
			else
			{
				//await PatronForm.HandlePurchaseCompletionAsync (t);
			}
		}

		static async Task HandlePurchaseFailAsync (StoreKit.SKPaymentTransaction t)
		{
			if (t.Payment.ProductIdentifier.Contains (".tip."))
			{
				//return TipJarForm.HandlePurchaseFailAsync (t);
			}
			if (t.Payment.ProductIdentifier.Contains(".pro."))
			{
				await ProService.Shared.HandlePurchaseFailAsync(t);
				await ProForm.HandlePurchaseFailAsync(t);
			}
			else
			{
				//return PatronForm.HandlePurchaseFailAsync(t);
			}
		}
	}
}
