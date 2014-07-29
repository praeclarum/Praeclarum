using System;
using MonoTouch.UIKit;
using Praeclarum.App;

namespace Praeclarum.UI
{
	public class DocumentEditor : UIViewController, IDocumentEditor
	{
		protected DocumentReference docRef;

		public DocumentReference DocumentReference { get { return docRef; } }

		public DocumentEditor (DocumentReference docRef)
		{
			this.docRef = docRef;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.White;

			OnCreated ();
		}

		IView editorView = null;

		public IView EditorView {
			get {
				return editorView;
			}
			set {
				if (editorView == value)
					return;

				editorView = value;

				var v = editorView as UIView;
				if (v != null) {
					v.Frame = View.Bounds;
					v.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
					View.AddSubview (v);
				}
			}
		}

		public virtual void OnCreated ()
		{
		}

		public override async void DidMoveToParentViewController (UIViewController parent)
		{
			if (parent == null) {
				DocumentListAppDelegate.Shared.OpenedDocIndex = -1;
				await DocumentListAppDelegate.Shared.CloseDocumentEditor (this, unbindUI: true, deleteThumbnail: true, reloadThumbnail: true);
			}
		}

		#region IDocumentEditor implementation

		public virtual void WillEnterForeground ()
		{
		}

		public virtual void BindDocument ()
		{

		}

		public virtual void UnbindDocument ()
		{

		}

		public virtual void UnbindUI ()
		{

		}

		#endregion
	}

	public class DocumentViewerAndEditor : DocumentEditor
	{
		public DocumentViewerAndEditor (DocumentReference docRef)
			: base (docRef)
		{
			NavigationItem.RightBarButtonItem = EditButtonItem;
		}
	}
}

