using System;
using System.Threading.Tasks;
using System.Reflection;

namespace Praeclarum
{
	public static class AsyncExtensions
	{
		public static Task<T> GetEventAsync<T> (this object eventSource, string eventName)
			where T : EventArgs
		{
			var tcs = new TaskCompletionSource<T>();
			
			var type = eventSource.GetType ();
			var ev = type.GetTypeInfo ().GetDeclaredEvent (eventName);

			EventHandler<T> handler;
			
			handler = delegate (object sender, T e) {
				ev.RemoveEventHandler (eventSource, handler);
				tcs.SetResult (e);
			};
			
			ev.AddEventHandler (eventSource, handler);
			return tcs.Task;
		}

		public static Task<EventArgs> GetEventAsync (this object eventSource, string eventName)
		{
			var tcs = new TaskCompletionSource<EventArgs>();
			
			var type = eventSource.GetType ();
			var ev = type.GetTypeInfo ().GetDeclaredEvent (eventName);
			
			EventHandler handler;
			
			handler = delegate (object sender, EventArgs e) {
				ev.RemoveEventHandler (eventSource, handler);
				tcs.SetResult (e);
			};	
			
			ev.AddEventHandler (eventSource, handler);
			return tcs.Task;
		}
	}
}

