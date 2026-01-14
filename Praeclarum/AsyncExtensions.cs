using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Praeclarum
{
	public static class AsyncExtensions
	{
		[RequiresUnreferencedCode("The event is found at runtime.")]
		public static Task<T> GetEventAsync<T> (this object eventSource, string eventName)
			where T : EventArgs
		{
			var tcs = new TaskCompletionSource<T>();
			
			var type = eventSource.GetType ();
			var ev = type.GetTypeInfo ().GetDeclaredEvent (eventName);

			EventHandler<T> handler = null;
			
			handler = delegate (object sender, T e) {
				ev.RemoveEventHandler (eventSource, handler);
				tcs.SetResult (e);
			};
			
			ev.AddEventHandler (eventSource, handler);
			return tcs.Task;
		}

		[RequiresUnreferencedCode("The event is found at runtime.")]
		public static Task<EventArgs> GetEventAsync (this object eventSource, string eventName)
		{
			var tcs = new TaskCompletionSource<EventArgs>();
			
			var type = eventSource.GetType ();
			var ev = type.GetTypeInfo ().GetDeclaredEvent (eventName);
			
			EventHandler handler = null;
			
			handler = delegate (object sender, EventArgs e) {
				ev.RemoveEventHandler (eventSource, handler);
				tcs.SetResult (e);
			};	
			
			ev.AddEventHandler (eventSource, handler);
			return tcs.Task;
		}
	}
}
