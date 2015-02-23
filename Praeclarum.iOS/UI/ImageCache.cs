using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Praeclarum.App;
using Praeclarum.IO;
using CoreGraphics;

namespace Praeclarum.UI
{
	public class ImageCache
	{
		readonly string cacheDirectory;

		public ImageCache (string cacheDirectory)
		{
			Debug.WriteLine ("Image Cache: {0} {1}", DateTime.Now, cacheDirectory);
			this.cacheDirectory = cacheDirectory;
		}

		string GetCachePath (string key)
		{
			return Path.Combine (cacheDirectory, key) + "@2x.png";
		}

		class MemoryImage
		{
			public UIImage Image;
			public DateTime GeneratedTime;
			public string Key;
			public DateTime LastAccessTime;
		}

		const int MaxMemorySize = 50;

		readonly Dictionary<string, MemoryImage> memory = new Dictionary<string, MemoryImage> ();

		public UIImage GetMemoryImage (string key)
		{
			lock (memory) {
				MemoryImage memImage;
				if (memory.TryGetValue (key, out memImage)) {
					return memImage.Image;
				}
			}
			return null;
		}

		public async Task<UIImage> GetImageAsync (string key, DateTime oldestTime, bool getFromDisk = true)
		{
//			Console.WriteLine ("GetImage: {0} {1}", key, oldestTime);

			var scale = UIScreen.MainScreen.Scale;

			key = key ?? "";

			//
			// Is it in memory?
			//
			lock (memory) {
				MemoryImage memImage;
				if (memory.TryGetValue (key, out memImage)) {
					if (memImage.GeneratedTime > oldestTime) {
						memImage.LastAccessTime = DateTime.UtcNow;
						return memImage.Image;
					} else {
						memory.Remove (key);
					}
				}
			}

			//
			// Is it on disk? If so, try to load it and add it to memory.
			//
			if (getFromDisk) {
				return await Task.Run (() => {
					var cachePath = GetCachePath (key);
					var info = new FileInfo (cachePath);
					if (info.Exists && info.LastWriteTimeUtc > oldestTime) {

						var uiImage = UIImage.LoadFromData (NSData.FromFile (cachePath), scale);

						if (uiImage != null) {
							SetMemoryImage (key, uiImage, info.LastWriteTimeUtc);
							return uiImage;
						}
					}

					//
					// Nowhere to be found
					//
					return null;
				});
			}

			//
			// Nowhere to be found
			//
			return null;
		}

		public void RemoveImage (string key, bool removeFromDisk = false)
		{
			key = key ?? "";

			lock (memory) {
				if (memory.ContainsKey (key))
					memory.Remove (key);
			}

			if (removeFromDisk) {
				try {
					var cachePath = GetCachePath (key);
					if (File.Exists (cachePath)) {
						File.Delete (cachePath);
					}
				} catch (IOException ex) {
					// Swallow IO exceptions
					Debug.WriteLine (ex);
				}
			}
		}

		MemoryImage SetMemoryImage (string key, UIImage uiImage, DateTime genTime)
		{
			lock (memory) {
				MemoryImage memImage;
				if (memory.TryGetValue (key, out memImage)) {
					memImage.LastAccessTime = DateTime.UtcNow;
					memImage.Image = uiImage;
					return memImage;
				}

				//
				// If not in memory, then let's make room to add it
				//
				var numToRemove = (memory.Count + 1) - MaxMemorySize;
				if (numToRemove > 0) {
					var toRemove = memory.Values.OrderBy (x => x.LastAccessTime).Take (numToRemove).ToList ();
					foreach (var r in toRemove) {
						memory.Remove (r.Key);
					}
				}

				//
				// Now add it
				//
				memImage = new MemoryImage {
					Key = key,
					Image = uiImage,
					LastAccessTime = DateTime.UtcNow,
					GeneratedTime = genTime,
				};
				memory.Add (key, memImage);
				return memImage;
			}
		}

		public async Task SetGeneratedImageAsync (string key, UIImage uiImage, bool saveToDisk = true)
		{
			if (uiImage == null) {
				RemoveImage (key);
				return;
			}

			//
			// Does it already exist in memory?
			//
			SetMemoryImage (key, uiImage, DateTime.UtcNow);

			//
			// If we added it to memory, then save it to disk
			//
			if (saveToDisk) {
				await Task.Run (() => {
					FileSystemManager.EnsureDirectoryExists (cacheDirectory);
					var cachePath = GetCachePath (key);
					NSError err;
					uiImage.AsPNG ().Save (cachePath, false, out err);
					if (err != null) {
						Debug.WriteLine (err);
					}
				});
			}
		}
	}
}
