#nullable enable

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language itself in future versions of C#.
global using nfloat = System.Double;
#pragma warning restore CS8981

using System;

namespace CoreGraphics
{
    public abstract class CGContext
    {

    }

    /// <summary>Structure defining a 2D point.</summary>
    public struct CGPoint : IEquatable<CGPoint>
    {
        private nfloat x;
        private nfloat y;

        public nfloat X { get => x; set => x = value; }
        public nfloat Y { get => y; set => y = value; }

        public static readonly CGPoint Empty = new CGPoint(0, 0);

        public static bool operator ==(CGPoint l, CGPoint r) => l.x == r.x && l.y == r.y;
        public static bool operator !=(CGPoint l, CGPoint r) => l.x != r.x || l.y != r.y;
        public static CGPoint operator +(CGPoint l, CGSize r) => new (l.x + r.Width, l.y + r.Height);
        public static CGPoint operator -(CGPoint l, CGSize r) => new (l.x - r.Width, l.y - r.Height);
        public static implicit operator CGPoint(System.Drawing.PointF point) => new (point.X, point.Y);
        public static implicit operator CGPoint(System.Drawing.Point point) => new (point.X, point.Y);
        public static explicit operator System.Drawing.PointF(CGPoint point) => new ((float)point.x, (float)point.y);
        public static explicit operator System.Drawing.Point(CGPoint point) => new ((int)point.x, (int)point.y);

        public static CGPoint Add(CGPoint point, CGSize size) => new (point.x + size.Width, point.y + size.Height);

        public static CGPoint Subtract(CGPoint point, CGSize size) => new (point.x - size.Width, point.y - size.Height);

        public bool IsEmpty { get => x == 0 && y == 0; }

        public CGPoint(nfloat x, nfloat y) {
            this.x = x;
            this.y = y;
        }

        // public CGPoint(double x, double y) {
        //     this.x = x;
        //     this.y = y;
        // }

        public CGPoint(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public CGPoint(CGPoint point) {
            this.x = point.x;
            this.y = point.y;
        }

        /// <summary>Attempts to parse the contents of an <see cref="T:Foundation.NSDictionary" /> with a serialized <see cref="T:CoreGraphics.CGPoint" /> into a <see cref="T:CoreGraphics.CGPoint" />.</summary>
        /// <param name="dictionaryRepresentation">The dictionary to parse.</param>
        /// <param name="point">If successful, the resulting <see cref="T:CoreGraphics.CGPoint" /> value.</param>
        // public static bool TryParse(NSDictionary? dictionaryRepresentation, out CGPoint point);

        /// <summary>Serializes a <see cref="T:CoreGraphics.CGPoint" /> into an <see cref="T:Foundation.NSDictionary" />.</summary>
        // public NSDictionary ToDictionary();

        /// <summary>Serializes a <see cref="T:CoreGraphics.CGPoint" /> into a <see cref="T:CoreGraphics.CGPointDictionary" />.</summary>
        // public CGPointDictionary ToCGPointDictionary();

        public override bool Equals (object? obj) => obj is CGPoint point && Equals(point);

        public bool Equals (CGPoint point) => x == point.x && y == point.y;

        public override int GetHashCode () => HashCode.Combine(x, y);

        public void Deconstruct (out nfloat x, out nfloat y) {
            x = this.x;
            y = this.y;
        }

        public override string? ToString () => $"{{X={x}, Y={y}}}";
    }

    public struct CGRect : IEquatable<CGRect>
    {
        public CGPoint Origin;
        public CGSize Size;

        public CGRect(CGPoint origin, CGSize size)
        {
            Origin = origin;
            Size = size;
        }

        public CGRect(nfloat x, nfloat y, nfloat width, nfloat height)
            : this(new CGPoint(x, y), new CGSize(width, height)) { }

        public nfloat X => Origin.X;
        public nfloat Y => Origin.Y;
        public nfloat Width => Size.Width;
        public nfloat Height => Size.Height;
        public nfloat Left => Origin.X;
        public nfloat Top => Origin.Y;
        public nfloat Right => Origin.X + Size.Width;
        public nfloat Bottom => Origin.Y + Size.Height;
        public bool IsEmpty => Size.Width == 0 && Size.Height == 0;
        public static readonly CGRect Empty = new (new CGPoint (0, 0), new CGSize (0, 0));

        public bool Contains (CGPoint p) => p.X >= Origin.X && p.Y >= Origin.Y && p.X <= Right && p.Y <= Bottom;
        public bool Contains (nfloat x, nfloat y) => x >= Origin.X && y >= Origin.Y && x <= Right && y <= Bottom;
        public bool IntersectsWith (CGRect r) => !(r.Origin.X >= Right || r.Right <= Origin.X || r.Origin.Y >= Bottom || r.Bottom <= Origin.Y);
        public CGRect Inset (nfloat dx, nfloat dy) => new (Origin.X + dx, Origin.Y + dy, Size.Width - 2 * dx, Size.Height - 2 * dy);

        public static bool operator == (CGRect l, CGRect r) => l.Origin == r.Origin && l.Size == r.Size;
        public static bool operator != (CGRect l, CGRect r) => !(l == r);
        public override bool Equals (object? obj) => obj is CGRect r && r == this;
        public bool Equals (CGRect other) => other == this;
        public override int GetHashCode () => HashCode.Combine (Origin, Size);
        public override string ToString () => $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }

    public struct CGSize : IEquatable<CGSize>
    {
        private nfloat width;
        private nfloat height;

        public nfloat Width { get => width; set => width = value; }
        public nfloat Height { get => height; set => height = value; }

        public static readonly CGSize Empty = new CGSize(0, 0);

        public CGSize(nfloat width, nfloat height)
        {
            this.width = width;
            this.height = height;
        }
        public CGSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
        public CGSize(CGSize size)
        {
            this.width = size.width;
            this.height = size.height;
        }

        public override bool Equals(object? obj) => obj is CGSize size && Equals(size);
        public override int GetHashCode() => HashCode.Combine(width, height);
        public bool Equals(CGSize size) => width == size.width && height == size.height;
        public override string? ToString() => $"{{Width={width}, Height={height}}}";

        public static bool operator ==(CGSize l, CGSize r) => l.width == r.width && l.height == r.height;
        public static bool operator !=(CGSize l, CGSize r) => l.width != r.width || l.height != r.height;
        public static CGSize operator +(CGSize l, CGSize r) => new (l.width + r.width, l.height + r.height);
        public static CGSize operator -(CGSize l, CGSize r) => new (l.width - r.width, l.height - r.height);
        public static implicit operator CGSize(System.Drawing.SizeF size) => new (size.Width, size.Height);
        public static implicit operator CGSize(System.Drawing.Size size) => new (size.Width, size.Height);
        public static explicit operator System.Drawing.SizeF(CGSize size) => new ((float)size.Width, (float)size.Height);
        public static explicit operator System.Drawing.Size(CGSize size) => new ((int)size.Width, (int)size.Height);
        public static explicit operator CGPoint(CGSize size) => new (size.Width, size.Height);
    }

    public class CGColor : ObjCRuntime.NativeObject
    {
        public nfloat[] Components { get; set; } = new nfloat[] { 0, 0, 0, 1 };
        public nfloat Alpha => Components.Length > 0 ? Components[Components.Length - 1] : 1;
        public CGColorSpace? ColorSpace { get; set; }
        public CGColor () { }
        public CGColor (nfloat red, nfloat green, nfloat blue, nfloat alpha) { Components = new[] { red, green, blue, alpha }; }
        public CGColor (nfloat red, nfloat green, nfloat blue) : this (red, green, blue, 1) { }
        public CGColor (nfloat gray, nfloat alpha) { Components = new[] { gray, gray, gray, alpha }; }
        public CGColor (CGColor other, nfloat alpha)
        {
            Components = (nfloat[])other.Components.Clone ();
            if (Components.Length > 0) Components[Components.Length - 1] = alpha;
        }
        public static CGColor CreateGenericRgb (nfloat r, nfloat g, nfloat b, nfloat a) => new (r, g, b, a);
        public static CGColor CreateSrgb (nfloat r, nfloat g, nfloat b, nfloat a) => new (r, g, b, a);
    }

    public class CGColorSpace : ObjCRuntime.NativeObject
    {
        public static CGColorSpace CreateDeviceRGB () => new ();
        public static CGColorSpace CreateDeviceGray () => new ();
        public static CGColorSpace CreateGenericRgb () => new ();
        public static CGColorSpace CreateSrgb () => new ();
    }

    public class CGImage : ObjCRuntime.NativeObject
    {
        public nint Width { get; set; }
        public nint Height { get; set; }
        public nint BitsPerComponent { get; set; } = 8;
        public nint BitsPerPixel { get; set; } = 32;
        public nint BytesPerRow { get; set; }
        public CGColorSpace? ColorSpace { get; set; }
    }

    public class CGPath : ObjCRuntime.NativeObject
    {
        public CGPath () { }
        public CGPath (CGPath other) { }
        public void MoveToPoint (nfloat x, nfloat y) { }
        public void MoveToPoint (CGPoint point) { }
        public void AddLineToPoint (nfloat x, nfloat y) { }
        public void AddLineToPoint (CGPoint point) { }
        public void AddCurveToPoint (nfloat cp1x, nfloat cp1y, nfloat cp2x, nfloat cp2y, nfloat x, nfloat y) { }
        public void AddQuadCurveToPoint (nfloat cpx, nfloat cpy, nfloat x, nfloat y) { }
        public void AddArc (nfloat x, nfloat y, nfloat radius, nfloat startAngle, nfloat endAngle, bool clockwise) { }
        public void AddRect (CGRect rect) { }
        public void AddEllipseInRect (CGRect rect) { }
        public void CloseSubpath () { }
    }

    public class CGGradient : ObjCRuntime.NativeObject
    {
        public CGGradient () { }
        public CGGradient (CGColorSpace? colorSpace, CGColor[] colors, nfloat[] locations) { }
    }

    public struct CGAffineTransform
    {
        public nfloat A, B, C, D, Tx, Ty;
        public CGAffineTransform (nfloat a, nfloat b, nfloat c, nfloat d, nfloat tx, nfloat ty)
        { A = a; B = b; C = c; D = d; Tx = tx; Ty = ty; }
        public static CGAffineTransform MakeIdentity () => new (1, 0, 0, 1, 0, 0);
        public static CGAffineTransform MakeTranslation (nfloat tx, nfloat ty) => new (1, 0, 0, 1, tx, ty);
        public static CGAffineTransform MakeScale (nfloat sx, nfloat sy) => new (sx, 0, 0, sy, 0, 0);
        public static CGAffineTransform MakeRotation (nfloat radians)
        {
            var c = (nfloat)Math.Cos (radians);
            var s = (nfloat)Math.Sin (radians);
            return new CGAffineTransform (c, s, -s, c, 0, 0);
        }
        public bool IsIdentity => A == 1 && B == 0 && C == 0 && D == 1 && Tx == 0 && Ty == 0;
        public CGAffineTransform Translate (nfloat tx, nfloat ty) =>
            new (A, B, C, D, Tx + A * tx + C * ty, Ty + B * tx + D * ty);
        public CGAffineTransform Scale (nfloat sx, nfloat sy) =>
            new (A * sx, B * sx, C * sy, D * sy, Tx, Ty);
        public CGAffineTransform Rotate (nfloat radians)
        {
            var r = MakeRotation (radians);
            return Multiply (r, this);
        }
        public CGPoint TransformPoint (CGPoint p) => new (A * p.X + C * p.Y + Tx, B * p.X + D * p.Y + Ty);
        public static CGAffineTransform Multiply (CGAffineTransform t1, CGAffineTransform t2) => new (
            t1.A * t2.A + t1.B * t2.C,
            t1.A * t2.B + t1.B * t2.D,
            t1.C * t2.A + t1.D * t2.C,
            t1.C * t2.B + t1.D * t2.D,
            t1.Tx * t2.A + t1.Ty * t2.C + t2.Tx,
            t1.Tx * t2.B + t1.Ty * t2.D + t2.Ty);
    }

    public enum CGLineCap { Butt, Round, Square }
    public enum CGLineJoin { Miter, Round, Bevel }
    public enum CGBlendMode
    {
        Normal, Multiply, Screen, Overlay, Darken, Lighten, ColorDodge, ColorBurn,
        SoftLight, HardLight, Difference, Exclusion, Hue, Saturation, Color, Luminosity,
        Clear, Copy, SourceIn, SourceOut, SourceAtop, DestinationOver, DestinationIn,
        DestinationOut, DestinationAtop, XOR, PlusDarker, PlusLighter,
    }
    public enum CGPathDrawingMode { Fill, EOFill, Stroke, FillStroke, EOFillStroke }
    [Flags]
    public enum CGGradientDrawingOptions { None = 0, DrawsBeforeStartLocation = 1, DrawsAfterEndLocation = 2 }
}

#endif
