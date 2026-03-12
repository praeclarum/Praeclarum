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

    public struct CGRect
    {
        public CGPoint Origin;
        public CGSize Size;

        public CGRect(CGPoint origin, CGSize size)
        {
            Origin = origin;
            Size = size;
        }
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
}

#endif
