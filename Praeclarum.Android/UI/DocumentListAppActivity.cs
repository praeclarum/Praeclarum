using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Praeclarum.UI
{
	[Activity (Label = "DocumentListAppActivity")]			
	public class DocumentListAppActivity : Activity
	{
		public static DocumentListAppActivity Shared { get; private set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Shared = this;

			// Create your application here
		}
	}
}

