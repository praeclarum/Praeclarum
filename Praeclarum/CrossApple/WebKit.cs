#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace WebKit
{
	public class WKWebView : NSObject
	{
		public WKWebView () { }
		public WKWebView (CoreGraphics.CGRect frame, WKWebViewConfiguration config) { }
		public WKNavigation? LoadRequest (NSUrlRequest request) => null;
		public WKNavigation? LoadHtmlString (string html, NSUrl? baseUrl) => null;
		public bool CanGoBack { get; set; }
		public bool CanGoForward { get; set; }
	}

	public class WKWebViewConfiguration : NSObject { }
	public class WKNavigation : NSObject { }
}

#endif
