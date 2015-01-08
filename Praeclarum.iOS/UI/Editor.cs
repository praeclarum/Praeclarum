using System;
using UIKit;
using Foundation;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public class Editor : UITextView, ITextEditor
	{
		public Editor (CoreGraphics.CGRect frame)
			: base (frame)
		{
		}

		#region IView implementation

		Color IView.BackgroundColor {
			get { return base.BackgroundColor.GetColor (); }
			set { base.BackgroundColor = value.GetUIColor (); }
		}

		RectangleF IView.Bounds {
			get {
				return Bounds.ToRectangleF ();
			}
		}

		#endregion

		#region ITextEditor implementation

		public void Modify (Action action)
		{
			BeginInvokeOnMainThread (() => action ());
		}

		void ITextEditor.ReplaceText (StringRange range, string text)
		{
			var b = this.GetPosition (BeginningOfDocument, range.Location);
			var e = this.GetPosition (BeginningOfDocument, range.Location + range.Length);
			var r = this.GetTextRange (b, e);
			ReplaceText (r, text);
		}

		StringRange ITextEditor.SelectedRange {
			get {
				var r = SelectedRange;
				return new StringRange ((int)r.Location, (int)r.Length);
			}
			set {
				SelectedRange = new NSRange (value.Location, value.Length);
			}
		}

		#endregion
	}
}

