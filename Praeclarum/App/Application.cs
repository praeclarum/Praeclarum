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
		public virtual IEnumerable<ProPriceSpec> GetProPrices () => Enumerable.Empty<ProPriceSpec> ();
		public virtual string? AppGroup { get { return null; } }
		public virtual string? CloudKitContainerId => null;
	}
}

