﻿using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Threading.Tasks;

namespace Praeclarum.App
{
	public class TextDocument : UIDocument, ITextDocument
	{
		public string LocalFilePath { get; private set; }

		public TextDocument (string localFilePath)
			: base (NSUrl.FromFilename (localFilePath))
		{
			LocalFilePath = localFilePath;
		}

		public bool IsOpen { get { return DocumentState != UIDocumentState.Closed; } }

		public event EventHandler<SavingEventArgs> Saving = delegate {};
		public event EventHandler Loading = delegate {};

		string textData = "";
		public virtual string TextData {
			get { return textData; }
			set { textData = value ?? ""; }
		}

		public void UpdateChangeCount (DocumentChangeKind changeKind)
		{
			base.UpdateChangeCount (UIDocumentChangeKind.Done);
		}

		public override NSObject ContentsForType (string typeName, out NSError outError)
		{
			outError = null;

			var text = TextData;

			var data = NSData.FromString (text, NSStringEncoding.UTF8);

			Saving (this, new SavingEventArgs {
				TextData = text
			});

			return data;
		}

		public override bool LoadFromContents (NSObject contents, string typeName, out NSError outError)
		{
			outError = null;
			var data = contents as NSData;
			if (data != null) {
				TextData = data.ToString (NSStringEncoding.UTF8);
			} else {
				TextData = "";
			}

			Loading (this, EventArgs.Empty);

			return true;
		}

		#region IDocument implementation

		async Task IDocument.OpenAsync ()
		{
			var ok = await OpenAsync ();
//			Console.WriteLine ("OpenAsync? {0}", ok);
			if (!ok)
				throw new Exception ("UIDocument.OpenAsync failed");
		}

		async Task IDocument.SaveAsync (string path, DocumentSaveOperation operation)
		{
			var ok = await SaveAsync (
				NSUrl.FromFilename (path), 
				operation == DocumentSaveOperation.ForCreating ?
				UIDocumentSaveOperation.ForCreating :
				UIDocumentSaveOperation.ForOverwriting);
			if (!ok)
				throw new Exception ("UIDocument.SaveAsync failed");
		}

		async Task IDocument.CloseAsync ()
		{
			var ok = await CloseAsync ();
			if (!ok)
				throw new Exception ("UIDocument.CloseAsync failed");
		}

		#endregion

		static readonly bool ios7 = UIDevice.CurrentDevice.CheckSystemVersion (7, 0);

		public virtual NSObject[] GetActivities ()
		{
			var str = new NSAttributedString (TextData);
			return new NSObject[] {
				str,
				ios7 ? new UISimpleTextPrintFormatter (str) : new UISimpleTextPrintFormatter (TextData),
			};
		}
	}

	public class SavingEventArgs : EventArgs
	{
		public string TextData;
	}
}
