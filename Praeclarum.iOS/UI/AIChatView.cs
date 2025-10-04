// ReSharper disable UseSymbolAlias
#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Foundation;
using UIKit;
using CoreGraphics;

using CrossIntelligence;

using ObjCRuntime;

// ReSharper disable once CheckNamespace
namespace Praeclarum.UI;

// ReSharper disable once InconsistentNaming
public class AIChatView : UIView
{
	static readonly NFloat maxInputBoxHeight = 168;
	static readonly NFloat inputBoxVPadding = 11;
	static readonly NFloat inputBoxHPadding = 11;
	
#if __MACOS__
	static readonly bool ios13 = true;
#else
	static readonly bool ios13 = UIDevice.CurrentDevice.CheckSystemVersion (13, 0);
#endif
	
	readonly UITableView _tableView;
	readonly UIView _inputBox = new() { BackgroundColor = ios13 ? UIColor.SecondarySystemBackground : UIColor.Gray, };
	readonly UITextView _inputField = new ();
	readonly ChatSource _chatSource;
	readonly UIFont _inputFieldFont = UIFont.SystemFontOfSize (UIFont.SystemFontSize);

	NFloat _inputBoxHeight = 22;
	
	bool _isSubmitting = false;

	private IntelligenceSession? _session = null;

    public AIChatView (CGRect frame)
        : base (frame)
    {
	    _chatSource = new ChatSource (this);
	    _tableView = new UITableView (frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    public AIChatView (NativeHandle handle)
        : base (handle)
    {
	    _chatSource = new ChatSource (this);
	    _tableView = new UITableView (base.Frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    void Initialize ()
    {
	    var bounds = base.Bounds;
	    
	    BackgroundColor = UIColor.Clear;

	    _tableView.BackgroundView = new UIView (bounds) { BackgroundColor = UIColor.Clear, };
	    _tableView.TranslatesAutoresizingMaskIntoConstraints = false;
	    _tableView.Source = _chatSource;
	    
	    var iframe = new CGRect (0, bounds.Height - _inputBoxHeight, bounds.Width, _inputBoxHeight);
	    _inputBox.Frame = iframe;
	    _inputBox.TranslatesAutoresizingMaskIntoConstraints = false;

	    var fframe = new CGRect (inputBoxHPadding, inputBoxVPadding, iframe.Width - 2*inputBoxHPadding, iframe.Height - 2*inputBoxVPadding);
	    _inputField.Frame = fframe;
	    _inputField.BackgroundColor = UIColor.SystemBackground;
	    _inputField.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
	    _inputField.TranslatesAutoresizingMaskIntoConstraints = true;
	    _inputField.Font = _inputFieldFont;
#if !__MACOS__
	    _inputField.ShouldChangeText = ShouldChangeText;
#endif
	    _inputField.Changed += InputFieldOnValueChanged;
	    _inputBox.AddSubview (_inputField);
	    
	    AddSubview (_tableView);
	    AddSubview (_inputBox);
	    AdjustInputBoxHeight (animated: false);
    }

    public void SetFocus ()
    {
	    _inputField.BecomeFirstResponder ();
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
	    AdjustInputBoxHeight (animated: true);
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

    void AdjustInputBoxHeight (bool animated)
    {
	    if (_inputField.Text is not { } text)
		    return;
	    if (string.IsNullOrEmpty (text))
		    text = "M";
	    var width = _inputField.Frame.Width;
	    var lineSize = text.StringSize (_inputFieldFont);
	    var lines = Math.Max(1, (int)Math.Ceiling (lineSize.Width * 1.1 / width));
	    var minTextHeight = lines * lineSize.Height;
	    var newTextHeight = minTextHeight * 1.15 + 2 * inputBoxVPadding + 16;
	    var newInputBoxHeight = (NFloat)Math.Min (maxInputBoxHeight, newTextHeight);
		Console.WriteLine ($"Adjust Text ({text.Length}) size {lineSize}: {lines} * {lineSize.Height} = {minTextHeight} => {newInputBoxHeight}");
	    if (Math.Abs (newInputBoxHeight - _inputBoxHeight) > 1.0f)
	    {
		    _inputBoxHeight = newInputBoxHeight;
		    SetNeedsLayout ();
			if (animated)
		    	Animate (0.5, LayoutIfNeeded);
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
			    AdjustInputBoxHeight (animated: true);
			    await _chatSource.AddPromptAsync (GetSession (), prompt, tableView: _tableView);
		    }
	    }
	    finally
	    {
		    _isSubmitting = false;
	    }
    }

    IntelligenceSession GetSession ()
    {
	    if (_session is { } s)
		    return s;
	    var ns = new IntelligenceSession (IntelligenceModel.AppleIntelligence);
	    _session = ns;
	    return ns;
    }

	class ChatSource : UITableViewSource
	{
		private WeakReference<AIChatView> _chatView;
		List<Chat> Chats { get; } = [new ()];
		private int ActiveChatIndex { get; } = 0;
		Chat ActiveChat => Chats[ActiveChatIndex];

		public ChatSource (AIChatView chatView)
		{
			_chatView = new WeakReference<AIChatView> (chatView);
		}

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
			var cell = tableView.DequeueReusableCell ("M") as MessageCell ?? new MessageCell ("M");
			var chat = ActiveChat;
#if __MACOS__
			var message = chat.Messages[(int)indexPath.Item];
#else
			var message = chat.Messages[indexPath.Row];
#endif
			cell.Message = message;
			return cell;
		}

		async Task AddMessageAsync (Message message, UITableView tableView)
		{
			var chat = ActiveChat;
			chat.Messages.Add (message);
#if __MACOS__
			var indexPath = NSIndexPath.FromItemSection ((IntPtr)(chat.Messages.Count - 1), IntPtr.Zero);
#else
			var indexPath = NSIndexPath.FromRowSection (chat.Messages.Count - 1, 0);
#endif
			tableView.InsertRows ([indexPath], UITableViewRowAnimation.Automatic);
			await Task.Delay (1);
			tableView.ScrollToRow (indexPath, UITableViewScrollPosition.Bottom, true);
			await Task.Delay (1);
		}

		public async Task AddPromptAsync (IntelligenceSession session, string prompt, UITableView tableView)
		{
			await AddMessageAsync (new Message { Text = prompt, Type = MessageType.User }, tableView);
			try
			{
				var response = await session.RespondAsync (prompt);
				await AddMessageAsync (new Message { Text = response, Type = MessageType.Assistant }, tableView);
			}
			catch (Exception ex)
			{
				await AddMessageAsync (new Message { Text = ex.Message, Type = MessageType.Error }, tableView);
			}
		}
	}
	
	class MessageCell : UITableViewCell
	{
		private Message? _message;
		public Message? Message
		{
			get => _message;
			set
			{
				_message = value;
				if (value is { } msg)
				{
					base.TextLabel.Text = msg.Text;
					if (msg.IsUser)
					{
						base.TextLabel.TextAlignment = UITextAlignment.Right;
						base.TextLabel.TextColor = UIColor.SystemBlue;
					}
					else if (msg.IsAssistant)
					{
						base.TextLabel.TextAlignment = UITextAlignment.Left;
						base.TextLabel.TextColor = UIColor.Label;
					}
					else if (msg.IsError)
					{
						base.TextLabel.TextAlignment = UITextAlignment.Left;
						base.TextLabel.TextColor = UIColor.SystemRed;
					}
					else
					{
						base.TextLabel.TextAlignment = UITextAlignment.Left;
						base.TextLabel.TextColor = UIColor.Gray;
					}
				}
				else
				{
					base.TextLabel.Text = "";
				}
			}
		}
		public MessageCell (string reuseIdentifier) : base (UITableViewCellStyle.Default, reuseIdentifier)
		{
			base.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			base.TextLabel.Lines = 1000000;
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
