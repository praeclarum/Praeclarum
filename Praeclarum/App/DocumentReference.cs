using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Praeclarum.IO;

namespace Praeclarum.App
{
	public delegate IDocument DocumentConstructor (string localFilePath);

	public class DocumentReference
	{
		public IFile File { get; private set; }

		public bool IsNew { get; set; }

		readonly DocumentConstructor dctor;

		public DocumentReference (IFile file, DocumentConstructor dctor, bool isNew)
		{
			if (file == null)
				throw new ArgumentNullException ("file");
			if (dctor == null)
				throw new ArgumentNullException ("dctor");
			File = file;
			IsNew = isNew;
			this.dctor = dctor;
		}

		LocalFileAccess local = null;
		string LocalFilePath { get { return local != null ? local.LocalPath : null; } }

		public string Name {
			get {
				return File.IsDirectory ?
					Path.GetFileName (File.Path) :
					Path.GetFileNameWithoutExtension (File.Path);
			}
		}

		public async Task<bool> Rename (string newName)
		{
			try {

				var dir = Path.GetDirectoryName (File.Path);
				var ext = Path.GetExtension (File.Path);

				var newPath = Path.Combine (dir, newName + ext);

				var r = await File.Move (newPath);
				return r;

			} catch (Exception ex) {
				Debug.WriteLine (ex);
				return false;
			}
		}

		public static Task<DocumentReference> New (string path, string defaultExt, IFileSystem fs, DocumentConstructor dctor, string contents = null)
		{
			var dir = Path.GetDirectoryName (path);
			var ext = Path.GetExtension (path);
			if (string.IsNullOrEmpty (ext))
				ext = defaultExt;
			var baseName = Path.GetFileNameWithoutExtension (path);

			return New (dir, baseName, ext, fs, dctor, contents);
		}

		public static async Task<DocumentReference> New (string directory, string baseName, string ext, IFileSystem fs, DocumentConstructor dctor, string contents = null)
		{
			if (ext [0] != '.') {
				ext = '.' + ext;
			}

			//
			// Get a name
			//
			var n = baseName + ext;
			var i = 1;
			var p = Path.Combine (directory, n);
			while (await fs.FileExists (p)) {
				i++;
				n = baseName + " " + i + ext;
				p = Path.Combine (directory, n);
			}

			return new DocumentReference (await fs.CreateFile (p, contents), dctor, isNew: true);
		}

		public async Task<DocumentReference> Duplicate (IFileSystem fs)
		{
			var baseName = Name;
			if (!baseName.EndsWith ("Copy", StringComparison.Ordinal)) {
				baseName = baseName + " Copy";
			}

			var directory = Path.GetDirectoryName (File.Path);
			LocalFileAccess local = null;
			var contents = "";
			try {
				local = await File.BeginLocalAccess ();
				var localPath = local.LocalPath;
				contents = System.IO.File.ReadAllText (localPath);
			} catch (Exception ex) {
				Debug.WriteLine (ex);
			}
			if (local != null)
				await local.End ();

			var ext = Path.GetExtension (File.Path);

			var dr = await New (directory, baseName, ext, fs, dctor, contents);

			dr.IsNew = false;

			return dr;
		}

		public IDocument Document { get; private set; }

		public bool IsOpen {
			get { return Document != null || local != null; }
		}

		public async Task<IDocument> Open ()
		{
			if (Document != null)
				throw new InvalidOperationException ("Cannot Open already opened document");

			if (local != null)
				throw new InvalidOperationException ("Cannot Open already locally accessed document");

			local = await File.BeginLocalAccess ();

			try {
				var doc = dctor (LocalFilePath);
				if (doc == null)
					throw new ApplicationException ("CreateDocument must return a document");

				if (!System.IO.File.Exists (LocalFilePath)) {
					Debug.WriteLine ("CREATE " + LocalFilePath);
					await doc.SaveAsync (LocalFilePath, DocumentSaveOperation.ForCreating);
				}
				else {
					Debug.WriteLine ("OPEN " + LocalFilePath);
					await doc.OpenAsync ();
				}
				Document = doc;

			} catch (Exception ex) {
				Document = null;
				Debug.WriteLine (ex);
			}

			return Document;
		}

		public async Task Close ()
		{
			if (Document == null)
				throw new InvalidOperationException ("Trying to Close an unopened document");

			await Document.CloseAsync ();
			Document = null;

			if (local != null) {
				await local.End ();
				local = null;
			}
		}

		public string ModifiedAgo {
			get {
				var t = ModifiedTime;
				var dt = DateTime.UtcNow - t;

				if (dt.TotalDays > 10000)
					dt = TimeSpan.Zero;

				if (dt.TotalDays >= 2.0) {
					return t.ToShortDateString ();
				} else if (dt.TotalDays >= 1.0) {
					return "yesterday";
				} else if (dt.TotalHours >= 1.0) {
					var n = (int)(dt.TotalHours + 0.5);
					if (n == 1) {
						return string.Format ("an hour ago");
					} else if (n == 24) {
						return string.Format ("yesterday");
					} else {
						return string.Format ("{0:0} hours ago", n);
					}
				} else if (dt.TotalMinutes >= 2.0) {
					return string.Format ("{0:0} mins ago", dt.TotalMinutes);
				} else if (dt.TotalMinutes >= 0.5) {
					return "a minute ago";
				} else {
					return "just now";
				}
			}
		}

		public DateTime ModifiedTime {
			get {
				try {
					return File.ModifiedTime;
				} catch (Exception) {
					return DateTime.UtcNow;
				}
			}
		}

		public override string ToString ()
		{
			return Name;
		}
	}
}

