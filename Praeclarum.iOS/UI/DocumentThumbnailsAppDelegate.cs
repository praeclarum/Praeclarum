using System;
using Foundation;
using UIKit;
using System.Linq;
using System.Collections.Generic;

namespace Praeclarum.UI
{
	[Register ("DocumentThumbnailsAppDelegate")]
	public class DocumentThumbnailsAppDelegate : DocumentAppDelegate
	{
		protected override void SetRootViewController ()
		{
			window.RootViewController = (UIViewController)docListNav ?? docBrowser;
		}

		protected override void ShowEditor (int docIndex, bool advance, bool animated, UIViewController newEditorVC)
		{
				//				Debug.WriteLine ("SHOWING EDITOR");
			var nc = docListNav;
			var vcs = nc.ViewControllers;
			var oldEditor = CurrentDocumentEditor;
			var nvcs = new List<UIViewController> (vcs.OfType<DocumentsViewController> ());
			nvcs.Add (newEditorVC);
			vcs = nvcs.ToArray ();

			nc.SetViewControllers (vcs, false);
		}

		protected override DocumentsViewController CreateDirectoryViewController (string path)
		{
			return new DocumentsViewController (path, DocumentsViewMode.Thumbnails);
		}

	}
}

