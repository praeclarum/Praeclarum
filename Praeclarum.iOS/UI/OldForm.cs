using System;
using System.Collections.ObjectModel;
using UIKit;
using System.Linq;

namespace Praeclarum.UI
{
	public class Form : ObservableCollection<FormElement>
	{
		public string Title { get; set; }
		public Form ()
		{
			Title = "";
		}
	}

	public class FormElement
	{
		public string Hint { get; set; }

		public FormElement ()
		{
			Hint = "";
		}
	}

	public class FormAction : FormElement
	{
		public string Title { get; set; }

		public event EventHandler Executed;

		public FormAction ()
		{
			Title = "";
		}

		public FormAction (string title)
		{
			Title = title;
		}

		public FormAction (string title, Action action)
		{
			Title = title;
			Executed += (s, e) => action ();
		}

		public void Execute ()
		{
			OnExecuted ();
		}

		protected virtual void OnExecuted ()
		{
			var ev = Executed;
			if (ev != null)
				ev (this, EventArgs.Empty);
		}
	}

	public static partial class FormUI
	{
		public static UIActionSheet ToActionSheet (this Form form)
		{
			var actions = form.OfType<FormAction> ().ToList ();

			var ActionSheet = new UIActionSheet (form.Title);
			foreach (var a in actions) {
				ActionSheet.Add (a.Title);
			}

			ActionSheet.CancelButtonIndex = ActionSheet.ButtonCount - 1;

			ActionSheet.Clicked += (ss, se) => {
				var index = (int)se.ButtonIndex;
				if (0 <= index && index < actions.Count) {
					actions[index].Execute ();
				}
			};

			return ActionSheet;
		}

		public static UIActionSheet ToActionSheet (this PForm form)
		{
			var q = from s in form.Sections
					from i in s.Items
					let c = i as Command
					where c != null
					select c;
			var actions = q.ToList ();

			var ActionSheet = new UIActionSheet (form.Title);
			foreach (var a in actions) {
				ActionSheet.Add (a.Name);
			}

			ActionSheet.CancelButtonIndex = ActionSheet.ButtonCount - 1;

			ActionSheet.Clicked += async (ss, se) => {
				var index = (int)se.ButtonIndex;
				if (0 <= index && index < actions.Count) {
					try {
						await actions[index].ExecuteAsync ();						
					} catch (Exception ex) {
						Console.WriteLine ("Failed to execute action: " + ex);
					}
				}
			};

			return ActionSheet;
		}
	}
}

