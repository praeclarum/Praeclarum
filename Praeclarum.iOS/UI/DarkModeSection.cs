using System;
using Foundation;

namespace Praeclarum.UI
{
	public class DarkModeSection : PFormSection
	{
		public DarkModeSection ()
		{
			Items.Add ("Dark Mode");
		}

		public override bool GetItemChecked (object item)
		{
			return DocumentAppDelegate.Shared.Settings.DarkMode;
		}

		public override bool SelectItem (object item)
		{
			var appdel = DocumentAppDelegate.Shared;
			NSTimer.CreateScheduledTimer (0.1, t => {
				appdel.Settings.DarkMode = !DocumentAppDelegate.Shared.Settings.DarkMode;
				DocumentAppDelegate.Shared.UpdateTheme ();
				SetNeedsFormat ();
			});
			return false;
		}
	}
}

