#nullable enable

using System;
using System.Threading.Tasks;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace CoreFoundation
{
	public class DispatchQueue : NSObject
	{
		public static DispatchQueue MainQueue { get; } = new DispatchQueue ("main");
		public static DispatchQueue CurrentQueue => MainQueue;
		public static DispatchQueue DefaultGlobalQueue => MainQueue;
		public static DispatchQueue GetGlobalQueue (DispatchQueuePriority priority) => MainQueue;
		public string Label { get; }
		public DispatchQueue () : this ("default") { }
		public DispatchQueue (string label) { Label = label; }
		public void DispatchAsync (Action action) => Task.Run (action);
		public void DispatchSync (Action action) => action ();
		public void DispatchAfter (DispatchTime when, Action action) => Task.Run (action);
		public void Activate () { }
		public void Suspend () { }
		public void Resume () { }
	}

	public enum DispatchQueuePriority
	{
		High = 2, Default = 0, Low = -2, Background = int.MinValue,
	}

	public struct DispatchTime
	{
		public ulong Nanoseconds;
		public static DispatchTime Now => default;
		public DispatchTime (DispatchTime when, long deltaNanoseconds) { Nanoseconds = (ulong)deltaNanoseconds; }
	}

	public class CFRunLoop : NSObject
	{
		public static CFRunLoop Main { get; } = new CFRunLoop ();
		public static CFRunLoop Current => Main;
	}

	public class OSLog : NSObject
	{
		public static OSLog Default { get; } = new OSLog ();
		public OSLog () { }
		public OSLog (string subsystem, string category) { }
	}
}

#endif
