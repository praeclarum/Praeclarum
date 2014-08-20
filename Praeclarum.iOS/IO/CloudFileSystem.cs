using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Diagnostics;


namespace Praeclarum.IO
{
	/// <summary>
	/// https://developer.apple.com/library/ios/documentation/General/Conceptual/iCloudDesignGuide/Chapters/iCloudFundametals.html#//apple_ref/doc/uid/TP40012094-CH6-SW18
	/// </summary>
	public class CloudFileSystemProvider : IFileSystemProvider
	{
		public bool CanAddFileSystem { get { return false; } }

		public Task ShowAddUI (object parent)
		{
			return Task.FromResult<object> (null);
		}

		public string Name { get { return "iCloud"; } }

		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			yield return new CloudFileSystem ();
		}

		static void CloudAccountAvailabilityChanged (object sender, NSNotificationEventArgs e)
		{
			Console.WriteLine ("UbiquityIdentityDidChange: {0}", CloudAvailable);
		}

		public CloudFileSystemProvider ()
		{
			//
			// Register for Availability Change Notifications
			//
			NSFileManager.Notifications.ObserveUbiquityIdentityDidChange (CloudAccountAvailabilityChanged);
		}

		public static bool CloudAvailable {
			get {
				if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
					var currentIdentityToken = NSFileManager.DefaultManager.UbiquityIdentityToken;
					return currentIdentityToken != null;
				}
				return true;
			}
		}

		public static async Task<bool> AskToUseCloud ()
		{
			var askCloudAlert = new UIAlertView (
				"iCloud",
				"Should documents be stored in iCloud and made available on all your devices?",
				null,
				"Local Only",
				"Use iCloud");

			var tcs = new TaskCompletionSource<bool> ();

			askCloudAlert.Clicked += (s, e) => {
				var useCloud = e.ButtonIndex == 1;
				tcs.SetResult (useCloud);
			};

			askCloudAlert.Show ();

			return await tcs.Task;
		}
	}

	public class CloudFileSystem : IFileSystem
	{
		NSUrl documentsUrl;

		NSMetadataQuery query;

		public int MaxDirectoryDepth { get { return 1; } }

		bool needsRefresh = true;

		public async Task<List<IFile>> ListFiles (string directory)
		{
			if (needsRefresh)
				await BeginQuery ();

			var q = from f in fileIndex.Values
					where Path.GetDirectoryName(f.Path) == directory
					select (IFile)f;

			return q.ToList ();
		}

		public Task<IFile> GetFile (string path)
		{
			var file = fileIndex.Values.First (f => f.Path == path);
			return Task.FromResult (file);
		}

		public Task<bool> FileExists (string path)
		{
			var e = fileIndex.Values.Any (x => x.Path == path);
			return Task.FromResult (e);
		}

		public event EventHandler FilesChanged;

		Dictionary<string, IFile> fileIndex = new Dictionary<string, IFile> ();

		public ICollection<string> FileExtensions { get; private set; }

		public CloudFileSystem ()
		{
			FileExtensions = new Collection<string> ();
			IsSyncing = false;
		}

		public string Id {
			get {
				return "iCloud";
			}
		}

		public string Description { get { return "iCloud"; } }
		public string ShortDescription { get { return Description; } }

		bool initialized = false;

		public async Task Initialize ()
		{
			if (initialized)
				return;

			NSMetadataQuery.Notifications.ObserveDidFinishGathering (HandleQueryFileListReceived);
			NSMetadataQuery.Notifications.ObserveDidUpdate (HandleQueryFileListReceived);

			await Task.Run (() => {
				var containerUrl = NSFileManager.DefaultManager.GetUrlForUbiquityContainer (null);

				if (containerUrl == null) {
					documentsUrl = null;
				}
				else {
					documentsUrl = containerUrl.Append ("Documents", true);

					try {
						Directory.CreateDirectory (documentsUrl.Path);
					} catch (Exception ex) {
						Debug.WriteLine (ex);
					}
				}
			});

			initialized = true;

			IsSyncing = true;

			await BeginQuery ();
		}

		public bool IsWritable { get { return this.documentsUrl != null; } }

		public bool IsAvailable { get { return CloudFileSystemProvider.CloudAvailable; } }
		public string AvailabilityReason { get { return "Not signed in"; } }

		public bool IsSyncing { get; private set; }
		public string SyncStatus { get { return "Syncing"; } }

		public string GetLocalPath (string path)
		{
			return documentsUrl.Append (path, false).Path;
		}

		public Task<IFile> CreateFile (string path, byte[] contents)
		{
			var tcs = new TaskCompletionSource<IFile> ();

			var f = new CloudFile (path, documentsUrl);

			if (contents != null) {
				Task.Factory.StartNew (() => {
					var c = new NSFileCoordinator (filePresenterOrNil: null);
					NSError coordErr;
					c.CoordinateWrite (f.LocalUrl, NSFileCoordinatorWritingOptions.ForReplacing, out coordErr, newUrl => {

						var newPath = newUrl.Path;

//						Console.WriteLine ("CLOUD WRITE TO " + newUrl + "    from: " + f.LocalUrl + "    at: " + newPath);

						var dir = Path.GetDirectoryName (newPath);
						if (!string.IsNullOrEmpty (dir) && dir != "/" && !Directory.Exists (dir)) {
							Directory.CreateDirectory (dir);
						}

						try {
							File.WriteAllBytes (newPath, contents);
							tcs.SetResult (f);							
						} catch (Exception ex) {
							tcs.SetException (ex);
						}
					});
					if (coordErr != null) {
						tcs.SetException (new Exception ("Could not coordinate iCloud write for CreateFile: " + coordErr.DebugDescription));
					}
				});
			} else {
				tcs.SetResult (f);
			}

			return tcs.Task;
		}

		public Task<bool> CreateDirectory (string path)
		{
			var tcs = new TaskCompletionSource<bool> ();

			var localPath = GetLocalPath (path);

			var url = NSUrl.FromFilename (localPath);


			Task.Factory.StartNew (() => {
				var c = new NSFileCoordinator (filePresenterOrNil: null);
				NSError coordErr;
				c.CoordinateWrite (url, NSFileCoordinatorWritingOptions.ForReplacing, out coordErr, newUrl => {
					try {
						if (Directory.Exists (newUrl.Path)) {
							tcs.SetResult (true);
							return;
						}

						var man = new NSFileManager ();
						NSError mkdirErr;
						var r = man.CreateDirectory (newUrl, false, null, out mkdirErr);
						Debug.WriteLineIf (!r, mkdirErr);

						tcs.SetResult (r);

					} catch (Exception ex) {
						Debug.WriteLine (ex);
						tcs.SetResult (false);
					}
				});
			});

			return tcs.Task;
		}

		public Task<bool> Move (string fromPath, string toPath)
		{
			var tcs = new TaskCompletionSource<bool> ();

			var fromLocalPath = GetLocalPath (fromPath);
			var toLocalPath = GetLocalPath (toPath);

			var fromUrl = NSUrl.FromFilename (fromLocalPath);
			var toUrl = NSUrl.FromFilename (toLocalPath);

			Task.Run (() => {
				var c = new NSFileCoordinator (filePresenterOrNil: null);
				NSError coordErr;
				c.CoordinateReadWrite (fromUrl, NSFileCoordinatorReadingOptions.WithoutChanges, toUrl, NSFileCoordinatorWritingOptions.ForReplacing, out coordErr, (newFromUrl, newToUrl) => {
					try {
						var man = new NSFileManager ();
						NSError moveErr;

						var r = man.Move (newFromUrl, newToUrl, out moveErr);
						Debug.WriteLineIf (!r, moveErr);

						needsRefresh = true;

						tcs.SetResult (r);

					} catch (Exception ex) {
						Debug.WriteLine (ex);
						tcs.SetResult (false);
					}
				});
			});

			return tcs.Task;
		}

		TaskCompletionSource<object> firstQueryResult;

		Task BeginQuery ()
		{
			if (firstQueryResult != null) {
				var t = firstQueryResult;
				firstQueryResult = null;
				if (!t.Task.IsCompleted)
					t.SetException (new Exception ("Stopped"));
			}
			if (query != null) {
				query.StopQuery ();
			}

			firstQueryResult = new TaskCompletionSource<object> ();

			if (documentsUrl == null) {
				firstQueryResult.SetResult (null);
				return firstQueryResult.Task;
			}

			var queryString = "";
			var head = "";
			var args = new List<NSObject> ();
			foreach (var e in FileExtensions) {
				queryString += head + "(%K LIKE '*." + e + "')";
				head = " OR ";
				args.Add (NSMetadataQuery.ItemFSNameKey);
			}

			query = new NSMetadataQuery ();
			query.SearchScopes = new NSObject[] { NSMetadataQuery.QueryUbiquitousDocumentsScope };
			query.Predicate = NSPredicate.FromFormat (
				queryString,
				args.ToArray ());

			Console.WriteLine ("START iCLOUD QUERY");

			query.StartQuery ();

			needsRefresh = false;

			return firstQueryResult.Task;
		}

		void HandleQueryFileListReceived (object sender, NSNotificationEventArgs e)
		{
			var fm = new NSFileManager ();

			var deadIndex = new HashSet<string> (fileIndex.Keys);
			var newList = new List<CloudFile> ();
			var newIndex = new Dictionary<string, IFile> ();

//			Console.WriteLine ("-------------- Received iCloud");

			foreach (var item in query.Results) {

				var cf = new CloudFile (item, documentsUrl);

				var key = cf.Path;

				if (deadIndex.Contains (key)) {
					deadIndex.Remove (key);
				}
				else {
					newList.Add (cf);
				}
				newIndex[key] = cf;

				//
				// Add a "file" for directories
				//
				var dir = Path.GetDirectoryName (cf.Path);
				if (!string.IsNullOrEmpty (dir) && !newIndex.ContainsKey (dir))
					newIndex [dir] = new DeviceFile (dir, documentsUrl.Path);


//				Console.WriteLine (cf);
			}

			foreach (var f in newList) {
				if (!f.IsDownloaded) {
					NSError error;
					fm.StartDownloadingUbiquitous (f.LocalUrl, out error);
				}
			}

			fileIndex = newIndex;

			IsSyncing = false;

			if (firstQueryResult != null) {
				var t = firstQueryResult;
				firstQueryResult = null;
				t.SetResult (null);
			}

			var ev = FilesChanged;
			if (ev != null) {
				ev (this, EventArgs.Empty);
			}
		}

		public Task<bool> DeleteFile (string path)
		{
			var localUrl = documentsUrl.Append (path, false);

			var tcs = new TaskCompletionSource<bool> ();

			Task.Factory.StartNew (() => {
				var c = new NSFileCoordinator (filePresenterOrNil: null);
				NSError coordErr;
				c.CoordinateWrite (localUrl, NSFileCoordinatorWritingOptions.ForDeleting, out coordErr, newUrl => {
					bool r = false;
					using (var m = new NSFileManager ()) {
						NSError remErr;
						r = m.Remove (newUrl, out remErr);
					}
					tcs.SetResult (r);
				});
				if (coordErr != null) {
					tcs.SetResult (false);
				}
			});

			return tcs.Task;
		}
	}

	public class CloudFile : IFile
	{
		readonly NSUrl documentsUrl;

		public string Path { get; private set; }
		public DateTime ModifiedTime { get; private set; }

		public string LocalPath { get; private set; }
		public NSUrl LocalUrl { get; private set; }

		public bool IsDownloaded { get; private set; }
		public double DownloadProgress { get; private set; }

		public bool IsDirectory { get { return false; } }


		public CloudFile (string path, NSUrl documentsUrl)
		{
			this.documentsUrl = documentsUrl;
			Path = path;
			LocalUrl = documentsUrl.Append (Path, false);
			LocalPath = LocalUrl.Path;
			ModifiedTime = DateTime.UtcNow;
			IsDownloaded = true;
			DownloadProgress = 1;
		}

		public CloudFile (NSMetadataItem item, NSUrl documentsUrl)
		{
			this.documentsUrl = documentsUrl;

			LocalPath = item.ValueForKey (NSMetadataQuery.ItemPathKey).ToString ();

			var docsPath = documentsUrl.Path;
			var docsPathLen = docsPath.Length;
			if (!docsPath.EndsWith ("/", StringComparison.Ordinal))
				docsPathLen++;

			Path = LocalPath.Substring (docsPathLen);

			LocalUrl = NSUrl.FromFilename (LocalPath);

			var t = (NSDate)item.ValueForKey (NSMetadataQuery.ItemFSContentChangeDateKey);
			ModifiedTime = new DateTime (2001, 1, 1).AddSeconds (t.SecondsSinceReferenceDate);
			IsDownloaded = ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemIsDownloadedKey)).BoolValue;
			var isDownloading = ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemIsDownloadingKey)).BoolValue;
			DownloadProgress = isDownloading ? ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemPercentDownloadedKey)).DoubleValue/100 : 1;
		}

//		public bool IsUploaded { get { return ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemIsUploadedKey)).BoolValue; } }
//		public bool IsUploading { get { return ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemIsUploadingKey)).BoolValue; } }
//		public double PercentUploaded { get { return IsUploading ? ((NSNumber)item.ValueForKey (NSMetadataQuery.UbiquitousItemPercentUploadedKey)).DoubleValue : 100; } }

		public Task<LocalFileAccess> BeginLocalAccess ()
		{
			return Task.FromResult (new LocalFileAccess (LocalPath));
		}

		public Task<bool> Delete ()
		{
			var tcs = new TaskCompletionSource<bool> ();

			Task.Factory.StartNew (() => {
				var c = new NSFileCoordinator (filePresenterOrNil: null);
				NSError coordErr;
				c.CoordinateWrite (LocalUrl, NSFileCoordinatorWritingOptions.ForDeleting, out coordErr, newUrl => {
					bool r = false;
					using (var m = new NSFileManager ()) {
						NSError remErr;
						r = m.Remove (newUrl, out remErr);
					}
					tcs.SetResult (r);
				});
			});

			return tcs.Task;
		}

		public Task<bool> Move (string newPath)
		{
			var tcs = new TaskCompletionSource<bool> ();

			NSUrl newUrl = documentsUrl.Append (newPath, false);

			Task.Factory.StartNew (() => {
				var c = new NSFileCoordinator (filePresenterOrNil: null);
				NSError coordErr;
				c.CoordinateWriteWrite (LocalUrl, NSFileCoordinatorWritingOptions.ForMoving, newUrl, NSFileCoordinatorWritingOptions.ForReplacing, out coordErr, (url1, url2) => {
					bool r = false;
					using (var m = new NSFileManager ()) {
						NSError remErr;
						r = m.Move (url1, url2, out remErr);
						if (r) {
							Path = newPath;
							LocalUrl = newUrl;
							LocalPath = LocalUrl.Path;
						}
					}
					tcs.SetResult (r);
				});
			});

			return tcs.Task;
		}

		public override string ToString ()
		{
			return !IsDownloaded ? 
				string.Format ("{0} (downloading {1})", Path, DownloadProgress) : 
				Path;
		}
	}

	public enum CloudErrorCode
	{
		SourceNotFound = 4,
		DestinationAlreadyExists = 516,
	}

	public class CloudException : Exception
	{
		public CloudErrorCode ErrorCode { get; private set; }

		public CloudException (NSError error)
			: base (((CloudErrorCode)error.Code) + " (" + error.Code + ")")
		{
			ErrorCode = (CloudErrorCode)error.Code;
		}
	}
}

