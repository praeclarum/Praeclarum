#nullable enable

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
		private DocumentApplication? _app;

		public DocumentApplication App
		{
			get
			{
				if (_app is null)
				{
					_app = CreateApplication();
				}
				return _app;
			}
		}

		public static DocumentAppDelegate? Shared { get; private set; }
		public static string AppName => Shared?.App.Name ?? "App";

		public IDocumentAppSettings? Settings
		{
			get;
			private set;
		}

		NSWindow? _proWindow;

		public override void WillFinishLaunching(NSNotification notification)
		{
			Shared = this;
			Settings = CreateSettings();
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			//
			// In-app Purchases
			//
			try
			{
				if (App is {} app && (app.IsPatronSupported || app.HasTips || app.HasPro))
				{
					if (Settings is {} settings && settings.IsPatron)
					{
						settings.IsPatron = DateTime.UtcNow <= Settings.PatronEndDate;
					}
					StoreManager.Shared.RestoredActions.Add (HandlePurchaseRestoredAsync);
					StoreManager.Shared.PurchasingActions.Add (HandlePurchasingAsync);
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

		static async Task HandlePurchasingAsync (StoreKit.SKPaymentTransaction t)
		{
			var pid = t.Payment.ProductIdentifier;
			if (pid.Contains (".tip."))
			{
			}
			else if (pid.Contains (".pro."))
			{
				await ProForm.HandlePurchasingAsync (t);
			}
			else
			{
			}
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

		[Export("showProPanel:")]
		public void ShowProPanel(NSObject? sender)
		{
			PForm.ShowWindow(sender, ref _proWindow, () => new ProForm());
		}

		public void PromotePro (string error, NSObject? sender)
		{
			BeginInvokeOnMainThread(() =>
			{
				if (App is not {} app )
					return;
				var message = $"{App.ProSymbol} This feature is only available in {App.Name} Pro.\n\nYou can unlock this feature and others by clicking \"Learn More\" below.";

				var alert = NSAlert.WithMessage (error, "Learn More About Pro", "Cancel", otherButton: null, full: message);
				var result = alert.RunSheetModal (null, NSApplication.SharedApplication);
				if (result.ToInt64 () == 1)
				{
					ShowProPanel (sender);
				}
			});
		}
	}
}
