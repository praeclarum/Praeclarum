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
		readonly NSScrollView backView = new NSScrollView();
		readonly NSView formFooterView = new NSView();

		readonly PFormSectionsView sectionsStack = new PFormSectionsView
		{
			Orientation = NSUserInterfaceLayoutOrientation.Vertical,
			Alignment = NSLayoutAttribute.Leading,
			HasEqualSpacing = true,
			Spacing = 22.0f,
			EdgeInsets = new NSEdgeInsets(22, 22, 22, 22),
		};

		readonly Dictionary<PFormSection, PFormSectionView> sectionViews = new();

		public PForm (string title = "")
			: base(nibNameOrNull: null, nibBundleOrNull: null)
		{
			Title = (title ?? "").Localize ();

			sections = new ObservableCollection<PFormSection> ();
			sections.CollectionChanged += HandleSectionsChanged;
		}

		public override void LoadView ()
		{
			var frame = new CGRect(0, 0, 512, 512);
			backView.Frame = frame;
			backView.HasVerticalScroller = true;

			sectionsStack.TranslatesAutoresizingMaskIntoConstraints = false;
			backView.DocumentView = sectionsStack;
			backView.AddConstraints(new[] {
				NSLayoutConstraint.Create(backView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, sectionsStack, NSLayoutAttribute.Width, 1, 22),
			});

			View = backView;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			ReloadAll();
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
			//Console.WriteLine($"ALERT {title}: {message}");
			BeginInvokeOnMainThread(() =>
			{
				var alert = NSAlert.WithMessage(title, "OK", null, null, message);
				alert.RunModal();
			});
		}

		public void PushForm (PForm f)
		{
			NSWindow? w = null;
			ShowWindow(this, ref w, () => f);
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
			var visibleViews = new List<NSView>(visibleSectionViews);
			visibleViews.Add(formFooterView);
			sectionsStack.SetViews(visibleViews.ToArray(), NSStackViewGravity.Top);
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
			}
			else
			{
				var form = createForm();
				w = NSWindow.GetWindowWithContentViewController (form);
				window = w;
			}
			w.MakeKeyAndOrderFront(sender);
		}
	}

	class PFormSectionsView : NSStackView
	{
		public override bool IsFlipped => true;
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
			Distribution = NSStackViewDistribution.FillEqually;

			titleLabel = NSTextField.CreateLabel (section.Title);
			if (NSFont.BoldSystemFontOfSize (NSFont.SystemFontSize * 1.25f) is { } titleFont)
			{
				titleLabel.Font = titleFont;
			}
			hintLabel = NSTextField.CreateWrappingLabel (section.Hint);
			if (NSFont.SystemFontOfSize(NSFont.SystemFontSize) is { } hintFont)
			{
				hintLabel.Font = hintFont;
			}

			ReloadSection();
		}

		public void ReloadSection()
		{
			var visibleItemViews = new List<PFormItemView>(ArrangedSubviews.OfType<PFormItemView>());
			visibleItemViews.MergeInto(Section.Items,
				(s, d) => s.Item.Equals(d),
				s => new PFormItemView(s, Section),
				(s, d) => s.ReloadItem(),
				d => { });

			var visibleViews = new List<NSView>(visibleItemViews);
			if (!string.IsNullOrEmpty(Section.Title))
			{
				titleLabel.StringValue = Section.Title;
				visibleViews.Insert(0, titleLabel);
			}
			if (!string.IsNullOrEmpty(Section.Hint))
			{
				hintLabel.StringValue = Section.Hint;
				visibleViews.Add(hintLabel);
			}
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
		NSTextField? valueLabel;

		static readonly NSStringAttributes valueAttrs = new NSStringAttributes
		{
			ForegroundColor = NSColor.SystemBlue,
			Font = NSFont.BoldSystemFontOfSize(NSFont.SystemFontSize),
			Shadow = new NSShadow
			{
				ShadowOffset = new CGSize(1, 1),
				ShadowColor = NSColor.FromWhite(0.0f, 0.5f),
				ShadowBlurRadius = 2,
			},
		};

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
				b.BezelStyle = NSBezelStyle.Rounded;
				b.Target = this;
				b.Action = new ObjCRuntime.Selector("tapItem:");
				button = b;
				AddView(button, NSStackViewGravity.Top);
			}
			var titleAS = new NSMutableAttributedString(title);
			if (display == PFormItemDisplay.TitleAndValue)
			{
				var valueAS = new NSAttributedString (" " + details, valueAttrs);
				if (valueLabel is not object)
				{
					valueLabel = NSTextField.CreateLabel(valueAS);
					AddView(valueLabel, NSStackViewGravity.Trailing);
				}
				else
				{
					valueLabel.AttributedStringValue = valueAS;
				}
			}
			b.AttributedTitle = titleAS;
		}
		public void FormatItem ()
		{
		}
		public void ApplyTheme (Theme theme)
		{
		}
		[Export("tapItem:")]
		public void TapItem(NSObject sender)
		{
			Section.SelectItem(Item);
		}
	}
}
