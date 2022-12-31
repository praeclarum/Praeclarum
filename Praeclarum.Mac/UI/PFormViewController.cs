#nullable enable

using System;
using AppKit;
using Foundation;
using System.Collections.Generic;
using CoreGraphics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;

namespace Praeclarum.UI
{
	public partial class PForm : NSViewController, IThemeAware
	{
		readonly NSStackView sectionsStack = new NSStackView
		{
			Orientation = NSUserInterfaceLayoutOrientation.Vertical,
			Alignment = NSLayoutAttribute.Leading,
		};

		readonly Dictionary<PFormSection, PFormSectionView> sectionViews = new();

		public PForm(string title = "")
			: base(nibNameOrNull: null, nibBundleOrNull: null)
		{
			Title = (title ?? "").Localize ();

			sections = new ObservableCollection<PFormSection> ();
			sections.CollectionChanged += HandleSectionsChanged;
		}

		public override void LoadView ()
		{
			View = sectionsStack;
			ReloadAll();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			//sectionsStack.Frame = this.View.Bounds;
			//sectionsStack.TranslatesAutoresizingMaskIntoConstraints = true;
			//this.View.AddSubview(sectionsStack);
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

		public void PushForm (PForm f)
		{
		}

		public void ReloadAll ()
		{
			var visibleSectionViews = new List<PFormSectionView> (sectionsStack.ArrangedSubviews.OfType<PFormSectionView> ());
			visibleSectionViews.MergeInto(sections,
				(s, d) => s.Section.Equals(d),
				(s =>
				{
					var d = new PFormSectionView(s);
					sectionViews.Add(s, d);
					return d;
				}),
				(s, d) => s.ReloadSection(),
				d => { });
			sectionsStack.SetViews(visibleSectionViews.ToArray(), NSStackViewGravity.Top);
		}

		public void ReloadSection (PFormSection section)
		{
			if (sectionViews.TryGetValue(section, out var sv))
			{
				sv.ReloadSection();
			}
		}

		public void FormatSection (PFormSection section)
		{
			if (sectionViews.TryGetValue (section, out var sv))
			{
				sv.FormatSection ();
			}
		}

		public void ApplyTheme (Theme theme)
		{
			foreach (var ssv in sectionViews)
			{
				ssv.Value.ApplyTheme(theme);
			}
		}

		public static void ShowWindow(NSObject? sender, ref NSWindow? window, Func<PForm> createForm)
		{
			if (window is NSWindow w)
			{
				w.OrderFront(sender);
			}
			else
			{
				var form = createForm();
				w = new PFormWindow(form);
				window = w;
				w.MakeKeyAndOrderFront(sender);
			}
		}

		public override CGSize PreferredContentSize {
			get => new CGSize(320.0, 480.0);
			set { } }
	}

	class PFormSectionView : NSStackView, IThemeAware
	{
		readonly NSTextField titleLabel;
		readonly NSTextField hintLabel;

		public PFormSection Section { get; }

		public PFormSectionView(PFormSection section)
		{
			Section = section;
			
			Orientation = NSUserInterfaceLayoutOrientation.Vertical;
			Alignment = NSLayoutAttribute.Leading;

			titleLabel = NSTextField.CreateLabel (section.Title);
			titleLabel.Font = NSFont.BoldSystemFontOfSize(NSFont.SystemFontSizeForControlSize(NSControlSize.Large));
			hintLabel = NSTextField.CreateWrappingLabel (section.Hint);
			hintLabel.Font = NSFont.SystemFontOfSize(NSFont.SystemFontSizeForControlSize(NSControlSize.Regular));

			AddView(titleLabel, NSStackViewGravity.Top);
			AddView(hintLabel, NSStackViewGravity.Top);
			ReloadSection();
		}

		public void ReloadSection ()
		{
			titleLabel.StringValue = Section.Title;
			hintLabel.StringValue = Section.Hint;

			var visibleItemViews = new List<PFormItemView>(ArrangedSubviews.OfType<PFormItemView>());
			visibleItemViews.MergeInto(Section.Items,
				(s, d) => s.Item.Equals(d),
				s => new PFormItemView(s, Section),
				(s, d) => s.ReloadItem(),
				d => { });
			var visibleViews = new List<NSView>(visibleItemViews);
			visibleViews.Insert(0, titleLabel);
			visibleViews.Add(hintLabel);
			SetViews(visibleViews.ToArray(), NSStackViewGravity.Top);
		}

		public void FormatSection ()
		{
		}

		public void ApplyTheme (Theme theme)
		{
		}
	}

	class PFormItemView : NSStackView, IThemeAware
	{
		public object Item { get; }
		public PFormSection Section { get; }

		NSButton? button;

		public PFormItemView(object item, PFormSection section)
		{
			Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
			Item = item;
			Section = section;
			ReloadItem();
		}
		public void ReloadItem()
		{
			var display = Section.GetItemDisplay(Item);
			var title = Section.GetItemTitle(Item);
			var details = Section.GetItemDetails(Item);
			var navigates = Section.GetItemNavigates(Item);

			if (button is { } b)
			{
			}
			else
			{
				b = new NSButton();
				button = b;
				AddView(button, NSStackViewGravity.Leading);
			}
			b.Title = title;
		}
		public void FormatItem ()
		{
		}
		public void ApplyTheme (Theme theme)
		{
		}
	}

	class PFormWindow : NSWindow
	{
		private readonly PForm form;

		public PFormWindow(PForm form)
			: base(new CGRect(CGPoint.Empty, form.PreferredContentSize), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, deferCreation: false)
		{
			this.form = form;
			this.ContentViewController = form;
		}
	}
}
