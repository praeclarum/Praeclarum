using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Praeclarum
{
	public class HtmlRichText : IRichText
	{
		readonly string text;
		readonly List<Span>[] spans;

		public bool ForPresentation { get; set; }

		class Span
		{
			public readonly StringRange Range;
			public string Classes;
			public string Url = null;
			public bool IsPre = false;
			public bool IsHidden { get { return Classes.Contains ("hidden"); } }
			public bool IsCode { get { return Classes == "code"; } }
			public bool IsXml { get { return Classes.Contains ("xml"); } }
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
					if (Classes == "a" || 
						Classes == "h1" || Classes == "h2" || Classes == "h3" || Classes == "h4" || 
						Classes == "code" || 
						Classes == "strong" || Classes == "em" ||
						Classes == "body") {
						return Classes;
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
					var name = Name;
					if (name == "span") {
						sb.Append ("<span class=\"");
						sb.Append (Classes);
						sb.Append ("\">");
					} else if (name == "body") {
						// Nothing
					} else {
						sb.Append ("<");
						sb.Append (name);
						sb.Append (">");
					}
				}
			}
			public void WriteEnd (StringBuilder sb)
			{
				var name = Name;
				if (name == "body") {
					// Nothing
				} else {
					sb.Append ("</");
					sb.Append (Name);
					sb.Append (">");
				}
			}
		}

		public HtmlRichText (string text)
		{
			this.text = text;
			this.spans = new List<Span>[text.Length];
		}

        static readonly Regex preCodeBrRepl = new Regex (@"<pre><code>\s*<br/>\s*</code></pre>");

		public string Html {
			get {
				var sb = new StringBuilder ();
				OutputHtml (sb);
				var raw = sb.ToString ();

				if (ForPresentation) {
                    var html = raw
                        .Replace ("<?", "<");

					return
                        preCodeBrRepl.Replace (html, "<p>")
						.Replace ("<p><h1>", "\n<h1>")
						.Replace ("<p><h2>", "\n<h2>")
						.Replace ("<p><h3>", "\n<h3>")
						.Replace ("<p><h4>", "\n<h4>")
						.Replace ("<br/><h1>", "\n<h1>")
						.Replace ("<br/><h2>", "\n<h2>")
						.Replace ("<br/><h3>", "\n<h3>")
						.Replace ("<br/><h4>", "\n<h4>")
						.Replace ("<br/></h1>", "</h1>\n")
						.Replace ("<br/></h2>", "</h2>\n")
						.Replace ("<br/></h3>", "</h3>\n")
						.Replace ("<br/></h4>", "</h4>\n")
						.Replace ("<br/>", "\n")
						.Replace ("<p>", "\n<p>")
						;
				} else {
					return raw;
				}
			}
		}

		void OutputHtml (StringBuilder sb)
		{
			var hidden = 0;
			var literal = 0;
			var newline = true;
			var stack = new List<Span> ();
			for (var i = 0; i < text.Length; i++) {

				//
				// Ends
				//
				if (stack.Count > 0) {
					var changed = true;
					while (changed) {
						changed = false;
						for (int j = stack.Count - 1; j >= 0; j--) {
							var s = stack [j];
							if (s.Range.End == i) {
								if (hidden == 0 && literal == 0) {
									s.WriteEnd (sb);
									if (s.IsPre) {
										sb.Append ("</pre>");
									}
								}
								if (ForPresentation) {
									if (s.IsHidden) {
										hidden--;
									}
									if (s.IsXml) {
										literal--;
									}
								}
								stack.RemoveAt (j);
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
						var s = starts [0];
						if (ForPresentation) {
							if (s.IsHidden) {
								hidden++;
							}
							if (s.IsXml) {
								literal++;
							}
						}
						if (hidden == 0 && literal == 0) {
							if (s.IsCode && newline) {
								s.IsPre = true;
								sb.Append ("<pre>");
							}
							s.WriteStart (sb);
						}
						stack.Add (s);
					} else {
						foreach (var s in starts) {
							if (ForPresentation) {
								if (s.IsHidden) {
									hidden++;
								}
								if (s.IsXml) {
									literal++;
								}
							}
							if (hidden == 0 && literal == 0) {
								if (s.IsCode && newline) {
									s.IsPre = true;
									sb.Append ("<pre>");
								}
								s.WriteStart (sb);
							}
							stack.Add (s);
						}
					}
				}

				newline = false;

				if (hidden == 0) {
					if (literal > 0) {
						sb.Append (text [i]);
					} else {
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
							newline = true;
							sb.Append ("<br/>");
							break;
						default:
							sb.Append (text [i]);
							break;
						}
					}
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

