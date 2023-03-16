using System;
using Praeclarum.Graphics;
using System.Runtime.InteropServices;

using Foundation;
using ObjCRuntime;
using NativeNSMutableAttributedString = Foundation.NSMutableAttributedString;
using NativeCTStringAttributes = CoreText.CTStringAttributes;
using CGColor = CoreGraphics.CGColor;
#if __IOS__
using NativeColor = UIKit.UIColor;
#elif MONOMAC || __MACOS__
using NativeColor = AppKit.NSColor;
#endif

namespace Praeclarum
{
	

	public class NSMutableAttributedStringWrapper : IRichText
	{
		NativeNSMutableAttributedString s;

		public NSMutableAttributedStringWrapper (NativeNSMutableAttributedString ns)
		{
			s = ns;
		}

		public NSMutableAttributedStringWrapper (string data)
		{
			s = new NativeNSMutableAttributedString (data);
		}

		public NativeNSMutableAttributedString AttributedText {
			get { return s; }
		}

		#region NSMutableAttributedString implementation

		public void AddAttributes (IRichTextAttributes styleClass, StringRange range)
		{
			var attrs = ((NativeStringAttributesWrapper)styleClass).Attributes;
			s.AddAttributes (attrs, new NSRange (range.Location, range.Length));
		}

		#endregion
	}

	public static class StringRangeEx
	{
		public static StringRange ToStringRange (this NSRange range)
		{
			return new StringRange ((int)range.Location, (int)range.Length);
		}

		public static NSRange ToNSRange (this StringRange r)
		{
			return new NSRange (r.Location, r.Length);
		}
	}

	public static class NSDictionaryEx
	{
		[DllImport ("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern void CFDictionarySetValue (IntPtr theDict, IntPtr key, IntPtr value);

		public static void SetValue (this NSDictionary theDict, NSString key, INativeObject value)
		{
			SetValue (theDict.Handle, key.Handle, value.Handle);
		}

		static void SetValue (IntPtr theDict, IntPtr key, IntPtr value)
		{
			CFDictionarySetValue (theDict, key, value);
		}

		public static void AddAttributes (this NativeNSMutableAttributedString s, NativeCTStringAttributes a, StringRange r)
		{
			s.AddAttributes (a, new NSRange (r.Location, r.Length));
		}
	}
}

