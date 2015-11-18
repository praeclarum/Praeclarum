using System;

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
			DocumentAppDelegate.Shared.Settings.DarkMode = !DocumentAppDelegate.Shared.Settings.DarkMode;
			DocumentAppDelegate.Shared.UpdateTheme ();
			SetNeedsFormat ();
			return false;
		}
	}
}

