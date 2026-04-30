#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;

// ReSharper disable InconsistentNaming

namespace Accessibility
{
	public class AXCustomContent : NSObject
	{
		public string Label { get; set; } = string.Empty;
		public string Value { get; set; } = string.Empty;
		public AXCustomContent () { }
		public AXCustomContent (string label, string value) { Label = label; Value = value; }
	}
}

#endif
