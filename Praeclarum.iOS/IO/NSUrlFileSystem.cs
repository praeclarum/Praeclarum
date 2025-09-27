#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Foundation;

using UIKit;

using UniformTypeIdentifiers;

// ReSharper disable once CheckNamespace
namespace Praeclarum.IO;

// ReSharper disable once InconsistentNaming
public class NSUrlFileSystemProvider : IFileSystemProvider
{
	private static readonly bool ios14 = UIDevice.CurrentDevice.CheckSystemVersion (14, 0);

	private readonly List<NSUrlFileSystem> _fss = new ();

	public NSUrlFileSystemProvider ()
	{
		var defaults = NSUserDefaults.StandardUserDefaults;
		var keys = (defaults.StringForKey ("NSUrlFileSystemKeys") ?? "").Split (['/'],
			StringSplitOptions.RemoveEmptyEntries);
		foreach (var key in keys)
		{
			if (string.IsNullOrWhiteSpace (key))
				continue;
			_fss.Add (new NSUrlFileSystem (key));
		}
	}

	public string Name => "Folder in the Files App";
	public string IconUrl => "systemimage://folder.fill";
	public bool CanAddFileSystem => ios14;

	private UIDocumentPickerViewController? _currentPicker; // Keep a reference to avoid GC

	public Task ShowAddUI (object parent)
	{
		if (ios14 && ((parent as UIViewController) ?? UIApplication.SharedApplication.KeyWindow?.RootViewController) is {} pvc) {
			var tcs = new TaskCompletionSource<object?> ();
			var picker = new UIDocumentPickerViewController (contentTypes: [UTTypes.Folder], asCopy: false);
			_currentPicker = picker; // Keep a reference to avoid GC
			picker.AllowsMultipleSelection = false;
			picker.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
			picker.DidPickDocumentAtUrls += (s, e) =>
			{
				var url = e.Urls.FirstOrDefault ();
				if (url is not null)
				{
					var name = url.LastPathComponent ?? "Folder";
					if (url.IsDirectory ())
					{
						AddFileSystemAtUrl (url, name: name);
					}
					else
					{
						var message = $"\"{name}\" is not a folder.\n\nPlease select a folder using the \"Open\" button when browsing.";
						var alert = UIAlertController.Create ("Not a Folder", message, UIAlertControllerStyle.Alert);
						alert.AddAction (UIAlertAction.Create ("OK", UIAlertActionStyle.Default, null));
						pvc.PresentViewController (alert, true, null);
					}
				}
				tcs.TrySetResult (null);
			};
			pvc.PresentViewController (picker, true, null);
			return tcs.Task;
		}
		return Task.CompletedTask;
	}

	private void AddFileSystemAtUrl (NSUrl url, string name)
	{
		var key = Guid.NewGuid ().ToString ("N") + "-" + name;
		var fs = new NSUrlFileSystem (key);
		var defaults = NSUserDefaults.StandardUserDefaults;
		var gotPermission = url.StartAccessingSecurityScopedResource ();
		if (!gotPermission)
		{
			Log.Error ("Failed to get security scoped resource for new file system URL");
		}
		var bookmarkData = url.CreateBookmarkData (
			NSUrlBookmarkCreationOptions.SuitableForBookmarkFile | NSUrlBookmarkCreationOptions.WithSecurityScope,
			resourceValues: null,
			relativeUrl: null,
			error: out var error);
		// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (error is not null || bookmarkData is null)
			// ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			return;
		
		defaults.SetValueForKey (bookmarkData, (NSString)fs.DefaultsUrlKey);
		
		_fss.Add (fs);
		var keys = string.Join ("/", _fss.Select (x => x.Key));
		defaults.SetString (keys, "NSUrlFileSystemKeys");
		defaults.Synchronize ();
	}

	public IEnumerable<IFileSystem> GetFileSystems ()
	{
		return _fss.AsReadOnly ();
	}
}

// ReSharper disable once InconsistentNaming
public class NSUrlFileSystem : IFileSystem
{
	private readonly NSFileManager _fileManager;

	private NSUrl? _cachedRootUrl;

	/// <summary>
	/// _key example: "3103de6e532745cda45fdac55b1278c8-LastPathComponent"
	/// </summary>
	public string Key { get; }

	public string IconUrl => "systemimage://folder.fill";

	// ReSharper disable once ConvertToPrimaryConstructor
	public NSUrlFileSystem (string key)
	{
		Key = key;
		_fileManager = NSFileManager.DefaultManager;
	}

	public string LastPathComponent
	{
		get
		{
			var parts = Key.Split ('-');
			if (parts.Length < 2)
				return "Files";
			return string.Join ('-', parts.Skip (1));
		}
	}

	public string Id => $"NSUrlFileSystem-{Key}";
	public string DefaultsUrlKey => Id;

	public Task Initialize ()
	{
		return Task.CompletedTask;
	}

	public string Description => $"\"{LastPathComponent}\" Files";
	public string ShortDescription => Description;

	public bool IsAvailable
	{
		get
		{
			try
			{
				GetRootUrl();
				return true;
			}
			catch (Exception ex)
			{
				Log.Error ("NSUrlFileSystem is not available", ex);
				return false;
			}
		}
	}
	public string AvailabilityReason => string.Empty;
	public bool IsSyncing => false;
	public string SyncStatus => string.Empty;
	public bool JustForApp => true;
	public bool IsWritable => true;
	public int MaxDirectoryDepth => short.MaxValue;
	public ICollection<string> FileExtensions { get; } = new List<string> ();
#pragma warning disable CS0067
	public event EventHandler? FilesChanged;
#pragma warning restore CS0067

	readonly Dictionary<string, NSUrlFile> _filesByPath = new ();

	NSUrlFile GetFileWithUrl (NSUrl url)
	{
		var root = GetRootUrl();
		var urlAbsPath = url.Path ?? "";
		var rootAbsPath = root.Path ?? "";
		var path = urlAbsPath;
		if (urlAbsPath.StartsWith (rootAbsPath, StringComparison.Ordinal))
		{
			path = urlAbsPath.Substring (rootAbsPath.Length).TrimStart ('/');
		}

		lock (_filesByPath)
		{
			if (_filesByPath.TryGetValue (path, out var existing))
				return existing;
		}
		var isDir = url.IsDirectory ();
		var newFile = new NSUrlFile (this, path, url, isDir);
		lock (_filesByPath)
		{
			_filesByPath[path] = newFile;
		}
		return newFile;
	}

	public Task<List<IFile>> ListFiles (string directory)
	{
		return Task.Run (() =>
		{
			var dirUrl = GetUrlForPath (directory, isDirectory: true);
			Console.WriteLine ("ListFiles: " + dirUrl);
			var contents = _fileManager.GetDirectoryContent (dirUrl, properties: null, options: 0, out var error);
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (error is not null)
			{
				Log.Error ($"Failed to list files in directory '{directory}'", new NSErrorException (error));
				return [];
			}
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (contents is null) {
				Log.Error ($"Failed to list files in directory '{directory}'");
				return [];
			}

			var goodContents = new List<NSUrl> ();
			var extensions = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			if (FileExtensions.Count > 0)
			{
				foreach (var ext in FileExtensions)
				{
					var e = ext;
					if (!e.StartsWith ("."))
						e = "." + e;
					extensions.Add (e);
				}
			}
			foreach (var content in contents)
			{
				var name = content.LastPathComponent ?? "";
				if (string.IsNullOrEmpty (name) || name.StartsWith ("."))
					continue;
				if (content.IsDirectory () || extensions.Contains (System.IO.Path.GetExtension (name)))
				{
					goodContents.Add (content);
				}
			}
			return goodContents.Select (GetFileWithUrl).Cast<IFile> ().ToList ();
		});
	}

	public bool ListFilesIsFast => true;
	public Task<IFile> GetFile (string path)
	{
		return Task.Run (IFile () => GetFileWithUrl (GetUrlForPath (path, isDirectory: false)));
	}

	public Task<IFile> CreateFile (string path, byte[] contents)
	{
		return Task.Run (IFile () =>
		{
			var url = GetUrlForPath (path, isDirectory: false);
			try
			{
				var created = _fileManager.CreateFile (url.Path ?? "", NSData.FromArray (contents), attributes: null);
				if (!created)
				{
					throw new Exception ($"Failed to create file at path '{path}'");
				}
			}
			catch (Exception ex)
			{
				Log.Error ($"Error creating file at path '{path}'", ex);
			}

			return GetFileWithUrl (url);
		});
	}

	public Task<bool> CreateDirectory (string path)
	{
		return Task.Run (() =>
		{
			try
			{
				var url = GetUrlForPath (path, isDirectory: true);
				var created =
					_fileManager.CreateDirectory (url, createIntermediates: true, attributes: null, out var error);
				return created;
			}
			catch (Exception ex)
			{
				Log.Error ($"Error creating directory at path '{path}'", ex);
				return false;
			}
		});
	}

	public Task<bool> FileExists (string path)
	{
		return Task.Run (() =>
		{
			try
			{
				var url = GetUrlForPath (path, isDirectory: false);
				var exists = _fileManager.FileExists (url.Path ?? "");
				return exists;
			}
			catch (Exception ex)
			{
				Log.Error ($"Error checking for file existence at path '{path}'", ex);
				return false;
			}
		});
	}

	public Task<bool> DeleteFile (string path)
	{
		return Task.Run (() =>
		{
			try
			{
				var url = GetUrlForPath (path, isDirectory: false);
				var removed = _fileManager.Remove (url, out var error);
				// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				if (error is not null)
					return false;
				return removed;
			}
			catch (Exception ex)
			{
				Log.Error ($"Error deleting file at path '{path}'", ex);
				return false;
			}
		});
	}

	public Task<bool> Move (string fromPath, string toPath)
	{
		throw new NotImplementedException();
	}

	NSUrl GetRootUrl ()
	{
		if (_cachedRootUrl is not null)
			return _cachedRootUrl;
		var defaults = NSUserDefaults.StandardUserDefaults;
		var bookmarkedUrlData = defaults.DataForKey (DefaultsUrlKey);
		if (bookmarkedUrlData is null)
		{
			throw new InvalidOperationException ($"Cannot get root URL for files: missing data");
		}
		var bookmarkedUrl = NSUrl.FromBookmarkData (
			bookmarkedUrlData,
			NSUrlBookmarkResolutionOptions.WithSecurityScope,
			relativeToUrl: null,
			isStale: out var isStale,
			error: out var error);
		if (error is not null)
		{
			throw new NSErrorException (error);
		}
		if (bookmarkedUrl is null)
		{
			throw new InvalidOperationException ($"Cannot get root URL for files: invalid bookmark data");
		}
		var gotPermission = bookmarkedUrl.StartAccessingSecurityScopedResource ();
		if (!gotPermission)
		{
			Log.Error ("Failed to get security scoped resource");
		}

		if (isStale)
		{
			try
			{
				var newBookmarkData = bookmarkedUrl.CreateBookmarkData (
					NSUrlBookmarkCreationOptions.SuitableForBookmarkFile,
					resourceValues: null,
					relativeUrl: null,
					error: out var createError);
				// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				if (createError is null && newBookmarkData is not null)
				{
					defaults.SetValueForKey (newBookmarkData, (NSString)DefaultsUrlKey);
					defaults.Synchronize();
				}
				// ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			}
			catch (Exception ex)
			{
				Log.Error ("Error updating stale bookmark data", ex);
			}
		}

		_cachedRootUrl = bookmarkedUrl;
		return _cachedRootUrl;
	}
	
	NSUrl GetUrlForPath (string path, bool isDirectory)
	{
		var root = GetRootUrl();
		if (path.StartsWith ("/"))
			path = path.Substring (1);
		if (string.IsNullOrEmpty (path))
			return root;
		return root.Append (path, isDirectory: isDirectory);
	}
}

// ReSharper disable once InconsistentNaming
public static class NSUrlExtensions
{
	public static bool IsDirectory (this NSUrl url)
	{
		var resourceValues = url.GetResourceValues ([NSUrl.IsDirectoryKey], out var error);
		return resourceValues?.TryGetValue (NSUrl.IsDirectoryKey, out var nsv) is true
		       && nsv is NSNumber { BoolValue: true };
	}
}

public class NSUrlFile : IFile
{
	public NSUrlFileSystem FileSystem { get; }
	public string Path { get; }
	public NSUrl Url { get; }
	public bool IsDirectory { get; }
	public DateTime ModifiedTime {
		get;
	}

	// ReSharper disable once ConvertToPrimaryConstructor
	public NSUrlFile (NSUrlFileSystem fs, string path, NSUrl url, bool isDirectory)
	{
		FileSystem = fs;
		Path = path;
		Url = url;
		IsDirectory = isDirectory;
		ModifiedTime = DateTime.MinValue;
	}

	public Task<LocalFileAccess> BeginLocalAccess ()
	{
		return Task.FromResult<LocalFileAccess> (new NSUrlLocalFileAccess(this));
	}

	public bool IsDownloaded => true;
	public double DownloadProgress => 1;
	
	public Task<bool> Move (string newPath)
	{
		return FileSystem.Move (Path, toPath: newPath);
	}
}

// ReSharper disable once InconsistentNaming
public class NSUrlLocalFileAccess (NSUrlFile file) : LocalFileAccess (file.Url.Path ?? "");
