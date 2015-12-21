using System;
using Foundation;

namespace Praeclarum.UI
{
	public class StorageSection : PFormSection
	{
		public StorageSection ()
			: base (new Command ("Storage"))
		{
			Title = "Storage";
//			Hint = "Select where to save documents.";
		}

		public override bool GetItemNavigates (object item)
		{
			return true;
		}

		public override bool SelectItem (object item)
		{
			var f = new StorageForm ();
			if (this.Form.NavigationController != null) {
				this.Form.NavigationController.PushViewController (f, true);
			}
			return false;
		}

		public override string GetItemTitle (object item)
		{
			var isp = DocumentAppDelegate.Shared.FileSystem;
			return isp != null ?
				isp.Description :
				"Storage";
		}
	}
}

