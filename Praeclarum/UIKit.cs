#nullable disable

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if __MACOS__

using AppKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;
// ReSharper disable InconsistentNaming

namespace UIKit
{
    public struct NSDirectionalEdgeInsets
    {
        public static readonly NSDirectionalEdgeInsets Zero;

        public nfloat Top;
        public nfloat Leading;
        public nfloat Bottom;
        public nfloat Trailing;

        public static implicit operator AppKit.NSDirectionalEdgeInsets (NSDirectionalEdgeInsets insets) =>
            new AppKit.NSDirectionalEdgeInsets (insets.Top, insets.Leading, insets.Bottom, insets.Trailing);

        public NSDirectionalEdgeInsets (nfloat top, nfloat leading, nfloat bottom, nfloat trailing)
        {
            Top = top;
            Leading = leading;
            Bottom = bottom;
            Trailing = trailing;
        }

        public bool Equals (NSDirectionalEdgeInsets other)
        {
            if (Leading != other.Leading) {
                return false;
            }
            if (Trailing != other.Trailing) {
                return false;
            }
            if (Top != other.Top) {
                return false;
            }
            return Bottom == other.Bottom;
        }
        public override bool Equals (object obj)
        {
            if (obj is NSDirectionalEdgeInsets) {
                return Equals ((NSDirectionalEdgeInsets)obj);
            }
            return false;
        }
        public static bool operator == (NSDirectionalEdgeInsets insets1, NSDirectionalEdgeInsets insets2) => insets1.Equals (insets2);
        public static bool operator != (NSDirectionalEdgeInsets insets1, NSDirectionalEdgeInsets insets2) => !insets1.Equals (insets2);
        public override int GetHashCode () => Top.GetHashCode () ^ Leading.GetHashCode () ^ Trailing.GetHashCode () ^ Bottom.GetHashCode ();
    }

    public class UIAlertController : UIViewController
    {
        public string Message { get; set; }        

        public static UIAlertController Create (string title, string message, UIAlertControllerStyle style)
        {
            return new UIAlertController {
                Title = title,
                Message = message,
            };
        }

        public void PresentAlert (NSWindow window)
        {
            var alert = new NSAlert {
                InformativeText = Message,
                MessageText = Title,
            };
            alert.BeginSheet (window);
        }
    }

    public enum UIAlertControllerStyle
    {
        Alert
    }

    public class UIBarButtonItem : NSObject
    {
        public nfloat Width { get; set; }

        public UIBarButtonItem (UIBarButtonSystemItem systemItem, Action<object, EventArgs> handlee)
        {
        }

        public UIBarButtonItem (string v, UIBarButtonItemStyle plain, Action<object, EventArgs> handler)
        {
        }

        public UIBarButtonItem (UIBarButtonSystemItem fixedSpace)
        {
        }
    }

    public enum UIBarButtonItemStyle
    {
        Plain,
    }

    public enum UIBarButtonSystemItem
    {
        Done,
        Undo,
        FixedSpace,
        Cancel
    }

    public class UIBezierPath
    {
        readonly NSBezierPath path;

        const double toDegrees = 180 / Math.PI;

        public nfloat Flatness { get; set; }

        public UIBezierPath (NSBezierPath path)
        {
            this.path = path;
        }

        public UIBezierPath ()
            : this (new NSBezierPath ())
        {
        }

        public CGRect Bounds => path.Bounds;

        public static implicit operator NSBezierPath (UIBezierPath path) => path?.path;

        public static UIBezierPath FromOval (CGRect rect) => new UIBezierPath (NSBezierPath.FromOvalInRect (rect));
        public static UIBezierPath FromRect (CGRect rect) => new UIBezierPath (NSBezierPath.FromRect (rect));

        public void MoveTo (CGPoint point) => path.MoveTo (point);

        public void AddLineTo (CGPoint point) => path.LineTo (point);

        public void ClosePath () => path.ClosePath ();

        public void AddArc (CGPoint center, nfloat radius, nfloat startAngle, nfloat endAngle, bool clockwise) =>
            path.AppendPathWithArc (center, radius, (nfloat)(startAngle * toDegrees), (nfloat)(endAngle * toDegrees), !clockwise);

        public void AddArc (CGPoint point1, CGPoint point2, nfloat radius) =>
            path.AppendPathWithArc (point1, point2, radius);

        public void AppendPath (UIBezierPath subpath) => path.AppendPath (subpath.path);

        public void ApplyTransform (CGAffineTransform transform) =>
            path.TransformUsingAffineTransform (new NSAffineTransform { TransformStruct = transform });

    }

    public class UIBlurEffect : UIVisualEffect
    {
        public static UIBlurEffect FromStyle (UIBlurEffectStyle style)
        {
            return new UIBlurEffect ();
        }
    }

    public enum UIBlurEffectStyle
    {
        Dark,
        ExtraDark,
        Light,
    }

    public class UIButton : NSButton
    {
        NSActionDispatcher dispatcher;

        public bool Selected
        {
	        get => base.Highlighted;
	        set => base.Highlighted = value;
        }

        public UIColor TintColor
        {
	        get => UIColor.FromNSColor (base.ContentTintColor);
	        set => base.ContentTintColor = value;
        }

        public UIButton ()
        {
            BezelStyle = NSBezelStyle.RoundRect;
            Action = NSDispatcher.Selector;
        }

        public event EventHandler TouchUpInside {
            add {
                dispatcher = new NSActionDispatcher (() => value.Invoke (this, EventArgs.Empty));
                this.Target = dispatcher;
            }
            remove {
                this.Target = null;
                dispatcher = null;
            }
        }

        public void SetTitle (string title, UIControlState controlState)
        {
            Title = title;
        }

        public static UIButton FromType (UIButtonType type)
        {
            return new UIButton ();
        }
    }

    public enum UIButtonType
    {
        RoundedRect,
    }

    public class UICollectionView : NSCollectionView
    {
        public bool Bounces { get; set; } = false;
        public bool AlwaysBounceVertical { get; set; } = false;

        public UICollectionView (CGRect frame, UICollectionViewLayout layout)
			: base(frame)
        {
	        base.CollectionViewLayout = layout;
        }

        public UICollectionView (CGRect frame, UICollectionViewFlowLayout layout)
			: base(frame)
        {
	        base.CollectionViewLayout = layout;
        }

        public UICollectionViewCell DequeueReusableCell (string identifier, NSIndexPath indexPath)
        {
	        if (base.MakeItem (identifier, indexPath) is UICollectionViewCell c)
	        {
		        return c;
	        }
	        return new UICollectionViewCell();
        }

        public void RegisterClassForCell (Type type, string v)
        {
            throw new NotImplementedException ();
        }
    }

    public class UICollectionViewController : NSViewController
    {
        public virtual nint NumberOfSections (UICollectionView collectionView) => 0;
        public virtual nint GetItemsCount (UICollectionView collectionView, nint section) => 0;
        public virtual void ItemSelected (UICollectionView collectionView, NSIndexPath indexPath) { }
        public virtual UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath) => throw new NotImplementedException ();

        public UINavigationItem NavigationItem { get; } = new UINavigationItem ();

        public UICollectionView CollectionView { get; } = new UICollectionView (new CGRect (0, 0, 320, 480), new UICollectionViewFlowLayout ());

        readonly Lazy<UITraitCollection> traitCollection = new Lazy<UITraitCollection> (() => new UITraitCollection ());
        public UITraitCollection TraitCollection => traitCollection.Value;

        public UICollectionViewController ()
        {
            View = CollectionView;
        }

        public UICollectionViewController (NSCollectionViewLayout layout)
        {
            CollectionView.CollectionViewLayout = layout;
        }

        public void DismissViewController (bool animated, Action completionHandler)
        {
            this.PresentingViewController?.DismissViewController (this);
            completionHandler?.Invoke ();
        }

        public Task DismissViewControllerAsync (bool animated)
        {
            var tcs = new TaskCompletionSource<object> ();
            DismissViewController (animated, () => tcs.SetResult (null));
            return tcs.Task;
        }

        public override void ViewWillAppear ()
        {
            ViewWillAppear (false);
        }
        public override void ViewDidAppear ()
        {
            ViewDidAppear (false);
        }
        public override void ViewWillDisappear ()
        {
            ViewWillDisappear (false);
        }
        public override void ViewDidDisappear ()
        {
            ViewDidDisappear (false);
        }
        public virtual void ViewWillAppear (bool animated)
        {
        }
        public virtual void ViewDidAppear (bool animated)
        {
        }
        public virtual void ViewWillDisappear (bool animated)
        {
        }
        public virtual void ViewDidDisappear (bool animated)
        {
        }
        public virtual void DidReceiveMemoryWarning ()
        {
        }
    }

    public class UICollectionViewCell : NSCollectionViewItem
    {
	    public NSView ContentView => this.View;

        public UICollectionViewCell (IntPtr handle)
            : base (handle)
        {
        }

        public UICollectionViewCell ()
        {
        }
    }

    public class UICollectionViewLayout : NSCollectionViewLayout
    {
    }

    public class UICollectionViewFlowLayout : NSCollectionViewFlowLayout
    {
    }

    public abstract class UICollectionViewDataSource : NSCollectionViewDataSource
    {
	    public override nint GetNumberofItems (NSCollectionView collectionView, nint section)
	    {
		    if (collectionView is UICollectionView cv)
		    {
			    return GetItemsCount (cv, section);
		    }

		    return 0;
	    }

	    public override NSCollectionViewItem GetItem (NSCollectionView collectionView, NSIndexPath indexPath)
	    {
		    if (collectionView is UICollectionView cv)
		    {
			    return GetCell (cv, indexPath);
		    }

		    return new NSCollectionViewItem();
	    }

	    public abstract nint GetItemsCount (UICollectionView collectionView, nint section);

	    public abstract UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath);
    }

    public class UIDevice : NSObject
	{
		public static UIDevice CurrentDevice { get; } = new UIDevice ();
		//public UIUserInterfaceIdiom UserInterfaceIdiom { get; } = UIUserInterfaceIdiom.Desktop;

		public bool CheckSystemVersion (int major, int minor)
		{
			return true;
		}
	}

    abstract class NSDispatcher : NSObject
    {
        public const string SelectorName = "uikitApplySelector";

        public static readonly ObjCRuntime.Selector Selector = new ObjCRuntime.Selector ("uikitApplySelector");

        protected NSDispatcher ()
        {
            base.IsDirectBinding = false;
        }

        [Export ("uikitApplySelector")]
        [Preserve (Conditional = true)]
        public abstract void Apply ();
    }

    class NSActionDispatcher : NSDispatcher
    {
        private readonly Action action;

        public NSActionDispatcher (Action action)
        {
            if (action == null) {
                throw new ArgumentNullException (nameof (action));
            }
            this.action = action;
        }

        public override void Apply ()
        {
            action ();
        }
    }

    public class UIColor
    {
        readonly NSColor color;

        public static readonly UIColor Clear = new UIColor (NSColor.Clear);
        public static readonly UIColor Black = new UIColor (NSColor.Black);
        public static readonly UIColor DarkGray = new UIColor (NSColor.DarkGray);
        public static readonly UIColor Gray = new UIColor (NSColor.Gray);
        public static readonly UIColor LightGray = new UIColor (NSColor.LightGray);
        public static readonly UIColor White = new UIColor (NSColor.White);
        public static readonly UIColor Red = new UIColor (NSColor.Red);
        public static readonly UIColor Green = new UIColor (NSColor.Green);
        public static readonly UIColor Blue = new UIColor (NSColor.Blue);
        public static readonly UIColor Yellow = new UIColor (NSColor.Yellow);

        public static readonly UIColor SystemBackground = new UIColor (NSColor.TextBackground);
        public static readonly UIColor Label = new UIColor (NSColor.Label);

        public static readonly ThreadLocal<UIColor> currentFill = new ThreadLocal<UIColor> ();
        public static UIColor CurrentFill => currentFill.Value;

        public CGColor CGColor => color.CGColor;
        public NSColor NSColor => color;

        public UIColor (NSColor color)
        {
            this.color = color;
        }

        public static UIColor FromNSColor (NSColor color)
        {
	        if (color is null)
	        {
		        return null;
	        }

	        return new UIColor (color);
        }

        public override string ToString () => color?.ToString () ?? "";

        public static implicit operator NSColor (UIColor color) => color?.color;

        public static UIColor FromRGB (byte r, byte g, byte b) => new UIColor (NSColor.FromRgb (r, g, b));
        public static UIColor FromRGB (int r, int g, int b) => new UIColor (NSColor.FromRgb (r, g, b));
        public static UIColor FromRGB (float r, float g, float b) => new UIColor (NSColor.FromRgb (r, g, b));
        public static UIColor FromRGB (nfloat r, nfloat g, nfloat b) => new UIColor (NSColor.FromRgb (r, g, b));
        public static UIColor FromRGBA (byte r, byte g, byte b, byte a) => new UIColor (NSColor.FromRgba (r, g, b, a));
        public static UIColor FromRGBA (int r, int g, int b, int a) => new UIColor (NSColor.FromRgba (r, g, b, a));
        public static UIColor FromRGBA (float r, float g, float b, float a) => new UIColor (NSColor.FromRgba (r, g, b, a));
        public static UIColor FromRGBA (nfloat r, nfloat g, nfloat b, nfloat a) => new UIColor (NSColor.FromRgba (r, g, b, a));
        public static UIColor FromWhiteAlpha (nfloat w, nfloat a) => new UIColor (NSColor.FromWhite (w, a));
        public UIColor ColorWithAlpha (nfloat alpha) => new UIColor (color.ColorWithAlphaComponent (alpha));

        public void GetRGBA (out nfloat r, out nfloat g, out nfloat b, out nfloat a) => color.GetRgba (out r, out g, out b, out a);
        public void GetWhite (out nfloat w, out nfloat a) => color.GetWhiteAlpha (out w, out a);

        public void SetColor ()
        {
            color.Set ();
            currentFill.Value = this;
        }
        public void SetFill ()
        {
            color.SetFill ();
            currentFill.Value = this;
        }
        public void SetStroke () => color.SetStroke ();
    }

    public enum UIControlState
    {
        Normal,
        Selected,
    }

    public class UIDocument : NSDocument
    {
        public UIDocument (IntPtr handle)
            : base (handle)
        {
        }

        public virtual NSDictionary GetFileAttributesToWrite (NSUrl forUrl, UIDocumentSaveOperation saveOperation, out NSError outError)
        {
            outError = null;
            return null;
        }

        public virtual Task<bool> CloseAsync ()
        {
            return Task.FromResult<bool> (true);
        }

        public virtual NSObject ContentsForType (string typeName, out NSError outError)
        {
            outError = null;
            return null;
        }

        public virtual bool LoadFromContents (NSObject contents, string typeName, out NSError outError)
        {
            outError = null;
            return false;
        }

        public override NSDictionary FileAttributesToWrite (NSUrl toUrl, string typeName, NSSaveOperationType saveOperation, NSUrl absoluteOriginalContentsUrl, out NSError outError)
        {
            return GetFileAttributesToWrite (toUrl, UIDocumentSaveOperation.Save, out outError);
        }

        public override NSData GetAsData (string typeName, out NSError outError)
        {
            return (NSData)ContentsForType (typeName, out outError);
        }

        public override bool ReadFromData (NSData data, string typeName, out NSError outError)
        {
            return LoadFromContents (data, typeName, out outError);
        }
    }

    public enum UIDocumentSaveOperation
    {
        Save
    }

    public struct UIEdgeInsets
    {
        public nfloat Top;
        public nfloat Left;
        public nfloat Bottom;
        public nfloat Right;

        public static implicit operator NSEdgeInsets (UIEdgeInsets i) => new NSEdgeInsets (i.Top, i.Left, i.Bottom, i.Right);
        public static implicit operator UIEdgeInsets (NSEdgeInsets i) => new UIEdgeInsets (i.Top, i.Left, i.Bottom, i.Right);

        public UIEdgeInsets (nfloat top, nfloat left, nfloat bottom, nfloat right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }
    }

    public class UIEvent : NSObject
    {
        public readonly NSEvent NSEvent;

        public UIEvent (NSEvent theEvent)
        {
            NSEvent = theEvent;
        }
    }

    public class UIFont
    {
        readonly NSFont font;

        public static nfloat SystemFontSize => NSFont.SystemFontSize;

        public nfloat PointSize => font.PointSize;

        //static readonly NSMutableParagraphStyle pstyle = new NSMutableParagraphStyle {
        //};
        public UIFont (NSFont font)
        {
            this.font = font;
        }
        public NSStringAttributes CreateAttributes (UIColor foregroundColor)
        {
            return new NSStringAttributes {
                Font = font,
                ForegroundColor = foregroundColor,
                //ParagraphStyle = pstyle,
            };
        }
        public static implicit operator NSFont (UIFont font) => font?.font;
        public static UIFont FromName (string fontName, nfloat size)
        {
            var r = new UIFont (NSFont.FromFontName (fontName, size));
            return r;
        }
        public static UIFont SystemFontOfSize (nfloat fontSize)
        {
            var r = new UIFont (NSFont.SystemFontOfSize (fontSize));
            return r;
        }
    }

    public abstract class UIGestureRecognizer : NSObject
    {
        protected abstract NSGestureRecognizer NSRecognizer { get; }
        public CGPoint LocationInView (NSView view)
        {
            return NSRecognizer.LocationInView (view);
        }
        public UIGestureRecognizerState State {
            get => (UIGestureRecognizerState)NSRecognizer.State;
        }
    }

    public enum UIGestureRecognizerState
    {
        Began = (int)NSGestureRecognizerState.Began,
        Changed = (int)NSGestureRecognizerState.Changed,
        Ended = (int)NSGestureRecognizerState.Ended,
        Cancelled = (int)NSGestureRecognizerState.Cancelled,
        Failed = (int)NSGestureRecognizerState.Failed,
    }

    public static class UIGraphics
    {
        static readonly ThreadLocal<CGBitmapContext> imageContext = new ThreadLocal<CGBitmapContext> ();
        static readonly ThreadLocal<Stack<(NSGraphicsContext PrevContext, NSGraphicsContext Context)>> contextStack =
            new ThreadLocal<Stack<(NSGraphicsContext PrevContext, NSGraphicsContext Context)>> ();

        public static void BeginImageContext (CGSize imageSize)
        {
            BeginImageContextWithOptions (imageSize, false, 1.0f);
        }

        public static void BeginImageContextWithOptions (CGSize imageSize, bool opaque, float scale)
        {
            if (imageContext.Value != null)
                return;
            var width = (nint)imageSize.Width;
            if (width < 1)
                width = 1;
            var height = (nint)imageSize.Height;
            if (height < 1)
                height = 1;
            var alphaInfo = opaque ? CGImageAlphaInfo.NoneSkipLast : CGImageAlphaInfo.PremultipliedLast;
            var context = new CGBitmapContext (IntPtr.Zero, width, height, 8, width * 8 * 4, CGColorSpace.CreateGenericRgb (), alphaInfo);
            context.TranslateCTM (0, height);
            context.ScaleCTM (1, -1);
            PushContext (context);
            imageContext.Value = context;
        }

        public static CGContext GetCurrentContext ()
        {
            return NSGraphicsContext.CurrentContext.CGContext;
        }

        public static void RectFill (CGRect rect)
        {
            NSGraphics.RectFill (rect);
        }

        public static CGSize GetSizeUsingAttributes (this string text, UIStringAttributes attributes)
        {
            return text.StringSize (attributes);
        }

        public static CGSize GetSizeUsingAttributes (this NSString text, UIStringAttributes attributes)
        {
            return text.StringSize (attributes);
        }

        public static void DrawString (this string text, CGPoint point, UIFont font)
        {
            //var flipped = NSGraphicsContext.CurrentContext.IsFlipped;
            //var yy = NSGraphicsContext.CurrentContext.CGContext.GetCTM ().yy;
            //Console.WriteLine ("DrawString: " + flipped);
            text.DrawAtPoint (point, font.CreateAttributes (UIColor.CurrentFill));
        }

        public static void DrawString (this NSString text, CGPoint point, UIFont font)
        {
            //var flipped = NSGraphicsContext.CurrentContext.IsFlipped;
            //var flipped = NSGraphicsContext.CurrentContext.CGContext.GetCTM ().yy;
            //Console.WriteLine ("DrawNSString: " + flipped);
            text.DrawAtPoint (point, font.CreateAttributes (UIColor.CurrentFill));
        }

        public static void DrawString (this string text, CGRect frame, UIFont font)
        {
            //var flipped = NSGraphicsContext.CurrentContext.CGContext.GetCTM().yy;
            //Console.WriteLine ("DrawStringFrame: " + flipped);
            text.DrawAtPoint (new CGPoint (frame.X, frame.Y), font.CreateAttributes (UIColor.CurrentFill));
        }

        public static CGSize StringSize (this string text, UIFont font)
        {
            return text.StringSize (font.CreateAttributes (UIColor.CurrentFill));
        }

        public static CGSize StringSize (this NSString text, UIFont font)
        {
            return text.StringSize (font.CreateAttributes (UIColor.CurrentFill));
        }

        public static UIImage GetImageFromCurrentImageContext ()
        {
            var context = imageContext.Value;
            if (context == null)
                return null;
            var cgimage = context.ToImage ();
            var nsimage = new NSImage (cgimage, new CGSize (cgimage.Width, cgimage.Height));

            return new UIImage (nsimage);
        }

        public static void EndImageContext ()
        {
            if (imageContext.Value is CGBitmapContext c) {
                imageContext.Value = null;
                PopContext ();
                c?.Dispose ();
            }
        }

        public static void PushContext (CGContext ctx)
        {
            var nsctx = NSGraphicsContext.FromCGContext (ctx, initialFlippedState: true);
            var stack = contextStack.Value;
            if (stack == null) {
                stack = new Stack<(NSGraphicsContext PrevContext, NSGraphicsContext Context)> ();
                contextStack.Value = stack;
            }
            stack.Push ((NSGraphicsContext.CurrentContext, nsctx));
            NSGraphicsContext.CurrentContext = nsctx;
        }

        // Workaround: https://github.com/xamarin/xamarin-macios/issues/9827
        static readonly IntPtr selSetCurrentContext_Handle = Selector.GetHandle ("setCurrentContext:");
        static readonly IntPtr classNSGraphicsContext_Handle = Class.GetHandle ("NSGraphicsContext");
        [DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        static extern void void_objc_msgSend_IntPtr (IntPtr receiver, IntPtr selector, IntPtr arg1);

        public static void PopContext ()
        {
            var stack = contextStack.Value;
            if (stack != null) {
                if (stack.TryPop (out var ctx)) {
                    var handle = ctx.PrevContext != null ? (IntPtr)ctx.PrevContext.Handle : IntPtr.Zero;
                    void_objc_msgSend_IntPtr (classNSGraphicsContext_Handle, selSetCurrentContext_Handle, handle);
                    ctx.Context.Dispose ();
                }
            }
        }
    }

    public class UIImage : NSObject
    {
        readonly NSImage image;

        public NSImage NSImage => image;

        public nfloat CurrentScale => (nfloat)1.0;
        public UIImageOrientation Orientation => UIImageOrientation.Up;

        public UIImage (NSImage image)
        {
            this.image = image;
        }

        public static implicit operator UIImage (NSImage image) => new UIImage (image);

        public CGSize Size => image.Size;

        public CGImage CGImage => image.CGImage;

        public static implicit operator NSImage (UIImage image) => image?.image;

        public static UIImage FromFile (string path) => new UIImage (new NSImage (path));

        public void Draw (CGPoint location)
        {
            var c = NSGraphicsContext.CurrentContext?.CGContext;
            if (c == null)
                return;
            c.SaveState ();
            c.TranslateCTM (location.X, location.Y);
            c.TranslateCTM (0, image.Size.Height);
            c.ScaleCTM (1, -1);
            image.Draw (CGPoint.Empty, new CGRect (CGPoint.Empty, image.Size), NSCompositingOperation.Copy, 1.0f);
            c.RestoreState ();
        }

        public void Draw (CGRect rect)
        {
            var c = NSGraphicsContext.CurrentContext?.CGContext;
            if (c == null)
                return;
            c.SaveState ();
            var size = image.Size;
            var xscale = rect.Width / size.Width;
            var yscale = rect.Height / size.Height;
            c.TranslateCTM (rect.X, rect.Y + rect.Height);
            c.ScaleCTM (xscale, -yscale);
            image.Draw (new CGRect (CGPoint.Empty, size), new CGRect (CGPoint.Empty, size), NSCompositingOperation.Copy, 1.0f);
            c.RestoreState ();
        }

        public static UIImage LoadFromData (NSData data, nfloat scale)
        {
            var nsimage = new NSImage (data);
            return new UIImage (nsimage);
        }

        public static UIImage FromImage (CGImage cgimage)
        {
            var size = new CGSize (cgimage.Width, cgimage.Height);
            var nsimage = new NSImage (cgimage, size);
            return new UIImage (nsimage);
        }

        public static UIImage FromImage (CGImage cgimage, nfloat x, UIImageOrientation y)
        {
            var size = new CGSize (cgimage.Width, cgimage.Height);
            var nsimage = new NSImage (cgimage, size);
            return new UIImage (nsimage);
        }

        public NSData AsJPEG ()
        {
            var cgimage = image.CGImage;
            using var rep = new NSBitmapImageRep (cgimage);
            rep.Size = image.Size;
            var data = rep.RepresentationUsingTypeProperties (NSBitmapImageFileType.Jpeg, null);
            return data;
        }

        public NSData AsPNG ()
        {
            var cgimage = image.CGImage;
            using var rep = new NSBitmapImageRep (cgimage);
            rep.Size = image.Size;
            var data = rep.RepresentationUsingTypeProperties (NSBitmapImageFileType.Png, null);
            return data;
        }
    }

    public enum UIImageOrientation
    {
        Up
    }

    public class UIImageView : NSImageView
    {
        public UIColor BackgroundColor { get; set; }
        public bool ClipsToBounds {
	        get => Messaging.bool_objc_msgSend(Handle, UIView.s_clipsToBounds);
	        set => Messaging.void_objc_msgSend_bool(Handle, UIView.s_setClipsToBounds, value);
        }
    }

    public class UIImpactFeedbackGenerator : NSObject
    {
        public UIImpactFeedbackGenerator (UIImpactFeedbackStyle style)
        {
        }
        public void ImpactOccurred ()
        {
        }
        public void Prepare ()
        {
        }
    }

    public enum UIImpactFeedbackStyle
    {
        Light
    }

    [Flags]
    public enum UIKeyModifierFlags : long
    {
        AlphaShift = 0x10000,
        Shift = 0x20000,
        Control = 0x40000,
        Alternate = 0x80000,
        Command = 0x100000,
        NumericPad = 0x200000
    }

    public class UILabel : NSTextField
    {
        public string Text {
            get => this.StringValue;
            set => this.StringValue = value;
        }

        public new UIColor TextColor
        {
	        get => new UIColor (base.TextColor);
	        set => base.TextColor = value;
        }
        public UITextAlignment TextAlignment {
            get {
                switch (this.Alignment) {
                    case NSTextAlignment.Left:
                        return UITextAlignment.Left;
                    case NSTextAlignment.Right:
                        return UITextAlignment.Right;
                    case NSTextAlignment.Center:
                        return UITextAlignment.Center;
                    case NSTextAlignment.Justified:
                        return UITextAlignment.Justified;
                    case NSTextAlignment.Natural:
                    default:
                        return UITextAlignment.Natural;
                }
            }
            set {
                switch (value) {
                    case UITextAlignment.Natural:
                        Alignment = NSTextAlignment.Natural;
                        break;
                    case UITextAlignment.Left:
                        Alignment = NSTextAlignment.Left;
                        break;
                    case UITextAlignment.Right:
                        Alignment = NSTextAlignment.Right;
                        break;
                    case UITextAlignment.Center:
                        Alignment = NSTextAlignment.Center;
                        break;
                    case UITextAlignment.Justified:
                    default:
                        Alignment = NSTextAlignment.Justified;
                        break;
                }
            }
        }
        public new UILineBreakMode LineBreakMode
        {
	        get
	        {
		        switch (base.LineBreakMode)
		        {
			        case NSLineBreakMode.CharWrapping:
				        return UILineBreakMode.CharacterWrap;
			        case NSLineBreakMode.Clipping:
				        return UILineBreakMode.Clip;
			        case NSLineBreakMode.TruncatingHead:
				        return UILineBreakMode.HeadTruncation;
			        case NSLineBreakMode.TruncatingTail:
				        return UILineBreakMode.TailTruncation;
			        case NSLineBreakMode.TruncatingMiddle:
				        return UILineBreakMode.MiddleTruncation;
			        case NSLineBreakMode.ByWordWrapping:
				    default:
				        return UILineBreakMode.WordWrap;
		        }
	        }
	        set
	        {
		        switch (value)
		        {
			        case UILineBreakMode.Clip:
				        base.LineBreakMode = NSLineBreakMode.Clipping;
				        break;
			        case UILineBreakMode.CharacterWrap:
				        base.LineBreakMode = NSLineBreakMode.CharWrapping;
				        break;
			        case UILineBreakMode.HeadTruncation:
				        base.LineBreakMode = NSLineBreakMode.TruncatingHead;
				        break;
			        case UILineBreakMode.MiddleTruncation:
				        base.LineBreakMode = NSLineBreakMode.TruncatingMiddle;
				        break;
			        case UILineBreakMode.TailTruncation:
				        base.LineBreakMode = NSLineBreakMode.TruncatingTail;
				        break;
			        case UILineBreakMode.WordWrap:
				    default:
				        base.LineBreakMode = NSLineBreakMode.ByWordWrapping;
				        break;
		        }
	        }
        }
        public int Lines
        {
	        get;
	        set;
        }

        public bool AdjustsFontSizeToFitWidth
        {
	        get;
	        set;
        }
        public nfloat Alpha
        {
	        get => base.AlphaValue;
	        set => base.AlphaValue = value;
        }
        public UILabel ()
        {
            Initialize ();
        }
        public UILabel (CGRect frame)
            : base (frame)
        {
            Initialize ();
        }
        void Initialize ()
        {
            Cell = new UILabelCell ();
            Cell.StringValue = "";
            Editable = false;
            Bordered = false;
            BackgroundColor = NSColor.Clear;
            Bezeled = false;
            BezelStyle = NSTextFieldBezelStyle.Square;
            Cell.TruncatesLastVisibleLine = true;
        }

        class UILabelCell : NSTextFieldCell
        {
            public override CGRect TitleRectForBounds (CGRect rect)
            {
                var titleRect = base.TitleRectForBounds (rect);

                var minimumHeight = CellSizeForBounds (rect).Height;
                titleRect.Y += (titleRect.Height - minimumHeight) / 2;
                titleRect.Height = minimumHeight;
                return titleRect;
            }

            public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
            {
                base.DrawInteriorWithFrame (TitleRectForBounds (cellFrame), inView);
            }

            public override void SelectWithFrame (CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, nint selStart, nint selLength)
            {
                base.SelectWithFrame (TitleRectForBounds (aRect), inView, editor, delegateObject, selStart, selLength);
            }
        }
    }

    public enum UILayoutConstraintAxis
    {
        Horizontal,
        Vertical
    }

    public enum UILineBreakMode
    {
	    WordWrap,
	    CharacterWrap,
	    Clip,
	    HeadTruncation,
	    TailTruncation,
	    MiddleTruncation,
    }

    public enum UIModalPresentationStyle
    {
        FullScreen
    }

    public enum UIModalTransitionStyle
    {
        CrossDissolve
    }

    public class UINavigationBar : UIView
    {
        public bool PrefersLargeTitles { get; set; }
    }

    public class UINavigationController : NSViewController
    {
        public UIModalTransitionStyle ModalTransitionStyle { get; set; }
        public UIModalPresentationStyle ModalPresentationStyle { get; set; }
        public UINavigationBar NavigationBar { get; } = new UINavigationBar ();

        public UINavigationController (NSViewController rootViewController)
        {
        }
    }

    public class UINavigationItem : NSObject
    {
        public UIBarButtonItem[] LeftBarButtonItems { get; set; } = new UIBarButtonItem[0];
        public UIBarButtonItem[] RightBarButtonItems { get; set; } = new UIBarButtonItem[0];
        public UIBarButtonItem LeftBarButtonItem { get; set; }
        public UINavigationItemLargeTitleDisplayMode LargeTitleDisplayMode { get; set; }
    }

    public enum UINavigationItemLargeTitleDisplayMode
    {
        Always
    }

    public class UIPanGestureRecognizer : UIGestureRecognizer
    {
        readonly NSPanGestureRecognizer recognizer;
        protected override NSGestureRecognizer NSRecognizer => recognizer;
        public static implicit operator NSPanGestureRecognizer (UIPanGestureRecognizer r) => r.recognizer;
        public Func<UIGestureRecognizer, bool> ShouldBegin { get; set; }
        public UIPanGestureRecognizer (NSPanGestureRecognizer recognizer)
        {
            this.recognizer = recognizer;
        }
        public UIPanGestureRecognizer (Action<UIPanGestureRecognizer> handlePan)
        {
            recognizer = new NSPanGestureRecognizer (_ => handlePan (this));
        }

        public CGPoint VelocityInView (UIView view)
        {
	        return CGPoint.Empty;
        }
    }

    public class UIPasteboard : NSObject
    {
        readonly NSPasteboard pasteboard;
        public UIPasteboard (NSPasteboard pasteboard)
        {
            this.pasteboard = pasteboard;
        }
        public static UIPasteboard General => new UIPasteboard (NSPasteboard.GeneralPasteboard);
        public override string ToString () => pasteboard.ToString ();
        public string[] Types => pasteboard.Types;
        public void SetData (NSData data, string pasteboardType)
        {
            pasteboard.ClearContents ();
            pasteboard.SetDataForType (data, pasteboardType);
        }
        public NSData DataForPasteboardType (string dataType)
        {
            return pasteboard.GetDataForType (dataType);
        }
    }

    public class UIPopoverPresentationController : NSObject
    {
        public NSView SourceView { get; set; }
        public CGRect SourceRect { get; set; }
    }

    public class UIScreen : NSObject
    {
        public CGRect Bounds { get; } = new CGRect ();
        public static readonly UIScreen MainScreen = new UIScreen ();
    }

    public class UIScrollView : NSScrollView
    {
        NSObject boundsNote;

        public new CGSize ContentSize {
            get => ((NSView)DocumentView).Frame.Size;
            set {
                ((NSView)DocumentView).Frame = new CGRect (CGPoint.Empty, value);
                //ScrollRectToVisible (new CGRect (0, 0, value.Width, Frame.Height));
            }
        }
        public UIEdgeInsets ContentInset {
            get => base.ContentInsets;
            set => base.ContentInsets = value;
        }

        public UIScrollViewDelegate Delegate { get; set; }
        public event EventHandler Scrolled;
        public bool AlwaysBounceVertical { get; set; }
        public bool AlwaysBounceHorizontal { get; set; }
        public bool DirectionalLockEnabled { get; set; }
        public bool ClipsToBounds {
	        get => Messaging.bool_objc_msgSend(Handle, UIView.s_clipsToBounds);
	        set => Messaging.void_objc_msgSend_bool(Handle, UIView.s_setClipsToBounds, value);
        }

        public UIScrollView (CGRect frameRect) : base (frameRect)
        {
            DrawsBackground = false;
            DocumentView = new UIScrollViewDocumentView {
                PostsBoundsChangedNotifications = true,
            };
            boundsNote = NSView.Notifications.ObserveBoundsChanged ((s, e) => {
                Delegate?.Scrolled (this);
                Scrolled?.Invoke (this, EventArgs.Empty);
            });
        }

        public UIScrollView () : this (CGRect.Empty) { }

        public new UIViewAutoresizing AutoresizingMask {
	        get => this.GetAutoresizingMask ();
	        set => this.SetAutoresizingMask (value);
        }

        public void ScrollRectToVisible (CGRect aRect, bool animated)
        {
            ScrollRectToVisible (aRect);
        }
    }

    class UIScrollViewDocumentView : NSView
    {
        public override bool IsFlipped => true;
    }

    public class UIScrollViewDelegate : NSObject
    {
        public virtual void Scrolled (UIScrollView scrollView)
        {
        }
    }

    public class UISegmentedControl : NSSegmentedControl
    {
        public event EventHandler ValueChanged;

        public UISegmentedControl (params string[] strings)
        {
            SegmentCount = strings.Length;
            for (var i = 0; i < strings.Length; i++) {
                SetLabel (strings[i], i);
            }
            base.Activated += HandleActivated;
        }

        void HandleActivated (object sender, EventArgs e)
        {
            ValueChanged?.Invoke (this, e);
        }
    }

    public class UISelectionFeedbackGenerator : NSObject
    {
        public void Prepare ()
        {
        }
        public void SelectionChanged ()
        {
        }
    }

    public class UIStackView : NSStackView
    {
        public UIColor BackgroundColor { get; set; }
        public UILayoutConstraintAxis Axis {
            get => Orientation == NSUserInterfaceLayoutOrientation.Horizontal ? UILayoutConstraintAxis.Horizontal : UILayoutConstraintAxis.Vertical;
            set => Orientation = (value == UILayoutConstraintAxis.Horizontal) ? NSUserInterfaceLayoutOrientation.Horizontal : NSUserInterfaceLayoutOrientation.Vertical;
        }
        public bool LayoutMarginsRelativeArrangement { get; set; } = true;
        public NSDirectionalEdgeInsets DirectionalLayoutMargins {
            get {
                var e = EdgeInsets;
                return new NSDirectionalEdgeInsets (e.Top, e.Left, e.Bottom, e.Right);
            }
            set => EdgeInsets = new NSEdgeInsets (value.Top, value.Leading, value.Bottom, value.Trailing);
        }
        public new UIStackViewAlignment Alignment {
            get {
                switch (base.Alignment) {
                    case NSLayoutAttribute.Leading:
                        return UIStackViewAlignment.Leading;
                    case NSLayoutAttribute.Trailing:
                        return UIStackViewAlignment.Trailing;
                    default:
                        return UIStackViewAlignment.Center;
                }
            }
            set {
                switch (value) {
                    case UIStackViewAlignment.Center:
                        base.Alignment = NSLayoutAttribute.CenterX;
                        break;
                    case UIStackViewAlignment.Leading:
                        base.Alignment = NSLayoutAttribute.Leading;
                        break;
                    case UIStackViewAlignment.Trailing:
                        base.Alignment = NSLayoutAttribute.Trailing;
                        break;
                }
            }
        }
        public new UIStackViewDistribution Distribution {
            get => (UIStackViewDistribution)base.Distribution;
            set => base.Distribution = (NSStackViewDistribution)value;
        }
        public new UIViewAutoresizing AutoresizingMask {
            get => this.GetAutoresizingMask ();
            set => this.SetAutoresizingMask (value);
        }
        public UIStackView ()
        {
            Spacing = 0;
        }
        public UIStackView (CGRect frame)
            : base (frame)
        {
            Spacing = 0;
        }
    }

    public enum UIStackViewAlignment
    {
        Center,
        Leading,
        Trailing,
        Fill,
    }

    public enum UIStackViewDistribution
    {
        EqualSpacing = (int)NSStackViewDistribution.EqualSpacing,
        Fill = (int)NSStackViewDistribution.Fill,
    }

    public class UIStringAttributes : NSStringAttributes
    {
    }

    public class UITapGestureRecognizer : UIGestureRecognizer
    {
        readonly NSClickGestureRecognizer recognizer;
        protected override NSGestureRecognizer NSRecognizer => recognizer;
        public static implicit operator NSClickGestureRecognizer (UITapGestureRecognizer r) => r.recognizer;
        public UITapGestureRecognizer (NSClickGestureRecognizer recognizer)
        {
            this.recognizer = recognizer;
        }
        public UITapGestureRecognizer (Action<UITapGestureRecognizer> handleTap)
        {
            recognizer = new NSClickGestureRecognizer (_ => handleTap (this));
        }
    }

    public enum UITextAlignment
    {
        Natural,
        Left,
        Right,
        Center,
        Justified,
    }

    public enum UITextAutocapitalizationType
    {
        None
    }

    public enum UITextAutocorrectionType
    {
        No
    }

    public enum UITextBorderStyle : long
    {
	    None,
	    Line,
	    Bezel,
	    RoundedRect,
    }

    public class UITextField : NSTextField
    {
        public event EventHandler EditingDidBegin;
        public event EventHandler EditingChanged;
        public event EventHandler EditingDidEnd;
        public event EventHandler ValueChanged;
        public string Text {
            get => StringValue;
            set => StringValue = value ?? "";
        }
        public string Placeholder { get => PlaceholderString; set => PlaceholderString = value; }
        public UITextFieldViewMode ClearButtonMode { get; set; }
        public Func<object, bool> ShouldReturn { get; set; }
        public UITextAutocorrectionType AutocorrectionType { get; set; }
        public UITextAutocapitalizationType AutocapitalizationType { get; set; }

        public UITextBorderStyle BorderStyle
        {
	        get
	        {
		        switch (BezelStyle)
		        {
			        case NSTextFieldBezelStyle.Square:
				        return UITextBorderStyle.Bezel;
			        default:
			        case NSTextFieldBezelStyle.Rounded:
				        return UITextBorderStyle.RoundedRect;
		        }
	        }
	        set
	        {
		        switch (value)
		        {
			        default:
			        case UITextBorderStyle.RoundedRect:
				        BezelStyle = NSTextFieldBezelStyle.Rounded;
				        break;
			        case UITextBorderStyle.Bezel:
				        BezelStyle = NSTextFieldBezelStyle.Square;
				        break;
		        }
	        }
        }

        public UITextField ()
        {
            base.EditingBegan += (s, e) => EditingDidBegin?.Invoke (s, e);
            base.EditingEnded += (s, e) => EditingDidEnd?.Invoke (s, e);
            base.Changed += (s, e) => {
                EditingChanged?.Invoke (s, e);
                ValueChanged?.Invoke (s, e);
            };
        }
    }

    public enum UITextFieldViewMode
    {
        Always
    }

    public class UITextView : NSScrollView
    {
        readonly NSTextView textView = new NSTextView {
            MaxSize = new CGSize (nfloat.MaxValue, nfloat.MaxValue),
            VerticallyResizable = true,
            HorizontallyResizable = true,
            AllowsUndo = true,
        };

        public string Text {
            get => textView.Value;
            set => textView.Value = value ?? "";
        }
        public override NSColor BackgroundColor {
            get => textView.BackgroundColor;
            set {
                textView.BackgroundColor = value;
            }
        }
        public UIFont Font {
            get => new UIFont (textView.Font);
            set => textView.Font = value;
        }
        public event EventHandler Changed;

        public UITextView ()
        {
            textView.TextContainer.ContainerSize = new CGSize (nfloat.MaxValue, nfloat.MaxValue);
            textView.TextContainer.WidthTracksTextView = true;
            textView.TextContainer.LineBreakMode = NSLineBreakMode.Clipping;

            VerticalScrollElasticity = NSScrollElasticity.Allowed;
            HorizontalScrollElasticity = NSScrollElasticity.Allowed;
            HasVerticalScroller = true;
            HasHorizontalScroller = true;
            AutomaticallyAdjustsContentInsets = true;
            DocumentView = textView;

            textView.DrawsBackground = true;
            //base.BackgroundColor = textView.BackgroundColor;
            DrawsBackground = false;
            ContentView.DrawsBackground = false;

            textView.TextDidChange += (s, e) => Changed?.Invoke (s, e);
        }
    }

    public class UITouch : NSObject
    {
        CGPoint plocationInWindow;
        CGPoint locationInWindow;
        int tapCount;

        public double Timestamp { get; set; }
        public int TapCount => tapCount;
        public nfloat Force => 0;

        public UITouch (NSEvent theEvent)
        {
            locationInWindow = theEvent.LocationInWindow;
            plocationInWindow = locationInWindow;
            tapCount = theEvent.Type != NSEventType.ScrollWheel ? (int)theEvent.ClickCount : 0;
            //Console.WriteLine ($"I {theEvent.LocationInWindow} {theEvent.DeltaX}");
        }

        public CGPoint LocationInView (NSView view)
        {
            var lview = view.ConvertPointFromView (locationInWindow, null);
            return lview;
        }

        public CGPoint PreviousLocationInView (NSView view)
        {
            var lview = view.ConvertPointFromView (plocationInWindow, null);
            return lview;
        }

        public void Move (NSEvent theEvent)
        {
            plocationInWindow = locationInWindow;
            locationInWindow = theEvent.LocationInWindow;
            //Console.WriteLine ($"M {theEvent.LocationInWindow} {theEvent.DeltaX}");
        }
    }

    public class UITraitCollection : NSObject
    {
        public UIUserInterfaceSizeClass HorizontalSizeClass { get; set; }
    }

    public enum UIUserInterfaceSizeClass
    {
        Unspecified,
        Compact,
        Regular
    }

    public class UIView : NSView
    {
        UIColor backgroundColor = UIColor.Black;
        public nfloat Alpha { get => AlphaValue; set => AlphaValue = value; }
        public override bool IsFlipped => true;
        public bool MultipleTouchEnabled { get; set; }
        public bool Opaque { get; set; }
        public UIViewContentMode ContentMode { get; set; }
        public UIColor BackgroundColor {
            get => backgroundColor;
            set {
                backgroundColor = value;
                base.SetNeedsDisplayInRect (base.Bounds);
            }
        }
        public override bool AcceptsFirstResponder () => CanBecomeFirstResponder;
        public virtual bool CanBecomeFirstResponder => false;
        public bool UserInteractionEnabled { get; set; }
        public new UIViewAutoresizing AutoresizingMask {
            get => this.GetAutoresizingMask ();
            set => this.SetAutoresizingMask (value);
        }
        public static readonly IntPtr s_clipsToBounds = Selector.GetHandle("clipsToBounds");
        public static readonly IntPtr s_setClipsToBounds = Selector.GetHandle("setClipsToBounds:");
        public bool ClipsToBounds {
	        get => Messaging.bool_objc_msgSend(Handle, s_clipsToBounds);
	        set => Messaging.void_objc_msgSend_bool(Handle, s_setClipsToBounds, value);
        }
        public UIView ()
        {
        }
        public UIView (IntPtr handle)
            : base (handle)
        {
        }
        public UIView (CGRect frame)
            : base (frame)
        {
        }
        public void SetNeedsLayout ()
        {
            this.NeedsLayout = true;
        }
        public virtual void LayoutSubviews ()
        {
        }
        public override void Layout ()
        {
            base.Layout ();
            LayoutSubviews ();
        }
        public virtual void SetNeedsDisplay ()
        {
            this.SetNeedsDisplayInRect (Bounds);
        }
        public virtual void Draw (CGRect rect)
        {
        }
        public override void DrawRect (CGRect dirtyRect)
        {
            backgroundColor.NSColor.SetFill ();
            NSGraphics.RectFill (dirtyRect);
            Draw (dirtyRect);
        }

        public virtual void TouchesBegan (Foundation.NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesMoved (Foundation.NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesCancelled (Foundation.NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesEnded (Foundation.NSSet touches, UIEvent evt)
        {
        }

        UITouch mouseTouch = null;

        public override void MouseDown (NSEvent theEvent)
        {
            //Console.WriteLine ("DOWN");
            mouseTouch = new UITouch (theEvent);
            TouchesBegan (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            //base.MouseDown (theEvent);
        }

        public override void MouseDragged (NSEvent theEvent)
        {
            //Console.WriteLine ("DRAG");
            if (mouseTouch == null)
                return;
            mouseTouch.Move (theEvent);
            TouchesMoved (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            //base.MouseDragged (theEvent);
        }

        public override void MouseUp (NSEvent theEvent)
        {
            //Console.WriteLine ("UP");
            if (mouseTouch == null)
                return;
            TouchesEnded (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            mouseTouch = null;
            //base.MouseUp (theEvent);
        }

        public static void BeginAnimations (string name)
        {
	        NSAnimationContext.BeginGrouping ();
	    }

        public static void CommitAnimations ()
        {
	        NSAnimationContext.EndGrouping ();
        }
    }

    public enum UIViewContentMode
    {
        Redraw,
    }

    public static class UIViewExtensions
    {
        public static void InsertSubviewAbove (this NSView superView, NSView view, NSView siblingView)
        {
            superView.AddSubview (view, NSWindowOrderingMode.Above, siblingView);
        }

        public static void SetAutoresizingMask (this NSView view, UIViewAutoresizing mask)
        {
            var nsmask = (NSViewResizingMask)0;
            if (mask.HasFlag (UIViewAutoresizing.FlexibleLeftMargin))
                nsmask |= NSViewResizingMask.MinXMargin;
            if (mask.HasFlag (UIViewAutoresizing.FlexibleTopMargin))
                nsmask |= NSViewResizingMask.MaxYMargin;
            if (mask.HasFlag (UIViewAutoresizing.FlexibleWidth))
                nsmask |= NSViewResizingMask.WidthSizable;
            if (mask.HasFlag (UIViewAutoresizing.FlexibleHeight))
                nsmask |= NSViewResizingMask.HeightSizable;
            view.AutoresizingMask = nsmask;
        }

        public static UIViewAutoresizing GetAutoresizingMask (this NSView view)
        {
            var mask = (UIViewAutoresizing)0;
            return mask;
        }
    }

    [Flags]
    public enum UIViewAutoresizing
    {
        None = 0,
        FlexibleLeftMargin = 1,
        FlexibleWidth = 2,
        FlexibleTopMargin = 8,
        FlexibleHeight = 16,
        FlexibleDimensions = 18,
    }

    public class UIViewController : NSViewController
    {
        public UINavigationItem NavigationItem { get; } = new UINavigationItem ();
        public UIPopoverPresentationController PopoverPresentationController { get; }

        public virtual bool PrefersHomeIndicatorAutoHidden { get; set; }

        readonly Lazy<UITraitCollection> traitCollection = new Lazy<UITraitCollection> (() => new UITraitCollection ());
        public UITraitCollection TraitCollection => traitCollection.Value;

        public UIViewController ()
        {
        }

        public UIViewController (IntPtr handle)
            : base (handle)
        {
        }

        public void PresentViewController (NSViewController viewController, bool animated, Action completionHandler)
        {
            if (viewController is UIAlertController alert) {
                alert.PresentAlert (this.View.Window);
            }
            else {
                this.PresentViewControllerAsModalWindow (viewController);
            }
            completionHandler?.Invoke ();
        }

        public Task PresentViewControllerAsync (NSViewController viewController, bool animated)
        {
            var tcs = new TaskCompletionSource<object> ();
            PresentViewController (viewController, animated, () => tcs.SetResult (null));
            return tcs.Task;
        }

        public void DismissViewController (bool animated, Action completionHandler)
        {
            this.PresentingViewController?.DismissViewController (this);
            completionHandler?.Invoke ();
        }

        public Task DismissViewControllerAsync (bool animated)
        {
            var tcs = new TaskCompletionSource<object> ();
            DismissViewController (animated, () => tcs.SetResult (null));
            return tcs.Task;
        }

        public override void ViewWillAppear ()
        {
            ViewWillAppear (false);
        }
        public override void ViewDidAppear ()
        {
            ViewDidAppear (false);
        }
        public override void ViewWillDisappear ()
        {
            ViewWillDisappear (false);
        }
        public override void ViewDidDisappear ()
        {
            ViewDidDisappear (false);
        }
        public virtual void ViewWillAppear (bool animated)
        {
        }
        public virtual void ViewDidAppear (bool animated)
        {
        }
        public virtual void ViewWillDisappear (bool animated)
        {
        }
        public virtual void ViewDidDisappear (bool animated)
        {
        }
        public virtual void DidReceiveMemoryWarning ()
        {
        }

        [Export ("cut:")]
        public virtual void Cut (NSObject sender)
        {
        }
        [Export ("copy:")]
        public virtual void Copy (NSObject sender)
        {
        }
        [Export ("paste:")]
        public virtual void Paste (NSObject sender)
        {
        }
        [Export ("delete:")]
        public virtual void Delete (NSObject sender)
        {
        }
        [Export ("selectAll:")]
        public virtual void SelectAll (NSObject sender)
        {
        }

        public virtual bool CanPerform (Selector action, NSObject withSender)
        {
            return RespondsToSelector (action);
        }

        public virtual void TouchesBegan (NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesMoved (NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesCancelled (NSSet touches, UIEvent evt)
        {
        }
        public virtual void TouchesEnded (NSSet touches, UIEvent evt)
        {
        }

        public virtual void ViewDidLayoutSubviews ()
        {
        }
        public override void ViewDidLayout ()
        {
            base.ViewDidLayout ();
            ViewDidLayoutSubviews ();
        }

        UITouch mouseTouch = null;

        public override void MouseDown (NSEvent theEvent)
        {
            //Console.WriteLine ("DOWN");
            mouseTouch = new UITouch (theEvent);
            TouchesBegan (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            //base.MouseDown (theEvent);
        }

        public override void MouseDragged (NSEvent theEvent)
        {
            //Console.WriteLine ("DRAG");
            if (mouseTouch == null)
                return;
            mouseTouch.Move (theEvent);
            TouchesMoved (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            //base.MouseDragged (theEvent);
        }

        public override void MouseUp (NSEvent theEvent)
        {
            //Console.WriteLine ("UP");
            if (mouseTouch == null)
                return;
            TouchesEnded (new NSSet (new NSObject[] { mouseTouch }), new UIEvent (theEvent));
            mouseTouch = null;
            //base.MouseUp (theEvent);
        }
    }

    public class UIVisualEffect : NSObject
    {
    }

    public class UIVisualEffectView : NSVisualEffectView
    {
        public NSView ContentView => this;
        public UIVisualEffect Effect { get; set; }
        public new UIViewAutoresizing AutoresizingMask {
            get => this.GetAutoresizingMask ();
            set => this.SetAutoresizingMask (value);
        }
        public UIVisualEffectView (UIVisualEffect visualEffect)
        {
	        this.Effect = visualEffect;
        }
        public UIVisualEffectView ()
        {
        }
    }

    static class Messaging
    {
	    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	    public static extern bool bool_objc_msgSend(IntPtr receiver, IntPtr selector);
	    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	    public static extern bool void_objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool arg0);
    }
}

#endif
