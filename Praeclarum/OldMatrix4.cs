/*
 * This keeps the code of OpenTK's Matrix4 almost intact, except we replace the
 * Vector4 with an OldVector4
 *
 * This is copied from Xamarin because I wrote a lot of code that uses
 * even though it's buggy as hell.
 *
 * New code should not use this thing. But it is useful for porting.
 
Copyright (c) 2006 - 2008 The Open Toolkit library.
Copyright (c) 2014 Xamarin Inc.  All rights reserved
Copyright (c) 2025 Krueger Systems, Inc.  All rights reserved

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

 */
#nullable enable

using System;
using System.Runtime.InteropServices;

using Element = System.Single;

namespace Praeclarum 
{
	// ReSharper disable CompareOfFloatsByEqualityOperator
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public struct OldVector3 : IEquatable<OldVector3>
	{
		/// <summary>
		/// The X component of the OldVector4.
		/// </summary>
		public Element X;

		/// <summary>
		/// The Y component of the OldVector4.
		/// </summary>
		public Element Y;

		/// <summary>
		/// The Z component of the OldVector4.
		/// </summary>
		public Element Z;

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the X-axis.
		/// </summary>
		public readonly static OldVector3 UnitX = new OldVector3(1, 0, 0);

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the Y-axis.
		/// </summary>
		public readonly static OldVector3 UnitY = new OldVector3(0, 1, 0);

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the Z-axis.
		/// </summary>
		public readonly static OldVector3 UnitZ = new OldVector3(0, 0, 1);
		
        public readonly static OldVector3 Zero = new OldVector3(0, 0, 0);

		/// <summary>
		/// Constructs a new OldVector3.
		/// </summary>
		/// <param name="x">The x component of the OldVector3.</param>
		/// <param name="y">The y component of the OldVector3.</param>
		/// <param name="z">The z component of the OldVector3.</param>
		public OldVector3(Element x, Element y, Element z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#if __IOS__ || __MACOS__ || __MACCATALYST__
        public static implicit operator SceneKit.SCNVector3(OldVector3 ov) {
            return new SceneKit.SCNVector3(ov.X, ov.Y, ov.Z);
        }
        public static implicit operator OldVector3(SceneKit.SCNVector3 sv) {
            return new OldVector3((Element)sv.X, (Element)sv.Y, (Element)sv.Z);
        }
#endif

		/// <summary>
		/// Returns a System.String that represents the current OldVector4.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"({X}, {Y}, {Z})";
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals (object? obj)
		{
			if (obj is not OldVector3 vector3)
				return false;

			return this.Equals(vector3);
		}
		
		public bool Equals(OldVector3 other)
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		/// <summary>
		/// Gets the length (magnitude) of the vector.
		/// </summary>
		public Element Length
		{
			get
			{
				return (Element)Math.Sqrt(X * X + Y * Y + Z * Z);
			}
		}
		
		/// <summary>
		/// Scales the OldVector3 to unit length.
		/// </summary>
		public void Normalize()
		{
			Element scale = 1.0f / this.Length;
			X *= scale;
			Y *= scale;
			Z *= scale;
		}
		
		/// <summary>
		/// Scale a vector to unit length
		/// </summary>
		/// <param name="vec">The input vector</param>
		/// <returns>The normalized vector</returns>
		public static OldVector3 Normalize(OldVector3 vec)
		{
			Element scale = 1.0f / vec.Length;
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			return vec;
		}
		
		/// <summary>
		/// Caclulate the cross (vector) product of two vectors
		/// </summary>
		/// <param name="left">First operand</param>
		/// <param name="right">Second operand</param>
		/// <returns>The cross product of the two inputs</returns>
		public static OldVector3 Cross(OldVector3 left, OldVector3 right)
		{
			return new OldVector3(left.Y * right.Z - left.Z * right.Y,
				left.Z * right.X - left.X * right.Z,
				left.X * right.Y - left.Y * right.X);
		}
		
		/// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator +(OldVector3 left, OldVector3 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator -(OldVector3 left, OldVector3 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            return left;
        }

        /// <summary>
        /// Negates an instance.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator -(OldVector3 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator *(OldVector3 vec, Element scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="scale">The scalar.</param>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator *(Element scale, OldVector3 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            return vec;
        }

        /// <summary>
        /// Divides an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector3 operator /(OldVector3 vec, Element scale)
        {
            Element mult = 1.0f / scale;
            vec.X *= mult;
            vec.Y *= mult;
            vec.Z *= mult;
            return vec;
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(OldVector3 left, OldVector3 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equa lright; false otherwise.</returns>
        public static bool operator !=(OldVector3 left, OldVector3 right)
        {
            return !left.Equals(right);
        }
	}

	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public struct OldVector4 : IEquatable<OldVector4>
	{
		/// <summary>
		/// The X component of the OldVector4.
		/// </summary>
		public Element X;

		/// <summary>
		/// The Y component of the OldVector4.
		/// </summary>
		public Element Y;

		/// <summary>
		/// The Z component of the OldVector4.
		/// </summary>
		public Element Z;

		/// <summary>
		/// The W component of the OldVector4.
		/// </summary>
		public Element W;

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the X-axis.
		/// </summary>
		public readonly static OldVector4 UnitX = new OldVector4(1, 0, 0, 0);

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the Y-axis.
		/// </summary>
		public readonly static OldVector4 UnitY = new OldVector4(0, 1, 0, 0);

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the Z-axis.
		/// </summary>
		public readonly static OldVector4 UnitZ = new OldVector4(0, 0, 1, 0);

		/// <summary>
		/// Defines a unit-length OldVector4 that points towards the W-axis.
		/// </summary>
		public readonly static OldVector4 UnitW = new OldVector4(0, 0, 0, 1);

		/// <summary>
		/// Defines a zero-length OldVector4.
		/// </summary>
		public readonly static OldVector4 Zero = new OldVector4(0, 0, 0, 0);
		
		/// <summary>
		/// Constructs a new OldVector4.
		/// </summary>
		/// <param name="x">The x component of the OldVector4.</param>
		/// <param name="y">The y component of the OldVector4.</param>
		/// <param name="z">The z component of the OldVector4.</param>
		/// <param name="w">The z component of the OldVector4.</param>
		public OldVector4(Element x, Element y, Element z, Element w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public OldVector4(OldVector3 xyz, Element w)
		{
			X = xyz.X;
            Y = xyz.Y;
            Z = xyz.Z;
			W = w;
		}

#if __IOS__ || __MACOS__ || __MACCATALYST__
        public static implicit operator SceneKit.SCNVector4(OldVector4 ov) {
            return new SceneKit.SCNVector4(ov.X, ov.Y, ov.Z, ov.W);
        }
        public static implicit operator OldVector4(SceneKit.SCNVector4 sv) {
            return new OldVector4((Element)sv.X, (Element)sv.Y, (Element)sv.Z, (Element)sv.W);
        }
#endif

		/// <summary>
		/// Returns a System.String that represents the current OldVector4.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"({X}, {Y}, {Z}, {W})";
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals (object? obj)
		{
			return obj is OldVector4 vector4 && this.Equals(vector4);
		}
		
		public bool Equals (OldVector4 other)
		{
			return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
		}
		
		/// <summary>
		/// Gets the length (magnitude) of the vector.
		/// </summary>
		public Element Length
		{
			get
			{
				return (Element)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}
		
		/// <summary>
		/// Scales the OldVector4 to unit length.
		/// </summary>
		public void Normalize()
		{
			Element scale = 1.0f / this.Length;
			X *= scale;
			Y *= scale;
			Z *= scale;
			W *= scale;
		}
		
		/// <summary>
		/// Scale a vector to unit length
		/// </summary>
		/// <param name="vec">The input vector</param>
		/// <returns>The normalized vector</returns>
		public static OldVector4 Normalize(OldVector4 vec)
		{
			Element scale = 1.0f / vec.Length;
			vec.X *= scale;
			vec.Y *= scale;
			vec.Z *= scale;
			vec.W *= scale;
			return vec;
		}
		
		/// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator +(OldVector4 left, OldVector4 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator -(OldVector4 left, OldVector4 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Negates an instance.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator -(OldVector4 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            vec.W = -vec.W;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator *(OldVector4 vec, Element scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="scale">The scalar.</param>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator *(Element scale, OldVector4 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        /// <summary>
        /// Divides an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>The result of the calculation.</returns>
        public static OldVector4 operator /(OldVector4 vec, Element scale)
        {
            Element mult = 1.0f / scale;
            vec.X *= mult;
            vec.Y *= mult;
            vec.Z *= mult;
            vec.W *= mult;
            return vec;
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(OldVector4 left, OldVector4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equa lright; false otherwise.</returns>
        public static bool operator !=(OldVector4 left, OldVector4 right)
        {
            return !left.Equals(right);
        }
        
        /// <summary>
        /// Calculate the dot product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The dot product of the two inputs</returns>
        public static Element Dot(OldVector4 left, OldVector4 right)
        {
	        return left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        }
        
        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <returns>The transformed vector</returns>
        public static OldVector4 Transform(OldVector4 vec, OldMatrix4 mat)
        {
	        OldVector4 result;
	        result.X = OldVector4.Dot(vec, mat.Column0);
	        result.Y = OldVector4.Dot(vec, mat.Column1);
	        result.Z = OldVector4.Dot(vec, mat.Column2);
	        result.W = OldVector4.Dot(vec, mat.Column3);
	        return result;
        }

        /// <summary>Transform a Vector by the given Matrix</summary>
        /// <param name="vec">The vector to transform</param>
        /// <param name="mat">The desired transformation</param>
        /// <param name="result">The transformed vector</param>
        public static void Transform(ref OldVector4 vec, ref OldMatrix4 mat, out OldVector4 result)
        {
            result.X = vec.X * mat.Row0.X +
                       vec.Y * mat.Row1.X +
                       vec.Z * mat.Row2.X +
                       vec.W * mat.Row3.X;

            result.Y = vec.X * mat.Row0.Y +
                       vec.Y * mat.Row1.Y +
                       vec.Z * mat.Row2.Y +
                       vec.W * mat.Row3.Y;

            result.Z = vec.X * mat.Row0.Z +
                       vec.Y * mat.Row1.Z +
                       vec.Z * mat.Row2.Z +
                       vec.W * mat.Row3.Z;

            result.W = vec.X * mat.Row0.W +
                       vec.Y * mat.Row1.W +
                       vec.Z * mat.Row2.W +
                       vec.W * mat.Row3.W;
        }

        public OldVector3 Xyz => new OldVector3(X, Y, Z);
	}
	// ReSharper restore CompareOfFloatsByEqualityOperator

	/// <summary>
    /// Represents a 4x4 Matrix
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct OldMatrix4 : IEquatable<OldMatrix4>
    {
        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public OldVector4 Row0;
        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public OldVector4 Row1;
        /// <summary>
        /// 3rd row of the matrix
        /// </summary>
        public OldVector4 Row2;
        /// <summary>
        /// Bottom row of the matrix
        /// </summary>
        public OldVector4 Row3;
 
        /// <summary>
        /// The identity matrix
        /// </summary>
        public readonly static OldMatrix4 Identity = new OldMatrix4(OldVector4.UnitX, OldVector4.UnitY, OldVector4.UnitZ, OldVector4.UnitW);

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        /// <param name="row2">Third row of the matrix</param>
        /// <param name="row3">Bottom row of the matrix</param>
        public OldMatrix4(OldVector4 row0, OldVector4 row1, OldVector4 row2, OldVector4 row3)
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row of the matrix.</param>
        /// <param name="m01">Second item of the first row of the matrix.</param>
        /// <param name="m02">Third item of the first row of the matrix.</param>
        /// <param name="m03">Fourth item of the first row of the matrix.</param>
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        /// <param name="m12">Third item of the second row of the matrix.</param>
        /// <param name="m13">Fourth item of the second row of the matrix.</param>
        /// <param name="m20">First item of the third row of the matrix.</param>
        /// <param name="m21">Second item of the third row of the matrix.</param>
        /// <param name="m22">Third item of the third row of the matrix.</param>
        /// <param name="m23">First item of the third row of the matrix.</param>
        /// <param name="m30">Fourth item of the fourth row of the matrix.</param>
        /// <param name="m31">Second item of the fourth row of the matrix.</param>
        /// <param name="m32">Third item of the fourth row of the matrix.</param>
        /// <param name="m33">Fourth item of the fourth row of the matrix.</param>
        public OldMatrix4(
            Element m00, Element m01, Element m02, Element m03,
            Element m10, Element m11, Element m12, Element m13,
            Element m20, Element m21, Element m22, Element m23,
            Element m30, Element m31, Element m32, Element m33)
        {
            Row0 = new OldVector4(m00, m01, m02, m03);
            Row1 = new OldVector4(m10, m11, m12, m13);
            Row2 = new OldVector4(m20, m21, m22, m23);
            Row3 = new OldVector4(m30, m31, m32, m33);
        }

#if __IOS__ || __MACOS__ || __MACCATALYST__
        public static implicit operator SceneKit.SCNMatrix4(OldMatrix4 om) {
            return new SceneKit.SCNMatrix4(om.M11, om.M21, om.M31, om.M41,
                                           om.M12, om.M22, om.M32, om.M42,
                                           om.M13, om.M23, om.M33, om.M43,
                                           om.M14, om.M24, om.M34, om.M44);
        }
        public static implicit operator OldMatrix4(SceneKit.SCNMatrix4 sm) {
	        return new OldMatrix4((Element)sm.M11, (Element)sm.M12, (Element)sm.M13, (Element)sm.M14,
		        (Element)sm.M21, (Element)sm.M22, (Element)sm.M23, (Element)sm.M24,
		        (Element)sm.M31, (Element)sm.M32, (Element)sm.M33, (Element)sm.M34,
		        (Element)sm.M41, (Element)sm.M42, (Element)sm.M43, (Element)sm.M44);
        }
#endif

        #region Public Members

        #region Properties

        /// <summary>
        /// The determinant of this matrix
        /// </summary>
        public Element Determinant
        {
            get
            {
                return
                    Row0.X * Row1.Y * Row2.Z * Row3.W - Row0.X * Row1.Y * Row2.W * Row3.Z + Row0.X * Row1.Z * Row2.W * Row3.Y - Row0.X * Row1.Z * Row2.Y * Row3.W
                  + Row0.X * Row1.W * Row2.Y * Row3.Z - Row0.X * Row1.W * Row2.Z * Row3.Y - Row0.Y * Row1.Z * Row2.W * Row3.X + Row0.Y * Row1.Z * Row2.X * Row3.W
                  - Row0.Y * Row1.W * Row2.X * Row3.Z + Row0.Y * Row1.W * Row2.Z * Row3.X - Row0.Y * Row1.X * Row2.Z * Row3.W + Row0.Y * Row1.X * Row2.W * Row3.Z
                  + Row0.Z * Row1.W * Row2.X * Row3.Y - Row0.Z * Row1.W * Row2.Y * Row3.X + Row0.Z * Row1.X * Row2.Y * Row3.W - Row0.Z * Row1.X * Row2.W * Row3.Y
                  + Row0.Z * Row1.Y * Row2.W * Row3.X - Row0.Z * Row1.Y * Row2.X * Row3.W - Row0.W * Row1.X * Row2.Y * Row3.Z + Row0.W * Row1.X * Row2.Z * Row3.Y
                  - Row0.W * Row1.Y * Row2.Z * Row3.X + Row0.W * Row1.Y * Row2.X * Row3.Z - Row0.W * Row1.Z * Row2.X * Row3.Y + Row0.W * Row1.Z * Row2.Y * Row3.X;
            }
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public OldVector4 Column0
        {
            get { return new OldVector4(Row0.X, Row1.X, Row2.X, Row3.X); }
            set {
                M11 = value.X;
                M21 = value.Y;
                M31 = value.Z;
                M41 = value.W;
            }
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public OldVector4 Column1
        {
            get { return new OldVector4(Row0.Y, Row1.Y, Row2.Y, Row3.Y); }
            set {
                M12 = value.X;
                M22 = value.Y;
                M32 = value.Z;
                M42 = value.W;
            }
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public OldVector4 Column2
        {
            get { return new OldVector4(Row0.Z, Row1.Z, Row2.Z, Row3.Z); }
            set {
                M13 = value.X;
                M23 = value.Y;
                M33 = value.Z;
                M43 = value.W;
            }
        }

        /// <summary>
        /// The fourth column of this matrix
        /// </summary>
        public OldVector4 Column3
        {
            get { return new OldVector4(Row0.W, Row1.W, Row2.W, Row3.W); }
            set {
                M14 = value.X;
                M24 = value.Y;
                M34 = value.Z;
                M44 = value.W;
            }
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 1 of this instance.
        /// </summary>
        public Element M11 { get { return Row0.X; } set { Row0.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 2 of this instance.
        /// </summary>
        public Element M12 { get { return Row0.Y; } set { Row0.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 3 of this instance.
        /// </summary>
        public Element M13 { get { return Row0.Z; } set { Row0.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 4 of this instance.
        /// </summary>
        public Element M14 { get { return Row0.W; } set { Row0.W = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 1 of this instance.
        /// </summary>
        public Element M21 { get { return Row1.X; } set { Row1.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 2 of this instance.
        /// </summary>
        public Element M22 { get { return Row1.Y; } set { Row1.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 3 of this instance.
        /// </summary>
        public Element M23 { get { return Row1.Z; } set { Row1.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 4 of this instance.
        /// </summary>
        public Element M24 { get { return Row1.W; } set { Row1.W = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 1 of this instance.
        /// </summary>
        public Element M31 { get { return Row2.X; } set { Row2.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 2 of this instance.
        /// </summary>
        public Element M32 { get { return Row2.Y; } set { Row2.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 3 of this instance.
        /// </summary>
        public Element M33 { get { return Row2.Z; } set { Row2.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 4 of this instance.
        /// </summary>
        public Element M34 { get { return Row2.W; } set { Row2.W = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 1 of this instance.
        /// </summary>
        public Element M41 { get { return Row3.X; } set { Row3.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 2 of this instance.
        /// </summary>
        public Element M42 { get { return Row3.Y; } set { Row3.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 3 of this instance.
        /// </summary>
        public Element M43 { get { return Row3.Z; } set { Row3.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 4, column 4 of this instance.
        /// </summary>
        public Element M44 { get { return Row3.W; } set { Row3.W = value; } }

        #endregion

        #region Instance

        #region public void Invert()

        /// <summary>
        /// Converts this instance into its inverse.
        /// </summary>
        public void Invert()
        {
            this = OldMatrix4.Invert(this);
        }

        #endregion

        #region public void Transpose()

        /// <summary>
        /// Converts this instance into its transpose.
        /// </summary>
        public void Transpose()
        {
            this = OldMatrix4.Transpose(this);
        }

        #endregion

        #endregion

        #region Static

        #region CreateFromColumns

        public static OldMatrix4 CreateFromColumns (OldVector4 column0, OldVector4 column1, OldVector4 column2, OldVector4 column3)
        {
            var result = new OldMatrix4 ();
            result.Column0 = column0;
            result.Column1 = column1;
            result.Column2 = column2;
            result.Column3 = column3;
            return result;
        }

        public static void CreateFromColumns (OldVector4 column0, OldVector4 column1, OldVector4 column2, OldVector4 column3, out OldMatrix4 result)
        {
            result = new OldMatrix4 ();
            result.Column0 = column0;
            result.Column1 = column1;
            result.Column2 = column2;
            result.Column3 = column3;
        }

        #endregion
        
        #region CreateFromAxisAngle
        
        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <param name="result">A matrix instance.</param>
        public static void CreateFromAxisAngle(OldVector3 axis, Element angle, out OldMatrix4 result)
        {
            Element cos = (Element)Math.Cos(-angle);
            Element sin = (Element)Math.Sin(-angle);
            Element t = 1.0f - cos;

            axis.Normalize();

            result = new OldMatrix4(t * axis.X * axis.X + cos, t * axis.X * axis.Y - sin * axis.Z, t * axis.X * axis.Z + sin * axis.Y, 0.0f,
                                 t * axis.X * axis.Y + sin * axis.Z, t * axis.Y * axis.Y + cos, t * axis.Y * axis.Z - sin * axis.X, 0.0f,
                                 t * axis.X * axis.Z - sin * axis.Y, t * axis.Y * axis.Z + sin * axis.X, t * axis.Z * axis.Z + cos, 0.0f,
                                 0, 0, 0, 1);
        }

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <returns>A matrix instance.</returns>
        public static OldMatrix4 CreateFromAxisAngle(OldVector3 axis, Element angle)
        {
	        CreateFromAxisAngle(axis, angle, out OldMatrix4 result);
            return result;
        }
        
        #endregion

        #region CreateRotation[XYZ]

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateRotationX(Element angle, out OldMatrix4 result)
        {
            Element cos = (Element)System.Math.Cos(angle);
            Element sin = (Element)System.Math.Sin(angle);

            result.Row0 = OldVector4.UnitX;
            result.Row1 = new OldVector4(0.0f, cos, sin, 0.0f);
            result.Row2 = new OldVector4(0.0f, -sin, cos, 0.0f);
            result.Row3 = OldVector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateRotationX(Element angle)
        {
	        CreateRotationX(angle, out OldMatrix4 result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateRotationY(Element angle, out OldMatrix4 result)
        {
            Element cos = (Element)Math.Cos(angle);
            Element sin = (Element)Math.Sin(angle);

            result.Row0 = new OldVector4(cos, 0.0f, -sin, 0.0f);
            result.Row1 = OldVector4.UnitY;
            result.Row2 = new OldVector4(sin, 0.0f, cos, 0.0f);
            result.Row3 = OldVector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateRotationY(Element angle)
        {
            OldMatrix4 result;
            CreateRotationY(angle, out result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateRotationZ(Element angle, out OldMatrix4 result)
        {
            Element cos = (Element)System.Math.Cos(angle);
            Element sin = (Element)System.Math.Sin(angle);

            result.Row0 = new OldVector4(cos, sin, 0.0f, 0.0f);
            result.Row1 = new OldVector4(-sin, cos, 0.0f, 0.0f);
            result.Row2 = OldVector4.UnitZ;
            result.Row3 = OldVector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateRotationZ(Element angle)
        {
	        CreateRotationZ(angle, out OldMatrix4 result);
            return result;
        }

        #endregion

        #region CreateTranslation

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateTranslation(Element x, Element y, Element z, out OldMatrix4 result)
        {
            result = Identity;
            result.Row3 = new OldVector4(x, y, z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateTranslation(ref OldVector3 vector, out OldMatrix4 result)
        {
            result = Identity;
            result.Row3 = new OldVector4(vector.X, vector.Y, vector.Z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateTranslation(Element x, Element y, Element z)
        {
            OldMatrix4 result;
            CreateTranslation(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateTranslation(OldVector3 vector)
        {
            OldMatrix4 result;
            CreateTranslation(vector.X, vector.Y, vector.Z, out result);
            return result;
        }

        #endregion

        #region CreateOrthographic

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateOrthographic(Element width, Element height, Element zNear, Element zFar, out OldMatrix4 result)
        {
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <rereturns>The resulting OldMatrix4 instance.</rereturns>
        public static OldMatrix4 CreateOrthographic(Element width, Element height, Element zNear, Element zFar)
        {
            OldMatrix4 result;
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
            return result;
        }

        #endregion

        #region CreateOrthographicOffCenter

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting OldMatrix4 instance.</param>
        public static void CreateOrthographicOffCenter(Element left, Element right, Element bottom, Element top, Element zNear, Element zFar, out OldMatrix4 result)
        {
            result = new OldMatrix4();

            Element invRL = 1 / (right - left);
            Element invTB = 1 / (top - bottom);
            Element invFN = 1 / (zFar - zNear);

            result.M11 = 2 * invRL;
            result.M22 = 2 * invTB;
            result.M33 = -2 * invFN;

            result.M41 = -(right + left) * invRL;
            result.M42 = -(top + bottom) * invTB;
            result.M43 = -(zFar + zNear) * invFN;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <returns>The resulting OldMatrix4 instance.</returns>
        public static OldMatrix4 CreateOrthographicOffCenter(Element left, Element right, Element bottom, Element top, Element zNear, Element zFar)
        {
            OldMatrix4 result;
            CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }

        #endregion
        
        #region CreatePerspectiveFieldOfView
        
        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveFieldOfView(Element fovy, Element aspect, Element zNear, Element zFar, out OldMatrix4 result)
        {
            if (fovy <= 0 || fovy > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspect <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");
            
            Element yMax = zNear * (float)System.Math.Tan(0.5f * fovy);
            Element yMin = -yMax;
            Element xMin = yMin * aspect;
            Element xMax = yMax * aspect;

            CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar, out result);
        }
        
        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static OldMatrix4 CreatePerspectiveFieldOfView(Element fovy, Element aspect, Element zNear, Element zFar)
        {
            OldMatrix4 result;
            CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar, out result);
            return result;
        }
        
        #endregion
        
        #region CreatePerspectiveOffCenter
        
        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static void CreatePerspectiveOffCenter(Element left, Element right, Element bottom, Element top, Element zNear, Element zFar, out OldMatrix4 result)
        {
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");
            
            Element x = (2.0f * zNear) / (right - left);
            Element y = (2.0f * zNear) / (top - bottom);
            Element a = (right + left) / (right - left);
            Element b = (top + bottom) / (top - bottom);
            Element c = -(zFar + zNear) / (zFar - zNear);
            Element d = -(2.0f * zFar * zNear) / (zFar - zNear);
            
            result = new OldMatrix4(x, 0, 0,  0,
                                 0, y, 0,  0,
                                 a, b, c, -1,
                                 0, 0, d,  0);
        }
        
        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        public static OldMatrix4 CreatePerspectiveOffCenter(Element left, Element right, Element bottom, Element top, Element zNear, Element zFar)
        {
            OldMatrix4 result;
            CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar, out result);
            return result;
        }
        
        #endregion

        #region Scale Functions

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Single scale factor for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static OldMatrix4 Scale(Element scale)
        {
            return Scale(scale, scale, scale);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Scale factors for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static OldMatrix4 Scale(OldVector3 scale)
        {
            return Scale(scale.X, scale.Y, scale.Z);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="x">Scale factor for x-axis</param>
        /// <param name="y">Scale factor for y-axis</param>
        /// <param name="z">Scale factor for z-axis</param>
        /// <returns>A scaling matrix</returns>
        public static OldMatrix4 Scale(Element x, Element y, Element z)
        {
            OldMatrix4 result;
            result.Row0 = OldVector4.UnitX * x;
            result.Row1 = OldVector4.UnitY * y;
            result.Row2 = OldVector4.UnitZ * z;
            result.Row3 = OldVector4.UnitW;
            return result;
        }

        #endregion

        #region Camera Helper Functions

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A OldMatrix4 that transforms world space to camera space</returns>
        public static OldMatrix4 LookAt(OldVector3 eye, OldVector3 target, OldVector3 up)
        {
            OldVector3 z = OldVector3.Normalize(eye - target);
            OldVector3 x = OldVector3.Normalize(OldVector3.Cross(up, z));
            OldVector3 y = OldVector3.Normalize(OldVector3.Cross(z, x));

            OldMatrix4 rot = new OldMatrix4(new OldVector4(x.X, y.X, z.X, 0.0f),
                                        new OldVector4(x.Y, y.Y, z.Y, 0.0f),
                                        new OldVector4(x.Z, y.Z, z.Z, 0.0f),
                                        OldVector4.UnitW);

            OldMatrix4 trans = OldMatrix4.CreateTranslation(-eye);

            return trans * rot;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eyeX">Eye (camera) position in world space</param>
        /// <param name="eyeY">Eye (camera) position in world space</param>
        /// <param name="eyeZ">Eye (camera) position in world space</param>
        /// <param name="targetX">Target position in world space</param>
        /// <param name="targetY">Target position in world space</param>
        /// <param name="targetZ">Target position in world space</param>
        /// <param name="upX">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upY">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upZ">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A OldMatrix4 that transforms world space to camera space</returns>
        public static OldMatrix4 LookAt(Element eyeX, Element eyeY, Element eyeZ, Element targetX, Element targetY, Element targetZ, Element upX, Element upY, Element upZ)
        {
            return LookAt(new OldVector3(eyeX, eyeY, eyeZ), new OldVector3(targetX, targetY, targetZ), new OldVector3(upX, upY, upZ));
        }

        #endregion

        #region Multiply Functions

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new instance that is the result of the multiplication</returns>
        public static OldMatrix4 Mult(OldMatrix4 left, OldMatrix4 right)
        {
            OldMatrix4 result;
            Mult(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new instance that is the result of the multiplication</param>
        public static void Mult(ref OldMatrix4 left, ref OldMatrix4 right, out OldMatrix4 result)
        {
            result = new OldMatrix4(
                left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
                left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
                left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
                left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
                left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
                left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
                left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
                left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
                left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
                left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
                left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
                left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
                left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
                left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
                left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
                left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
        }

        #endregion

        #region Invert Functions

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the OldMatrix4 is singular.</exception>
        public static OldMatrix4 Invert(OldMatrix4 mat)
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            Element[,] inverse = {{mat.Row0.X, mat.Row0.Y, mat.Row0.Z, mat.Row0.W}, 
                                {mat.Row1.X, mat.Row1.Y, mat.Row1.Z, mat.Row1.W}, 
                                {mat.Row2.X, mat.Row2.Y, mat.Row2.Z, mat.Row2.W}, 
                                {mat.Row3.X, mat.Row3.Y, mat.Row3.Z, mat.Row3.W} };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                Element maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                Element absVal = (Element)System.Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        Element f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                Element pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                    //return mat;
                }

                // Scale row so it has a unit diagonal
                Element oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        Element f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    Element f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            mat.Row0 = new OldVector4(inverse[0, 0], inverse[0, 1], inverse[0, 2], inverse[0, 3]);
            mat.Row1 = new OldVector4(inverse[1, 0], inverse[1, 1], inverse[1, 2], inverse[1, 3]);
            mat.Row2 = new OldVector4(inverse[2, 0], inverse[2, 1], inverse[2, 2], inverse[2, 3]);
            mat.Row3 = new OldVector4(inverse[3, 0], inverse[3, 1], inverse[3, 2], inverse[3, 3]);
            return mat;
        }

        #endregion

        #region Transpose

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        public static OldMatrix4 Transpose(OldMatrix4 mat)
        {
            return new OldMatrix4(mat.Column0, mat.Column1, mat.Column2, mat.Column3);
        }


        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <param name="result">The result of the calculation</param>
        public static void Transpose(ref OldMatrix4 mat, out OldMatrix4 result)
        {
            result.Row0 = mat.Column0;
            result.Row1 = mat.Column1;
            result.Row2 = mat.Column2;
            result.Row3 = mat.Column3;
        }

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new OldMatrix44 which holds the result of the multiplication</returns>
        public static OldMatrix4 operator *(OldMatrix4 left, OldMatrix4 right)
        {
            return OldMatrix4.Mult(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(OldMatrix4 left, OldMatrix4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(OldMatrix4 left, OldMatrix4 right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Overrides

        #region public override string ToString()

        /// <summary>
        /// Returns a System.String that represents the current OldMatrix44.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}\n{1}\n{2}\n{3}", Row0, Row1, Row2, Row3);
        }

        #endregion

        #region public override int GetHashCode()

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode() ^ Row2.GetHashCode() ^ Row3.GetHashCode();
        }

        #endregion

        #region public override bool Equals(object obj)

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare tresult.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals (object? obj)
        {
            if (!(obj is OldMatrix4))
                return false;

            return this.Equals((OldMatrix4)obj);
        }

        #endregion

        #endregion

        #endregion

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(OldMatrix4 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1 &&
                Row2 == other.Row2 &&
                Row3 == other.Row3;
        }
    }
}
