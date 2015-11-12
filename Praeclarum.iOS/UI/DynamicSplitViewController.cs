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

		public readonly nfloat SplitterWidth = 10.0f;

		public DynamicSplitViewController ()
			: this (new UITableViewController (UITableViewStyle.Grouped), new UITableViewController (UITableViewStyle.Plain))
		{
			
		}
		public DynamicSplitViewController (UIViewController first, UIViewController second)
		{
			this.First = first;
			this.Second = second;
			containerView = new ContainerView (this);
			Splitter = new SplitterView ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			Console.WriteLine ("VDL");

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

		public class SplitterView : UIView
		{
			bool touching = false;
			public bool Touching {
				get { return touching; }
				set { touching = value; SetNeedsDisplay (); }
			}
			public SplitterView ()
			{
				BackgroundColor = UIColor.Red;
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
				c.FillRect (b);

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
			readonly DynamicSplitViewController c;
			public ContainerView (DynamicSplitViewController c)
			{
				this.c = c;
				BackgroundColor = UIColor.DarkGray;
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();

				var r = c.Ratio;
				var b = Bounds;
				var w = (double)b.Width;

				var splitW = c.SplitterWidth;
				// w = (r)*a*w + (r-1)*a*w + sw
				// a => -sw/w + 1
				var a = -splitW/w + 1;
				try {
					var fw = a * r * w;
					var sw = a * (1-r) * w;
					var h = b.Height;
					c.First.View.Frame = new CGRect(0, 0, (nfloat)fw, h);
					c.Second.View.Frame = new CGRect((nfloat)(fw + splitW), 0, (nfloat)sw, h);
					c.Splitter.Frame = new CGRect((nfloat)(fw), 0, (nfloat)splitW, h);
//					Console.WriteLine ("SPLIT CONTAINER LAYOUT {0} {1} {2}", fw, sw, w);					
				} catch (Exception ex) {
					Console.WriteLine ("ERROR {0}", ex);
				}
			}
		}
	}
}

