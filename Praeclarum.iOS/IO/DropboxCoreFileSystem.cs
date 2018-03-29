using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;
using Foundation;

using Praeclarum.IO;

#if !NO_DROPBOX
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Users;
using SafariServices;
using System.Threading;

namespace Praeclarum.IO
{
	public class DropboxFileSystemProvider : IFileSystemProvider
	{
		public static TaskCompletionSource<DropboxFileSystem> AddCompletionSource {
			get;
			set;
		}

		public DropboxFileSystemProvider (string appKey, string appSecret, bool appFolder)
		{
			var session = new DropboxSession (appKey, appSecret, appFolder ? DropboxSessionRoot.App : DropboxSessionRoot.Dropbox);
			DropboxSession.SharedSession = session;
		}

#region IFileSystemProvider implementation

		public Task ShowAddUI (object parent)
		{
			var vc = parent as UIViewController;
			if (vc == null)
				return Task.FromResult<IFileSystem> (null);

			return DropboxSession.SharedSession.LinkFromControllerAsync (vc);
		}

		public IEnumerable<IFileSystem> GetFileSystems ()
		{
			var fss = new List<IFileSystem> ();

			var session = DropboxSession.SharedSession;

			if (session != null && session.IsLinked) {
				var fs = new DropboxFileSystem (session);
				fss.Add (fs);
			}

			return fss;
		}

		public string Name {
			get {
				return "Dropbox";
			}
		}

		public bool CanAddFileSystem {
			get {
				return true;
			}
		}

#endregion
	}

	public enum DropboxSessionRoot
	{
		App,
		Dropbox,
	}

	public class DropboxSession
	{
		public static DropboxSession SharedSession { get; set; }

		//readonly string appSecret;

		public DropboxSessionRoot Root { get; }
		public string AppKey { get; }
		public string OAuthState { get; } = Guid.NewGuid ().ToString ();

		public bool IsLinked { get; private set; }
		public string AccessToken { get; private set; }
		public string AccountId { get; private set; }

		public DropboxSession(string appKey, string appSecret, DropboxSessionRoot root)
		{
			AppKey = appKey;
			//this.appSecret = appSecret;
			Root = root;
			AccessToken = "";
			AccountId = "";
		}

		public bool HandleOpenUrl (NSUrl url)
		{
			var scheme = url.Scheme;
			if (!scheme.StartsWith ("db-", StringComparison.Ordinal))
				return false;

			LinkWithUrl (url);

			var tcs = linkTcs;
			var b = browser;

			if (b != null)
			{
				b.DismissViewController(true, () =>
				{
					tcs?.TrySetResult(null);
				});
			}
			else
			{
				tcs?.TrySetResult(null);
			}
			return true;
		}

		void LinkWithUrl (NSUrl url)
		{
			var parts = url.Fragment.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => Uri.UnescapeDataString (x[1]));
			if (parts.TryGetValue("access_token", out var at))
			{
				if (parts.TryGetValue("account_id", out var ai))
				{
					AccessToken = at;
					AccountId = ai;
					IsLinked = true;
					return;
				}
			}
			IsLinked = false;
		}

		TaskCompletionSource<object> linkTcs;
		DropboxAuthBrowserController browser;

		public async Task LinkFromControllerAsync (UIViewController vc)
		{
			if (linkTcs != null)
			{
				await linkTcs.Task;
			}

			linkTcs = new TaskCompletionSource<object> ();
			try
			{
				var del = new DropboxAuthBrowserDelegate (linkTcs);
				browser = new DropboxAuthBrowserController (GetAuthUrl()) { Delegate = del };
				await vc.PresentViewControllerAsync (browser, true);
				await linkTcs.Task;
			}
			finally
			{
				linkTcs = null;
			}
		}

		string GetAuthUrl ()
		{
			var r = "https://www.dropbox.com/oauth2/authorize?response_type=token";

			var locale = NSBundle.MainBundle.PreferredLocalizations.FirstOrDefault () ?? "en";

			var queryItems = new Dictionary<string, string> {
				{"client_id", AppKey},
				{"redirect_uri", GetRedirectUrl ()},
				{"disable_signup", "true"},
				{"locale", locale},
				{"state", OAuthState},
			};
			foreach (var q in queryItems)
			{
				r += "&" + Uri.EscapeDataString (q.Key) + "=" + Uri.EscapeDataString (q.Value);
			}

			return r;
		}

		string GetRedirectUrl ()
		{
			return $"db-{AppKey}://2/token";
		}
	}

	public class DropboxFileSystem : IFileSystem
	{
		readonly DropboxSession session;
		readonly DropboxClient sharedClient;

		public string UserId { get; private set; }
		public string DisplayName { get; private set; }

		public DropboxSession Session => session;

		public DropboxFileSystem (DropboxSession session)
		{
			this.session = session;
			sharedClient = new DropboxClient (session.AccessToken);
			UserId = session.AccountId;
			FileExtensions = new System.Collections.ObjectModel.Collection<string> ();
		}

		public DropboxClient GetClient ()
		{
			return sharedClient;
		}

		DropboxFile GetDropboxFile (Metadata meta)
		{
			return new DropboxFile (this, meta);
		}

		public Task<Metadata> DropboxLoadMetadataAsync (string path)
		{
			var c = GetClient ();
			return c.Files.GetMetadataAsync(path);
		}

		public async Task DropboxLoadFileAsync (string path, string destinationPath)
		{
			var c = GetClient ();
			using (var dr = await c.Files.DownloadAsync (path).ConfigureAwait (false))
			{
				using (var ss = await dr.GetContentAsStreamAsync().ConfigureAwait (false))
				{
					using (var ds = new System.IO.FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						await ss.CopyToAsync(ds).ConfigureAwait (false);
					}
				}
			}
		}

		public Task<FileMetadata> DropboxUploadFileAsync (string path, string parentRev, string sourcePath)
		{
			var c = GetClient ();
			return Task.Run(async () =>
			{
				using (var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
				{
					return await c.Files.UploadAsync(path, mode: WriteMode.Overwrite.Instance, body: inputStream).ConfigureAwait (false);
				}
			});
		}

		public Task<CreateFolderResult> DropboxCreateDirectoryAsync (string path)
		{
			var c = GetClient();
			return c.Files.CreateFolderV2Async (path);
		}

		public Task<DeleteResult> DropboxDeletePathAsync (string path)
		{
			var c = GetClient ();
			return c.Files.DeleteV2Async(path);
		}

		public Task<RelocationResult> DropboxMovePathAsync (string fromPath, string toPath)
		{
			var c = GetClient ();
			return c.Files.MoveV2Async(fromPath, toPath);
		}

		public Task<BasicAccount> DropboxLoadAccountInfoAsync ()
		{
			var c = GetClient ();
			return c.Users.GetAccountAsync (session.AccountId);
		}

#region IFileSystem implementation

		public bool JustForApp {
			get {
				return session.Root == DropboxSessionRoot.App;
			}
		}

		public event EventHandler FilesChanged;

		public async Task Initialize ()
		{
			try {
				var r = await DropboxLoadAccountInfoAsync ();
				DisplayName = r.Name.DisplayName;
			} catch (Exception ex) {
				Log.Error (ex);
				DisplayName = UserId;
			}
		}

		public bool ListFilesIsFast { get { return false; } }

		public async Task<List<IFile>> ListFiles (string directory)
		{
			var c = GetClient();
			var d = directory;
			var r = await c.Files.ListFolderAsync (d).ConfigureAwait (false);

			var exts = FileExtensions.Select (x => "." + x).ToDictionary (x => x);

			var res =
				r.Entries.
				Select(GetDropboxFile).
				 Where(x =>
				{
					var ext = Path.GetExtension(x.Path);
					return x.IsDirectory || (!string.IsNullOrEmpty (ext) && exts.ContainsKey (ext));
				}).
				Cast<IFile>().
				 ToList();
			return res;
		}

		public async Task<IFile> GetFile (string path)
		{
			var meta = await DropboxLoadMetadataAsync (path).ConfigureAwait (false);
			return GetDropboxFile (meta);
		}

		public async Task<IFile> CreateFile (string path, byte[] contents)
		{
			var src = await Task.Run(() => {
				var p = Path.GetTempFileName ();
				File.WriteAllBytes (p, contents);
				return p;
			}).ConfigureAwait (false);
			await DropboxUploadFileAsync (path, null, src).ConfigureAwait (false);
			return await GetFile (path).ConfigureAwait (false);
		}
		public async Task<bool> CreateDirectory (string path)
		{
			try {
				await DropboxCreateDirectoryAsync (path).ConfigureAwait (false);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public async Task<bool> FileExists (string path)
		{
			try {
				var c = GetClient();
				var m = await c.Files.GetMetadataAsync (path).ConfigureAwait (false);
				return !m.IsDeleted;
			} catch (Exception) {
				return false;
			}
		}
		public async Task<bool> DeleteFile (string path)
		{
			try {
				await DropboxDeletePathAsync (path);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public async Task<bool> Move (string fromPath, string toPath)
		{
			try {
				await DropboxMovePathAsync (fromPath, toPath);
				return true;
			} catch (Exception ex) {
				Log.Error (ex);
				return false;
			}
		}
		public string Id {
			get {
				return "Dropbox";
			}
		}
		public string Description {
			get {
				return IsAvailable ? "Dropbox: " + DisplayName : "Dropbox";
			}
		}
		public string ShortDescription {
			get {
				return "Dropbox";
			}
		}
		public bool IsAvailable {
			get {
				return true;
			}
		}
		public string AvailabilityReason {
			get {
				return "";
			}
		}
		public bool IsSyncing {
			get {
				return false;
			}
		}
		public string SyncStatus {
			get {
				return "Syncing";
			}
		}
		public bool IsWritable {
			get {
				return true;
			}
		}
		public int MaxDirectoryDepth {
			get {
				return short.MaxValue;
			}
		}
		public ICollection<string> FileExtensions {
			get;
			private set;
		}
#endregion
	}

	class DropboxFile : IFile
	{
		public readonly DropboxFileSystem FileSystem;
		Metadata meta;
		public DropboxFile (DropboxFileSystem fs, Metadata meta)
		{
			this.FileSystem = fs;
			this.meta = meta;

			Path = meta.PathDisplay;
			DropboxPath = Path;
		}

		public override string ToString ()
		{
			return DropboxPath + "@" + Rev;
		}

		public string Rev { get { return meta.IsFile ? meta.AsFile.Rev : ""; } }

#region IFile implementation

		public async Task<LocalFileAccess> BeginLocalAccess ()
		{
			if (IsDirectory)
				throw new InvalidOperationException ("Only files permit local access");

			var docsDir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var cachesDir = System.IO.Path.GetFullPath (System.IO.Path.Combine (docsDir, "../Library/Caches"));

			var tmpDir = System.IO.Path.Combine (cachesDir, "DropboxTemp");

			var filename = Path;
			if (filename.StartsWith ("/", StringComparison.Ordinal))
			{
				filename = filename.Substring (1);
			}
			
			var lp = System.IO.Path.Combine (tmpDir, filename);

			FileSystemManager.EnsureDirectoryExists (System.IO.Path.GetDirectoryName (lp));

			await FileSystem.DropboxLoadFileAsync (DropboxPath, lp);

			var data = await Task.Run (() => File.ReadAllText (lp, Encoding.UTF8));

			return new DropboxLocal (this, lp, data);
		}

		public async Task<bool> Move (string newPath)
		{
			if (await FileSystem.Move (Path, newPath)) {
				try {
					var f = (DropboxFile)await FileSystem.GetFile (newPath);
					meta = f.meta;
					return true;
				} catch (Exception ex) {
					Log.Error (ex);
				}
			}
			return false;
		}

		public string Path { get; }

		public string DropboxPath { get; }

		public bool IsDirectory {
			get {
				return meta.IsFolder;
			}
		}

		public DateTime ModifiedTime {
			get {
				return meta.IsFile ? meta.AsFile.ClientModified : new DateTime (1970, 1, 1);
			}
		}

		public bool IsDownloaded {
			get {
				return true;
			}
		}

		public double DownloadProgress {
			get {
				return 1;
			}
		}

#endregion
	}

	class DropboxLocal : LocalFileAccess
	{
		readonly DropboxFile file;
		readonly string remoteData;

		public DropboxLocal (DropboxFile file, string localPath, string data)
			: base (localPath)
		{
			this.file = file;
			this.remoteData = data;
		}

		public override async Task End ()
		{
			var data = await Task.Run (() => File.ReadAllText (LocalPath, Encoding.UTF8));
			if (data != remoteData) {
				// Get the newest revision
				var newestFile = (DropboxFile)await file.FileSystem.GetFile (file.Path);
				await file.FileSystem.DropboxUploadFileAsync (file.DropboxPath, newestFile.Rev, LocalPath);
			}
		}
	}

	class DropboxAuthBrowserController : SFSafariViewController
	{
		public DropboxAuthBrowserController(string url)
			: base (NSUrl.FromString (url))
		{
		}
	}

	class DropboxAuthBrowserDelegate : SFSafariViewControllerDelegate
	{
		public TaskCompletionSource<object> tcs;

		public DropboxAuthBrowserDelegate(TaskCompletionSource<object> tcs)
		{
			this.tcs = tcs;
		}

		public override void DidFinish(SFSafariViewController controller)
		{
			tcs.TrySetResult (null);
		}
	}
}

#endif
