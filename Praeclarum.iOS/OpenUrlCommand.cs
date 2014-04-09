using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Praeclarum
{
	public class OpenUrlCommand : Command
	{
		public string Url { get; set; }

		public OpenUrlCommand (string name, string url, Action action)
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
}

