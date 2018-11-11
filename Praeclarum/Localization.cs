using System;
using System.Collections.Generic;

namespace Praeclarum
{
	public static class Localization
	{
		public static string Localize (object obj)
		{
			if (obj == null)
				return "";
			var english = obj.ToString ();
			return Note (english, Foundation.NSBundle.MainBundle.LocalizedString (key: english, comment: ""));
		}

		public static string Localize (this string english)
		{
			return Note (english, Foundation.NSBundle.MainBundle.LocalizedString (key: english, comment: ""));
		}

		public static string Localize (this string format, params object[] args)
		{
			return string.Format (Note (format, Foundation.NSBundle.MainBundle.LocalizedString (key: format, comment: "")), args);
		}

#if DEBUG
		static readonly HashSet<string> notes = new HashSet<string> ();
#endif
		static string Note (string english, string translation)
		{
#if DEBUG
			var lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
			if (lang != "en" && english == translation && english.Length > 0 && !notes.Contains (english)) {
				notes.Add (english);
				Log.Info ($"Needs Translation [{lang}]: \"{english}\" = \"\";");
			}
#endif
			return translation.Length > 0 ? translation : english;
		}
	}
}
