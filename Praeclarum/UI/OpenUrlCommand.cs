using System;
using Foundation;
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
			if (string.IsNullOrWhiteSpace(Url))
				return;
#if __IOS__
			UIKit.UIApplication.SharedApplication.OpenUrl (NSUrl.FromString (Url));
#elif __MACOS__
			AppKit.NSWorkspace.SharedWorkspace.OpenUrl (NSUrl.FromString (Url));
#else
			throw new NotSupportedException();
#endif
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

#if __IOS__
			var fromVC = UIKit.UIApplication.SharedApplication.Windows [0].RootViewController;
			var c = new MessageUI.MFMailComposeViewController ();
			c.Finished += (sender, e) => c.DismissViewController (true, null);
			c.SetToRecipients (new [] { Address });
			c.SetSubject (Subject);
			c.SetMessageBody (BodyHtml, true);
			await fromVC.PresentViewControllerAsync (c, true);
#else
			throw new NotSupportedException();
#endif
		}
	}

	public class EmailSupportCommand : EmailCommand
	{
		public EmailSupportCommand (string name, string address, AsyncAction action = null)
			: base (name, address, action)
		{
			var mainBundle = NSBundle.MainBundle;

#if __IOS__
			var dev = UIKit.UIDevice.CurrentDevice;
			var devSystemName = dev.SystemName;
#elif __MACOS__
			var devSystemName = "Mac";
#endif

			var appName = mainBundle.ObjectForInfoDictionary ("CFBundleDisplayName")?.ToString ();
			if (string.IsNullOrEmpty (appName)) {
				appName = mainBundle.ObjectForInfoDictionary ("CFBundleName")?.ToString ();
			}
			var version = mainBundle.ObjectForInfoDictionary ("CFBundleVersion");

			Subject = appName + " Feedback (" + devSystemName + ")";

			var sb = new System.Text.StringBuilder();

			sb.AppendFormat ("<br/><br/><ul>");
			sb.AppendFormat ("<li>Software: <b>{0} {1}</b></li>", appName, version);
#if __IOS__
			sb.AppendFormat ("<li>Model: <b>{0}</b></li>", dev.Model);
			var scr = UIKit.UIScreen.MainScreen;
			sb.AppendFormat ("<li>Screen: <b>{0}x{1} @{2}x</b></li>", scr.Bounds.Width, scr.Bounds.Height, scr.Scale);
			sb.AppendFormat ("<li>System: <b>{0} {1}</b></li>", devSystemName, dev.SystemVersion);
#endif
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
#if __IOS__
			UIKit.UIPasteboard.General.String = Text ?? "";
			new UIKit.UIAlertView ("Copied", Text ?? "", (UIKit.IUIAlertViewDelegate)null, "OK").Show ();
#else
			throw new NotSupportedException();
#endif
		}
	}
}

