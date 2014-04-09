using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Threading.Tasks;

namespace Praeclarum.UI
{
	public class TextInputController : UITableViewController
	{
		public string LabelText { get; set; }
		public string InputText { get; set; }
		public string Hint { get; set; }
		public bool CancelOnBlank { get; set; }

		public event EventHandler Cancelled = delegate {};
		public event EventHandler Done = delegate {};

		public Func<string, Task<string>> ValidateFunc { get; set; }

		public TextInputController ()
			: base (UITableViewStyle.Grouped)
		{
			Title = "Input";

			LabelText = "";
			InputText = "";
			Hint = "";
			CancelOnBlank = true;

			if (!CancelOnBlank) {
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem (
					UIBarButtonSystemItem.Cancel,
					HandleCancel);
			}

//			NavigationItem.RightBarButtonItem = new UIBarButtonItem (
//				UIBarButtonSystemItem.Done,
//				HandleDone);
			DocumentAppDelegate.Shared.Theme.Apply (TableView);
//			TableView.Delegate = new TextInputDelegate (this);
			TableView.DataSource = new TextInputDataSource (this);
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.All;
		}
		
		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation ()
		{
			return UIInterfaceOrientation.Portrait;
		}
		
		[Obsolete ("Deprecated in iOS6. Replace it with both GetSupportedInterfaceOrientations and PreferredInterfaceOrientationForPresentation")]
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}

		void HandleCancel (object sender, EventArgs e)
		{
			Cancelled (this, EventArgs.Empty);
		}

		UIAlertView noAlert;

		async void HandleDone (object sender, EventArgs e)
		{
			await ValidateAndNotify ();
		}

		public async Task<bool> ValidateAndNotify ()
		{
			var inputField = ((TextInputCell)TableView.CellAt (NSIndexPath.FromRowSection (0, 0))).InputField;
			inputField.ResignFirstResponder ();

			InputText = inputField.Text.Trim ();

			if (string.IsNullOrWhiteSpace (InputText)) {

				if (CancelOnBlank) {
					Cancelled (this, EventArgs.Empty);
					return true;
				} else {
					noAlert = new UIAlertView ("Required", "You must enter some text to continue.", null, "OK");
					noAlert.Show ();
					return false;
				}

			} else {

				if (ValidateFunc != null) {
					var error = await ValidateFunc (InputText);
					if (error != null) {
						noAlert = new UIAlertView ("", error, null, "OK");
						noAlert.Show ();
						return false;
					}
				}

				Done (this, EventArgs.Empty);
				return true;
			}
		}

//		class TextInputDelegate : UITableViewDelegate
//		{
//			TextInputController controller;
//				
//			public TextInputDelegate (TextInputController controller)
//			{
//				this.controller = controller;
//			}
//				
//			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
//			{
//			}
//		}
			
		class TextInputDataSource : UITableViewDataSource
		{
			TextInputController controller;
			TextInputCell cell;
				
			public TextInputDataSource (TextInputController controller)
			{
				this.controller = controller;
			}
				
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
				
			public override int RowsInSection (UITableView tableView, int section)
			{
				return 1;
			}

			public override string TitleForFooter (UITableView tableView, int section)
			{
				return controller.Hint;
			}
				
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (cell == null) {
					cell = new TextInputCell (
						"C", 
						controller.LabelText, 
						controller.ValidateAndNotify);
					var i = cell.InputField;
					i.Placeholder = controller.InputText;
					i.AccessibilityLabel = controller.Title;
					i.BecomeFirstResponder ();
				}
					
				return cell;
			}
		}
	}

	public class TextInputCell : UITableViewCell
	{
		public readonly UITextField InputField;
		
		public TextInputCell (string reuseId, string labelText, Func<Task<bool>> done)
			: base (UITableViewCellStyle.Default, reuseId)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			
			TextLabel.Text = labelText;
			
			InputField = new UITextField () {
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
				BackgroundColor = UIColor.Clear,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				TextColor = UIColor.Black,
				TextAlignment = UITextAlignment.Center,
				ClearButtonMode = UITextFieldViewMode.WhileEditing,
				AutocorrectionType = UITextAutocorrectionType.Default,
				AutocapitalizationType = UITextAutocapitalizationType.Sentences,
				ReturnKeyType = UIReturnKeyType.Done,
				ShouldReturn = tf => {
					done ().ContinueWith (ta => {
						if (ta.IsCompleted && !ta.Result)
							tf.BecomeFirstResponder ();
					}, TaskScheduler.FromCurrentSynchronizationContext ());
					return false;
				},
			};
			
//			InputField.BecomeFirstResponder ();
			
			ContentView.AddSubview (InputField);
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			var b = ContentView.Bounds;

			var text = TextLabel.Text;
			if (!string.IsNullOrEmpty (text)) {
				var lw = TextLabel.StringSize (TextLabel.Text, TextLabel.Font).Width;
			
				b.Width -= lw + 33;
				b.X += lw + 33;
			} else {
				b.Width -= 44;
				b.X += 22;
			}
			
			InputField.Frame = b;
		}
	}
}

