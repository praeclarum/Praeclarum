using System;

namespace Praeclarum
{
	public static class StringHelper
	{
		public static bool IsBlank (this string s)
		{
			if (s == null) return true;
			var len = s.Length;
			if (len == 0) return true;
			for (var i = 0; i < len; i++)
				if (!char.IsWhiteSpace (s[i]))
					return false;
			return true;
		}
	}
}

