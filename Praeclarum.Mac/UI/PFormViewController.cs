#nullable enable

using System;
using AppKit;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Praeclarum.UI
{
	public partial class PForm : NSViewController, IThemeAware
	{
		public PForm(string title = "")
		{
			Title = (title ?? "").Localize ();

			sections = new ObservableCollection<PFormSection> ();
			sections.CollectionChanged += HandleSectionsChanged;
		}

		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			ViewWillAppear(true);
		}
		public virtual void ViewWillAppear(bool animated) { }

		public override void ViewWillDisappear ()
		{
			base.ViewWillDisappear ();
			ViewWillDisappear(true);
		}
		public virtual void ViewWillDisappear(bool animated) { }

		public void ShowAlert(string title, string message)
		{
		}

		public void ShowError (Exception ex)
		{
		}

		public void PushForm (PForm f)
		{
		}

		public void ReloadAll ()
		{
		}

		public void ReloadSection (PFormSection section)
		{
		}

		public void FormatSection (PFormSection section)
		{
		}

		public void ApplyTheme (Theme theme)
		{
		}
	}
}
