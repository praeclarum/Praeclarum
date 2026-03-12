#nullable enable

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language itself in future versions of C#.
global using nfloat = System.Double;
#pragma warning restore CS8981

namespace CoreGraphics
{
    public struct CGPoint
    {
        public nfloat X;
        public nfloat Y;

        public CGPoint(nfloat x, nfloat y)
        {
            X = x;
            Y = y;
        }
    }

    public struct CGSize
    {
        public nfloat Width;
        public nfloat Height;

        public CGSize(nfloat width, nfloat height)
        {
            Width = width;
            Height = height;
        }
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
}

#endif
