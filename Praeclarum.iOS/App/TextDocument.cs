using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Praeclarum.UI;
using Praeclarum.Graphics;

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

		public TextDocument (NSUrl url)
			: base (url)
		{
			LocalFilePath = url.AbsoluteString;
		}

		public TextDocument (IntPtr handle)
			: base (handle)
		{
			LocalFilePath = "";
			try {
				if (this.FileUrl is NSUrl url) {
					LocalFilePath = url.AbsoluteString;
				}
			}
			catch (Exception ex) {
				Log.Error (ex);
			}
		}

		public bool IsOpen { get { return !DocumentState.HasFlag (UIDocumentState.Closed); } }

		public event EventHandler<SavingEventArgs> Saving = delegate {};
		public event EventHandler Loading = delegate {};

		string textData = "";
		public virtual string TextData {
			get { return textData; }
			set { textData = value ?? ""; }
		}

		public void UpdateChangeCount (DocumentChangeKind changeKind)
		{
			try {
				base.UpdateChangeCount (UIDocumentChangeKind.Done);				
			} catch {
				throw;
			}
		}

		public override NSObject ContentsForType (string typeName, out NSError outError)
		{
			try {
				outError = null;

				var text = TextData;

				var data = NSData.FromString (text, NSStringEncoding.UTF8);

				Debug.WriteLine ("SAVE " + LocalFilePath);

				Saving (this, new SavingEventArgs {
					TextData = text
				});

				return data;

			} catch (Exception ex) {
				Log.Error (ex);
				outError = new NSError (new NSString ("Praeclarum"), 334);
				return new NSData ();
			}
		}

		public override bool LoadFromContents (NSObject contents, string typeName, out NSError outError)
		{
			try {				
				outError = null;
				var data = contents as NSData;
				if (data != null) {
					TextData = data.ToString (NSStringEncoding.UTF8);
				} else {
					TextData = "";
				}

				Loading (this, EventArgs.Empty);

				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				outError = new NSError (new NSString ("Praeclarum"), 335);
				return false;
			}
		}

		public override NSDictionary GetFileAttributesToWrite (NSUrl forUrl, UIDocumentSaveOperation saveOperation, out NSError outError)
		{
			try {
				var imageScale = (nfloat)1.0;
				var size = new SizeF (1024, 1024);

				UIGraphics.BeginImageContextWithOptions (new CoreGraphics.CGSize (size.Width, size.Height), true, imageScale);
				var c = UIGraphics.GetCurrentContext ();
				var g = new Graphics.CoreGraphicsGraphics (c, true);

				DocumentAppDelegate.Shared.App.DrawThumbnail (this, g, size, DocumentAppDelegate.Shared.Theme, readOnlyDoc: true);

				var image = UIGraphics.GetImageFromCurrentImageContext ();
				UIGraphics.EndImageContext ();

				outError = null;
				var images = NSDictionary.FromObjectsAndKeys (
					new NSObject[] { image },
					new NSObject[] { new NSString ("NSThumbnail1024x1024SizeKey") });
				return NSDictionary.FromObjectsAndKeys (
					new NSObject[] { NSNumber.FromBoolean (true), images },
					new NSObject[] { NSUrl.HasHiddenExtensionKey, NSUrl.ThumbnailDictionaryKey });
			}
			catch (Exception ex) {
				Log.Error (ex);
				return base.GetFileAttributesToWrite (forUrl, saveOperation, out outError);
			}
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
			if (DocumentState != UIDocumentState.Closed) {
				var ok = await CloseAsync ();
				if (!ok)
					throw new Exception ("UIDocument.CloseAsync failed");
			}
		}

		#endregion

		public virtual Task<NSObject[]> GetActivityItemsAsync (UIViewController fromController)
		{
			var str = new NSAttributedString (TextData);
			return Task.FromResult (new NSObject[] {
				str,
				new UISimpleTextPrintFormatter (str),
			});
		}

		public virtual Task<UIActivity[]> GetActivitiesAsync (UIViewController fromController)
		{
			return Task.FromResult (new UIActivity[0]);
		}
	}

	public class TextDocumentHistory
	{
		List<TextDocumentRevision> revisions = new List<TextDocumentRevision> ();
		int revisionIndex = 0;

		public TextDocumentHistory ()
		{
			SaveInitialRevision ("Initial", "");
		}

		public void SaveInitialRevision (string title, string textData)
		{
			revisions.Clear ();
			revisionIndex = -1;
			SaveRevision (title, textData);
		}

		public void SaveRevision (string title, string textData)
		{
			Console.WriteLine ("SAVE REV " + title);
			var r = new TextDocumentRevision {
				ModifiedTimeUtc = DateTime.UtcNow,
				Title = title,
				TextData = textData,
			};

			if (revisionIndex >= 0 && revisionIndex + 1 < revisions.Count) {
				revisions.RemoveRange (revisionIndex + 1, (revisions.Count - (revisionIndex + 1)));
			}

			revisions.Add (r);
			revisionIndex = revisions.Count - 1;
		}

		public bool CanUndo { get { return revisionIndex > 0; } }
		public bool CanRedo { get { return revisionIndex < revisions.Count - 1; } }

		public void Undo ()
		{
			if (!CanUndo)
				return;
			revisionIndex--;
		}

		public void Redo ()
		{
			if (!CanRedo)
				return;
			revisionIndex++;
		}

		public string TextData
		{
			get { return revisions [revisionIndex].TextData; }
		}
	}

	public class TextDocumentRevision
	{
		public string Title;
		public DateTime ModifiedTimeUtc;
		public string TextData;
	}

	public class SavingEventArgs : EventArgs
	{
		public string TextData;
	}
}

