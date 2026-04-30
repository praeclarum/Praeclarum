#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreGraphics;
using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace CoreAnimation
{
	public class CALayer : NSObject
	{
		public CALayer () { }
		public CALayer (CALayer other) { }
		public virtual CGRect Bounds { get; set; }
		public virtual CGRect Frame { get; set; }
		public virtual CGPoint Position { get; set; }
		public virtual CGPoint AnchorPoint { get; set; }
		public virtual nfloat Opacity { get; set; } = 1;
		public virtual bool Hidden { get; set; }
		public virtual bool MasksToBounds { get; set; }
		public virtual nfloat CornerRadius { get; set; }
		public virtual nfloat BorderWidth { get; set; }
		public virtual CGColor? BorderColor { get; set; }
		public virtual CGColor? BackgroundColor { get; set; }
		public virtual nfloat ShadowOpacity { get; set; }
		public virtual CGSize ShadowOffset { get; set; }
		public virtual nfloat ShadowRadius { get; set; }
		public virtual CGColor? ShadowColor { get; set; }
		public virtual string? Name { get; set; }
		public virtual nfloat ContentsScale { get; set; } = 1;
		public virtual nfloat ZPosition { get; set; }
		public virtual CALayer[] Sublayers { get; set; } = Array.Empty<CALayer> ();
		public virtual CALayer? SuperLayer { get; internal set; }
		public virtual CALayer? Mask { get; set; }
		public virtual NSObject? Contents { get; set; }
		public virtual CGRect ContentsRect { get; set; }
		public virtual string? ContentsGravity { get; set; }
		public virtual CGPath? ShadowPath { get; set; }
		public virtual CATransform3D Transform { get; set; } = CATransform3D.Identity;
		public virtual CGAffineTransform AffineTransform { get; set; } = CGAffineTransform.MakeIdentity ();
		public virtual void AddSublayer (CALayer layer) { }
		public virtual void RemoveFromSuperLayer () { }
		public virtual void SetNeedsDisplay () { }
		public virtual void SetNeedsDisplayInRect (CGRect rect) { }
		public virtual void DrawInContext (CGContext ctx) { }
		public virtual void RenderInContext (CGContext ctx) { }
		public virtual void AddAnimation (CAAnimation anim, string? key) { }
		public virtual void RemoveAnimation (string key) { }
		public virtual void RemoveAllAnimations () { }
		public virtual CAAnimation? AnimationForKey (string key) => null;
	}

	public class CAShapeLayer : CALayer
	{
		public CGPath? Path { get; set; }
		public CGColor? FillColor { get; set; }
		public CGColor? StrokeColor { get; set; }
		public nfloat LineWidth { get; set; } = 1;
		public string? LineCap { get; set; }
		public string? LineJoin { get; set; }
		public nfloat MiterLimit { get; set; } = 10;
		public nfloat StrokeStart { get; set; }
		public nfloat StrokeEnd { get; set; } = 1;
		public nfloat[]? LineDashPattern { get; set; }
		public nfloat LineDashPhase { get; set; }
		public string? FillRule { get; set; }
	}

	public class CATextLayer : CALayer
	{
		public string? String { get; set; }
		public NSAttributedString? AttributedString { get; set; }
		public NSObject? Font { get; set; }
		public nfloat FontSize { get; set; } = 12;
		public CGColor? ForegroundColor { get; set; }
		public CATextLayerAlignmentMode TextAlignmentMode { get; set; }
		public CATextLayerTruncationMode TruncationMode { get; set; }
		public bool Wrapped { get; set; }
	}

	public enum CATextLayerAlignmentMode { Natural, Left, Right, Center, Justified }
	public enum CATextLayerTruncationMode { None, Start, Middle, End }

	public class CAGradientLayer : CALayer
	{
		public CGColor[]? Colors { get; set; }
		public NSNumber[]? Locations { get; set; }
		public CGPoint StartPoint { get; set; }
		public CGPoint EndPoint { get; set; }
		public CAGradientLayerType LayerType { get; set; }
	}

	public enum CAGradientLayerType { Axial, Radial, Conic }

	public abstract class CAAnimation : NSObject
	{
		public double Duration { get; set; }
		public double BeginTime { get; set; }
		public float RepeatCount { get; set; }
		public double RepeatDuration { get; set; }
		public bool AutoReverses { get; set; }
		public string? FillMode { get; set; }
		public CAMediaTimingFunction? TimingFunction { get; set; }
		public string? KeyPath { get; set; }
		public bool RemovedOnCompletion { get; set; } = true;
		public float Speed { get; set; } = 1;
		public double TimeOffset { get; set; }
	}

	public class CABasicAnimation : CAAnimation
	{
		public NSObject? From { get; set; }
		public NSObject? To { get; set; }
		public NSObject? By { get; set; }
		public static CABasicAnimation FromKeyPath (string keyPath) => new () { KeyPath = keyPath };
	}

	public class CAKeyFrameAnimation : CAAnimation
	{
		public NSObject[]? Values { get; set; }
		public NSNumber[]? KeyTimes { get; set; }
		public CGPath? Path { get; set; }
		public static CAKeyFrameAnimation FromKeyPath (string keyPath) => new () { KeyPath = keyPath };
	}

	public class CAAnimationGroup : CAAnimation
	{
		public CAAnimation[]? Animations { get; set; }
	}

	public class CAMediaTimingFunction : NSObject
	{
		public CAMediaTimingFunction (float c1x, float c1y, float c2x, float c2y) { }
		public static CAMediaTimingFunction FromName (string name) => new (0, 0, 1, 1);
		public static readonly string Linear = "linear";
		public static readonly string EaseIn = "easeIn";
		public static readonly string EaseOut = "easeOut";
		public static readonly string EaseInEaseOut = "easeInEaseOut";
		public static readonly string Default = "default";
	}

	public static class CATransaction
	{
		public static void Begin () { }
		public static void Commit () { }
		public static void Flush () { }
		public static void Lock () { }
		public static void Unlock () { }
		public static double AnimationDuration { get; set; }
		public static CAMediaTimingFunction? AnimationTimingFunction { get; set; }
		public static bool DisableActions { get; set; }
		public static void SetCompletionBlock (Action action) { }
	}

	public struct CATransform3D
	{
		public nfloat M11, M12, M13, M14;
		public nfloat M21, M22, M23, M24;
		public nfloat M31, M32, M33, M34;
		public nfloat M41, M42, M43, M44;
		public static readonly CATransform3D Identity = new () { M11 = 1, M22 = 1, M33 = 1, M44 = 1 };
	}

	public class CADisplayLink : NSObject
	{
		public bool Paused { get; set; }
		public double Duration { get; set; }
		public double Timestamp { get; set; }
		public double TargetTimestamp { get; set; }
		public nint PreferredFramesPerSecond { get; set; }
		public void Invalidate () { }
		public void AddToRunLoop (NSObject runloop, NSString mode) { }
		public void RemoveFromRunLoop (NSObject runloop, NSString mode) { }
		public static CADisplayLink Create (Action action) => new ();
	}
}

#endif
