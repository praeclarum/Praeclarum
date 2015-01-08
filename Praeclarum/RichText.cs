using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Praeclarum;

using Praeclarum.Graphics;

namespace Praeclarum
{
	public interface IRichText
	{
//		string Text { get; }
		void AddAttributes (IRichTextAttributes styleClass, StringRange range);
	}

	public class RichText : IRichText
	{
		public string PlainText { get; private set; }

		readonly Classes[] classes;

		public int Length { get { return PlainText.Length; } }

		public RichText (string text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			PlainText = text;
			classes = new Classes[text.Length];
		}

		struct Classes
		{
			public RichTextAttributes Class1;
			public RichTextAttributes Class2;
			public RichTextAttributes Class3;
			public RichTextAttributes Class4;
		}

		public void AddAttributes (IRichTextAttributes istyleClass, StringRange range)
		{
			var end = range.End;

			var styleClass = (RichTextAttributes)istyleClass;

			for (var i = range.Location; i < end; i++) {

				var c = classes[i];

				if (c.Class1 == null)
					c.Class1 = styleClass;
				else if (c.Class2 == null)
					c.Class2 = styleClass;
				else if (c.Class3 == null)
					c.Class3 = styleClass;
				else if (c.Class4 == null)
					c.Class4 = styleClass;
				else
					throw new Exception ("Too much style");

				classes[i] = c;
			}
		}

		public string ToRtf (IEnumerable<IRichTextAttributes> classes)
		{
			var fg = from c in classes
					 where c.ForegroundColor != null
					 select c.ForegroundColor;
			var bg = from c in classes
					 where c.BackgroundColor != null
					 select c.BackgroundColor;
			var u  = from c in classes
					 where c.UnderlineColor != null
					 select c.UnderlineColor;

			var fonts = from c in classes
						where !string.IsNullOrEmpty (c.FontName)
						select c.FontName;

			return ToRtf (bg.Concat (fg).Concat (u), fonts);
		}

		string ToRtf (IEnumerable<Color> knownColors, IEnumerable<string> knownFonts)
		{
			var r = new StringBuilder ();

			var colorTableIndex = new Dictionary<int, int> ();
			var colorTable = new List<Color> ();
			foreach (var col in knownColors) {
				var coli = col.ToArgb ();
				if (!colorTableIndex.ContainsKey (coli)) {
					colorTable.Add (col);
					colorTableIndex.Add (coli, colorTableIndex.Count + 1);
				}
			}

			var fontTableIndex = new Dictionary<string, int> ();
			var fontTable = new List<string> ();
			foreach (var col in knownFonts) {
				if (!fontTableIndex.ContainsKey (col)) {
					fontTable.Add (col);
					fontTableIndex.Add (col, fontTableIndex.Count);
				}
			}

			r.Append (@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033");

			r.Append (@"{\fonttbl");
			foreach (var k in fontTableIndex)
				r.AppendFormat (@"{{\f{0}\fnil\fcharset0 {1};}}", k.Value, k.Key);
			r.AppendLine ("}");

			r.Append (@"{\colortbl ;");
			foreach (var c in colorTable)
				r.AppendFormat (@"\red{0}\green{1}\blue{2};", c.Red, c.Green, c.Blue);
			r.AppendLine ("}");

			r.AppendLine (@"{\*\generator Calca 1}\viewkind4\uc1");
			r.Append (@"\pard\sa0\sb0\sl0\slmult1\f0\fs22\lang9 ");

			AppendRtfBody (r, colorTableIndex, fontTableIndex);

			if (PlainText.Length > 0 && PlainText[PlainText.Length-1] == '\n')
				r.AppendLine (@"\par");

			r.AppendLine ("}");

			//Debug.WriteLine (r.ToString ());

			return r.ToString ();
		}

		void AppendRtfBody (StringBuilder r, Dictionary<int, int> colorTableIndex, Dictionary<string, int> fontTable)
		{
			var n = PlainText.Length;

			var pc = new Classes ();
			var inspan = false;

			var sc = new RichTextAttributes ("merged");

			for (var i = 0; i < n; i++) {

				var cs = classes[i];
				if (cs.Class1 != pc.Class1 || cs.Class2 != pc.Class2 || 
					cs.Class3 != pc.Class3 || cs.Class4 != pc.Class4) {

					if (inspan) {
						//r.Append (!string.IsNullOrEmpty (sc.Link) ? "}}}}}" : "}");
						r.Append ('}');
					}
					inspan = true;
					r.Append ('{');
					sc.Reset ();
					sc.Merge (cs.Class1);
					sc.Merge (cs.Class2);
					sc.Merge (cs.Class3);
					sc.Merge (cs.Class4);
					sc.AppendRtf (r, colorTableIndex, fontTable);
				}
				pc = cs;

				//
				// Write the char
				//
				var ch = PlainText[i];
				if (ch == '\\' || ch == '{' || ch == '}') {
					r.Append ('\\');
					r.Append (ch);
				}
				else if (ch == '\n') {
					r.AppendLine (@"\par");
				}
				else if (ch == '\r') {
					// Nothing
				}
				else {
					r.Append (ch);
				}				
			}

			if (inspan) {
				//r.Append (!string.IsNullOrEmpty (sc.Link) ? "}}}}}" : "}");
				r.Append ('}');
			}
		}
	}

	public interface IRichTextAttributes
	{
		string ClassName { get; set; }

		string FontName { get; set; }
		float FontSize { get; set; }

		Color ForegroundColor { get; set; }
		Color BackgroundColor { get; set; }

		UnderlineStyle UnderlineStyle { get; set; }
		Color UnderlineColor { get; set; }

		string Link { get; set; }
	}

	public class RichTextAttributes : IRichTextAttributes
	{
		public string ClassName { get; set; }

		public string FontName { get; set; }
		public float FontSize { get; set; }

		public Color ForegroundColor { get; set; }
		public Color BackgroundColor { get; set; }

		public UnderlineStyle UnderlineStyle { get; set; }
		public Color UnderlineColor { get; set; }

		public string Link { get; set; }

		public RichTextAttributes (string name)
		{
			ClassName = name;
			Reset ();
		}

		public void Reset ()
		{
			FontSize = 0.0f;
			FontName = null;
			ForegroundColor = null;
			BackgroundColor = null;
			Link = null;
			UnderlineStyle = UnderlineStyle.None;
		}

		public void Merge (IRichTextAttributes c)
		{
			if (c == null) return;
			if (c.FontSize > 1)
				FontSize = c.FontSize;
			if (c.FontName != null && c.FontName.Length > 0)
				FontName = c.FontName;
			if (c.ForegroundColor != null)
				ForegroundColor = c.ForegroundColor;
			if (c.BackgroundColor != null)
				BackgroundColor = c.BackgroundColor;
			if (!string.IsNullOrEmpty (c.Link))
				Link = c.Link;
			if (c.UnderlineStyle != UnderlineStyle.None) {
				UnderlineStyle = c.UnderlineStyle;
				UnderlineColor = c.UnderlineColor;
			}
		}

		public void AppendRtf (StringBuilder r, Dictionary<int, int> colorTable, Dictionary<string, int> fontTable)
		{
			var a = false;

			if (FontName != null && FontName.Length > 0) {
				var ci = fontTable[FontName];
				r.AppendFormat (@"\f{0}", ci);
				a = true;
			}
			if (FontSize > 1) {
				r.AppendFormat (@"\fs{0}", (int)(2 * FontSize + 0.5f));
				a = true;
			}

			if (ForegroundColor != null) {
				var ci = colorTable[ForegroundColor.ToArgb ()];
				r.AppendFormat (@"\cf{0}", ci);
				a = true;
			}

			if (BackgroundColor != null) {
				var ci = colorTable[BackgroundColor.ToArgb ()];
				r.AppendFormat (@"\highlight{0}", ci);
				a = true;
			}

			if (UnderlineStyle != UnderlineStyle.None) {
				var ci = colorTable[UnderlineColor.ToArgb ()];
				r.AppendFormat (@"\ulc{0}\ulwave", ci);
				a = true;
			}

			if (!string.IsNullOrEmpty (Link)) {
				//r.AppendFormat (@"{{{{\field{{\*\fldinst{{HYPERLINK ""{0}"" }}}}{{\fldrslt{{\ul ", Link.Replace ('\"', ' '));
				r.Append (@"\ul");
				a = true;
			}

			if (a) {
				r.Append (' ');
			}
		}
	}

	public enum UnderlineStyle
	{
		None,
		Single,
		Thick,
	}
}
