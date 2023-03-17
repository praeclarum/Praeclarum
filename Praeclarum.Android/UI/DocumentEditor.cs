using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public IDocument Document => null;

		public virtual bool IsPreviewing { get; set; }

		public virtual void OnCreated ()
		{
		}

		public virtual void DidEnterBackground ()
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

		public virtual Task SaveDocument ()
		{
			return Task.CompletedTask;
		}

		#endregion
	}
}

