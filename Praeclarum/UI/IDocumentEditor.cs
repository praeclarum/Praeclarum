using System;
using Praeclarum.App;

namespace Praeclarum.UI
{
	public interface IDocumentEditor
	{
		DocumentReference DocumentReference { get; }

		IView EditorView { get; }

		void WillEnterForeground ();
		void Created ();

		void BindDocument ();
		void UnbindDocument ();

		void UnbindUI ();
	}
}

