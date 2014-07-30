using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using System.IO;
using System.Threading.Tasks;

namespace Praeclarum
{
	public class OpenUrlCommand : Command
	{
		public string Url { get; set; }

		public OpenUrlCommand (string name, string url, AsyncAction action = null)
			: base (name, action)
		{
			Url = url;
		}

		public override async Task ExecuteAsync ()
		{
			await base.ExecuteAsync ();

			if (!string.IsNullOrWhiteSpace (Url))
				UIApplication.SharedApplication.OpenUrl (NSUrl.FromString (Url));
		}
	}

	public class EmailCommand : Command
	{
		public string Address { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }

		public EmailCommand (string name, string address, AsyncAction action = null)
			: base (name, action)
		{
			Address = address;
			Subject = "";
			Body = "";
		}

		public override async Task ExecuteAsync ()
		{
			await base.ExecuteAsync ();

			if (string.IsNullOrWhiteSpace (Address))
				return;

			var fromVC = UIApplication.SharedApplication.Windows [0].RootViewController;

			var c = new MFMailComposeViewController ();
			c.Finished += (sender, e) => c.DismissViewController (true, null);
			c.SetToRecipients (new [] { Address });
			c.SetSubject (Subject);
			c.SetMessageBody (Body, false);

			fromVC.PresentViewController (c, true, null);
		}
	}

	public class EmailSupportCommand : EmailCommand
	{
		public EmailSupportCommand (string name, string address, AsyncAction action = null)
			: base (name, address, action)
		{
			var mainBundle = NSBundle.MainBundle;

			var dev = UIDevice.CurrentDevice;

			Subject =
				mainBundle.ObjectForInfoDictionary ("CFBundleDisplayName") + 
				" " + mainBundle.ObjectForInfoDictionary ("CFBundleVersion") +
				" Feedback (" + dev.SystemName + ")";

			var b = new StringWriter ();

			b.WriteLine ();
			b.WriteLine ();

			b.WriteLine ("Model: {0}", dev.Model);
			b.WriteLine ("System: {0} {1}", dev.SystemName, dev.SystemVersion);

			Body = b.ToString ();
		}
	}

	public class CopyToClipboardCommand : Command
	{
		public string Text { get; set; }

		public CopyToClipboardCommand (string name, string text, AsyncAction action = null)
			: base (name, action)
		{
			Text = text;
		}

		public override async Task ExecuteAsync ()
		{
			await base.ExecuteAsync ();

			UIPasteboard.General.String = Text ?? "";

			new UIAlertView ("Copied", Text ?? "", null, "OK").Show ();
		}
	}
}

