using System;
using UIKit;
using Foundation;
using MessageUI;
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
		public string BodyHtml { get; set; }

		public EmailCommand (string name, string address, AsyncAction action = null)
			: base (name, action)
		{
			Address = address;
			Subject = "";
			BodyHtml = "";
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
			c.SetMessageBody (BodyHtml, true);

			await fromVC.PresentViewControllerAsync (c, true);
		}
	}

	public class EmailSupportCommand : EmailCommand
	{
		public EmailSupportCommand (string name, string address, AsyncAction action = null)
			: base (name, address, action)
		{
			var mainBundle = NSBundle.MainBundle;

			var dev = UIDevice.CurrentDevice;
			var scr = UIScreen.MainScreen;

			var appName = mainBundle.ObjectForInfoDictionary ("CFBundleDisplayName");
			var version = mainBundle.ObjectForInfoDictionary ("CFBundleVersion");

			Subject = appName + " Feedback (" + dev.SystemName + ")";

			var sb = new System.Text.StringBuilder();

			sb.AppendFormat ("<br/><br/><ul>");
			sb.AppendFormat ("<li>Software: <b>{0} {1}</b></li>", appName, version);
			sb.AppendFormat ("<li>Model: <b>{0}</b></li>", dev.Model);
			sb.AppendFormat ("<li>Screen: <b>{0}x{1} @{2}x</b></li>", scr.Bounds.Width, scr.Bounds.Height, scr.Scale);
			sb.AppendFormat ("<li>System: <b>{0} {1}</b></li>", dev.SystemName, dev.SystemVersion);
			sb.AppendFormat ("<li>Culture: <b>{0}</b></li>", System.Globalization.CultureInfo.CurrentCulture.EnglishName);
			sb.AppendFormat ("</ul>");

			BodyHtml = sb.ToString ();
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

			new UIAlertView ("Copied", Text ?? "", (IUIAlertViewDelegate)null, "OK").Show ();
		}
	}
}

