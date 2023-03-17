﻿using System;
using System.Threading.Tasks;

namespace Praeclarum.App
{
	public class TextDocument : ITextDocument
	{
		string path;

		public TextDocument (string path)
		{
			this.path = path;
		}

		#region IDocument implementation

		bool isOpen = false;

		public async Task OpenAsync ()
		{
			TextData = System.IO.File.ReadAllText (path);
			isOpen = true;
		}

		public async Task SaveAsync (string path, DocumentSaveOperation operation)
		{
			this.path = path;
			System.IO.File.WriteAllText (path, TextData);
		}

		public async Task CloseAsync ()
		{
			if (!isOpen)
				return;
			isOpen = false;
		}

		public bool IsOpen {
			get {
				return isOpen;
			}
		}

		#endregion

		#region ITextDocument implementation

		public void UpdateChangeCount (DocumentChangeKind changeKind)
		{
		}

		string textData = "";
		private bool _disposedValue;

		public virtual string TextData {
			get {
				return textData;
			}
			set {
				textData = value ?? "";
			}
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
				}
				_disposedValue = true;
			}
		}

		public void Dispose ()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}

		#endregion
	}
}

