using System;
using System.Collections.Generic;
using UIKit;
using Praeclarum.App;
using System.Threading.Tasks;
using Foundation;

namespace Praeclarum.UI
{
	public class DocumentEditor : UIViewController, IDocumentEditor
	{
		protected DocumentReference docRef;
		protected NSUrl docUrl;

		public DocumentReference DocumentReference { get { return docRef; } }

		public bool IsPreviewing { get; set; }

		public DocumentEditor (DocumentReference docRef)
		{
			this.docRef = docRef;
		}

		public DocumentEditor(NSUrl url)
		{
			this.docUrl = url;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try {
				View.BackgroundColor = UIColor.White;

				OnCreated ();

				viewLoaded = true;
				foreach (var a in this.viewLoadedActions) {
					try {
						a ();
					}
					catch (Exception aex) {
						Log.Error (aex);
					}
				}
				viewLoadedActions.Clear ();

			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public override async void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			try {
				if (DocumentReference != null && DocumentReference.IsOpen && IsPreviewing) {
					Console.WriteLine ("CLOSING PREVIEW DOCUMENT");
					UnbindDocument ();
					UnbindUI ();
					await DocumentReference.Close ();
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
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
			try {
				if (parent == null) {
					DocumentAppDelegate.Shared.OpenedDocIndex = -1;
					await DocumentAppDelegate.Shared.CloseDocumentEditor (this, unbindUI: true, deleteThumbnail: true, reloadThumbnail: true);
				}
			} catch (Exception ex) {
				Log.Error (ex);
			}
		}

		bool viewLoaded = false;

		List<Action> viewLoadedActions = new List<Action>();
		protected void WhenViewLoaded(Action action) {
			if (viewLoaded) {
				try {
					action ();
				}
				catch (Exception ex) {
					Log.Error (ex);
				}
			} else {
				viewLoadedActions.Add (action);
			}
		}

		#region IDocumentEditor implementation

		public virtual void DidEnterBackground ()
		{
		}

		public virtual void WillEnterForeground ()
		{
		}

		public virtual void BindDocument ()
		{

		}

		public virtual Task SaveDocument ()
		{
			return Task.FromResult<object> (null);
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

