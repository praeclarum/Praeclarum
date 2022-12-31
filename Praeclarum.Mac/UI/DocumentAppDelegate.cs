using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Praeclarum.IO;
using System.IO;
using System.Globalization;
using System.Linq;
using Praeclarum.App;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Praeclarum.UI
{
	[Register("DocumentAppDelegate")]
	public class DocumentAppDelegate : NSApplicationDelegate
	{
		public DocumentApplication App { get; protected set; }

		public static DocumentAppDelegate Shared { get; private set; }

		public IDocumentAppSettings Settings
		{
			get;
			private set;
		}

		protected virtual IDocumentAppSettings CreateSettings()
		{
			return new DocumentAppSettings(NSUserDefaults.StandardUserDefaults);
		}

		public virtual IEnumerable<(int Months, string Name)> GetProPrices ()
		{
			return Enumerable.Empty<ValueTuple<int, string>> ();
		}
	}
}
