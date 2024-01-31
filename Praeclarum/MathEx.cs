#nullable enable

using System;

#if __IOS__ || __MACOS__
using SceneKit;
using UIKit;
using CoreGraphics;
#endif

namespace Praeclarum
{
    public static class MathEx
    {
        public static bool Plat (bool ios, bool mac)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return mac;
#endif
        }
        public static int Plat (int ios, int mac)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return mac;
#endif
        }
        public static float Plat (float ios, float mac)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return mac;
#endif
        }
        public static nfloat Plat (nfloat ios, nfloat mac)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return mac;
#endif
        }
        public static double Plat (double ios, double mac)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return mac;
#endif
        }
        public static int Plat (int ios)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return (int)(ios * 0.77 + 0.5);
#endif
        }
        public static float Plat (float ios)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return ios * 0.77f;
#endif
        }
        public static nfloat Plat (nfloat ios)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return ios * (nfloat)0.77;
#endif
        }
        public static double Plat (double ios)
        {
#if __IOS__
            return ios;
#elif __MACOS__
            return ios * 0.77;
#endif
        }

        public static double Map (double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            double d = (fromHigh - fromLow);
            if (d == 0) return toHigh;
            var r = (value - fromLow) / d;
            return (toHigh - toLow) * r + toLow;
        }

        public static float Map (this float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            float d = (fromHigh - fromLow);
            if (d == 0) return toHigh;
            var r = (value - fromLow) / d;
            return (toHigh - toLow) * r + toLow;
        }

        public static double MapClamped (this double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            if (value >= fromHigh) return toHigh;
            if (value <= fromLow) return toLow;
            double d = (fromHigh - fromLow);
            if (d == 0) return toHigh;
            var r = (value - fromLow) / d;
            return (toHigh - toLow) * r + toLow;
        }

        public static double MapClamped (this int value, int fromLow, int fromHigh, double toLow, double toHigh)
        {
            if (value >= fromHigh) return toHigh;
            if (value <= fromLow) return toLow;
            double d = (fromHigh - fromLow);
            if (d == 0) return toHigh;
            var r = (value - fromLow) / d;
            return (toHigh - toLow) * r + toLow;
        }

        public static double Clamp (this double value, double low, double high)
        {
            if (value >= high) return high;
            if (value <= low) return low;
            return value;
        }

        public static int NextRange (this Random random, Range range)
        {
            var max = range.End.GetOffset (int.MaxValue);
            return random.Next (range.Start.GetOffset (int.MaxValue), max);
        }

        #if __IOS__ || __MACOS__

        public static UIColor Grayaf (double g, double a) => UIColor.FromWhiteAlpha ((nfloat)g, (nfloat)a);

        public static UIColor Grayf (double g) => UIColor.FromWhiteAlpha ((nfloat)g, (nfloat)1.0);

        public static UIColor NextRgb (this Random random, int min = 0, int max = 256) => Rgb (random.Next (min, max), random.Next (min, max), random.Next (min, max));

        public static UIColor Rgb (byte r, byte g, byte b) => UIColor.FromRGB (r, g, b);

        public static UIColor Rgb (int r, int g, int b) => UIColor.FromRGB (r, g, b);

        public static UIColor Rgba (byte r, byte g, byte b, byte a) => UIColor.FromRGBA (r, g, b, a);

        public static UIColor Rgba (int r, int g, int b, int a) => UIColor.FromRGBA (r, g, b, a);

        public static UIColor Rgbf (double r, double g, double b) => UIColor.FromRGB ((nfloat)r, (nfloat)g, (nfloat)b);

        public static UIColor Rgbf (SCNVector3 c) => UIColor.FromRGB ((nfloat)c.X, (nfloat)c.Y, (nfloat)c.Z);

        public static UIColor Rgbf (nfloat r, nfloat g, nfloat b) => UIColor.FromRGB (r, g, b);

        public static UIColor Rgbaf (double r, double g, double b, double a) => UIColor.FromRGBA ((nfloat)r, (nfloat)g, (nfloat)b, (nfloat)a);

        public static UIColor Rgbaf (nfloat r, nfloat g, nfloat b, nfloat a) => UIColor.FromRGBA (r, g, b, a);

        public static UIColor Rgbaf (SCNVector3 c, double a) => UIColor.FromRGBA ((nfloat)c.X, (nfloat)c.Y, (nfloat)c.Z, (nfloat)a);

        public static uint GetCacheKey (this UIColor color)
        {
            nfloat or, og, ob, oa;
            if (color.CGColor.ColorSpace?.Components >= 3) {
                color.GetRGBA (out or, out og, out ob, out oa);
            }
            else {
                color.GetWhite (out or, out oa);
                og = ob = or;
            }
            if (or > 1) or = 1;
            if (og > 1) og = 1;
            if (ob > 1) ob = 1;
            if (oa > 1) oa = 1;
            var ir = (uint)(or * 255 + 0.5);
            var ig = (uint)(og * 255 + 0.5);
            var ib = (uint)(ob * 255 + 0.5);
            var ia = (uint)(oa * 255 + 0.5);
            var key = (ir << 24) | (ig << 16) | (ib << 8) | ia;
            return key;
        }

        public static UIColor ColorFromCacheKey (uint key)
        {
            var ir = (key >> 24);
            var ig = (key >> 16);
            var ib = (key >> 8);
            var ia = (key >> 0);
            var color = Rgba ((byte)ir, (byte)ig, (byte)ib, (byte)ia);
            return color;
        }

        public static UIColor BlendTo (this UIColor fromColor, UIColor toColor, double toFraction)
        {
            nfloat tr, tg, tb, ta;
            if (toColor.CGColor.ColorSpace?.Components >= 3) {
                fromColor.GetRGBA (out tr, out tg, out tb, out ta);
            }
            else {
                toColor.GetWhite (out tr, out ta);
                tg = tb = tr;
            }
            return BlendTo (fromColor, tr, tg, tb, toFraction);
        }

        public static UIColor BlendTo (this UIColor fromColor, double toRed, double toGreen, double toBlue, double toFraction)
        {
            nfloat or, og, ob, oa;
            if (fromColor.CGColor.ColorSpace?.Components >= 3) {
                fromColor.GetRGBA (out or, out og, out ob, out oa);
            }
            else {
                fromColor.GetWhite (out or, out oa);
                og = ob = or;
            }
            nfloat tr = (nfloat)toRed, tg = (nfloat)toGreen, tb = (nfloat)toBlue;
            var f = (float)toFraction;
            var f1 = 1.0f - f;
            return UIColor.FromRGB (f1 * or + f * tr, f1 * og + f * tg, f1 * ob + f * tb);
        }

        public static UIColor BlendTo (this UIColor fromColor, double toGray, double toFraction)
        {
            return BlendTo (fromColor, toGray, toGray, toGray, toFraction);
        }

        public static NGraphics.Color GetNGraphicsColor (this UIColor color)
        {
	        var v = GetColorVector (color);
	        return GetNGraphicsColor (v);
        }

        public static SCNVector4 GetColorVector (this UIColor color)
        {
	        var cgc = color.CGColor;
	        if (cgc is not null)
		        return GetColorVector (cgc);
	        return SCNVector4.UnitW;
        }

	    public static NGraphics.Color GetNGraphicsColor (this SCNVector3 color)
	    {
		    return new NGraphics.Color ((double)color.X, (double)color.Y, (double)color.Z, 1.0);
	    }

	    public static NGraphics.Color GetNGraphicsColor (this SCNVector4 color)
	    {
		    return new NGraphics.Color ((double)color.X, (double)color.Y, (double)color.Z, (double)color.W);
	    }
        
	    public static OpenTK.Vector2 ToVector2 (this CGPoint xy) => new OpenTK.Vector2 ((float)xy.X, (float)xy.Y);

	    public static CGPoint ToCGPoint (this OpenTK.Vector2 xy) => new CGPoint (xy.X, xy.Y);

        public static SCNMatrix4 Rotate (SCNVector3 axis, double radians) => SCNMatrix4.CreateFromAxisAngle (axis, (float)radians);

        public static SCNMatrix4 RotateX (double radians) => SCNMatrix4.CreateRotationX ((float)radians);

        public static SCNMatrix4 RotateY (double radians) => SCNMatrix4.CreateRotationY ((float)radians);

        public static SCNMatrix4 RotateZ (double radians) => SCNMatrix4.CreateRotationZ ((float)radians);

        public static SCNMatrix4 Scale (double scale) => SCNMatrix4.Scale ((float)scale);

        public static SCNMatrix4 Scale (float scale) => SCNMatrix4.Scale (scale);

        public static SCNMatrix4 Scale (double x, double y, double z) => SCNMatrix4.Scale ((float)x, (float)y, (float)z);

        public static SCNMatrix4 Scale (float x, float y, float z) => SCNMatrix4.Scale (x, y, z);

        public static SCNMatrix4 Scale (SCNVector3 v) => SCNMatrix4.Scale (v);

        public static SCNMatrix4 Translate (double x, double y, double z) => SCNMatrix4.CreateTranslation ((float)x, (float)y, (float)z);

        public static SCNMatrix4 Translate (float x, float y, float z) => SCNMatrix4.CreateTranslation (x, y, z);

        public static SCNMatrix4 Translate (SCNVector3 v) => SCNMatrix4.CreateTranslation (v);

        public static CGPoint Xy (double xy) => new CGPoint ((nfloat)xy, (nfloat)xy);

        public static CGPoint Xy (float xy) => new CGPoint (xy, xy);

        public static CGPoint Xy (nfloat xy) => new CGPoint (xy, xy);

        public static CGPoint Xy (double x, double y) => new CGPoint ((nfloat)x, (nfloat)y);

        public static CGPoint Xy (float x, float y) => new CGPoint (x, y);

        public static CGPoint Xy (nfloat x, nfloat y) => new CGPoint (x, y);

        public static CGPoint Xy (SCNVector3 xyz) => new CGPoint (xyz.X, xyz.Y);

        public static SCNVector3 Xyz (CGPoint xy, double z) => new SCNVector3 ((float)xy.X, (float)xy.Y, (float)z);

        public static SCNVector3 Xyz (double xyz) => new SCNVector3 ((float)xyz, (float)xyz, (float)xyz);

        public static SCNVector3 Xyz (float xyz) => new SCNVector3 (xyz, xyz, xyz);

        public static SCNVector3 Xyz (nfloat xyz) => new SCNVector3 ((float)xyz, (float)xyz, (float)xyz);

        public static SCNVector3 Xyz (double x, double y, double z) => new SCNVector3 ((float)x, (float)y, (float)z);

        public static SCNVector3 Xyz (float x, float y, float z) => new SCNVector3 (x, y, z);

        public static SCNVector3 Xyz (nfloat x, nfloat y, nfloat z) => new SCNVector3 ((float)x, (float)y, (float)z);

        public static SCNVector4 Xyzw (SCNVector3 xyz, double w) => new SCNVector4 (xyz, (float)w);

        public static SCNVector4 Xyzw (float xyzw) => new SCNVector4 (xyzw, xyzw, xyzw, xyzw);

        public static SCNVector4 Xyzw (SCNVector3 xyz, float w) => new SCNVector4 (xyz, w);

        public static SCNVector4 WithW (this SCNVector4 xyzw, float w) => new SCNVector4 (xyzw.Xyz, w);

        public static SCNVector4 Xyzw (double x, double y, double z, double w) => new SCNVector4 ((float)x, (float)y, (float)z, (float)w);

        public static SCNVector4 Xyzw (float x, float y, float z, float w) => new SCNVector4 (x, y, z, w);

        public static SCNVector4 Xyzw (nfloat x, nfloat y, nfloat z, nfloat w) => new SCNVector4 ((float)x, (float)y, (float)z, (float)w);

        public static SCNVector4 GetColorVector (this CGColor color)
        {
            var comps = color.Components;
            if (color.ColorSpace?.Components >= 3) {
                var a = comps.Length > 3 ? comps[3] : (nfloat)1.0;
                return Xyzw (comps[0], comps[1], comps[2], a);
            }
            else {
                var a = comps.Length > 1 ? comps[1] : (nfloat)1.0;
                var w = comps[0];
                return Xyzw (w, w, w, a);
            }
        }

        public static SCNVector3 SnapNormal (this SCNVector4 n)
        {
            double x = 0, y = 0, z = 0;
            var ax = Math.Abs (n.X);
            var ay = Math.Abs (n.Y);
            var az = Math.Abs (n.Z);
            if (ax >= ay) {
                if (ax >= az) {
                    x = n.X >= 0 ? 1.0 : -1.0;
                }
                else {
                    z = n.Z >= 0 ? 1.0 : -1.0;
                }
            }
            else {
                if (ay >= az) {
                    y = n.Y >= 0 ? 1.0 : -1.0;
                }
                else {
                    z = n.Z >= 0 ? 1.0 : -1.0;
                }
            }
            return Xyz (x, y, z);
        }

        public static nfloat GetLength (this CGPoint a)
        {
            var dx = a.X;
            var dy = a.Y;
            return (nfloat)Math.Sqrt (dx * dx + dy * dy);
        }

        public static nfloat DistanceTo (this CGPoint a, CGPoint b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (nfloat)Math.Sqrt (dx * dx + dy * dy);
        }

        public static CGPoint Subtract (this CGPoint a, CGPoint b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return new CGPoint (dx, dy);
        }

        public static SCNVector3 NextOrthogonal (SCNVector3 v)
        {
            var ax = Math.Abs (v.X);
            var ay = Math.Abs (v.Y);
            var az = Math.Abs (v.Z);
            if (ax >= ay) {
                if (ax >= az) return SCNVector3.UnitY;
                else return SCNVector3.UnitX;
            }
            else {
                if (ay >= az) return SCNVector3.UnitZ;
                else return SCNVector3.UnitX;
            }
        }

        public static SCNMatrix4 OrientY (SCNVector3 start, SCNVector3 end)
        {
            var b = end;
            var by = start - end;
            var length = by.Length;

            var up = SCNVector3.UnitZ;
            by.Normalize ();
            SCNVector3 bx =
                (Math.Abs (SCNVector3.Dot (by, up)) > 0.999)
                    ? NextOrthogonal (by)
                    : SCNVector3.Cross (by, up);
            bx.Normalize ();
            if (Math.Abs (SCNVector3.Dot (bx, by)) > 0.999) {
                bx = NextOrthogonal (by);
            }
            var bz = SCNVector3.Cross (bx, by);
            bz.Normalize ();
            bx = SCNVector3.Cross (by, bz);
            bx.Normalize ();

            return new SCNMatrix4 (new SCNVector4 (bx, 0), new SCNVector4 (by, 0), new SCNVector4 (bz, 0), new SCNVector4 (0, 0, 0, 1));
        }

        public static SCNMatrix4 BasisFromOriginNormalZ (SCNVector3 origin, SCNVector3 normal)
        {
            var b = origin;
            var bz = normal;
            bz.Normalize ();

            var up = SCNVector3.UnitY;
            SCNVector3 bx =
                (Math.Abs (SCNVector3.Dot (up, bz)) > 0.999)
                    ? NextOrthogonal (bz)
                    : SCNVector3.Cross (up, bz);
            bx.Normalize ();
            if (Math.Abs (SCNVector3.Dot (bz, bx)) > 0.999) {
                bx = NextOrthogonal (bz);
            }
            var by = SCNVector3.Cross (bz, bx);
            by.Normalize ();
            bx = SCNVector3.Cross (by, bz);
            bx.Normalize ();

            return new SCNMatrix4 (new SCNVector4 (bx, 0), new SCNVector4 (by, 0), new SCNVector4 (bz, 0), new SCNVector4 (b, 1));
        }
        #endif
    }
}
