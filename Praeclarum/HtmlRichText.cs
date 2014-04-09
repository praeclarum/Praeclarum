using System;
using System.Collections.Generic;
using System.Text;

namespace Praeclarum
{
	public class HtmlRichText : IRichText
	{
		readonly string text;
		readonly List<Span>[] spans;

		class Span
		{
			public readonly StringRange Range;
			public string Classes;
			public string Url = null;
			public Span(StringRange range, string className)
			{
				Range = range;
				Classes = className;
			}
			public void AddClass (string cls)
			{
				if (Classes.Length == 0)
					Classes = cls;
				else
					Classes += " " + cls;
			}
			public string Name {
				get {
					if (Classes == "a") {
						return "a";
					} else {
						return "span";
					}
				}
			}
			public void WriteStart (StringBuilder sb)
			{
				if (Classes == "a" && Url != null) {
					sb.Append ("<a href=\"");
					sb.Append (Url);
					sb.Append ("\">");
				} else {
					sb.Append ("<span class=\"");
					sb.Append (Classes);
					sb.Append ("\">");
				}
			}
			public void WriteEnd (StringBuilder sb)
			{
				sb.Append ("</");
				sb.Append (Name);
				sb.Append (">");
			}
		}

		public HtmlRichText (string text)
		{
			this.text = text;
			this.spans = new List<Span>[text.Length];
		}

		public string Html {
			get {
				var sb = new StringBuilder ();
				OutputHtml (sb);
				return sb.ToString ();
			}
		}

		void OutputHtml (StringBuilder sb)
		{
			var stack = new List<Span> ();
			for (var i = 0; i < text.Length; i++) {

				//
				// Ends
				//
				if (stack.Count > 0) {
					var changed = true;
					while (changed) {
						changed = false;
						foreach (var s in stack) {
							if (s.Range.End == i) {
								s.WriteEnd (sb);
								stack.RemoveAt (stack.Count - 1);
								changed = true;
								break;
							}
						}
					}
				}

				//
				// Starts
				//
				var starts = spans [i];

				if (starts != null && starts.Count > 0) {
					if (starts.Count == 1) {
						starts[0].WriteStart (sb);
						stack.Add (starts[0]);
					} else {
						foreach (var s in starts) {
							s.WriteStart (sb);
							stack.Add (s);
						}
					}
				}

				switch (text [i]) {
				case '&':
					sb.Append ("&amp;");
					break;
				case '<':
					sb.Append ("&lt;");
					break;
				case '>':
					sb.Append ("&gt;");
					break;
				case '\n':
					sb.Append ("<br/>");
					break;
				default:
					sb.Append (text [i]);
					break;
				}

			}
			foreach (var s in stack) {
				s.WriteEnd (sb);
			}
		}

		public void AddAttributes (IRichTextAttributes styleClass, StringRange range)
		{
			if (range.Length == 0)
				return;

			var starts = spans [range.Location];

			//
			// If there already an identical span
			//
			Span span = null;
			if (starts != null) {
				foreach (var s in starts) {
					if (s.Range.Length == range.Length) {
						span = s;
						break;
					}
				}
				if (span != null) {
					span.AddClass (styleClass.ClassName);
					return;
				}
			}

			//
			// If not, we need to insert it
			//
			span = new Span (range, styleClass.ClassName);
			span.Url = styleClass.Link;
			if (starts == null) {
				starts = new List<Span> ();
				spans [range.Location] = starts;
				starts.Add (span);
				return;
			}

			//
			// Insert it before shorter spans
			//
			var i = 0;
			while (i < starts.Count && starts [i].Range.Length >= range.Length) {
				i++;
			}
			starts.Insert (i, span);
		}
	}
}

