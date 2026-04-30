#nullable enable

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Numerics;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreAnimation;
using CoreGraphics;
using Foundation;
using Metal;
using ModelIO;
using ObjCRuntime;
using UIKit;

// ReSharper disable InconsistentNaming

namespace SceneKit
{
    [Flags]
    public enum SCNDebugOptions : ulong
    {
        None = 0,
        ShowPhysicsShapes = 1 << 0,
        ShowWireframe = 1 << 1,
    }

    public enum SCNAntialiasingMode : long
    {
        None,
        Multisampling2X,
        Multisampling4X,
    }

    public enum SCNFilterMode : long
    {
        None,
        Nearest,
        Linear,
    }

    public enum SCNWrapMode : long
    {
        Clamp,
        Repeat,
        ClampToBorder,
        Mirror,
    }

    public enum SCNCullMode : long
    {
        Back,
        Front,
    }

    [Flags]
    public enum SCNBillboardAxis : ulong
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        All = X | Y | Z,
    }

    [Flags]
    public enum SCNPhysicsCollisionCategory : ulong
    {
        Default = 1 << 0,
        Static = 1 << 1,
        All = ulong.MaxValue,
    }

    public enum SCNGeometryPrimitiveType : long
    {
        Triangles,
        TriangleStrip,
        Line,
        Point,
    }

    public enum SCNGeometrySourceSemantics
    {
        Vertex,
        Normal,
        Color,
        Texcoord,
        BoneWeights,
        BoneIndices,
    }

    public enum SCNPhysicsBodyType : long
    {
        Static,
        Dynamic,
        Kinematic,
    }

    public enum SCNPhysicsShapeType : long
    {
        BoundingBox,
        ConvexHull,
        ConcavePolyhedron,
    }

    public enum SCNHitTestSearchMode : long
    {
        Closest,
        All,
        Any,
    }

    public static class SCNLightingModel
    {
        public const string Blinn = "blinn";
        public const string Lambert = "lambert";
        public const string Phong = "phong";
        public const string Constant = "constant";
        public const string PhysicallyBased = "physicallyBased";
    }

    public static class SCNLightType
    {
        public const string Omni = "omni";
        public const string Spot = "spot";
        public const string Directional = "directional";
        public const string Ambient = "ambient";
    }

    public static class SCNHitTest
    {
        public static NSString SearchModeKey { get; } = new ("searchMode");
        public static NSString BackFaceCullingKey { get; } = new ("backFaceCulling");
        public static NSString SortResultsKey { get; } = new ("sortResults");
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct SCNVector3 : IEquatable<SCNVector3>
    {
        public nfloat X;
        public nfloat Y;
        public nfloat Z;

        public static readonly SCNVector3 Zero = new (0, 0, 0);
        public static readonly SCNVector3 UnitX = new (1, 0, 0);
        public static readonly SCNVector3 UnitY = new (0, 1, 0);
        public static readonly SCNVector3 UnitZ = new (0, 0, 1);

        public SCNVector3 (nfloat x, nfloat y, nfloat z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SCNVector3 (SCNVector4 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public readonly nfloat Length => (nfloat)Math.Sqrt ((double)LengthSquared);
        public readonly nfloat LengthSquared => X * X + Y * Y + Z * Z;

        public void Normalize ()
        {
            var len = Length;
            if (len == 0) {
                return;
            }
            X /= len;
            Y /= len;
            Z /= len;
        }

        public readonly bool Equals (SCNVector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override readonly bool Equals (object? obj) => obj is SCNVector3 v && Equals (v);
        public override readonly int GetHashCode () => HashCode.Combine (X, Y, Z);
        public override readonly string ToString () => $"({X}, {Y}, {Z})";

        public static SCNVector3 Normalize (SCNVector3 v)
        {
            v.Normalize ();
            return v;
        }

        public static nfloat Dot (SCNVector3 left, SCNVector3 right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

        public static SCNVector3 Cross (SCNVector3 left, SCNVector3 right) =>
            new (
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X);

        public static SCNVector3 TransformNormal (SCNVector3 normal, SCNMatrix4 matrix)
        {
            return new SCNVector3 (
                normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
                normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
                normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
        }

        // Accept any matrix-like value so callers can pass older matrix wrappers without SceneKit depending on them.
        public static SCNVector3 TransformNormal (SCNVector3 normal, object? matrix)
        {
            return matrix is SCNMatrix4 m ? TransformNormal (normal, m) : normal;
        }

        public static SCNVector3 operator + (SCNVector3 left, SCNVector3 right) => new (left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static SCNVector3 operator - (SCNVector3 left, SCNVector3 right) => new (left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static SCNVector3 operator - (SCNVector3 v) => new (-v.X, -v.Y, -v.Z);
        public static SCNVector3 operator * (SCNVector3 v, nfloat s) => new (v.X * s, v.Y * s, v.Z * s);
        public static SCNVector3 operator * (nfloat s, SCNVector3 v) => v * s;
        public static SCNVector3 operator / (SCNVector3 v, nfloat s) => new (v.X / s, v.Y / s, v.Z / s);
        public static bool operator == (SCNVector3 left, SCNVector3 right) => left.Equals (right);
        public static bool operator != (SCNVector3 left, SCNVector3 right) => !left.Equals (right);
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct SCNVector4 : IEquatable<SCNVector4>
    {
        public nfloat X;
        public nfloat Y;
        public nfloat Z;
        public nfloat W;

        public static readonly SCNVector4 Zero = new (0, 0, 0, 0);
        public static readonly SCNVector4 UnitX = new (1, 0, 0, 0);
        public static readonly SCNVector4 UnitY = new (0, 1, 0, 0);
        public static readonly SCNVector4 UnitZ = new (0, 0, 1, 0);
        public static readonly SCNVector4 UnitW = new (0, 0, 0, 1);

        public SCNVector4 (nfloat x, nfloat y, nfloat z, nfloat w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public SCNVector4 (SCNVector3 xyz, nfloat w)
        {
            X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
            W = w;
        }

        public readonly nfloat Length => (nfloat)Math.Sqrt ((double)LengthSquared);
        public readonly nfloat LengthSquared => X * X + Y * Y + Z * Z + W * W;

        public void Normalize ()
        {
            var len = Length;
            if (len == 0) {
                return;
            }
            X /= len;
            Y /= len;
            Z /= len;
            W /= len;
        }

        public readonly bool Equals (SCNVector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override readonly bool Equals (object? obj) => obj is SCNVector4 v && Equals (v);
        public override readonly int GetHashCode () => HashCode.Combine (X, Y, Z, W);
        public override readonly string ToString () => $"({X}, {Y}, {Z}, {W})";

        public static nfloat Dot (SCNVector4 left, SCNVector4 right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;

        public static SCNVector4 operator + (SCNVector4 left, SCNVector4 right) => new (left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        public static SCNVector4 operator - (SCNVector4 left, SCNVector4 right) => new (left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        public static SCNVector4 operator - (SCNVector4 v) => new (-v.X, -v.Y, -v.Z, -v.W);
        public static SCNVector4 operator * (SCNVector4 v, nfloat s) => new (v.X * s, v.Y * s, v.Z * s, v.W * s);
        public static SCNVector4 operator * (nfloat s, SCNVector4 v) => v * s;
        public static SCNVector4 operator / (SCNVector4 v, nfloat s) => new (v.X / s, v.Y / s, v.Z / s, v.W / s);
        public static bool operator == (SCNVector4 left, SCNVector4 right) => left.Equals (right);
        public static bool operator != (SCNVector4 left, SCNVector4 right) => !left.Equals (right);
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct SCNQuaternion : IEquatable<SCNQuaternion>
    {
        public nfloat X;
        public nfloat Y;
        public nfloat Z;
        public nfloat W;

        public static readonly SCNQuaternion Identity = new (0, 0, 0, 1);

        public SCNQuaternion (nfloat x, nfloat y, nfloat z, nfloat w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Normalize ()
        {
            var len = (nfloat)Math.Sqrt ((double)(X * X + Y * Y + Z * Z + W * W));
            if (len == 0) {
                return;
            }
            X /= len;
            Y /= len;
            Z /= len;
            W /= len;
        }

        public static SCNQuaternion FromAxisAngle (SCNVector3 axis, float angle)
        {
            var n = axis;
            n.Normalize ();
            var half = angle * 0.5f;
            var s = (float)Math.Sin (half);
            var c = (float)Math.Cos (half);
            return new SCNQuaternion (n.X * s, n.Y * s, n.Z * s, c);
        }

        public readonly bool Equals (SCNQuaternion other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override readonly bool Equals (object? obj) => obj is SCNQuaternion q && Equals (q);
        public override readonly int GetHashCode () => HashCode.Combine (X, Y, Z, W);
        public static bool operator == (SCNQuaternion left, SCNQuaternion right) => left.Equals (right);
        public static bool operator != (SCNQuaternion left, SCNQuaternion right) => !left.Equals (right);
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct SCNMatrix4 : IEquatable<SCNMatrix4>
    {
        public nfloat M11, M12, M13, M14;
        public nfloat M21, M22, M23, M24;
        public nfloat M31, M32, M33, M34;
        public nfloat M41, M42, M43, M44;

        public static readonly SCNMatrix4 Identity = new () {
            M11 = 1, M22 = 1, M33 = 1, M44 = 1,
        };

        public SCNMatrix4 (
            nfloat m11, nfloat m12, nfloat m13, nfloat m14,
            nfloat m21, nfloat m22, nfloat m23, nfloat m24,
            nfloat m31, nfloat m32, nfloat m33, nfloat m34,
            nfloat m41, nfloat m42, nfloat m43, nfloat m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        public readonly SCNVector4 Column0 => new (M11, M21, M31, M41);
        public readonly SCNVector4 Column1 => new (M12, M22, M32, M42);
        public readonly SCNVector4 Column2 => new (M13, M23, M33, M43);
        public readonly SCNVector4 Column3 => new (M14, M24, M34, M44);

        public static SCNMatrix4 CreateTranslation (nfloat x, nfloat y, nfloat z)
        {
            var m = Identity;
            m.M14 = x;
            m.M24 = y;
            m.M34 = z;
            return m;
        }

        public static SCNMatrix4 CreateScale (nfloat x, nfloat y, nfloat z)
        {
            var m = Identity;
            m.M11 = x;
            m.M22 = y;
            m.M33 = z;
            return m;
        }

        public static SCNMatrix4 CreateRotationX (double radians)
        {
            var c = (nfloat)Math.Cos (radians);
            var s = (nfloat)Math.Sin (radians);
            return new SCNMatrix4 (
                1, 0, 0, 0,
                0, c, -s, 0,
                0, s, c, 0,
                0, 0, 0, 1);
        }

        public static SCNMatrix4 CreateRotationY (double radians)
        {
            var c = (nfloat)Math.Cos (radians);
            var s = (nfloat)Math.Sin (radians);
            return new SCNMatrix4 (
                c, 0, s, 0,
                0, 1, 0, 0,
                -s, 0, c, 0,
                0, 0, 0, 1);
        }

        public static SCNMatrix4 CreateRotationZ (double radians)
        {
            var c = (nfloat)Math.Cos (radians);
            var s = (nfloat)Math.Sin (radians);
            return new SCNMatrix4 (
                c, -s, 0, 0,
                s, c, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        }

        public static bool TryInvert (SCNMatrix4 matrix, out SCNMatrix4 inverse)
        {
            var ok = Matrix4x4.Invert (matrix.ToNumerics (), out var inv);
            inverse = FromNumerics (inv);
            return ok;
        }

        public static SCNVector4 Transform (SCNVector4 v, SCNMatrix4 m)
        {
            var x = v.X * m.M11 + v.Y * m.M12 + v.Z * m.M13 + v.W * m.M14;
            var y = v.X * m.M21 + v.Y * m.M22 + v.Z * m.M23 + v.W * m.M24;
            var z = v.X * m.M31 + v.Y * m.M32 + v.Z * m.M33 + v.W * m.M34;
            var w = v.X * m.M41 + v.Y * m.M42 + v.Z * m.M43 + v.W * m.M44;
            return new SCNVector4 (x, y, z, w);
        }

        public static SCNVector3 TransformPoint (SCNVector3 v, SCNMatrix4 m)
        {
            var p = Transform (new SCNVector4 (v, 1), m);
            if (p.W != 0) {
                return new SCNVector3 (p.X / p.W, p.Y / p.W, p.Z / p.W);
            }
            return new SCNVector3 (p.X, p.Y, p.Z);
        }

        public readonly bool Equals (SCNMatrix4 other) =>
            M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
            M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
            M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M34 == other.M34 &&
            M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44;

        public override readonly bool Equals (object? obj) => obj is SCNMatrix4 m && Equals (m);
        public override readonly int GetHashCode ()
        {
            var h = new HashCode ();
            h.Add (M11); h.Add (M12); h.Add (M13); h.Add (M14);
            h.Add (M21); h.Add (M22); h.Add (M23); h.Add (M24);
            h.Add (M31); h.Add (M32); h.Add (M33); h.Add (M34);
            h.Add (M41); h.Add (M42); h.Add (M43); h.Add (M44);
            return h.ToHashCode ();
        }
        public static bool operator == (SCNMatrix4 left, SCNMatrix4 right) => left.Equals (right);
        public static bool operator != (SCNMatrix4 left, SCNMatrix4 right) => !left.Equals (right);

        public static SCNMatrix4 operator * (SCNMatrix4 left, SCNMatrix4 right)
        {
            return FromNumerics (left.ToNumerics () * right.ToNumerics ());
        }

        readonly Matrix4x4 ToNumerics ()
        {
            return new Matrix4x4 (
                (float)M11, (float)M12, (float)M13, (float)M14,
                (float)M21, (float)M22, (float)M23, (float)M24,
                (float)M31, (float)M32, (float)M33, (float)M34,
                (float)M41, (float)M42, (float)M43, (float)M44);
        }

        static SCNMatrix4 FromNumerics (Matrix4x4 m)
        {
            return new SCNMatrix4 (
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }
    }

    public class SCNShaderModifiers : NSObject
    {
        public string? EntryPointGeometry { get; set; }
        public string? EntryPointSurface { get; set; }
        public string? EntryPointLightingModel { get; set; }
        public string? EntryPointFragment { get; set; }
    }

    public class SCNProgram : NSObject
    {
        public string? VertexFunctionName { get; set; }
        public string? FragmentFunctionName { get; set; }
    }

    public class SCNMaterialProperty : NSObject
    {
        public object? Contents { get; private set; }
        public UIColor? ContentColor
        {
            get => Contents as UIColor;
            set => Contents = value;
        }
        public UIImage? ContentImage
        {
            get => Contents as UIImage;
            set => Contents = value;
        }

        public SCNMatrix4 ContentsTransform { get; set; } = SCNMatrix4.Identity;
        public SCNFilterMode MagnificationFilter { get; set; } = SCNFilterMode.Linear;
        public SCNWrapMode WrapT { get; set; } = SCNWrapMode.Clamp;
        public SCNWrapMode WrapS { get; set; } = SCNWrapMode.Clamp;
        public nfloat Intensity { get; set; } = 1;

        public void SetContents (INativeObject? contents)
        {
            Contents = contents;
        }
    }

    public class SCNMaterial : NSObject
    {
        readonly SCNMaterialProperty diffuse = new ();
        readonly SCNMaterialProperty roughness = new ();
        readonly SCNMaterialProperty metalness = new ();
        readonly SCNMaterialProperty emission = new ();
        readonly SCNMaterialProperty normal = new ();
        readonly SCNMaterialProperty selfIllumination = new ();
        readonly SCNMaterialProperty specular = new ();
        readonly SCNMaterialProperty ambientOcclusion = new ();
        readonly SCNMaterialProperty transparent = new ();
        readonly SCNMaterialProperty multiply = new ();

        public static SCNMaterial Create () => new ();

        public SCNMaterialProperty Diffuse => diffuse;
        public SCNMaterialProperty Roughness => roughness;
        public SCNMaterialProperty Metalness => metalness;
        public SCNMaterialProperty Emission => emission;
        public SCNMaterialProperty Normal => normal;
        public SCNMaterialProperty SelfIllumination => selfIllumination;
        public SCNMaterialProperty Specular => specular;
        public SCNMaterialProperty AmbientOcclusion => ambientOcclusion;
        public SCNMaterialProperty Transparent => transparent;
        public SCNMaterialProperty Multiply => multiply;

        public string LightingModelName { get; set; } = SCNLightingModel.Blinn;
        public SCNCullMode CullMode { get; set; } = SCNCullMode.Back;
        public bool DoubleSided { get; set; }
        public bool IsDoubleSided { get => DoubleSided; set => DoubleSided = value; }
        public bool ReadsFromDepthBuffer { get; set; } = true;
        public bool WritesToDepthBuffer { get; set; } = true;
        public nfloat Shininess { get; set; }
        public SCNShaderModifiers? ShaderModifiers { get; set; }
        public SCNProgram? Program { get; set; }

        public NSObject Copy ()
        {
            return new SCNMaterial {
                LightingModelName = LightingModelName,
                CullMode = CullMode,
                DoubleSided = DoubleSided,
                ReadsFromDepthBuffer = ReadsFromDepthBuffer,
                WritesToDepthBuffer = WritesToDepthBuffer,
                Shininess = Shininess,
                ShaderModifiers = ShaderModifiers,
                Program = Program,
            };
        }

        public NSObject Copy (NSZone? zone)
        {
            return Copy ();
        }
    }

    public class SCNGeometrySource : NSObject
    {
        public SCNGeometrySourceSemantics Semantic { get; set; }
        public int VectorCount { get; set; }

        public static SCNGeometrySource FromData (NSData data, SCNGeometrySourceSemantics semantic, int vectorCount, bool floatComponents, int componentsPerVector, int bytesPerComponent, int dataOffset, int dataStride)
        {
            return new SCNGeometrySource {
                Semantic = semantic,
                VectorCount = vectorCount,
            };
        }

        public static SCNGeometrySource FromVertices (SCNVector3[] vertices)
        {
            return new SCNGeometrySource {
                Semantic = SCNGeometrySourceSemantics.Vertex,
                VectorCount = vertices.Length,
            };
        }

        public static SCNGeometrySource FromNormals (SCNVector3[] normals)
        {
            return new SCNGeometrySource {
                Semantic = SCNGeometrySourceSemantics.Normal,
                VectorCount = normals.Length,
            };
        }

        public static SCNGeometrySource FromTextureCoordinates (CGPoint[] texCoords)
        {
            return new SCNGeometrySource {
                Semantic = SCNGeometrySourceSemantics.Texcoord,
                VectorCount = texCoords.Length,
            };
        }
    }

    public class SCNGeometryElement : NSObject
    {
        public SCNGeometryPrimitiveType PrimitiveType { get; set; }
        public int PrimitiveCount { get; set; }

        public static SCNGeometryElement FromData (NSData data, SCNGeometryPrimitiveType primitiveType, int primitiveCount, int bytesPerIndex)
        {
            return new SCNGeometryElement {
                PrimitiveType = primitiveType,
                PrimitiveCount = primitiveCount,
            };
        }
    }

    public class SCNGeometry : NSObject
    {
        SCNMaterial[] materials = Array.Empty<SCNMaterial> ();

        public virtual SCNMaterial[] Materials
        {
            get => materials;
            set => materials = value ?? Array.Empty<SCNMaterial> ();
        }

        public virtual SCNMaterial? FirstMaterial
        {
            get => materials.Length > 0 ? materials[0] : null;
            set
            {
                if (value is null) {
                    materials = Array.Empty<SCNMaterial> ();
                }
                else if (materials.Length == 0) {
                    materials = new[] { value };
                }
                else {
                    materials[0] = value;
                }
            }
        }

        public virtual void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            min = SCNVector3.Zero;
            max = SCNVector3.Zero;
        }

        public static SCNGeometry Create (SCNGeometrySource[] sources, SCNGeometryElement[] elements)
        {
            return new SCNGeometryCollection {
                Sources = sources,
                Elements = elements,
            };
        }

        public NSObject Copy ()
        {
            var g = new SCNGeometry ();
            g.Materials = Materials.ToArray ();
            return g;
        }

        public NSObject Copy (NSZone? zone)
        {
            return Copy ();
        }
    }

    class SCNGeometryCollection : SCNGeometry
    {
        public SCNGeometrySource[] Sources { get; set; } = Array.Empty<SCNGeometrySource> ();
        public SCNGeometryElement[] Elements { get; set; } = Array.Empty<SCNGeometryElement> ();
    }

    public class SCNBox : SCNGeometry
    {
        public nfloat Width { get; set; }
        public nfloat Height { get; set; }
        public nfloat Length { get; set; }
        public nfloat ChamferRadius { get; set; }
        public int ChamferSegmentCount { get; set; }

        public static SCNBox Create (nfloat width, nfloat height, nfloat length, nfloat chamferRadius)
        {
            return new SCNBox {
                Width = width,
                Height = height,
                Length = length,
                ChamferRadius = chamferRadius,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var hx = Width * 0.5f;
            var hy = Height * 0.5f;
            var hz = Length * 0.5f;
            min = new SCNVector3 (-hx, -hy, -hz);
            max = new SCNVector3 (hx, hy, hz);
        }
    }

    public class SCNCapsule : SCNGeometry
    {
        public nfloat CapRadius { get; set; }
        public nfloat Height { get; set; }
        public int HeightSegmentCount { get; set; }
        public int RadialSegmentCount { get; set; }
        public int CapSegmentCount { get; set; }

        public static SCNCapsule Create (nfloat capRadius, nfloat height)
        {
            return new SCNCapsule {
                CapRadius = capRadius,
                Height = height,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var hy = Height * 0.5f;
            min = new SCNVector3 (-CapRadius, -hy, -CapRadius);
            max = new SCNVector3 (CapRadius, hy, CapRadius);
        }
    }

    public class SCNCylinder : SCNGeometry
    {
        public nfloat Radius { get; set; }
        public nfloat Height { get; set; }
        public int HeightSegmentCount { get; set; }
        public int RadialSegmentCount { get; set; }

        public static SCNCylinder Create (nfloat radius, nfloat height)
        {
            return new SCNCylinder {
                Radius = radius,
                Height = height,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var hy = Height * 0.5f;
            min = new SCNVector3 (-Radius, -hy, -Radius);
            max = new SCNVector3 (Radius, hy, Radius);
        }
    }

    public class SCNTube : SCNGeometry
    {
        public nfloat InnerRadius { get; set; }
        public nfloat OuterRadius { get; set; }
        public nfloat Height { get; set; }
        public int HeightSegmentCount { get; set; }
        public int RadialSegmentCount { get; set; }

        public static SCNTube Create (nfloat innerRadius, nfloat outerRadius, nfloat height)
        {
            return new SCNTube {
                InnerRadius = innerRadius,
                OuterRadius = outerRadius,
                Height = height,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var hy = Height * 0.5f;
            min = new SCNVector3 (-OuterRadius, -hy, -OuterRadius);
            max = new SCNVector3 (OuterRadius, hy, OuterRadius);
        }
    }

    public class SCNSphere : SCNGeometry
    {
        public nfloat Radius { get; set; }
        public int SegmentCount { get; set; }

        public static SCNSphere Create (nfloat radius)
        {
            return new SCNSphere {
                Radius = radius,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            min = new SCNVector3 (-Radius, -Radius, -Radius);
            max = new SCNVector3 (Radius, Radius, Radius);
        }
    }

    public class SCNCone : SCNGeometry
    {
        public nfloat TopRadius { get; set; }
        public nfloat BottomRadius { get; set; }
        public nfloat Height { get; set; }

        public static SCNCone Create (nfloat topRadius, nfloat bottomRadius, nfloat height)
        {
            return new SCNCone {
                TopRadius = topRadius,
                BottomRadius = bottomRadius,
                Height = height,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var r = Math.Max ((double)TopRadius, (double)BottomRadius);
            var hy = Height * 0.5f;
            min = new SCNVector3 ((nfloat)(-r), -hy, (nfloat)(-r));
            max = new SCNVector3 ((nfloat)r, hy, (nfloat)r);
        }
    }

    public class SCNPlane : SCNGeometry
    {
        public nfloat Width { get; set; }
        public nfloat Height { get; set; }

        public static SCNPlane Create (nfloat width, nfloat height)
        {
            return new SCNPlane {
                Width = width,
                Height = height,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            min = new SCNVector3 (-Width * 0.5f, -Height * 0.5f, 0);
            max = new SCNVector3 (Width * 0.5f, Height * 0.5f, 0);
        }
    }

    public class SCNTorus : SCNGeometry
    {
        public nfloat RingRadius { get; set; }
        public nfloat PipeRadius { get; set; }

        public static SCNTorus Create (nfloat ringRadius, nfloat pipeRadius)
        {
            return new SCNTorus {
                RingRadius = ringRadius,
                PipeRadius = pipeRadius,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var r = RingRadius + PipeRadius;
            min = new SCNVector3 (-r, -PipeRadius, -r);
            max = new SCNVector3 (r, PipeRadius, r);
        }
    }

    public class SCNShape : SCNGeometry
    {
        public UIBezierPath? Path { get; set; }
        public nfloat ExtrusionDepth { get; set; }
        public nfloat ChamferRadius { get; set; }

        public static SCNShape Create (UIBezierPath path, nfloat extrusionDepth)
        {
            return new SCNShape {
                Path = path,
                ExtrusionDepth = extrusionDepth,
            };
        }
    }

    public class SCNText : SCNGeometry
    {
        public NSObject? String { get; set; }
        public UIFont? Font { get; set; }
        public nfloat Flatness { get; set; }
        public CGRect ContainerFrame { get; set; }
        public NSString? AlignmentMode { get; set; }
        public bool Wrapped { get; set; }
        public nfloat ExtrusionDepth { get; set; }

        public static SCNText Create (string text, nfloat extrusionDepth)
        {
            return new SCNText {
                String = new NSString (text),
                ExtrusionDepth = extrusionDepth,
            };
        }

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            var s = String?.ToString () ?? string.Empty;
            var width = Math.Max (1, s.Length) * 0.5;
            var height = 1.0;
            min = new SCNVector3 (0, 0, 0);
            max = new SCNVector3 ((nfloat)width, (nfloat)height, 0.01f + ExtrusionDepth);
        }
    }

    public class SCNFloor : SCNGeometry
    {
        public nfloat Reflectivity { get; set; }
        public nfloat ReflectionFalloffEnd { get; set; }

        public static SCNFloor Create () => new ();

        public override void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            min = new SCNVector3 (-10000, -0.001f, -10000);
            max = new SCNVector3 (10000, 0.001f, 10000);
        }
    }

    public class SCNSkinner : NSObject
    {
        public SCNGeometry? BaseGeometry { get; set; }
        public SCNNode[] Bones { get; set; } = Array.Empty<SCNNode> ();
        public SCNMatrix4[] BoneInverseBindTransforms { get; set; } = Array.Empty<SCNMatrix4> ();
        public SCNGeometrySource? BoneWeights { get; set; }
        public SCNGeometrySource? BoneIndices { get; set; }

        public static SCNSkinner Create (SCNGeometry baseGeometry, SCNNode[] bones, SCNMatrix4[] boneInverseBindTransforms, SCNGeometrySource boneWeights, SCNGeometrySource boneIndices)
        {
            return new SCNSkinner {
                BaseGeometry = baseGeometry,
                Bones = bones,
                BoneInverseBindTransforms = boneInverseBindTransforms,
                BoneWeights = boneWeights,
                BoneIndices = boneIndices,
            };
        }
    }

    public abstract class SCNConstraint : NSObject
    {
    }

    public class SCNBillboardConstraint : SCNConstraint
    {
        public SCNBillboardAxis FreeAxes { get; set; } = SCNBillboardAxis.All;
        public static SCNBillboardConstraint Create () => new ();
    }

    public class SCNTransformConstraint : SCNConstraint
    {
        public bool InWorldSpace { get; set; }
        public Func<SCNMatrix4, SCNNode, SCNMatrix4>? Handler { get; set; }

        public static SCNTransformConstraint Create (bool inWorldSpace, Func<SCNMatrix4, SCNNode, SCNMatrix4> handler)
        {
            return new SCNTransformConstraint {
                InWorldSpace = inWorldSpace,
                Handler = handler,
            };
        }
    }

    public class SCNAction : NSObject
    {
        public Action<SCNNode>? Apply { get; set; }
        public double Duration { get; set; }

        public static SCNAction RotateBy (nfloat angle, SCNVector3 axis, double duration)
        {
            return new SCNAction {
                Duration = duration,
                Apply = node => {
                    var r = node.Rotation;
                    r.X = axis.X;
                    r.Y = axis.Y;
                    r.Z = axis.Z;
                    r.W += angle;
                    node.Rotation = r;
                },
            };
        }

        public static SCNAction RepeatActionForever (SCNAction action)
        {
            return new SCNAction {
                Duration = action.Duration,
                Apply = action.Apply,
            };
        }
    }

    public class SCNCamera : NSObject
    {
        public nfloat ZNear { get; set; } = 1;
        public nfloat ZFar { get; set; } = 1000;
        public nfloat YFov { get; set; } = 60;
        public bool WantsHdr { get; set; }
        public bool WantsDepthOfField { get; set; }
        public nfloat FocusDistance { get; set; }
        public nfloat FStop { get; set; } = 1;
        public nfloat ScreenSpaceAmbientOcclusionIntensity { get; set; }
        public SCNMatrix4 ProjectionTransform { get; set; } = SCNMatrix4.Identity;
    }

    public class SCNLight : NSObject
    {
        public string LightType { get; set; } = SCNLightType.Omni;
        public bool CastsShadow { get; set; }
        public NSObject? Color { get; set; }
        public nfloat Intensity { get; set; }
        public NSObject? ShadowColor { get; set; }
        public nfloat AttenuationStartDistance { get; set; }
        public nfloat AttenuationEndDistance { get; set; }
        public nfloat ZNear { get; set; }
        public nfloat ZFar { get; set; }
        public nfloat SpotInnerAngle { get; set; }
        public nfloat SpotOuterAngle { get; set; }

        public static SCNLight Create () => new ();
    }

    public class SCNParticleSystem : NSObject
    {
        public static SCNParticleSystem? Create (string particleSystemName, string directory)
        {
            return new SCNParticleSystem ();
        }
    }

    public class SCNAudioSource : NSObject
    {
        public static SCNAudioSource? FromFile (string path) => new SCNAudioSource ();
    }

    public class SCNAudioPlayer : NSObject
    {
        public SCNAudioPlayer (NSObject audioNode)
        {
        }
    }

    public class SCNPhysicsShapeOptions : NSObject
    {
        public bool KeepAsCompound { get; set; }
        public SCNPhysicsShapeType ShapeType { get; set; } = SCNPhysicsShapeType.BoundingBox;
    }

    public class SCNPhysicsShape : NSObject
    {
        public SCNGeometry? Geometry { get; set; }
        public SCNNode? Node { get; set; }
        public SCNPhysicsShapeOptions? Options { get; set; }
        public SCNPhysicsShape[]? Shapes { get; set; }
        public SCNMatrix4[]? Transforms { get; set; }

        public static SCNPhysicsShape Create (SCNGeometry geometry)
        {
            return new SCNPhysicsShape {
                Geometry = geometry,
            };
        }

        public static SCNPhysicsShape Create (SCNGeometry geometry, SCNPhysicsShapeType shapeType)
        {
            return new SCNPhysicsShape {
                Geometry = geometry,
                Options = new SCNPhysicsShapeOptions {
                    ShapeType = shapeType,
                },
            };
        }

        public static SCNPhysicsShape Create (SCNGeometry geometry, SCNPhysicsShapeOptions options)
        {
            return new SCNPhysicsShape {
                Geometry = geometry,
                Options = options,
            };
        }

        public static SCNPhysicsShape Create (SCNNode node, SCNPhysicsShapeOptions options)
        {
            return new SCNPhysicsShape {
                Node = node,
                Options = options,
            };
        }

        public static SCNPhysicsShape Create (SCNPhysicsShape[] shapes, SCNMatrix4[] transforms)
        {
            return new SCNPhysicsShape {
                Shapes = shapes,
                Transforms = transforms,
                Options = new SCNPhysicsShapeOptions {
                    KeepAsCompound = true,
                },
            };
        }
    }

    public class SCNPhysicsBody : NSObject
    {
        public SCNPhysicsBodyType Type { get; set; }
        public SCNPhysicsShape? PhysicsShape { get; set; }
        public nfloat Mass { get; set; } = 1;
        public nfloat Friction { get; set; }
        public nuint CategoryBitMask { get; set; } = (nuint)SCNPhysicsCollisionCategory.Default;
        public nuint CollisionBitMask { get; set; } = unchecked ((nuint)ulong.MaxValue);
        public nuint ContactTestBitMask { get; set; }
        public bool AffectedByGravity { get; set; } = true;
        public SCNVector3 Velocity { get; set; }
        public SCNVector4 AngularVelocity { get; set; }
        public SCNVector3 CenterOfMassOffset { get; set; }

        public static SCNPhysicsBody CreateBody (SCNPhysicsBodyType type, SCNPhysicsShape? shape)
        {
            return new SCNPhysicsBody {
                Type = type,
                PhysicsShape = shape,
            };
        }

        public void ResetTransform () { }
        public void ClearAllForces () { }
        public void SetResting (bool resting) { }
        public void ApplyForce (SCNVector3 force, SCNVector3 atPosition, bool asImpulse)
        {
            if (!asImpulse) {
                Velocity += force;
            }
        }
    }

    public abstract class SCNPhysicsBehavior : NSObject
    {
    }

    public class SCNPhysicsSliderJoint : SCNPhysicsBehavior
    {
        public SCNPhysicsBody? BodyA { get; private set; }
        public SCNPhysicsBody? BodyB { get; private set; }
        public SCNVector3 AxisA { get; private set; }
        public SCNVector3 AxisB { get; private set; }
        public SCNVector3 AnchorA { get; private set; }
        public SCNVector3 AnchorB { get; private set; }

        public nfloat MotorTargetLinearVelocity { get; set; }
        public nfloat MinimumLinearLimit { get; set; }
        public nfloat MaximumLinearLimit { get; set; }
        public nfloat MotorTargetAngularVelocity { get; set; }
        public nfloat MotorMaximumTorque { get; set; }

        public static SCNPhysicsSliderJoint Create (SCNPhysicsBody bodyA, SCNVector3 axisA, SCNVector3 anchorA, SCNPhysicsBody bodyB, SCNVector3 axisB, SCNVector3 anchorB)
        {
            return new SCNPhysicsSliderJoint {
                BodyA = bodyA,
                BodyB = bodyB,
                AxisA = axisA,
                AxisB = axisB,
                AnchorA = anchorA,
                AnchorB = anchorB,
            };
        }
    }

    public class SCNPhysicsContact : NSObject
    {
        public SCNNode NodeA { get; set; } = SCNNode.Create ();
        public SCNNode NodeB { get; set; } = SCNNode.Create ();
        public SCNVector3 ContactPoint { get; set; }
        public SCNVector3 ContactNormal { get; set; }
    }

    public class SCNPhysicsContactEventArgs : EventArgs
    {
        public SCNPhysicsContact Contact { get; }

        public SCNPhysicsContactEventArgs (SCNPhysicsContact contact)
        {
            Contact = contact;
        }
    }

    public class SCNPhysicsWorld : NSObject
    {
        readonly List<SCNPhysicsBehavior> behaviors = new ();

        public SCNVector3 Gravity { get; set; } = new (0, -9.81f, 0);
        public double TimeStep { get; set; } = 1.0 / 60.0;
        public nfloat Speed { get; set; } = 1;
        public NSObject? WeakContactDelegate { get; set; }

        public event EventHandler<SCNPhysicsContactEventArgs>? DidBeginContact;

        public void BeginInvokeOnMainThread (Action action)
        {
            action?.Invoke ();
        }

        public void AddBehavior (SCNPhysicsBehavior behavior)
        {
            if (behavior is null) {
                return;
            }
            behaviors.Add (behavior);
        }

        public void RemoveBehavior (SCNPhysicsBehavior behavior)
        {
            behaviors.Remove (behavior);
        }

        public void RaiseDidBeginContact (SCNPhysicsContact contact)
        {
            DidBeginContact?.Invoke (this, new SCNPhysicsContactEventArgs (contact));
        }
    }

    public class SCNNode : NSObject
    {
        readonly List<SCNNode> children = new ();
        readonly Dictionary<string, SCNAction> actions = new (StringComparer.Ordinal);

        SCNNode? parent;
        SCNMatrix4 transform = SCNMatrix4.Identity;

        public string? Name { get; set; }
        public SCNGeometry? Geometry { get; set; }
        public SCNCamera? Camera { get; set; }
        public SCNLight? Light { get; set; }
        public SCNPhysicsBody? PhysicsBody { get; set; }
        public SCNConstraint[]? Constraints { get; set; }
        public nfloat Opacity { get; set; } = 1;
        public bool Hidden { get; set; }
        public bool CastsShadow { get; set; } = true;
        public int RenderingOrder { get; set; }
        public SCNQuaternion Orientation { get; set; } = SCNQuaternion.Identity;
        public SCNVector4 Rotation { get; set; } = SCNVector4.Zero;
        public SCNVector3 EulerAngles { get; set; } = SCNVector3.Zero;
        public SCNVector3 Scale { get; set; } = new (1, 1, 1);

        public SCNMatrix4 Transform
        {
            get => transform;
            set => transform = value;
        }

        public SCNMatrix4 WorldTransform
        {
            get => parent is null ? Transform : parent.WorldTransform * Transform;
            set
            {
                if (parent is null) {
                    Transform = value;
                }
                else if (SCNMatrix4.TryInvert (parent.WorldTransform, out var pinv)) {
                    Transform = pinv * value;
                }
                else {
                    Transform = value;
                }
            }
        }

        public SCNVector3 Position
        {
            get => new (Transform.M14, Transform.M24, Transform.M34);
            set
            {
                var t = Transform;
                t.M14 = value.X;
                t.M24 = value.Y;
                t.M34 = value.Z;
                Transform = t;
            }
        }

        public SCNVector3 WorldPosition
        {
            get => new (WorldTransform.M14, WorldTransform.M24, WorldTransform.M34);
            set
            {
                var wt = WorldTransform;
                wt.M14 = value.X;
                wt.M24 = value.Y;
                wt.M34 = value.Z;
                WorldTransform = wt;
            }
        }

        public SCNNode PresentationNode => this;
        public SCNNode? ParentNode => parent;
        public SCNNode[] ChildNodes => children.ToArray ();

        public static SCNNode Create () => new ();

        public static SCNNode FromGeometry (SCNGeometry geometry)
        {
            return new SCNNode {
                Geometry = geometry,
            };
        }

        public void AddChildNode (SCNNode child)
        {
            if (child is null) {
                return;
            }
            if (child.parent == this) {
                return;
            }
            child.RemoveFromParentNode ();
            child.parent = this;
            children.Add (child);
        }

        public void AddNodes (params SCNNode[] nodes)
        {
            foreach (var n in nodes) {
                AddChildNode (n);
            }
        }

        public void Add (SCNNode child)
        {
            AddChildNode (child);
        }

        public void RemoveFromParentNode ()
        {
            if (parent is null) {
                return;
            }
            parent.children.Remove (this);
            parent = null;
        }

        public void RunAction (SCNAction action, string key)
        {
            if (action is null) {
                return;
            }
            if (!string.IsNullOrEmpty (key)) {
                actions[key] = action;
            }
            action.Apply?.Invoke (this);
        }

        public void RemoveAllActions ()
        {
            actions.Clear ();
        }

        public SCNVector3 ConvertPositionFromNode (SCNVector3 position, SCNNode? fromNode)
        {
            var worldPos = fromNode is null ? position : SCNMatrix4.TransformPoint (position, fromNode.WorldTransform);
            if (SCNMatrix4.TryInvert (WorldTransform, out var inv)) {
                return SCNMatrix4.TransformPoint (worldPos, inv);
            }
            return worldPos;
        }

        public SCNVector3 ConvertPositionToNode (SCNVector3 position, SCNNode? toNode)
        {
            var worldPos = SCNMatrix4.TransformPoint (position, WorldTransform);
            if (toNode is null) {
                return worldPos;
            }
            return toNode.ConvertPositionFromNode (worldPos, null);
        }

        public SCNVector3 ConvertVectorFromNode (SCNVector3 vector, SCNNode? fromNode)
        {
            var worldVec = fromNode is null ? vector : SCNVector3.TransformNormal (vector, fromNode.WorldTransform);
            if (SCNMatrix4.TryInvert (WorldTransform, out var inv)) {
                return SCNVector3.TransformNormal (worldVec, inv);
            }
            return worldVec;
        }

        public SCNMatrix4 ConvertTransformFromNode (SCNMatrix4 t, SCNNode? fromNode)
        {
            var world = fromNode is null ? t : fromNode.WorldTransform * t;
            if (SCNMatrix4.TryInvert (WorldTransform, out var inv)) {
                return inv * world;
            }
            return world;
        }

        public SCNMatrix4 ConvertTransformToNode (SCNMatrix4 t, SCNNode? toNode)
        {
            var world = WorldTransform * t;
            if (toNode is null) {
                return world;
            }
            if (SCNMatrix4.TryInvert (toNode.WorldTransform, out var inv)) {
                return inv * world;
            }
            return world;
        }

        public virtual void GetBoundingBox (ref SCNVector3 min, ref SCNVector3 max)
        {
            if (Geometry is null) {
                min = SCNVector3.Zero;
                max = SCNVector3.Zero;
                return;
            }
            Geometry.GetBoundingBox (ref min, ref max);
        }

        public virtual void GetBoundingSphere (ref SCNVector3 center, ref nfloat radius)
        {
            var min = SCNVector3.Zero;
            var max = SCNVector3.Zero;
            GetBoundingBox (ref min, ref max);
            center = (min + max) / 2;
            radius = (max - center).Length;
        }

        public NSObject Copy ()
        {
            var n = new SCNNode {
                Name = Name,
                Geometry = Geometry,
                Camera = Camera,
                Light = Light,
                PhysicsBody = PhysicsBody,
                Constraints = Constraints,
                Opacity = Opacity,
                Hidden = Hidden,
                CastsShadow = CastsShadow,
                RenderingOrder = RenderingOrder,
                Orientation = Orientation,
                Rotation = Rotation,
                EulerAngles = EulerAngles,
                Scale = Scale,
                Transform = Transform,
            };
            return n;
        }

        public NSObject Copy (NSZone? zone)
        {
            return Copy ();
        }
    }

    public class SCNHitTestResult : NSObject
    {
        public SCNNode Node { get; set; } = SCNNode.Create ();
        public SCNVector3 WorldCoordinates { get; set; }
        public SCNVector3 WorldNormal { get; set; }
        public SCNVector3 LocalCoordinates { get; set; }
        public SCNVector3 LocalNormal { get; set; }
    }

    public class SCNHitTestOptions : NSObject
    {
        public bool BackFaceCulling { get; set; }
        public bool SortResults { get; set; }
        public bool IgnoreHiddenNodes { get; set; }
        public bool FirstFoundOnly { get; set; }
        public SCNHitTestSearchMode SearchMode { get; set; }
    }

    public class SCNSceneLoadingOptions : NSObject
    {
    }

    public class SCNScene : NSObject
    {
        public SCNNode RootNode { get; set; } = SCNNode.Create ();
        public SCNPhysicsWorld PhysicsWorld { get; set; } = new SCNPhysicsWorld ();
        public SCNMaterialProperty Background { get; } = new SCNMaterialProperty ();
        public SCNMaterialProperty LightingEnvironment { get; } = new SCNMaterialProperty ();

        public static SCNScene Create () => new SCNScene ();

        public static SCNScene? FromUrl (NSUrl url, SCNSceneLoadingOptions options, out NSError? error)
        {
            error = null;
            return new SCNScene ();
        }

        public static SCNScene? FromAsset (MDLAsset asset)
        {
            return new SCNScene ();
        }
    }

    public interface ISCNSceneRenderer
    {
        SCNScene Scene { get; set; }
        SCNNode? PointOfView { get; set; }
    }

    public abstract class SCNSceneRendererDelegate : NSObject
    {
        public virtual void Update (ISCNSceneRenderer renderer, double timeInSeconds) { }
        public virtual void DidSimulatePhysics (ISCNSceneRenderer renderer, double timeInSeconds) { }
    }

    public abstract class SCNViewDelegate : NSObject
    {
    }

    public class SCNView : UIView, ISCNSceneRenderer
    {
        public SCNScene Scene { get; set; } = SCNScene.Create ();
        public SCNNode? PointOfView { get; set; }
        public SCNSceneRendererDelegate? SceneRendererDelegate { get; set; }
        public SCNViewDelegate? Delegate { get; set; }
        public bool AutoenablesDefaultLighting { get; set; }
        public bool AllowsCameraControl { get; set; }
        public bool JitteringEnabled { get; set; }
        public bool RendersContinuously { get; set; }
        public bool Playing { get; set; }
        public bool Loops { get; set; }
        public SCNAntialiasingMode AntialiasingMode { get; set; }
        public SCNDebugOptions DebugOptions { get; set; }
        public nint PreferredFramesPerSecond { get; set; }
        public IMTLDevice? Device { get; set; }
        public IMTLCommandQueue? CommandQueue { get; set; }

        public SCNView ()
        {
            Device = MTLDevice.SystemDefault;
            CommandQueue = Device?.CreateCommandQueue ();
        }

        public SCNView (CGRect frame) : base (frame)
        {
            Device = MTLDevice.SystemDefault;
            CommandQueue = Device?.CreateCommandQueue ();
        }

        public virtual SCNHitTestResult[] HitTest (CGPoint point, SCNHitTestOptions options)
        {
            return Array.Empty<SCNHitTestResult> ();
        }

        public virtual SCNHitTestResult[] HitTest (CGPoint point, NSDictionary options)
        {
            return Array.Empty<SCNHitTestResult> ();
        }

        public virtual SCNVector3 ProjectPoint (SCNVector3 point)
        {
            return point;
        }

        public virtual SCNVector3 UnprojectPoint (SCNVector3 point)
        {
            return point;
        }
    }

    public class SCNLayer : NSObject, ISCNSceneRenderer
    {
        public SCNScene Scene { get; set; } = SCNScene.Create ();
        public SCNNode? PointOfView { get; set; }
        public IMTLDevice? Device { get; set; }
        public IMTLCommandQueue? CommandQueue { get; set; }
    }

    public class SCNRenderer : NSObject, ISCNSceneRenderer
    {
        public SCNScene Scene { get; set; } = SCNScene.Create ();
        public SCNNode? PointOfView { get; set; }
        public IMTLDevice? Device { get; set; }
        public IMTLCommandQueue? CommandQueue { get; set; }

        public static SCNRenderer FromDevice (IMTLDevice device, NSDictionary options)
        {
            return new SCNRenderer {
                Device = device,
                CommandQueue = device.CreateCommandQueue (),
            };
        }

        public static SCNRenderer FromContext (NSObject context, NSDictionary options)
        {
            return new SCNRenderer ();
        }

        public void Render (double timeInSeconds)
        {
        }

        public void Render (double timeInSeconds, CGRect viewport, IMTLCommandBuffer commandBuffer, MTLRenderPassDescriptor renderPassDescriptor)
        {
        }
    }

    public static class SCNTransaction
    {
        static readonly Stack<Action?> completionStack = new ();

        public static double AnimationDuration { get; set; }
        public static CAMediaTimingFunction? AnimationTimingFunction { get; set; }
        public static bool DisableActions { get; set; }

        public static void Begin ()
        {
            completionStack.Push (null);
        }

        public static void Commit ()
        {
            var action = completionStack.Count > 0 ? completionStack.Pop () : null;
            action?.Invoke ();
            AnimationDuration = 0;
            AnimationTimingFunction = null;
            DisableActions = false;
        }

        public static void SetCompletionBlock (Action action)
        {
            if (completionStack.Count == 0) {
                action?.Invoke ();
                return;
            }
            completionStack.Pop ();
            completionStack.Push (action);
        }
    }
}

#endif
