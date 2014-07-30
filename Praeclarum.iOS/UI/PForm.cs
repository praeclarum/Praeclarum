using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Praeclarum.UI
{
	public partial class PForm
	{
		ObservableCollection<PFormSection> sections;
		public IList<PFormSection> Sections { get { return sections; } }

		public PForm (string title = "")
			: base (MonoTouch.UIKit.UITableViewStyle.Grouped)
		{
			Title = title ?? "";

			sections = new ObservableCollection<PFormSection> ();
			sections.CollectionChanged += HandleSectionsChanged;

			InitializeUI ();
		}

		partial void InitializeUI ();

		void HandleSectionsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
				foreach (PFormSection o in e.OldItems)
					o.Form = null;
			if (e.NewItems != null)
				foreach (PFormSection o in e.NewItems)
					o.Form = this;
		}
	}

	public class PFormSection
	{
		public PForm Form { get; internal set; }

		public string Title { get; set; }
		public string Hint { get; set; }

		ObservableCollection<object> items;
		public IList<object> Items { get { return items; } }

		public PFormSection (params object[] items)
		{
			this.items = new ObservableCollection<object> ();
			foreach (var i in items)
				this.items.Add (i);
		}

		public virtual void Dismiss ()
		{
			Form = null;
		}

		public virtual void SetNeedsReload ()
		{
			if (Form != null)
				Form.ReloadSection (this);
		}

		public virtual void SetNeedsFormat ()
		{
			if (Form != null)
				Form.FormatSection (this);
		}

		public virtual bool GetItemEnabled (object item)
		{
			return true;
		}

		public virtual bool GetItemChecked (object item)
		{
			return false;
		}

		public virtual string GetItemImage (object item)
		{
			return "";
		}

		public virtual string GetItemTitle (object item)
		{
			var c = item as Command;
			if (c != null)
				return c.Name;

			return item.ToString ();
		}

		public virtual bool GetItemNavigates (object item)
		{
			return false;
		}

		public virtual bool SelectItem (object item)
		{
			var c = item as Command;
			if (c != null) {
				c.Execute ();
				return false;
			}

			return true;
		}
	}
}

