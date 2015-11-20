using System;
using Foundation;

namespace Praeclarum.UI
{
	public class DarkModeSection : PFormSection
	{		
		public DarkModeSection ()
		{
			Items.Add ("Light Mode");
			Items.Add ("Dark Mode");
		}

		public override bool GetItemChecked (object item)
		{
			var isDark = DocumentAppDelegate.Shared.Settings.DarkMode;
			if ("Dark Mode" == item.ToString ()) {
				return isDark;
			}
			return !isDark;
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

