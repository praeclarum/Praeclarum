using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Praeclarum
{
	/// <summary>
	/// Wraps a value and only allows access to it using a single thread
	/// of execution (see SingleThreadQueue).
	/// </summary>
	public class SingleThreaded<T>
	{
		readonly T value;
		readonly SingleThreadQueue queue;

		public SingleThreaded (T value)
		{
			this.value = value;
			this.queue = new SingleThreadQueue ();
		}

		public void Stop ()
		{
			queue.Stop ();
		}

		public Task RunAsync (Action<T> action)
		{
			return queue.RunAsync (() => action (value));
		}

		public Task<TResult> RunAsync<TResult> (Func<T, TResult> func)
		{
			return queue.RunAsync (() => func (value));
		}
	}

	/// <summary>
	/// A queue of tasks that are run serially on a single thread.
	/// This queue owns and controls that thread.
	/// </summary>
	/// <description>
	/// This is useful for turning single-threaded blocking code into asynchronous
	/// code. It's like using Task.Run() except that it ensures that
	/// the code is run on the same thread in a serial fashion (one action at a time).
	/// </description>
	public class SingleThreadQueue
	{
		readonly Thread thread;

		readonly ConcurrentQueue<QueuedTask> queue = new ConcurrentQueue<QueuedTask> ();

		readonly AutoResetEvent newTaskEvent = new AutoResetEvent (false);

		/// <summary>
		/// Initializes the queue and starts the thread.
		/// </summary>
		public SingleThreadQueue ()
		{
			thread = new Thread ((ThreadStart)Loop);
			thread.Start ();
		}

		/// <summary>
		/// Stops the thread controlled by this queue. Run() will no longer work.
		/// </summary>
		public void Stop ()
		{
			thread.Abort ();
		}

		/// <summary>
		/// Queues the specified action to be run.
		/// </summary>
		public Task RunAsync (Action action)
		{
			var qt = new QueuedAction (action);
			queue.Enqueue (qt);
			newTaskEvent.Set ();
			return qt.Task;
		}

		/// <summary>
		/// Queues the specified function to be run.
		/// </summary>
		public Task<T> RunAsync<T> (Func<T> func)
		{
			var qt = new QueuedFunc<T> (func);
			queue.Enqueue (qt);
			newTaskEvent.Set ();
			return qt.Task;
		}

		// Analysis disable once FunctionNeverReturns
		void Loop ()
		{
			for (;;) {

				// Wait for new events to show up
				newTaskEvent.WaitOne ();

				// Execute all the tasks in the queue
				QueuedTask qt;

				var queueHasData = queue.TryDequeue (out qt);

				while (queueHasData) {
					qt.Run ();
					queueHasData = queue.TryDequeue (out qt);
				}
			}
		}

		abstract class QueuedTask
		{
			public abstract void Run ();
		}
		class QueuedAction : QueuedTask
		{
			readonly Action action;
			readonly TaskCompletionSource<object> tcs;
			public Task Task { get { return tcs.Task; } }
			public QueuedAction (Action action)
			{
				this.action = action;
				tcs = new TaskCompletionSource<object> ();
			}
			public override void Run ()
			{
				try {
					action ();
					tcs.SetResult (null);
				} catch (Exception ex) {
					tcs.SetException (ex);
				}
			}
		}
		class QueuedFunc<T> : QueuedTask
		{
			readonly Func<T> func;
			readonly TaskCompletionSource<T> tcs;
			public Task<T> Task { get { return tcs.Task; } }
			public QueuedFunc (Func<T> func)
			{
				this.func = func;
				tcs = new TaskCompletionSource<T> ();
			}
			public override void Run ()
			{
				try {
					var r = func ();
					tcs.SetResult (r);					
				} catch (Exception ex) {
					tcs.SetException (ex);
				}
			}
		}
	}
}

