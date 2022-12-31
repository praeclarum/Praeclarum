#nullable enable

using System;
using Praeclarum.UI;
using System.Collections.Generic;
using System.Linq;
using Praeclarum.Graphics;

namespace Praeclarum.App
{
	public class Application
	{
		public virtual string Name { get { return "App"; } }
		public virtual string UrlScheme { get { return "app"; } }
		public virtual Color TintColor { get { return Colors.Blue; } }
		public virtual Color VibrantTintColor { get { return Colors.Blue; } }
		public virtual string ProSymbol { get { return "🔷"; } }
		public virtual string ProMarketing { get { return "Upgrade to Pro"; } }
		public virtual IEnumerable<(int Months, string Name)> GetProPrices () => Enumerable.Empty<ValueTuple<int, string>> ();
		public virtual string? AppGroup { get { return null; } }
	}
}

