using System;

namespace Praeclarum.UI
{
	public class AnalyticsSection : PFormSection
	{
		readonly Action disable;

		public AnalyticsSection (Action disable)
		{
			this.disable = disable;
			Hint = "In order to improve iCircuit, anonymous usage data can be collected and sent to the developer. " +
				"This data includes which elements you add, which properties you set (not including values), and what errors occur. " +
				"To opt out of this, tap the option to turn it off (unchecked).";
			
			Items.Add ("Enable Anonymous Analytics");
		}

		public override bool GetItemChecked (object item)
		{
			return !DocumentAppDelegate.Shared.Settings.DisableAnalytics;
		}

		public override bool SelectItem (object item)
		{
			var disabled = !DocumentAppDelegate.Shared.Settings.DisableAnalytics;
			DocumentAppDelegate.Shared.Settings.DisableAnalytics = disabled;

			// Wait till next launch to enable to make sure everything is inited
			if (disabled) disable ();

			SetNeedsFormat ();
			return false;
		}
	}
}
