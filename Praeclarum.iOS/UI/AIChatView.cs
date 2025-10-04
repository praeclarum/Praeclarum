// ReSharper disable UseSymbolAlias
#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;

// ReSharper disable once CheckNamespace
namespace Praeclarum.UI;

// ReSharper disable once InconsistentNaming
public class AIChatView : UIView
{
	static readonly NFloat minInputBoxHeight = 56;
	static readonly NFloat maxInputBoxHeight = 168;
	static readonly NFloat inputBoxVPadding = 7;
	static readonly NFloat inputBoxHPadding = 11;
	
	readonly UITableView _tableView;
	readonly UIView _inputBox = new() { BackgroundColor = UIColor.Red, };
	readonly UITextView _inputField = new ();
	readonly ChatSource _chatSource;
	readonly UIFont _inputFieldFont = UIFont.SystemFontOfSize (UIFont.SystemFontSize);

	NFloat _inputBoxHeight = minInputBoxHeight;
	
	bool _isSubmitting = false;

    public AIChatView (CGRect frame)
        : base (frame)
    {
	    _chatSource = new ChatSource();
	    _tableView = new UITableView (frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    public AIChatView (NativeHandle handle)
        : base (handle)
    {
	    _chatSource = new ChatSource();
	    _tableView = new UITableView (base.Frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    void Initialize ()
    {
	    var bounds = base.Bounds;
	    
	    BackgroundColor = UIColor.Yellow;

	    _tableView.BackgroundView = new UIView (bounds) { BackgroundColor = UIColor.Blue, };
	    _tableView.TranslatesAutoresizingMaskIntoConstraints = false;
	    _tableView.Source = _chatSource;
	    
	    var iframe = new CGRect (0, bounds.Height - minInputBoxHeight, bounds.Width, minInputBoxHeight);
	    _inputBox.Frame = iframe;
	    _inputBox.TranslatesAutoresizingMaskIntoConstraints = false;

	    var fframe = new CGRect (inputBoxHPadding, inputBoxVPadding, iframe.Width - 2*inputBoxHPadding, iframe.Height - 2*inputBoxVPadding);
	    _inputField.Frame = fframe;
	    _inputField.BackgroundColor = UIColor.Green;
	    _inputField.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
	    _inputField.TranslatesAutoresizingMaskIntoConstraints = true;
	    _inputField.Font = _inputFieldFont;
	    _inputField.ShouldChangeText = ShouldChangeText;
	    _inputField.Changed += InputFieldOnValueChanged;
	    _inputBox.AddSubview (_inputField);
	    
	    AddSubview (_tableView);
	    AddSubview (_inputBox);
	    base.SetNeedsLayout ();
    }

    private bool ShouldChangeText (UITextView textView, NSRange range, string text)
    {
	    if (_isSubmitting)
		    return false;
	    if (text == "\n")
	    {
		    _isSubmitting = true;
		    SubmitInputAsync ().ContinueWith (Log.TaskError);
		    return false;
	    }

	    return true;
    }

    private void InputFieldOnValueChanged (object? sender, EventArgs e)
    {
	    AdjustInputBoxHeight ();
    }

    public override void LayoutSubviews ()
    {
	    base.LayoutSubviews ();

	    var bounds = this.Bounds;
	    
	    var tframe = new CGRect (0, 0, bounds.Width, bounds.Height - _inputBoxHeight);
	    _tableView.Frame = tframe;
	    
	    var iframe = new CGRect (0, bounds.Height - _inputBoxHeight, bounds.Width, _inputBoxHeight);
	    _inputBox.Frame = iframe;
    }

    void AdjustInputBoxHeight ()
    {
	    if (_inputField.Text is not { } text)
		    return;
	    var newInputBoxHeight = minInputBoxHeight;
	    if (!string.IsNullOrEmpty (text))
	    {
		    var width = _inputField.Frame.Width;
		    var lineSize = text.StringSize (_inputFieldFont);
		    var lines = (int)Math.Ceiling (lineSize.Width / width);
		    var minTextHeight = lines * lineSize.Height;
		    var newTextHeight = minTextHeight * 1.4 + 18 + 2 * inputBoxVPadding;
		    newInputBoxHeight = (NFloat)Math.Min (maxInputBoxHeight, Math.Max (minInputBoxHeight, newTextHeight));
			Console.WriteLine ($"Adjust Text ({text.Length}) size {lineSize}: {lines} * {lineSize.Height} = {minTextHeight} => {newInputBoxHeight}");
	    }
	    if (Math.Abs (newInputBoxHeight - _inputBoxHeight) > 1.0f)
	    {
		    _inputBoxHeight = newInputBoxHeight;
		    SetNeedsLayout ();
	    }
	}
    
    async Task SubmitInputAsync ()
    {
	    _isSubmitting = true;
	    try
	    {
		    var prompt = _inputField.Text;
		    if (!string.IsNullOrWhiteSpace (prompt))
		    {
			    await Task.Delay (1);
			    _inputField.Text = "";
			    AdjustInputBoxHeight ();
			    await _chatSource.AddPromptAsync (prompt, tableView: _tableView);
		    }
	    }
	    finally
	    {
		    _isSubmitting = false;
	    }
    }

	class ChatSource : UITableViewSource
	{
		List<Chat> Chats { get; } = [new ()];
		private int ActiveChatIndex { get; } = 0;
		Chat ActiveChat => Chats[ActiveChatIndex];

		public override IntPtr NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override IntPtr RowsInSection (UITableView tableView, IntPtr section)
		{
			var chat = ActiveChat;
			return (IntPtr)chat.Messages.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("C") as ChatCell ?? new ChatCell ("C");
			var chat = ActiveChat;
			var message = chat.Messages[indexPath.Row];
			cell.MessageText = message.Text;
			return cell;
		}

		public async Task AddPromptAsync (string prompt, UITableView tableView)
		{
			var chat = ActiveChat;
			chat.Messages.Add (new Message { Text = prompt, Type = MessageType.User });
			var indexPath = NSIndexPath.FromRowSection (chat.Messages.Count - 1, 0);
			tableView.InsertRows ([indexPath], UITableViewRowAnimation.Automatic);
			await Task.Delay (1);
			tableView.ScrollToRow (indexPath, UITableViewScrollPosition.Bottom, true);
			await Task.Delay (1);
		}
	}
	
	class ChatCell : UITableViewCell
	{
		public string MessageText
		{
			get => TextLabel.Text ?? "";
			set => TextLabel.Text = value;
		}
		public ChatCell (string reuseIdentifier) : base (UITableViewCellStyle.Default, reuseIdentifier)
		{
			base.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
		}
	}
	
	class Message
	{
		public string Text { get; set; } = "";
		public MessageType Type { get; set; } = MessageType.Assistant;
		public bool IsSystem => Type == MessageType.System;
		public bool IsUser => Type == MessageType.User;
		public bool IsAssistant => Type == MessageType.Assistant;
		public bool IsError => Type == MessageType.Error;
	}

	enum MessageType
	{
		System,
		User,
		Assistant,
		Error
	}

	class Chat
	{
		public List<Message> Messages { get; }

		public Chat ()
		{
			Messages = new();
		}
	}
}
