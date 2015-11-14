using System;
using UIKit;
using System.Linq;
using CoreGraphics;

namespace Praeclarum.UI
{
	public class DynamicSplitViewController : UIViewController
	{
		public readonly UIViewController First;
		public readonly UIViewController Second;

		public readonly SplitterView Splitter;

		readonly ContainerView containerView;

		// Magic number to make second view 320pts
		public double Ratio = 0.76401179941002;

		public readonly nfloat SplitterWidth = 44.0f;
		public readonly nfloat SplitterVisibleWidth = 10.0f;

		bool secondVisible = true;

		public bool SecondVisible {
			get { return secondVisible; }
			set {
				if (secondVisible != value) {
					secondVisible = value;
					ShowOrHideSecond (false);
				}
			}
		}

		public void SetSecondVisible (bool value, bool animated)
		{
			if (secondVisible != value) {
				secondVisible = value;
				ShowOrHideSecond (animated);
			}
		}

		UIBarButtonItem toggleButton;

		public DynamicSplitViewController ()
			: this (new UITableViewController (UITableViewStyle.Plain), new UITableViewController (UITableViewStyle.Grouped))
		{
			
		}

		public DynamicSplitViewController (UIViewController first, UIViewController second)
		{
			this.First = first;
			this.Second = second;
			containerView = new ContainerView (this);
			Splitter = new SplitterView ();

			containerView.BackgroundColor = first.View.BackgroundColor;
		}

		protected virtual UIBarButtonItem CreateToggleButton ()
		{
			return new UIBarButtonItem ("Toggle", UIBarButtonItemStyle.Plain, HandleSplitToggleButton);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Console.WriteLine ("DYNAMIC SPLIT VIEW DID LOAD");

			toggleButton = CreateToggleButton ();
			NavigationItem.RightBarButtonItems =
				(NavigationItem.RightBarButtonItems ?? new UIBarButtonItem[0]{}).
				Concat (new [] { toggleButton }).
				ToArray ();

			containerView.Frame = View.Bounds;
			containerView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

			containerView.AddSubview (First.View);
			containerView.AddSubview (Second.View);
			containerView.AddSubview (Splitter);
			View.AddSubview (containerView);
			containerView.SetNeedsLayout ();

			AddChildViewController (First);
			AddChildViewController (Second);

			Splitter.AddGestureRecognizer (new UIPanGestureRecognizer (HandlePanSplitter));
		}

		double panStartRatio = 0.5;

		void HandlePanSplitter (UIPanGestureRecognizer pan)
		{
			try {
				
				var t = pan.TranslationInView (View);

				if (pan.State == UIGestureRecognizerState.Began) {
					panStartRatio = Ratio;
					Splitter.Touching = true;
				}
				else if (pan.State == UIGestureRecognizerState.Changed) {
					var w = View.Bounds.Width;
					var rr = panStartRatio + t.X / w;
					Ratio = rr;
					containerView.SetNeedsLayout ();
				}
				else {
					Splitter.Touching = false;
				}

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		protected async void HandleSplitToggleButton (object sender, EventArgs e)
		{
//			Console.WriteLine ("TOGGLE");
			var hclass = TraitCollection.HorizontalSizeClass;
//			hclass = UIUserInterfaceSizeClass.Compact;

			if (hclass == UIUserInterfaceSizeClass.Regular) {
				secondVisible = !secondVisible;
				ShowOrHideSecond (true);
			} else {
				HideSecond (false);
				var dismissButton = new UIBarButtonItem (UIBarButtonSystemItem.Done, (ss, ee) => this.DismissViewController (true, null));
				Second.NavigationItem.RightBarButtonItem = dismissButton;
				Second.View.Alpha = 1.0f;
				var nav = new UINavigationController (Second);
				nav.ModalPresentationStyle = UIModalPresentationStyle.Popover;
				nav.PopoverPresentationController.BarButtonItem = toggleButton;
				await PresentViewControllerAsync (nav, true);
			}
		}

		public override void TraitCollectionDidChange (UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange (previousTraitCollection);
			ShowOrHideSecond (false);
		}

		void ShowOrHideSecond (bool animated)
		{
			if (!this.IsViewLoaded)
				return;
			
			var hclass = TraitCollection.HorizontalSizeClass;
//			hclass = UIUserInterfaceSizeClass.Compact;

			if (hclass == UIUserInterfaceSizeClass.Regular && secondVisible) {
				ShowSecond (animated);
			} else {
				HideSecond (animated);
			}				
		}

		async void HideSecond (bool animated)
		{
			if (!ChildViewControllers.Contains (Second)) {
				return;
			}
			Second.ViewWillDisappear (animated);
			containerView.OnlyFirst = true;
			containerView.SetNeedsLayout ();
			if (animated) {
				await UIView.AnimateAsync (0.333, () => {
					Second.View.Alpha = 0.0f;
					Splitter.Alpha = 0.0f;
					containerView.LayoutIfNeeded ();
				});
			} else {
				Second.View.Alpha = 0.0f;
				Splitter.Alpha = 0.0f;
			}
			Second.RemoveFromParentViewController ();
			Second.View.RemoveFromSuperview ();
			Splitter.RemoveFromSuperview ();
			containerView.SetNeedsLayout ();
		}

		async void ShowSecond (bool animated)
		{
			if (ChildViewControllers.Contains (Second)) {
				return;
			}
			if (PresentedViewController != null) {
				await DismissViewControllerAsync (animated);
			}
			Second.View.Alpha = animated ? 0.0f : 1.0f;
			Splitter.Alpha = animated ? 0.0f : 1.0f;
			Second.ViewWillAppear (animated);
			AddChildViewController (Second);
			containerView.AddSubview (Second.View);
			containerView.AddSubview (Splitter);
			containerView.OnlyFirst = false;
			containerView.SetNeedsLayout ();
			if (animated) {
				await UIView.AnimateAsync (0.333, () => {
					Second.View.Alpha = 1.0f;
					Splitter.Alpha = 1.0f;
					containerView.LayoutIfNeeded ();
				});
			}
		}

		public class SplitterView : UIView
		{
			bool touching = false;
			public bool Touching {
				get { return touching; }
				set { touching = value; SetNeedsDisplay (); }
			}
			public SplitterView ()
			{
				BackgroundColor = UIColor.Clear;
			}

			public override void Draw (CoreGraphics.CGRect rect)
			{
				base.Draw (rect);

				var c = UIGraphics.GetCurrentContext ();

				var b = Bounds;

				//
				// Background
				//
				var backColor =
					touching ?
					UIColor.FromRGB ((nfloat)229/2/255,(nfloat)229/2/255,(nfloat)238/2/255) :
					UIColor.FromRGB ((nfloat)229/255,(nfloat)229/255,(nfloat)238/255);

				backColor.SetFill ();
				var sw = (nfloat)10.0f;
				c.FillRect (new CGRect ((b.Width-sw)/2, 0, sw, b.Height));

				//
				// Draw the button
				//
				var buttonColor = UIColor.FromRGB ((nfloat)255/255,(nfloat)255/255,(nfloat)255/255);
				var bw = (nfloat)4.0f;
				var bh = (nfloat)44.0f;
				var l = (b.Width - bw) / 2;
				var t = (b.Height - bh) / 2;
				var bRect = new CGRect (l, t, bw, bh);
				var bp = UIBezierPath.FromRoundedRect (bRect, bw/2);
				buttonColor.SetFill ();
				bp.Fill ();
			}
		}

		class ContainerView : UIView
		{
			public bool OnlyFirst = false;
			readonly DynamicSplitViewController c;
			public ContainerView (DynamicSplitViewController c)
			{
				this.c = c;
				BackgroundColor = UIColor.Red;
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();

				var r = c.Ratio;
				var b = Bounds;
				var w = (double)b.Width;

//				Console.WriteLine ("LAYOUT");

				try {

					var isSplit = !OnlyFirst && Subviews.Length > 1;

					if (isSplit) {
						var splitW = c.SplitterVisibleWidth;
						// w = (r)*a*w + (r-1)*a*w + sw
						// a => -sw/w + 1
						var a = -splitW/w + 1;
						var fw = a * r * w;
						var sw = a * (1-r) * w;
						var h = b.Height;
						c.First.View.Frame = new CGRect(0, 0, (nfloat)fw, h);
						c.Second.View.Frame = new CGRect((nfloat)(fw + splitW), 0, (nfloat)sw, h);
						c.Splitter.Frame = new CGRect((nfloat)(fw - (c.SplitterWidth-splitW)/2), 0, (nfloat)c.SplitterWidth, h);
//					Console.WriteLine ("SPLIT CONTAINER LAYOUT {0} {1} {2}", fw, sw, w);					
					}
					else {
						c.First.View.Frame = b;
					}
				} catch (Exception ex) {
					Console.WriteLine ("ERROR {0}", ex);
				}
			}
		}
	}
}

