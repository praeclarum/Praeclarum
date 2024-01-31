using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Praeclarum
{
	public static class Log
	{
		public static string Domain = "Praeclarum";

		public class LogExtras
		{
			public Action<string, Dictionary<string, string>> TrackEvent;
			public Action<Exception, Dictionary<string, string>> TrackError;
		}

		public static LogExtras Logger;

		public static void TaskError (Task task)
		{
			if (task == null)
				return;
			if (task.IsFaulted)
				Error (task.Exception);
		}

		public static void Error (string context, Exception ex)
		{
			try {
				if (ex == null)
					return;
				var props = new Dictionary<string, string> ();
				if (!string.IsNullOrWhiteSpace (context)) {
					props["Context"] = context;
				}
				Logger?.TrackError (ex, props);
				WriteLine("E", ex.ToString());
			}
			catch
			{
			}
		}
		
		public static void QuietError (string context, Exception ex)
		{
			try {
				if (ex == null)
					return;
				WriteLine("E", String.Format ("{0}: {1}", context, ex));
			}
			catch
			{
			}
		}

		public static void Error (string message)
		{
			try {
				Logger?.TrackEvent ("Error", new Dictionary<string, string>{{
						"Message", message}});
				WriteLine ("E", message);
			}
			catch {
			}
		}

		public static void Analytics (string message)
		{
			try {
				Logger?.TrackEvent (message, null);
				WriteLine ("A", message);
			}
			catch {
			}
		}
		
		public static void Analytics (string message, params ValueTuple<string, string>[] values)
		{
			try {
				Logger?.TrackEvent (message, null);
				WriteLine ("A", message + " " + string.Join (" ", values.Select (v => v.Item1 + "=" + v.Item2)));
			}
			catch {
			}
		}

		public static void Info (string message)
		{
			try {
				WriteLine ("I", message);
			}
			catch {
			}
		}

		public static void Error (Exception ex)
		{
			Error("", ex);
		}

		static void WriteLine (string type, string line)
		{
#if MONODROID
			if (type == "E") {
				Android.Util.Log.Error (Domain, line);
			}
			else {
				Android.Util.Log.Info (Domain, line);
			}
#elif __MACOS__ || __IOS__
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
								(UIKit.IUIAlertViewDelegate)null,
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

