#nullable enable

using System;
using StoreKit;
using Foundation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudKit;

namespace Praeclarum.App
{
	public class StoreManager : SKPaymentTransactionObserver
	{
		public static readonly StoreManager Shared = new StoreManager ();

		readonly List<SKPaymentTransaction> productsPurchased = new List<SKPaymentTransaction> ();
		readonly List<SKPaymentTransaction> productsRestored = new List<SKPaymentTransaction> ();

		public readonly List<Func<NSError?, Task>> RestoredActions = new List<Func<NSError?, Task>> ();
		public readonly List<Func<SKPaymentTransaction, Task>> PurchasingActions = new List<Func<SKPaymentTransaction, Task>> ();
		public readonly List<Func<SKPaymentTransaction, Task>> CompletionActions = new List<Func<SKPaymentTransaction, Task>> ();
		public readonly List<Func<SKPaymentTransaction, Task>> FailActions = new List<Func<SKPaymentTransaction, Task>> ();

		public Task<SKProductsResponse> FetchProductInformationAsync (string[] ids)
		{
			var request = new SKProductsRequest (
				NSSet.MakeNSObjectSet (ids.Select (x => new NSString(x)).ToArray ()));
			var del = new TaskRequestDelegate ();
			request.Delegate = del;
			request.Start ();
			return del.Task;
		}

		public void Buy (SKProduct product)
		{			
			Console.WriteLine ("STORE Buy({0})", product.ProductIdentifier);
			var payment = SKMutablePayment.PaymentWithProduct (product);
			SKPaymentQueue.DefaultQueue.AddPayment (payment);
		}

		public bool IsRestoring => restoreCompletion is not null;
		// This task represents Apple's request, not how long a screen waits for it.
		// StoreKit provides neither a restore cancellation API nor a request ID in its callbacks.
		TaskCompletionSource<bool>? restoreCompletion;
		Task transactionProcessing = Task.CompletedTask;
		Exception? restoreProcessingError;

		public void Restore ()
		{
			RestoreAsync ().ContinueWith (Log.TaskError);
		}

		public Task RestoreAsync ()
		{
			// Another caller joins the existing request. Only Apple's callback completes it.
			if (restoreCompletion is { } pending)
				return pending.Task;
			var completion = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
			restoreCompletion = completion;
			restoreProcessingError = null;
			Console.WriteLine ("STORE Restore()");
			productsRestored.Clear ();
			try {
				SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions ();
			}
			catch (Exception ex) {
				restoreCompletion = null;
				completion.TrySetException (ex);
			}
			return completion.Task;
		}

		public override async void RestoreCompletedTransactionsFinished (SKPaymentQueue queue)
		{
			Console.WriteLine ("STORE RestoreCompleted()");
			await CompleteRestoreAsync (error: null);
		}

		public override async void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
		{
			Console.WriteLine ("STORE ERROR RestoreError ({0})", error);
			await CompleteRestoreAsync (error);
		}

		async Task CompleteRestoreAsync (NSError? error)
		{
			var completion = restoreCompletion;
			// StoreKit's finished callback can arrive while an async transaction handler is still running.
			await transactionProcessing;
			if (ReferenceEquals (restoreCompletion, completion))
				restoreCompletion = null;
			if (error is not null)
				completion?.TrySetException (new NSErrorException (error));
			else if (restoreProcessingError is { } processingError)
				completion?.TrySetException (processingError);
			else
				completion?.TrySetResult (true);
			Log.Info ($"STORE RestoreProcessed: restoredTransactions={productsRestored.Count}");
			await RestoredAsync (error);
		}

		public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
		{
			transactionProcessing = ProcessTransactionsAsync (transactionProcessing, transactions);
		}

		async Task ProcessTransactionsAsync (Task previous, SKPaymentTransaction[] transactions)
		{
			await previous;
			if (transactions is null)
				return;
			try {
				foreach (var t in transactions) {
					try {
						Console.WriteLine ("STORE Transaction: {0} {1} {2} {3} {4}", t.Payment.ProductIdentifier, t.TransactionState, t.TransactionIdentifier, t.TransactionDate, t.Error);
						switch (t.TransactionState) {
						case SKPaymentTransactionState.Purchasing:
							foreach (var a in PurchasingActions)
							{
								await a (t);
							}
							break;
						case SKPaymentTransactionState.Purchased:
							productsPurchased.Add (t);
							await CompleteTransactionAsync (t);
							break;
						case SKPaymentTransactionState.Restored:
							productsRestored.Add (t);
							await CompleteTransactionAsync (t);
							break;
						case SKPaymentTransactionState.Failed:
							await CompleteTransactionAsync (t);
							break;
						}
					} catch (Exception ex) {
						if (IsRestoring)
							restoreProcessingError ??= ex;
						Log.Error ("STORE transaction processing failed", ex);
					}
				}
			} catch (Exception ex) {
				if (IsRestoring)
					restoreProcessingError ??= ex;
				Log.Error (ex);
			}
		}

		async Task CompleteTransactionAsync (SKPaymentTransaction t)
		{
			if (t == null)
				return;

			if (t.TransactionState == SKPaymentTransactionState.Failed) {
				Console.WriteLine ("STORE ERROR CompleteTransaction: {0} {1} {2}", t.TransactionState, t.TransactionIdentifier, t.TransactionDate);
				foreach (var a in FailActions) {
					await a (t);
				}
			} else {
				Console.WriteLine ("STORE CompleteTransaction: {0} {1} {2} {3}", t.Payment.ProductIdentifier, t.TransactionState, t.TransactionIdentifier, t.TransactionDate);
				foreach (var a in CompletionActions) {
					await a (t);
				}
			}
			Console.WriteLine ("STORE FinishTransaction()");
			SKPaymentQueue.DefaultQueue.FinishTransaction (t);
		}

		async Task RestoredAsync (NSError? error)
		{
			foreach (var a in RestoredActions)
			{
				try
				{
					await a(error);
				}
				catch (Exception ex)
				{
					Log.Error (ex);
				}
			}
		}

		class TaskRequestDelegate : SKProductsRequestDelegate
		{
			readonly TaskCompletionSource<SKProductsResponse> tcs = new TaskCompletionSource<SKProductsResponse> ();

			public Task<SKProductsResponse> Task { get { return tcs.Task; } }

			public override void ReceivedResponse (SKProductsRequest request, SKProductsResponse response)
			{
				tcs.SetResult (response);
			}
			public override void RequestFailed (SKRequest request, NSError error)
			{
				tcs.SetException (new Exception (error != null ? error.ToString () : ""));
			}
		}
	}
}
