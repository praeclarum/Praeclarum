using System;
using Foundation;
using Praeclarum.IO;
using Praeclarum.UI;

namespace Praeclarum.App
{
	public class DocumentAppSettings : IDocumentAppSettings
	{
		protected readonly NSUserDefaults defs;

		public DocumentAppSettings (NSUserDefaults defaults)
		{
			if (defaults == null)
				throw new ArgumentNullException ("defaults");
			defs = defaults;
		}

		public DocumentsSort DocumentsSort {
			get {
				var str = defs.StringForKey ("DocumentsSort");
				return str == "Name" ? DocumentsSort.Name : DocumentsSort.Date;
			}
			set {
				var str = value == DocumentsSort.Name ? "Name" : "Date";
				defs.SetString (str, "DocumentsSort");
				defs.Synchronize ();
			}
		}

		public string DocumentationVersion {
			get {
				return defs.StringForKey ("DocumentationVersion");
			}
			set {
				defs.SetString (value, "DocumentationVersion");
				defs.Synchronize ();
			}
		}

		public string GetWorkingDirectory (IFileSystem fileSystem)
		{
			var path = defs.StringForKey (fileSystem.Id + " CWD") ?? "";
			if (path.StartsWith ("file:", StringComparison.Ordinal))
				return "";
			return path;
		}

		public void SetWorkingDirectory (IFileSystem fileSystem, string directory)
		{
			defs.SetString (directory, fileSystem.Id + " CWD");
			defs.Synchronize ();
		}

		static readonly DateTime Epoch = new DateTime (1970, 1, 1);

		protected DateTime GetDateTime (string key)
		{
			var s = defs.IntForKey (key);
			return Epoch + TimeSpan.FromSeconds (s);
		}

		protected void SetDateTime (string key, DateTime value)
		{
			var s = (int)(value - Epoch).TotalSeconds;
			defs.SetInt (s, key);
		}

		protected double GetDouble (string key, double defaultValue)
		{
			var s = defs [key] as NSNumber;
			if (s != null) {
				return s.DoubleValue;
			}
			return defaultValue;
		}

		protected void SetDouble (string key, double value)
		{
			defs.SetDouble (value, key);
		}

		public string FileSystem {
			get {
				return defs.StringForKey ("FileSystem") ?? "";
			}
			set {
				defs.SetString (value ?? "", "FileSystem");
			}
		}

		public string LastDocumentPath {
			get {
				return defs.StringForKey ("LastDocumentPath") ?? "";
			}
			set {
				defs.SetString (value ?? "", "LastDocumentPath");
				defs.Synchronize ();
			}
		}

		public int RunCount {
			get {
				return (int)defs.IntForKey ("RunCount");
			}
			set {
				defs.SetInt (value, "RunCount");
			}
		}

		public bool DisableAnalytics {
			get {
				return defs.BoolForKey ("DisableAnalytics");
			}
			set {
				defs.SetBool (value, "DisableAnalytics");
			}
		}

		public bool AskedToUseCloud {
			get {
				return defs.BoolForKey ("AskedToUseCloud");
			}
			set {
				defs.SetBool (value, "AskedToUseCloud");
			}
		}

		public bool UseEnglish {
			get {
				return defs.BoolForKey ("UseEnglish");
			}
			set {
				defs.SetBool (value, "UseEnglish");
			}
		}

		public bool UseCloud {
			get {
				return defs.BoolForKey ("UseCloud");
			}
			set {
				defs.SetBool (value, "UseCloud");
			}
		}

		public bool FirstRun {
			get {
				return RunCount == 1;
			}
		}

		public bool DarkMode {
			get {
				return defs.BoolForKey ("DarkMode");
			}
			set {
				defs.SetBool (value, "DarkMode");
			}
		}

		public bool IsPatron {
			get {
				return defs.BoolForKey ("IsPatron");
			}
			set {
				defs.SetBool (value, "IsPatron");
			}
		}

		public DateTime PatronEndDate {
			get {
				return GetDateTime ("PatronEndDate");
			}
			set {
				SetDateTime ("PatronEndDate", value);
			}
		}

		public bool HasTipped {
			get {
				return defs.BoolForKey ("HasTipped");
			}
			set {
				defs.SetBool (value, "HasTipped");
			}
		}

		public DateTime TipDate {
			get {
				return GetDateTime ("TipDate");
			}
			set {
				SetDateTime ("TipDate", value);
			}
		}

		public bool SubscribedToPro
		{
			get
			{
				return defs.BoolForKey("SubscribedToPro");
			}
			set
			{
				defs.SetBool(value, "SubscribedToPro");
			}
		}

		public DateTime SubscribedToProDate
		{
			get
			{
				return GetDateTime("SubscribedToProDate");
			}
			set
			{
				SetDateTime("SubscribedToProDate", value);
			}
		}

		public int SubscribedToProMonths
		{
			get
			{
				return (int)defs.IntForKey("SubscribedToProMonths");
			}
			set
			{
				defs.SetInt(value, "SubscribedToProMonths");
			}
		}

	}
}

