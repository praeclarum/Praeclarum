using System;
using System.Collections.Generic;
using Foundation;
using System.IO;
using System.Linq;

namespace Praeclarum.UI
{
	public class TranslateSection : PFormSection
	{
		class Contrib
		{
			public string Name;
			public string Language;
		}

		readonly List<Contrib> contribs = new List<Contrib> ();

		public TranslateSection ()
			: base ("Use English", new Command ("Help Translate"))
		{
			try {
				var path = NSBundle.MainBundle.PathForResource ("LanguageCredits", "txt");
				var q = from line in File.ReadAllLines (path)
						let tline = line.Trim ()
						where tline.Length > 0
						let parts = tline.Split ('/')
						where parts.Length == 2
						select new Contrib { Name = parts[0].Trim (), Language = parts[1].Trim () };
				contribs.AddRange (q);
			}
			catch (Exception ex) {
				Log.Error (ex);
			}			
			Hint = "iCircuit is translated into several languages thanks to volunteers.".Localize ();
		}

		public override bool GetItemNavigates (object item)
		{
			return "Use English" != item.ToString ();
		}

		public override bool GetItemChecked (object item)
		{
			if ("Use English" == item.ToString ()) {
				return DocumentAppDelegate.Shared.Settings.UseEnglish;
			}
			return false;
		}

		public override bool SelectItem (object item)
		{
			if (item.ToString () == "Use English") {
				DocumentAppDelegate.Shared.Settings.UseEnglish = !DocumentAppDelegate.Shared.Settings.UseEnglish;
				SetNeedsReloadAll ();
			}
			else {
				var f = new TranslateForm (contribs);
				f.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Done, (s, e) => {
					if (f != null && f.PresentingViewController != null) {
						f.DismissViewController (true, null);
					}
				});
				if (this.Form.NavigationController != null) {
					this.Form.NavigationController.PushViewController (f, true);
				}
			}
			return false;
		}

		class TranslateForm : PForm
		{
			public TranslateForm (List<Contrib> contribs) : base ("Help Translate".Localize ())
			{
				var github = new PFormSection {
					Hint = "GitHub is used to coordinate the translation effort.".Localize (),
					Items = { new OpenUrlCommand ("Translations on Github", "https://github.com/praeclarum/CircuitTranslations") },
				};

				var people = new PFormSection {
					Title = "Contributors".Localize (),
					Hint = "Thank you for your help!".Localize ()
				};
				foreach (var c in contribs) {
					people.Items.Add (c.Name);
				}

				Sections.Add (github);
				Sections.Add (people);
			}
		}
	}
}
