#nullable enable

// ReSharper disable UseSymbolAlias
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Foundation;
using UIKit;
using CoreGraphics;
using ObjCRuntime;

using Praeclarum.App;
using CrossIntelligence;

// ReSharper disable once CheckNamespace
namespace Praeclarum.UI;

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
	
	readonly AIChatHistory _history;
	
	readonly UITableView _tableView;
	readonly UIView _inputBox = new() { BackgroundColor = ios13 ? UIColor.SecondarySystemBackground : UIColor.Gray, };
	readonly UITextView _inputField = new ();
	readonly ChatSource _chatSource;
	readonly UIFont _inputFieldFont = UIFont.SystemFontOfSize (UIFont.SystemFontSize);

	NFloat _inputBoxHeight = 22;
	
	bool _isSubmitting = false;

	private IntelligenceSession? _session = null;
	
	public string Instructions { get; set; } = "";
	public IIntelligenceTool[] Tools { get; set; } = [];

	public string Prompt
	{
		get => _inputField.Text ?? "";
		set
		{
			_inputField.Text = value;
			AdjustInputBoxHeight (animated: false);
		}
	}

	private string? _initialPrompt = null;
	public string? InitialPrompt
	{
		get => _initialPrompt;
		set
		{
			_initialPrompt = value;
			if (_history.ActiveChat.Messages.Count == 0 && !string.IsNullOrWhiteSpace (value))
			{
				Prompt = value;
			}
		}
	}

    public AIChatView (CGRect frame, AIChatHistory? history = null)
        : base (frame)
    {
	    _history = history ?? new AIChatHistory ();
	    _chatSource = new ChatSource (this, _history);
	    _tableView = new UITableView (frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    public AIChatView (NativeHandle handle)
        : base (handle)
    {
	    _history = new AIChatHistory ();
	    _chatSource = new ChatSource (this, _history);
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
	    _tableView.AllowsSelection = false;
	    
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

    public void ScrollToBottom (bool animated)
    {
	    _chatSource.ScrollToBottom (_tableView, animated);
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
		// Console.WriteLine ($"Adjust Text ({text.Length}) size {lineSize}: {lines} * {lineSize.Height} = {minTextHeight} => {newInputBoxHeight}");
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
	    var ns = new IntelligenceSession (IntelligenceModels.AppleIntelligence, tools: Tools, instructions: Instructions);
	    _session = ns;
	    return ns;
    }

	class ChatSource : UITableViewSource
	{
		private readonly WeakReference<AIChatView> _chatView;
		private readonly AIChatHistory _history;

		NSIndexPath? BottomIndexPath
		{
			get
			{
				var chat = _history.ActiveChat;
				var row = chat.Messages.Count - 1;
				if (row < 0)
					return null;
#if __MACOS__
				return NSIndexPath.FromItemSection ((IntPtr)row, IntPtr.Zero);
#else
				return NSIndexPath.FromRowSection (row, 0);
#endif
			}
		}

		public ChatSource (AIChatView chatView, AIChatHistory history)
		{
			_chatView = new WeakReference<AIChatView> (chatView);
			_history = history;
		}

		public override IntPtr NumberOfSections (UITableView tableView)
		{
			return 1;
		}

		public override IntPtr RowsInSection (UITableView tableView, IntPtr section)
		{
			var chat = _history.ActiveChat;
			return chat.Messages.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell ("M") as MessageCell ?? new MessageCell ("M");
			var chat = _history.ActiveChat;
#if __MACOS__
			var message = chat.Messages[(int)indexPath.Item];
#else
			var message = chat.Messages[indexPath.Row];
#endif
			cell.Message = message;
			return cell;
		}

		async Task AddMessageAsync (AIChat.Message message, UITableView tableView)
		{
			var chat = _history.ActiveChat;
			chat.Messages.Add (message);
			if (BottomIndexPath is not {} bottom)
				return;
			tableView.InsertRows ([bottom], UITableViewRowAnimation.Automatic);
			await Task.Delay (1);
			ScrollToBottom (tableView, true);
		}

		public void ScrollToBottom (UITableView tableView, bool animated)
		{
			if (BottomIndexPath is not {} bottom)
				return;
			tableView.ScrollToRow (bottom, UITableViewScrollPosition.Bottom, animated);
		}

		public async Task AddPromptAsync (IntelligenceSession session, string prompt, UITableView tableView)
		{
			await AddMessageAsync (new AIChat.Message { Text = prompt, Type = AIChat.MessageType.User }, tableView);
			var newMessage = new AIChat.Message { Text = "Thinking...", Type = AIChat.MessageType.Assistant, ShowProgress = true};
			await AddMessageAsync (newMessage, tableView);
			try
			{
				var response = await session.RespondAsync (prompt);
				tableView.BeginUpdates ();
				newMessage.Text = response;
				newMessage.ShowProgress = false;
				tableView.EndUpdates ();
			}
			catch (Exception ex)
			{
				tableView.BeginUpdates ();
				newMessage.Text = ex.Message;
				newMessage.Type = AIChat.MessageType.Error;
				newMessage.ShowProgress = false;
				tableView.EndUpdates ();
			}
		}
	}
	
	class MessageCell : UITableViewCell
	{
		private UIActivityIndicatorView? _progressView;
		private AIChat.Message? _message;
		public AIChat.Message? Message
		{
			get => _message;
			set
			{
				if (ReferenceEquals (_message, value))
					return;
				if (_message is { } oldMessage)
				{
					oldMessage.PropertyChanged -= HandleMessagePropertyChanged;
				}
				_message = value;
				if (_message is { } msg)
				{
					msg.PropertyChanged += HandleMessagePropertyChanged;
					UpdateUI ();
				}
			}
		}

		public MessageCell (string reuseIdentifier) : base (UITableViewCellStyle.Default, reuseIdentifier)
		{
			base.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			base.TextLabel.Lines = 1000000;
		}

		private void HandleMessagePropertyChanged (object? sender, PropertyChangedEventArgs e)
		{
			UpdateUI ();
		}

		private void UpdateUI ()
		{
			var shouldShowProgress = false;
			if (_message is { } msg)
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
					base.TextLabel.TextColor = msg.ShowProgress ? UIColor.SecondaryLabel : UIColor.Label;
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
				shouldShowProgress = msg.ShowProgress;
			}
			else
			{
				base.TextLabel.TextAlignment = UITextAlignment.Left;
				base.TextLabel.TextColor = UIColor.Gray;
				base.TextLabel.Text = "";
				shouldShowProgress = false;
			}

			if (shouldShowProgress)
			{
				ShowProgress ();
			}
			else
			{
				HideProgress ();
			}
		}

		void ShowProgress ()
		{
			if (_progressView is { } pv)
			{
				return;
			}

			pv = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Medium);
			var size = pv.Frame.Height;
			var bounds = ContentView.Bounds;
			pv.Frame = new CGRect (bounds.Width - size - 11, bounds.Top + (bounds.Height - size)/2 + 3, size, size);
			pv.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleBottomMargin;
			pv.StartAnimating ();
			ContentView.AddSubview (pv);
			_progressView = pv;
		}
		
		void HideProgress ()
		{
			if (_progressView is { } pv)
			{
				pv.StopAnimating ();
				pv.RemoveFromSuperview ();
				_progressView = null;
			}
		}
	}
}

public class AIChatViewController : UIViewController
{
	private readonly AIChatView chatView;

	public string Instructions
	{
		get => chatView.Instructions;
		set => chatView.Instructions = value;
	}
	public IIntelligenceTool[] Tools
	{
		get => chatView.Tools;
		set => chatView.Tools = value ?? [];
	}
	public string Prompt
	{
		get => chatView.Prompt;
		set => chatView.Prompt = value;
	}
	public string? InitialPrompt
	{
		get => chatView.InitialPrompt;
		set => chatView.InitialPrompt = value;
	}

	public AIChatViewController(AIChatHistory? history) 
	{
		base.Title = "AI Chat".Localize();
		chatView = new AIChatView (new CGRect (0, 0, 320, 480), history);
		base.View = chatView;

		// base.NavigationItem.RightBarButtonItems =
		// [
		// 	new UIBarButtonItem(UIBarButtonSystemItem.Add, HandleAddChat),
		// ];
	}

	public override void ViewDidAppear (bool animated)
	{
		base.ViewDidAppear (animated);
		chatView.SetFocus ();
		chatView.ScrollToBottom (animated: false);
	}
	
	void HandleAddChat (object? sender, EventArgs e)
	{
		chatView.Prompt = "";
		chatView.SetFocus ();
	}
}
