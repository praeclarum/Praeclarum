using System;
using Praeclarum.IO;
using Praeclarum.UI;

namespace Praeclarum.App
{
	public interface IDocumentAppSettings : IAppSettings
	{
		string LastDocumentPath { get; set; }

		DocumentsSort DocumentsSort { get; set; }

		string FileSystem { get; set; }

		bool UseCloud { get; set; }
		bool AskedToUseCloud { get; set; }

		string GetWorkingDirectory (IFileSystem fileSystem);
		void SetWorkingDirectory (IFileSystem fileSystem, string path);

		string DocumentationVersion { get; set; }

		bool DarkMode { get; set; }

		bool IsPatron {
			get;
			set;
		}

		DateTime PatronEndDate {
			get;
			set;
		}
	}

	public interface IAppSettings
	{
		int RunCount { get; set; }
	}

	public static class AppSettingsEx
	{
		public static bool IsFirstRun (this IAppSettings settings)
		{
			return settings.RunCount == 1;
		}
	}
}

