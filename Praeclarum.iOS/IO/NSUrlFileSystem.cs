#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Foundation;

// ReSharper disable once CheckNamespace
namespace Praeclarum.IO;

// ReSharper disable once InconsistentNaming
public class NSUrlFileSystem : IFileSystem
{
	private readonly string _key;
	private readonly NSFileManager _fileManager;

	private NSUrl? _cachedRootUrl;

	// ReSharper disable once ConvertToPrimaryConstructor
	public NSUrlFileSystem (string key = "Default")
	{
		_key = key;
		_fileManager = NSFileManager.DefaultManager;
	}

	public string Id => $"NSUrlFileSystem-{_key}";
	private string UrlKey => Id;

	public Task Initialize ()
	{
		return Task.CompletedTask;
	}

	public string Description => "Files";
	public string ShortDescription => "Files";
	public bool IsAvailable => true;
	public string AvailabilityReason => string.Empty;
	public bool IsSyncing => false;
	public string SyncStatus => string.Empty;
	public bool JustForApp => true;
	public bool IsWritable => true;
	public int MaxDirectoryDepth => short.MaxValue;
	public ICollection<string> FileExtensions { get; } = new List<string> ();
	public event EventHandler? FilesChanged;
	public Task<List<IFile>> ListFiles (string directory)
	{
		throw new NotImplementedException();
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
		var bookmarkedUrlData = defaults.DataForKey (UrlKey);
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
					defaults.SetValueForKey (newBookmarkData, (NSString)UrlKey);
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
