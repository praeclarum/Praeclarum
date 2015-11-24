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

		public readonly List<Func<SKPaymentTransaction, Task>> CompletionActions = new List<Func<SKPaymentTransaction, Task>> ();

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

		public void Restore ()
		{
			Console.WriteLine ("STORE Restore()");
			productsRestored.Clear ();
			SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions ();
		}

		public override void PaymentQueueRestoreCompletedTransactionsFinished (SKPaymentQueue queue)
		{
			Console.WriteLine ("STORE RestoreCompleted()");
		}

		public override void RestoreCompletedTransactionsFailedWithError (SKPaymentQueue queue, NSError error)
		{
			Console.WriteLine ("STORE ERROR RestoreError ({0})", error);
		}

		public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] transactions)
		{
			if (transactions == null)
				return;
			try {
				foreach (var t in transactions) {
					Console.WriteLine ("STORE Transaction: {0} {1} {2} {3} {4}", t.Payment.ProductIdentifier, t.TransactionState, t.TransactionIdentifier, t.TransactionDate, t.Error);
					switch (t.TransactionState) {
					case SKPaymentTransactionState.Purchased:
						productsPurchased.Add (t);
						CompleteTransaction (t);
						break;
					case SKPaymentTransactionState.Restored:
						productsRestored.Add (t);
						CompleteTransaction (t);
						break;
					}
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		async void CompleteTransaction (SKPaymentTransaction t)
		{
			if (t == null)
				return;

			if (t.TransactionState == SKPaymentTransactionState.Failed) {
				Console.WriteLine ("STORE ERROR CompleteTransaction: {0} {1} {2}", t.TransactionState, t.TransactionIdentifier, t.TransactionDate);
				return;
			}

			Console.WriteLine ("STORE CompleteTransaction: {0} {1} {2} {3}", t.Payment.ProductIdentifier, t.TransactionState, t.TransactionIdentifier, t.TransactionDate);
			foreach (var a in CompletionActions) {
				await a (t);
			}
			Console.WriteLine ("STORE FinishTransaction()");
			SKPaymentQueue.DefaultQueue.FinishTransaction (t);
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

