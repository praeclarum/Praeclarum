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

		bool DisableAnalytics { get; set; }

		bool HasTipped { get; set; }
		DateTime TipDate { get; set; }

		DateTime SubscribedToProDate { get; set; }
		string SubscribedToProFromPlatform { get; set; }
		int SubscribedToProMonths { get; set; }
	}

	public static class DocumentAppSettingsExtensions
	{
		public static DateTime SubscribedToProEndDate (this IDocumentAppSettings settings) => settings.SubscribedToProDate.AddMonths(settings.SubscribedToProMonths);
	}

	public interface IAppSettings
	{
		int RunCount { get; set; }
		bool UseEnglish { get; set; }
	}

	public static class AppSettingsEx
	{
		public static bool IsFirstRun (this IAppSettings settings)
		{
			return settings.RunCount == 1;
		}
	}
}

