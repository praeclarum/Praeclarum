using System;
using System.Collections.Generic;
using Praeclarum.UI;
using Praeclarum.App;
using System.Linq;
using System.Collections.ObjectModel;

namespace Praeclarum.UI
{
	public enum DocumentsSort
	{
		Date,
		Name
	}

	public class DocumentsViewItem
	{
		public DocumentReference Reference { get; set; }
		public List<DocumentReference> SubReferences { get; set; }

		public DateTime ModifiedTime {
			get {
				if (SubReferences == null || SubReferences.Count == 0)
					return Reference.File.ModifiedTime;
				return SubReferences.Max (x => x.File.ModifiedTime);
			}
		}

		public DocumentsViewItem (DocumentReference reference)
		{
			Reference = reference;
		}
	}

	public interface IDocumentsView
	{
		bool IsSyncing { get; set; }
		List<DocumentsViewItem> Items { get; set; }
		DocumentsViewItem GetItemAtPoint (Praeclarum.Graphics.PointF p);
		void ReloadData ();

		DocumentsSort Sort { get; set; }
		event EventHandler SortChanged;

		void InsertItems (int[] docIndices);
		void DeleteItems (int[] docIndices, bool animated);
		void UpdateItem (int docIndex);
		void ShowItem (int docIndex, bool animated);
		void RefreshListTimes ();
		void SetOpenedDocument (int docIndex, bool animated);

		event Action<DocumentReference, object> RenameRequested;

		void SetEditing (bool editing, bool animated);

		void SetSelecting (bool selecting, bool animated);

		ObservableCollection<string> SelectedDocuments { get; }
	}
}
