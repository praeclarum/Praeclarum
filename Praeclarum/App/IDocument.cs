using System;
using System.Threading.Tasks;

namespace Praeclarum.App
{
	public enum DocumentSaveOperation
	{
		ForCreating,
		ForOverwriting,
	}

	public enum DocumentChangeKind
	{
		Done,
	}

	public interface IDocument : IDisposable
	{
		bool IsOpen { get; }
		Task OpenAsync ();
		Task SaveAsync (string path, DocumentSaveOperation operation);
		Task CloseAsync ();
		void UpdateChangeCount (DocumentChangeKind changeKind);
	}

	public interface ITextDocument : IDocument
	{
		string TextData { get; set; }
	}
}

