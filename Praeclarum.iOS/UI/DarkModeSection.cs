using System;
using Foundation;

namespace Praeclarum.UI
{
	public class DarkModeSection : PFormSection
	{		
		public DarkModeSection ()
			: this (() => DocumentAppDelegate.Shared.Settings.DarkMode, () => {
				var appdel = DocumentAppDelegate.Shared;
				appdel.Settings.DarkMode = !appdel.Settings.DarkMode;
				appdel.UpdateTheme ();
			})
		{
		}

		readonly Func<bool> isDarkFunc;
		readonly Action toggleAction;

		public DarkModeSection (Func<bool> isDark, Action toggle)
		{
			Items.Add ("Light Mode");
			Items.Add ("Dark Mode");

			this.isDarkFunc = isDark;
			this.toggleAction = toggle;
		}

		public override bool GetItemChecked (object item)
		{
			var isDark = isDarkFunc ();
			if ("Dark Mode" == item.ToString ()) {
				return isDark;
			}
			return !isDark;
		}

		public override bool SelectItem (object item)
		{
			NSTimer.CreateScheduledTimer (0.1, t => {
				toggleAction ();
				SetNeedsFormat ();
			});
			return false;
		}
	}
}

