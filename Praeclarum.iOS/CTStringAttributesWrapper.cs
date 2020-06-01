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
using NativeStringAttributes = UIKit.UIStringAttributes;
#elif MONOMAC
using NativeColor = AppKit.NSColor;
using NativeStringAttributes = AppKit.NSStringAttributes;
#endif

namespace Praeclarum
{
	public class CTStringAttributesWrapper : IRichTextAttributes
	{
		NativeCTStringAttributes attrs;

		public NativeCTStringAttributes Attributes { get { return attrs; } }

		public CTStringAttributesWrapper (string className)
		{
			attrs = new NativeCTStringAttributes ();
			ClassName = className;
		}

#if __IOS__
		static readonly NSString FontAttributeName = new NSString ("NSFont");
		static readonly NSString ForegroundColorAttributeName = new NSString ("NSColor");
		static readonly NSString BackgroundColorAttributeName = new NSString ("NSBackgroundColor");
		static readonly NSString UnderlineColorAttributeName = new NSString ("NSUnderlineColor");
		public static readonly NSString LinkAttributeName = new NSString ("NSLink");
#elif MONOMAC
		public static readonly NSString ForegroundColorAttributeName = AppKit.NSStringAttributeKey.ForegroundColor;
		public static readonly NSString BackgroundColorAttributeName = AppKit.NSStringAttributeKey.BackgroundColor;
		public static readonly NSString LinkAttributeName = AppKit.NSStringAttributeKey.Link;
#endif


		#region IRichTextAttributes implementation

		public string ClassName { get; set; }

		string fontName = "";
		public string FontName {
			get {
				return fontName;
			}
			set {
				fontName = value;
				SetFont ();
			}
		}

		float size = 0;
		public float FontSize {
			get {
				return size;
			}
			set {
				size = value;
				SetFont ();
			}
		}

		void SetFont ()
		{
			if (string.IsNullOrEmpty (fontName) || size <= 0.1f)
				return;

#if __IOS__
			attrs.Dictionary.SetValue (FontAttributeName, UIKit.UIFont.FromName (fontName, size));
#elif MONOMAC
			attrs.Font = new CoreText.CTFont (fontName, size);
#endif
		}

		public bool UseCGColor = false;

		INativeObject ToNativeColor (Color color)
		{
			if (UseCGColor) {
				return new CGColor (
					color.RedValue,
					color.GreenValue,
					color.BlueValue,
					color.AlphaValue);
			}
#if __IOS__
			return NativeColor.FromRGBA (
				color.Red,
				color.Green,
				color.Blue,
				color.Alpha);
#elif MONOMAC
			return NativeColor.FromDeviceRgba (
				color.RedValue,
				color.GreenValue,
				color.BlueValue,
				color.AlphaValue);
#endif
		}

		Color foregroundColor = Colors.Empty;
		public Color ForegroundColor {
			get {
				return foregroundColor;
			}
			set {
				foregroundColor = value;
				attrs.Dictionary.SetValue (ForegroundColorAttributeName, ToNativeColor (value));
			}
		}

		Color backgroundColor = Colors.Empty;
		public Color BackgroundColor {
			get {
				return backgroundColor;
			}
			set {
				backgroundColor = value;
				attrs.Dictionary.SetValue (BackgroundColorAttributeName, ToNativeColor (backgroundColor));
			}
		}

		UnderlineStyle underlineStyle;
		public UnderlineStyle UnderlineStyle {
			get {
				return underlineStyle;
			}
			set {
				underlineStyle = value;
				SetUnderline ();
			}
		}

		Color underlineColor = Colors.Empty;
		public Color UnderlineColor {
			get {
				return underlineColor;
			}
			set {
				underlineColor = value;
				SetUnderline ();
			}
		}

		void SetUnderline ()
		{
#if __IOS__
			var color = UIKit.UIColor.FromRGBA (
				underlineColor.Red,
				underlineColor.Green,
				underlineColor.Blue,
				underlineColor.Alpha);
			attrs.UnderlineColor = color.CGColor;
			attrs.Dictionary.SetValue (UnderlineColorAttributeName, color);
			attrs.UnderlineStyle = underlineStyle == UnderlineStyle.None ?
				CoreText.CTUnderlineStyle.None :
								   CoreText.CTUnderlineStyle.Single;
#elif MONOMAC
			attrs.Dictionary.SetValue (AppKit.NSStringAttributeKey.UnderlineColor, ToNativeColor (underlineColor));
			var ns = underlineStyle == UnderlineStyle.None ? 0 :
			         (underlineStyle == UnderlineStyle.Single ? 0x1 : 0x9);
			attrs.Dictionary.SetValue (AppKit.NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 (ns));
#endif
		}

		string link = "";
		public string Link {
			get {
				return link;
			}
			set {
				link = value;
				if (!string.IsNullOrEmpty (link)) {
#if __IOS__
					attrs.Dictionary.SetValue (LinkAttributeName, new NSString (link));
#elif MONOMAC
					attrs.Dictionary.SetValue (LinkAttributeName, new NSString (link));
#endif
				}
			}
		}

		void SetLink ()
		{
		}

		#endregion
	}

	public class NativeStringAttributesWrapper : IRichTextAttributes
	{
		NativeStringAttributes attrs;

		public NativeStringAttributes Attributes { get { return attrs; } }

		public NativeStringAttributesWrapper (string className)
		{
			attrs = new NativeStringAttributes ();
			ClassName = className;
		}

		#region IRichTextAttributes implementation

		public string ClassName { get; set; }

		string fontName = "";
		public string FontName {
			get {
				return fontName;
			}
			set {
				fontName = value;
				SetFont ();
			}
		}

		float size = 0;
		public float FontSize {
			get {
				return size;
			}
			set {
				size = value;
				SetFont ();
			}
		}

		void SetFont ()
		{
			if (string.IsNullOrEmpty (fontName) || size <= 0.1f)
				return;

#if __IOS__
			attrs.Font = UIKit.UIFont.FromName (fontName, size);
#elif MONOMAC
			attrs.Font = new CoreText.CTFont (fontName, size);
#endif
		}

		NativeColor ToNativeColor (Color color)
		{
#if __IOS__
			return NativeColor.FromRGBA (
				color.Red,
				color.Green,
				color.Blue,
				color.Alpha);
#elif MONOMAC
			return NativeColor.FromDeviceRgba (
				color.RedValue,
				color.GreenValue,
				color.BlueValue,
				color.AlphaValue);
#endif
		}

		Color foregroundColor = Colors.Empty;
		public Color ForegroundColor {
			get {
				return foregroundColor;
			}
			set {
				foregroundColor = value;
				attrs.ForegroundColor = ToNativeColor (value);
			}
		}

		Color backgroundColor = Colors.Empty;
		public Color BackgroundColor {
			get {
				return backgroundColor;
			}
			set {
				backgroundColor = value;
				attrs.BackgroundColor = ToNativeColor (backgroundColor);
			}
		}

		UnderlineStyle underlineStyle;
		public UnderlineStyle UnderlineStyle {
			get {
				return underlineStyle;
			}
			set {
				underlineStyle = value;
				SetUnderline ();
			}
		}

		Color underlineColor = Colors.Empty;
		public Color UnderlineColor {
			get {
				return underlineColor;
			}
			set {
				underlineColor = value;
				SetUnderline ();
			}
		}

		void SetUnderline ()
		{
#if __IOS__
			var color = NativeColor.FromRGBA (
				underlineColor.Red,
				underlineColor.Green,
				underlineColor.Blue,
				underlineColor.Alpha);
			attrs.UnderlineColor = color;
			attrs.UnderlineStyle = underlineStyle == UnderlineStyle.None ?
				NSUnderlineStyle.None : NSUnderlineStyle.Single;
#elif MONOMAC
			attrs.Dictionary.SetValue (AppKit.NSStringAttributeKey.UnderlineColor, ToNativeColor (underlineColor));
			var ns = underlineStyle == UnderlineStyle.None ? 0 :
			         (underlineStyle == UnderlineStyle.Single ? 0x1 : 0x9);
			attrs.Dictionary.SetValue (AppKit.NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 (ns));
#endif
		}

		string link = "";
		public string Link {
			get {
				return link;
			}
			set {
				link = value;
				if (!string.IsNullOrEmpty (link)) {
#if __IOS__
					attrs.Link = new NSUrl (link);
#elif MONOMAC
					attrs.Dictionary.SetValue (LinkAttributeName, new NSString (link));
#endif
				}
			}
		}

		void SetLink ()
		{
		}

		#endregion
	}
}

