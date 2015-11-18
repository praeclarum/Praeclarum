using System;
using System.Threading.Tasks;
using UIKit;
using Foundation;
using Praeclarum.IO;
using System.IO;
using System.Globalization;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;
using DropBoxSync.iOS;

namespace Praeclarum.UI
{
	[Register ("DocumentListAppDelegate")]
	public class DocumentListAppDelegate : DocumentAppDelegate
	{
		UISplitViewController split;

		protected override void SetRootViewController ()
		{
			if (IsPhone) {

				window.RootViewController = docListNav;

			} else {

				var blankVC = new BlankVC ();
				blankVC.View.BackgroundColor = UIColor.White;

				detailNav = new UINavigationController (blankVC);
				detailNav.NavigationBar.BarStyle = Theme.NavigationBarStyle;
				detailNav.ToolbarHidden = false;
				Theme.Apply (detailNav.Toolbar);

				split = new UISplitViewController {
					PresentsWithGesture = false,
					ViewControllers = new UIViewController[] {
						docListNav,
						detailNav,
					},
					Delegate = new SplitDelegate (),
				};

				window.RootViewController = split;
			}
		}

		protected override void ShowEditor (int docIndex, bool advance, bool animated, UIViewController newEditorVC)
		{
			//
			// Control Animation
			//
			var transition = advance ?
			                 UIViewAnimationTransition.CurlUp :
			                 UIViewAnimationTransition.CurlDown;
			var useTransition = true;


			//
			// Change the UI
			//
			UINavigationController nc;
			UIViewController[] vcs;
			if (IsPhone) {
				//				Debug.WriteLine ("SHOWING EDITOR");
				nc = docListNav;
				vcs = nc.ViewControllers;
				var oldEditor = CurrentDocumentEditor;
				useTransition = useTransition && (oldEditor != null);
				var nvcs = new List<UIViewController> (vcs.OfType<DocumentsViewController> ());
				nvcs.Add (newEditorVC);
				vcs = nvcs.ToArray ();
			}
			else {
				//
				// Close the master list
				//
				var p = ((SplitDelegate)split.Delegate).Popover;
				if (p != null) {
					p.Dismiss (animated);
				}
				//
				// Set the button
				//
				var oldC = detailNav.TopViewController;
				var left = oldC.NavigationItem.LeftBarButtonItem;
				oldC.NavigationItem.LeftBarButtonItem = null;
				newEditorVC.NavigationItem.LeftBarButtonItem = left;
				nc = detailNav;
				vcs = new UIViewController[] {
					newEditorVC,
				};
			}

			//
			// Set View Controllers
			// Throttle the animations
			//
			var now = DateTime.UtcNow;
			if (animated && (now - lastOpenTime).TotalSeconds > 1) {
				if (useTransition) {
					UIView.Animate (0.5, () =>  {
						try {
							UIView.SetAnimationTransition (transition, nc.View, true);
							nc.SetViewControllers (vcs, false);
						} catch (Exception ex) {
							Log.Error (ex);							
						}
					});
				}
				else {
					nc.SetViewControllers (vcs, true);
				}
			}
			else {
				nc.SetViewControllers (vcs, false);
			}
		}

		class BlankVC : UIViewController
		{
			bool showed = false;
			public BlankVC ()
			{
				Title = DocumentListAppDelegate.Shared.App.Name;
			}
			public override void ViewDidAppear (bool animated)
			{
				base.ViewDidAppear (animated);

				try {
					if (!showed) {
						showed = true;
						if (InterfaceOrientation == UIInterfaceOrientation.Portrait) {
							var left = NavigationItem.LeftBarButtonItem;
							if (left != null) {
								left.Target.PerformSelector (left.Action, NavigationItem, 0.1);
							}
						}
					}

				} catch (Exception ex) {
					Debug.WriteLine (ex);

				}
			}
		}

		class SplitDelegate : UISplitViewControllerDelegate
		{
			public UIPopoverController Popover { get; private set; }
			public UIBarButtonItem Button { get; private set; }

			public override bool ShouldHideViewController (UISplitViewController svc, UIViewController viewController, UIInterfaceOrientation inOrientation)
			{
				return true;
			}

			public override void WillHideViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem barButtonItem, UIPopoverController pc)
			{
				try {
					Popover = pc;
					var detailNav = (UINavigationController)svc.ViewControllers [1];

					var newItem = new UIBarButtonItem (UIImage.FromBundle ("Hamburger.png"), UIBarButtonItemStyle.Plain, barButtonItem.Target, barButtonItem.Action);

					detailNav.TopViewController.NavigationItem.LeftBarButtonItem = newItem;

					Button = newItem;

				} catch (Exception ex) {
					Log.Error (ex);					
				}
			}

			public override void WillShowViewController (UISplitViewController svc, UIViewController aViewController, UIBarButtonItem button)
			{
				try {
					Popover = null;
					var detailNav = (UINavigationController)svc.ViewControllers [1];
					detailNav.TopViewController.NavigationItem.LeftBarButtonItem = null;
				} catch (Exception ex) {
					Log.Error (ex);
				}
			}
		}
	}
}

