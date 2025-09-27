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
					var resourceValues = url.GetResourceValues ([NSUrl.IsDirectoryKey], out var error);
					var name = url.LastPathComponent ?? "Folder";
					if (resourceValues?.TryGetValue (NSUrl.IsDirectoryKey, out var nsv) is true && nsv is NSNumber number && number.BoolValue)
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
	public Task<List<IFile>> ListFiles (string directory)
	{
		Console.WriteLine ("ListFiles: " + directory);
		return Task.FromResult (new List<IFile>());
	}

	public bool ListFilesIsFast => true;
	public Task<IFile> GetFile (string path)
	{
		throw new NotImplementedException();
	}

	public Task<IFile> CreateFile (string path, byte[] contents)
	{
		throw new NotImplementedException();
	}

	public Task<bool> CreateDirectory (string path)
	{
		throw new NotImplementedException();
	}

	public Task<bool> FileExists (string path)
	{
		throw new NotImplementedException();
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
		return root.Append (path, isDirectory: isDirectory);
	}
}
