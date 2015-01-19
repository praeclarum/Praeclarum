using System;

namespace Praeclarum
{
	public static class Log
	{
		public static string Domain = "Praeclarum";

		public static void Error (Exception ex)
		{
			WriteLine ("E", ex.ToString ());
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
			if (_pendingType == "E") {
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
	}
}

