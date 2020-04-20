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
			var t = Translate (english);
			return Note (english, t);
		}

		public static string Localize (this string english)
		{
			var t = Translate (english);
			return Note (english, t);
		}

		public static string Localize (this string format, params object[] args)
		{
			var t = Translate (format);
			return string.Format (Note (format, t), args);
		}

		static string Translate (string english)
		{
#if __IOS__ || __MACOS__
			if (Foundation.NSUserDefaults.StandardUserDefaults.BoolForKey ("UseEnglish"))
				return english;
#pragma warning disable CS0618 // Type or member is obsolete
			return Foundation.NSBundle.MainBundle.LocalizedString (key: english, comment: "");
#pragma warning restore CS0618 // Type or member is obsolete
#else
			return english;
#endif
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
				System.Diagnostics.Debug.WriteLine ($"Needs Translation [{lang}]: \"{english}\" = \"\";");
			}
#endif
			return translation.Length > 0 ? translation : english;
		}
	}
}
