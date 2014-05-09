using System;
using Praeclarum.UI;
using System.Collections.Generic;
using Praeclarum.Graphics;
using System.Linq;

namespace Praeclarum.App
{
	public abstract class DocumentApplication : Application
	{
		public virtual string AutoOpenDocumentPath { get { return ""; } }

		public virtual IDocument CreateDocument (string localFilePath)
		{
			return null;
		}

		public virtual IDocumentEditor CreateDocumentEditor (int docIndex, List<DocumentReference> docs)
		{
			return null;
		}

		public virtual IEnumerable<string> FileExtensions
		{
			get {
				yield return "txt";
			}
		}

		public virtual string DefaultExtension { get { return FileExtensions.First (); } }

		public virtual string DocumentBaseName { get { return "Document"; } }
		public virtual string DocumentBaseNamePluralized { get { return Pluralize (DocumentBaseName); } }

		public virtual float ThumbnailAspectRatio { get { return 8.5f/11.0f; } }

		public virtual void DrawThumbnail (IDocument doc, IGraphics g, SizeF size)
		{
			g.SetColor (Colors.White);
			g.FillRect (new RectangleF (PointF.Empty, size));
		}

		public virtual Color ThumbnailBackgroundColor {
			get {
				return Colors.White;
			}
		}

		static string Pluralize (string word)
		{
			if (string.IsNullOrEmpty (word))
				return "s";

			var last = word [word.Length - 1];

			if (last == 'y') {
				if (word.Length <= 1)
					return "ies";
				var prev = word [word.Length - 2];
				if (prev == 'a' || prev == 'e' || prev == 'i' || prev == 'o')
					return word + "s";
				if (prev == 'u' && !word.EndsWith ("quy", StringComparison.Ordinal))
					return word + "s";
				return word.Substring (0, word.Length - 1) + "ies";
			}

			if (last == 'o' ||
				last == 's' ||
				last == 'z' ||
			    word.EndsWith ("sh", StringComparison.Ordinal) ||
			    word.EndsWith ("ch", StringComparison.Ordinal))
				return word + "es";

			return word + "s";
		}
	}
}
