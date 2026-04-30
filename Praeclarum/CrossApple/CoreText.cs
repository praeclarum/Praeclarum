#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreGraphics;
using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace CoreText
{
	public class CTFont : NSObject
	{
		public CTFont (string name, nfloat size) { Size = size; FullName = name; FamilyName = name; }
		public CTFont (NSString name, nfloat size) : this ((string)(name?.ToString () ?? ""), size) { }
		public CTFontDescriptor GetFontDescriptor () => new ();
		public CTFont WithSize (nfloat size) => new (FullName, size);
		public string FullName { get; }
		public string FamilyName { get; }
		public nfloat Size { get; }
		public nfloat AscentMetric { get; set; }
		public nfloat DescentMetric { get; set; }
		public nfloat LeadingMetric { get; set; }
		public CGRect BoundingBox { get; set; }
		public CGPath? GetPathForGlyph (ushort glyph) => null;
	}

	public class CTFontDescriptor : NSObject
	{
		public CTFontDescriptor WithSize (nfloat size) => new ();
	}

	public class CTLine : NSObject
	{
		public static CTLine? Create (NSAttributedString str) => null;
		public CGRect GetImageBounds (CGContext ctx) => default;
		public void Draw (CGContext ctx) { }
	}

	public class CTFramesetter : NSObject
	{
		public CTFramesetter (NSAttributedString str) { }
	}

	public class CTStringAttributes
	{
		public CTFont? Font { get; set; }
		public CGColor? ForegroundColor { get; set; }
		public NSDictionary? Dictionary { get; set; }
	}
}

#endif
