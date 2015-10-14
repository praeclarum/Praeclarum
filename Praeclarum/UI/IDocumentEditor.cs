using System;
using Praeclarum.App;

namespace Praeclarum.UI
{
	public interface IDocumentEditor
	{
		DocumentReference DocumentReference { get; }

		IView EditorView { get; }

		void DidEnterBackground ();
		void WillEnterForeground ();
		void OnCreated ();

		void BindDocument ();
		void UnbindDocument ();

		void UnbindUI ();

		bool IsPreviewing { get; set; }
	}
}

