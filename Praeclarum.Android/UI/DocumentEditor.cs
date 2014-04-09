using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Praeclarum.App;

namespace Praeclarum.UI
{
	public class DocumentEditor : Fragment, IDocumentEditor
	{
		public DocumentReference DocumentReference { get; private set; }

		public DocumentEditor (DocumentReference docRef)
		{
			DocumentReference = docRef;
		}

		#region IDocumentEditor implementation

		IView editorView = null;

		public virtual IView EditorView
		{
			get { return editorView; }
			set {
				View.AddTouchables (new List<global::Android.Views.View> {
					(global::Android.Views.View)editorView,
				});
			}
		}

		public virtual void Created ()
		{
		}

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
}

