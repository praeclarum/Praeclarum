// ReSharper disable UseSymbolAlias
#nullable enable


using System;

// ReSharper disable once CheckNamespace
namespace Praeclarum.UI;

using UIKit;
using CoreGraphics;
using ObjCRuntime;
using System.Runtime.InteropServices;

// ReSharper disable once InconsistentNaming
public class AIChatView : UIView
{
	private readonly UITableView tableView;
	private readonly UIView inputBox = new() { BackgroundColor = UIColor.Red, };
	private readonly UITextView inputField = new ();

	static readonly NFloat minInputBoxHeight = 56;
	static readonly NFloat maxInputBoxHeight = 168;
	static readonly NFloat inputBoxVPadding = 7;
	static readonly NFloat inputBoxHPadding = 11;
	
	NFloat inputBoxHeight = minInputBoxHeight;
	
	readonly UIFont inputFieldFont = UIFont.SystemFontOfSize (UIFont.SystemFontSize);

    public AIChatView (CGRect frame)
        : base (frame)
    {
	    tableView = new UITableView (frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    public AIChatView (NativeHandle handle)
        : base (handle)
    {
	    tableView = new UITableView (base.Frame, UITableViewStyle.Plain);
	    Initialize ();
    }

    void Initialize ()
    {
	    var bounds = base.Bounds;

	    BackgroundColor = UIColor.Yellow;

	    tableView.BackgroundView = new UIView (bounds) { BackgroundColor = UIColor.Blue, };
	    tableView.TranslatesAutoresizingMaskIntoConstraints = false;
	    
	    var iframe = new CGRect (0, bounds.Height - minInputBoxHeight, bounds.Width, minInputBoxHeight);
	    inputBox.Frame = iframe;
	    inputBox.TranslatesAutoresizingMaskIntoConstraints = false;

	    var fframe = new CGRect (inputBoxHPadding, inputBoxVPadding, iframe.Width - 2*inputBoxHPadding, iframe.Height - 2*inputBoxVPadding);
	    inputField.Frame = fframe;
	    inputField.BackgroundColor = UIColor.Green;
	    inputField.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
	    inputField.TranslatesAutoresizingMaskIntoConstraints = true;
	    inputField.Font = inputFieldFont;
	    inputField.Changed += InputFieldOnValueChanged;
	    inputBox.AddSubview (inputField);
	    
	    AddSubview (tableView);
	    AddSubview (inputBox);
	    base.SetNeedsLayout ();
    }

    private void InputFieldOnValueChanged (object? sender, EventArgs e)
    {
	    AdjustInputBoxHeight ();
    }

    public override void LayoutSubviews ()
    {
	    base.LayoutSubviews ();

	    var bounds = this.Bounds;
	    
	    var tframe = new CGRect (0, 0, bounds.Width, bounds.Height - inputBoxHeight);
	    tableView.Frame = tframe;
	    
	    var iframe = new CGRect (0, bounds.Height - inputBoxHeight, bounds.Width, inputBoxHeight);
	    inputBox.Frame = iframe;
    }
    
    void AdjustInputBoxHeight ()
	{
		if (inputField.Text is not {} text)
			return;
		
	    var width = inputField.Frame.Width;
	    var lineSize = text.StringSize (inputFieldFont);
	    var lines = (int)Math.Ceiling (lineSize.Width / width);
	    var minTextHeight = lines * lineSize.Height;
	    var newTextHeight = minTextHeight * 1.4 + 22 + 2*inputBoxVPadding;
	    var newInputBoxHeight = (NFloat)Math.Min(maxInputBoxHeight, Math.Max (minInputBoxHeight, newTextHeight));
	    // Console.WriteLine ($"Adjust Text ({text.Length}) size {lineSize}: {lines} * {lineSize.Height} = {minTextHeight} => {newInputBoxHeight}");
	    if (Math.Abs (newInputBoxHeight - inputBoxHeight) > 1.0f)
	    {
		    inputBoxHeight = newInputBoxHeight;
		    SetNeedsLayout ();
	    }
	}
}
