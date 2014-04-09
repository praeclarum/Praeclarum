using System;

namespace Praeclarum
{
	public struct StringRange
	{
		public int Location;
		public int Length;

		public int End { get { return Location + Length; } }

		public StringRange (int location, int length)
		{
			Location = location;
			Length = length;
		}

		public StringRange FitIn (int length)
		{
			var loc = Location;
			if (loc > length)
				loc = length;
			var end = loc + Length;
			if (end > length)
				end = length;
			return new StringRange (loc, end - loc);
		}

		public StringRange Offset (int offset)
		{
			return new StringRange (offset + Location, Length);
		}

		public string Substring (string source)
		{
			return source.Substring (Location, Length);
		}

		public StringRange WithLength (int length)
		{
			return new StringRange (Location, length);
		}

		public bool Contains (StringRange other)
		{
			var end = Location + Length;
			var oend = other.Location + other.Length;

			return
				Location <= other.Location && other.Location < end &&
				Location <= oend && oend < end;
		}

		public bool Intersects (StringRange other)
		{
			if (other.Location >= Location + Length)
				return false;
			if (Location >= other.Location + other.Length)
				return false;
			return true;
		}

		public bool EndsWith (string right, string source)
		{
			if (right.Length > Length)
				return false;

			var o = Location + Length - 1;

			for (var i = 0; i < right.Length; i++) {
				if (source [o - i] != right [i])
					return false;
			}

			return true;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is StringRange)) return false;
			var o = (StringRange)obj;
			return o.Location == Location && o.Length == Length;
		}

		public override int GetHashCode ()
		{
			return Location.GetHashCode () + Length.GetHashCode ();
		}

		public static bool operator == (StringRange a, StringRange b)
		{
			return a.Location == b.Location && a.Length == b.Length;
		}

		public static bool operator != (StringRange a, StringRange b)
		{
			return a.Location != b.Location || a.Length != b.Length;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}, {1}]", Location, Length);
		}
	}

	public struct StringReplacement
	{
		public string Text;
		public StringRange Range;

		public StringReplacement (string text, StringRange range)
		{
			Text = text;
			Range = range;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is StringReplacement)) return false;
			var o = (StringReplacement)obj;
			return o.Range == Range && o.Text == Text;
		}

		public override int GetHashCode ()
		{
			return Text.GetHashCode () + Range.GetHashCode ();
		}

		public static bool operator == (StringReplacement a, StringReplacement b)
		{
			return a.Text == b.Text && a.Range == b.Range;
		}

		public static bool operator != (StringReplacement a, StringReplacement b)
		{
			return a.Text != b.Text || a.Range != b.Range;
		}

		public override string ToString ()
		{
			return string.Format ("\"{0}\"@{1}", Text, Range);
		}
	}

}

