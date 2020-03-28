using System;
using Foundation;

namespace Praeclarum.UI
{
	public class DarkModeSection : PFormSection
	{		
		public DarkModeSection ()
			: this (() => DocumentAppDelegate.Shared.Settings.DarkMode, v => {
				var appdel = DocumentAppDelegate.Shared;
				appdel.Settings.DarkMode = v;
				appdel.UpdateTheme ();
			})
		{
		}

		readonly Func<bool?> isDarkFunc;
		readonly Action<bool?> toggleAction;

		readonly string autoTitle = "Automatic".Localize ();
		readonly string darkTitle = "Dark Mode".Localize ();
		readonly string lightTitle = "Light Mode".Localize ();

		public DarkModeSection (Func<bool?> isDark, Action<bool?> toggle)
		{
			Title = "Theme".Localize ();

			Items.Add ("Automatic".Localize ());
			Items.Add ("Dark Mode".Localize ());
			Items.Add ("Light Mode".Localize ());

			this.isDarkFunc = isDark;
			this.toggleAction = toggle;
		}

		public override bool GetItemChecked (object item)
		{
			var isDark = isDarkFunc ();
			if ("Dark Mode".Localize () == item.ToString ()) {
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

