using System;
using Foundation;

namespace Praeclarum.UI
{
	public class StorageSection : PFormSection
	{
		public StorageSection ()
			: base (new Command ("Storage"))
		{
			Title = "Storage".Localize ();
//			Hint = "Select where to save documents.";
		}

		public override bool GetItemNavigates (object item)
		{
			return true;
		}

		public override bool SelectItem (object item)
		{
			var f = new StorageForm ();
			f.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Done, (s, e) => {
				if (f != null && f.PresentingViewController != null) {
					f.DismissViewController (true, null);
				}
			});
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
				"Storage".Localize ();
		}
	}
}

