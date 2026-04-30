#nullable enable

using System;
using System.Threading.Tasks;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreGraphics;
using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace AppKit
{
	public class NSView : NSObject
	{
		public NSView () { }
		public NSView (CGRect frame) { Frame = frame; }
		public CGRect Frame { get; set; }
		public CGRect Bounds { get; set; }
		public NSView[] Subviews { get; set; } = Array.Empty<NSView> ();
		public NSView? Superview { get; set; }
		public NSWindow? Window { get; set; }
		public NSColor? BackgroundColor { get; set; }
		public bool WantsLayer { get; set; }
		public bool Hidden { get; set; }
		public bool IsFlipped { get => false; }
		public NSViewResizingMask AutoresizingMask { get; set; }
		public virtual void AddSubview (NSView subview) { }
		public virtual void RemoveFromSuperview () { }
		public virtual void SetNeedsDisplayInRect (CGRect rect) { }
		public virtual void SetFrameSize (CGSize size) => Frame = new CGRect (Frame.Origin, size);
		public virtual void SetFrameOrigin (CGPoint origin) => Frame = new CGRect (origin, Frame.Size);
		public virtual void DrawRect (CGRect dirtyRect) { }
		public virtual void Layout () { }
	}

	[Flags]
	public enum NSViewResizingMask : ulong
	{
		NotSizable = 0,
		MinXMargin = 1,
		WidthSizable = 2,
		MaxXMargin = 4,
		MinYMargin = 8,
		HeightSizable = 16,
		MaxYMargin = 32,
	}

	public class NSColor : NSObject
	{
		public nfloat RedComponent { get; set; }
		public nfloat GreenComponent { get; set; }
		public nfloat BlueComponent { get; set; }
		public nfloat AlphaComponent { get; set; } = 1;
		public static NSColor Clear { get; } = new ();
		public static NSColor Black { get; } = new ();
		public static NSColor White { get; } = new ();
		public static NSColor Red { get; } = new ();
		public static NSColor Green { get; } = new ();
		public static NSColor Blue { get; } = new ();
		public static NSColor Gray { get; } = new ();
		public static NSColor LightGray { get; } = new ();
		public static NSColor DarkGray { get; } = new ();
		public static NSColor LabelColor { get; } = new ();
		public static NSColor SystemBackgroundColor { get; } = new ();
		public static NSColor FromRgb (nfloat r, nfloat g, nfloat b) => new () { RedComponent = r, GreenComponent = g, BlueComponent = b };
		public static NSColor FromRgba (nfloat r, nfloat g, nfloat b, nfloat a) => new () { RedComponent = r, GreenComponent = g, BlueComponent = b, AlphaComponent = a };
		public static NSColor FromCalibratedRgba (nfloat r, nfloat g, nfloat b, nfloat a) => FromRgba (r, g, b, a);
		public static NSColor FromDeviceRgba (nfloat r, nfloat g, nfloat b, nfloat a) => FromRgba (r, g, b, a);
		public CoreGraphics.CGColor? CGColor { get; set; }
	}

	public class NSWindow : NSObject
	{
		public CGRect Frame { get; set; }
		public string Title { get; set; } = string.Empty;
		public NSView? ContentView { get; set; }
	}

	public class NSScreen : NSObject
	{
		public static NSScreen MainScreen { get; } = new ();
		public CGRect Frame { get; set; }
		public CGRect VisibleFrame { get; set; }
		public nfloat BackingScaleFactor { get; set; } = 1;
	}

	public class NSSegmentedControl : NSView
	{
		public nint SegmentCount { get; set; }
		public nint SelectedSegment { get; set; }
		public NSSegmentSwitchTracking TrackingMode { get; set; }
		public void SetLabel (string label, nint segment) { }
		public string GetLabel (nint segment) => string.Empty;
		public void SetSelected (bool selected, nint segment) { }
		public bool IsSelectedForSegment (nint segment) => false;
	}

	public enum NSSegmentSwitchTracking : ulong { SelectOne, SelectAny, Momentary, MomentaryAccelerator }

	public class NSMenuItem : NSObject
	{
		public string Title { get; set; } = string.Empty;
		public bool Enabled { get; set; } = true;
		public NSObject? RepresentedObject { get; set; }
		public nint Tag { get; set; }
	}

	public class NSOpenPanel : NSObject
	{
		public static NSOpenPanel OpenPanel { get; } = new ();
		public bool CanChooseFiles { get; set; } = true;
		public bool CanChooseDirectories { get; set; }
		public bool AllowsMultipleSelection { get; set; }
		public string[] AllowedFileTypes { get; set; } = Array.Empty<string> ();
		public NSUrl[] Urls { get; set; } = Array.Empty<NSUrl> ();
		public NSUrl? Url { get; set; }
		public nint RunModal () => 0;
		public void Begin (Action<nint> handler) => handler (0);
	}

	public class NSPasteboard : NSObject
	{
		public static NSPasteboard GeneralPasteboard { get; } = new ();
		public string[] Types { get; set; } = Array.Empty<string> ();
		public NSData? GetDataForType (string type) => null;
		public string? GetStringForType (string type) => null;
		public bool SetDataForType (NSData data, string type) => false;
		public bool SetStringForType (string str, string type) => false;
		public nint ClearContents () => 0;
	}

	public enum NSRectEdge : ulong { MinXEdge, MinYEdge, MaxXEdge, MaxYEdge }
	public enum NSPopoverBehavior : long { ApplicationDefined, Transient, Semitransient }

	public static class NSAnimationContext
	{
		public static NSAnimationContextHandle CurrentContext { get; } = new ();
		public static void BeginGrouping () { }
		public static void EndGrouping () { }
		public static void RunAnimation (Action<NSAnimationContextHandle> changes, Action? completion = null)
		{
			changes (CurrentContext);
			completion?.Invoke ();
		}
	}

	public class NSAnimationContextHandle
	{
		public double Duration { get; set; }
		public bool AllowsImplicitAnimation { get; set; }
		public Foundation.NSObject? TimingFunction { get; set; }
	}

	public class NSImage : NSObject
	{
		public CGSize Size { get; set; }
		public NSImage () { }
		public NSImage (NSData data) { }
		public NSImage (string path) { }
	}
}

#endif
