using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Praeclarum
{
	public class OpenUrlCommand : Command
	{
		public string Url { get; set; }

		public OpenUrlCommand (string name, string url, Action action = null)
			: base (name, action)
		{
			Url = url;
		}

		public override void Execute ()
		{
			base.Execute ();

			if (!string.IsNullOrWhiteSpace (Url))
				UIApplication.SharedApplication.OpenUrl (NSUrl.FromString (Url));
		}
	}

	public class CopyToClipboardCommand : Command
	{
		public string Text { get; set; }

		public CopyToClipboardCommand (string name, string text, Action action = null)
			: base (name, action)
		{
			Text = text;
		}

		public override void Execute ()
		{
			base.Execute ();

			UIPasteboard.General.String = Text ?? "";

			new UIAlertView ("Copied", Text ?? "", null, "OK").Show ();
		}
	}
}

