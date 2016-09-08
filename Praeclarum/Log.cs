using System;

namespace Praeclarum
{
	public static class Log
	{
		public static string Domain = "Praeclarum";

		public static void Error(string context, Exception ex)
		{
#if XAMARIN_INSIGHTS
			Xamarin.Insights.Report(ex, "Context", context??"", Xamarin.Insights.Severity.Warning);
#endif
			try
			{
				if (ex == null)
					return;
				WriteLine("E", ex.ToString());
			}
			catch
			{
			}
		}

		public static void Error (Exception ex)
		{
			Error("", ex);
		}

		static void WriteLine (string type, string line)
		{
			#if MONODROID
			if (_pendingType == "E") {
				Android.Util.Log.Error (Domain, line);
			}
			else {
				Android.Util.Log.Info (Domain, line);
			}
			#elif MONOMAC
			if (type == "E") {
				Console.WriteLine ("ERROR: " + line);
			}
			else {
				Console.WriteLine (line);
			}
			#else
			if (type == "E") {
				System.Diagnostics.Debug.WriteLine ("ERROR: " + line);
			}
			else {
				System.Diagnostics.Debug.WriteLine (line);
			}
			//Console.WriteLine (line);
			#endif
		}

		public static string GetUserErrorMessage (Exception ex)
		{
			if (ex == null)
				return "";

			var i = ex;
			while (i.InnerException != null) {
				i = i.InnerException;
			}
			return i.Message;
		}

		#if __IOS__
		public static void ShowError (this Foundation.NSObject obj, Exception ex, string format, params string[] args)
		{
			var title = format;
			try {
				title = string.Format (format, args);
			} catch (Exception ex2) {
				Log.Error (ex2);
			}
			ShowError (obj, ex, title);
		}
		public static void ShowError (this Foundation.NSObject obj, Exception ex, string title = "")
		{
			if (ex == null)
				return;

			Error (ex);

			try {
				if (string.IsNullOrEmpty (title)) {
					title = "Error";
				}
				var message = GetUserErrorMessage (ex);
				#if DEBUG
				message += "\n\n" + ((ex.GetType () == typeof (Exception)) ? "" : ex.GetType ().Name);
				message += " " + ex.StackTrace;
				#endif
				if (obj != null) {
					obj.BeginInvokeOnMainThread (() => {
						try {
							var alert = new UIKit.UIAlertView (
								title,
								message,
								null,
								"OK");
							alert.Show ();
						} catch (Exception ex3) {
							Error (ex3);
						}
					});
				}
			} catch (Exception ex2) {
				Error (ex2);
			}
		}
		#endif
	}
}

