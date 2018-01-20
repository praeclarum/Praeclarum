using System;
using Praeclarum.App;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public interface IDocumentEditor
	{
		DocumentReference DocumentReference { get; }

//		IView EditorView { get; }

		void DidEnterBackground ();
		void WillEnterForeground ();
		void OnCreated ();

		void BindDocument ();
		Task SaveDocument ();
		void UnbindDocument ();

		void UnbindUI ();

		bool IsPreviewing { get; set; }
	}
}

