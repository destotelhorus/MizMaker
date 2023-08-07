#region --- License ---
/*
Copyright (c) 2006 - 2008 The Open Toolkit library.
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
#endregion

using System;
using System.Runtime.InteropServices;
using MizMaker.Lua;

namespace MizMaker
{
    /// <summary>Represents a 2D vector using two double-precision floating-point numbers.</summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vec2d : IEquatable<Vec2d>
    {
        #region Fields

        /// <summary>The X coordinate of this instance.</summary>
        public double X;

        /// <summary>The Y coordinate of this instance.</summary>
        public double Y;

        /// <summary>
        /// Defines a unit-length Vector2d that points towards the X-axis.
        /// </summary>
        public static Vec2d UnitX = new(1, 0);

        /// <summary>
        /// Defines a unit-length Vector2d that points towards the Y-axis.
        /// </summary>
        public static Vec2d UnitY = new(0, 1);

        /// <summary>
        /// Defines a zero-length Vector2d.
        /// </summary>
        public static Vec2d Zero = new(0, 0);

        /// <summary>
        /// Defines an instance with all components set to 1.
        /// </summary>
        public static readonly Vec2d One = new(1, 1);

        /// <summary>
        /// Defines the size of the Vector2d struct in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new Vec2d());

        #endregion

        #region Constructors

        /// <summary>Constructs left vector with the given coordinates.</summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vec2d(double x, double y)
        {
            X = x;
            Y = y;
        }
        
        public Vec2d(LsonValue unit)
        {
            X = unit["x"].GetDouble();
            Y = unit["y"].GetDouble();
        }

        public Vec2d(Angle angle, double magnitude)
        {
            X = magnitude * Math.Cos(angle.Rad);
            Y = magnitude * Math.Sin(angle.Rad);
        }

        #endregion

        #region Public Members

        #region Instance

        #region public void Add()

        /// <summary>Add the Vector passed as parameter to this instance.</summary>
        /// <param name="right">Right operand. This parameter is only read from.</param>
        public void Add(Vec2d right)
        {
            X += right.X;
            Y += right.Y;
        }

        /// <summary>Add the Vector passed as parameter to this instance.</summary>
        /// <param name="right">Right operand. This parameter is only read from.</param>
        public void Add(ref Vec2d right)
        {
            X += right.X;
            Y += right.Y;
        }

        #endregion public void Add()

        #region public void Sub()

        /// <summary>Subtract the Vector passed as parameter from this instance.</summary>
        /// <param name="right">Right operand. This parameter is only read from.</param>
        public void Sub(Vec2d right)
        {
            X -= right.X;
            Y -= right.Y;
        }

        /// <summary>Subtract the Vector passed as parameter from this instance.</summary>
        /// <param name="right">Right operand. This parameter is only read from.</param>
        public void Sub(ref Vec2d right)
        {
            X -= right.X;
            Y -= right.Y;
        }

        #endregion public void Sub()

        #region public void Mult()

        /// <summary>Multiply this instance by a scalar.</summary>
        /// <param name="f">Scalar operand.</param>
        public void Mult(double f)
        {
            X *= f;
            Y *= f;
        }

        #endregion public void Mult()

        #region public void Div()

        /// <summary>Divide this instance by a scalar.</summary>
        /// <param name="f">Scalar operand.</param>
        public void Div(double f)
        {
            var mult = 1.0 / f;
            X *= mult;
            Y *= mult;
        }

        #endregion public void Div()

        #region public double Length

        /// <summary>
        /// Gets the length (magnitude) of the vector.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public double Length => (float)Math.Sqrt(X * X + Y * Y);

        public double LengthNautical => Length * 0.0005399568;

        #endregion
        
        #region public double Angle
        /// <summary>
        /// Gets the angle of this vector.
        /// </summary>
        public Angle Angle => Angle.FromRad(Math.Atan2(Y, X));
        
        #endregion

        #region public double LengthSquared

        /// <summary>
        /// Gets the square of the vector length (magnitude).
        /// </summary>
        /// <remarks>
        /// This property avoids the costly square root operation required by the Length property. This makes it more suitable
        /// for comparisons.
        /// </remarks>
        /// <see cref="Length"/>
        public double LengthSquared => X * X + Y * Y;

        #endregion

        #region public Vector2d PerpendicularRight

        /// <summary>
        /// Gets the perpendicular vector on the right side of this vector.
        /// </summary>
        public Vec2d PerpendicularRight => new(Y, -X);

        #endregion

        #region public Vector2d PerpendicularLeft

        /// <summary>
        /// Gets the perpendicular vector on the left side of this vector.
        /// </summary>
        public Vec2d PerpendicularLeft => new(-Y, X);

        #endregion

        #region public void Normalize()

        /// <summary>
        /// Scales the Vector2 to unit length.
        /// </summary>
        public void Normalize()
        {
            var scale = 1.0f / Length;
            X *= scale;
            Y *= scale;
        }

        #endregion

        #region public void Scale()

        /// <summary>
        /// Scales the current Vector2 by the given amounts.
        /// </summary>
        /// <param name="sx">The scale of the X component.</param>
        /// <param name="sy">The scale of the Y component.</param>
        public void Scale(double sx, double sy)
        {
            X *= sx;
            Y *= sy;
        }

        /// <summary>Scales this instance by the given parameter.</summary>
        /// <param name="scale">The scaling of the individual components.</param>
        public void Scale(Vec2d scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        /// <summary>Scales this instance by the given parameter.</summary>
        /// <param name="scale">The scaling of the individual components.</param>
        public void Scale(ref Vec2d scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        #endregion public void Scale()

        #endregion

        #region Static

        #region Add

        /// <summary>
        /// Add two Vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of addition</returns>
        public static Vec2d Add(Vec2d a, Vec2d b)
        {
            a.X += b.X;
            a.Y += b.Y;
            return a;
        }

        /// <summary>
        /// Add two Vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">Result of addition</param>
        public static void Add(ref Vec2d a, ref Vec2d b, out Vec2d result)
        {
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
        }

        #endregion

        #region Sub

        /// <summary>
        /// Subtract one Vector from another
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of subtraction</returns>
        public static Vec2d Sub(Vec2d a, Vec2d b)
        {
            a.X -= b.X;
            a.Y -= b.Y;
            return a;
        }

        /// <summary>
        /// Subtract one Vector from another
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">Result of subtraction</param>
        public static void Sub(ref Vec2d a, ref Vec2d b, out Vec2d result)
        {
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
        }

        #endregion

        #region Mult

        /// <summary>
        /// Multiply a vector and a scalar
        /// </summary>
        /// <param name="a">Vector operand</param>
        /// <param name="d">Scalar operand</param>
        /// <returns>Result of the multiplication</returns>
        public static Vec2d Mult(Vec2d a, double d)
        {
            a.X *= d;
            a.Y *= d;
            return a;
        }

        /// <summary>
        /// Multiply a vector and a scalar
        /// </summary>
        /// <param name="a">Vector operand</param>
        /// <param name="d">Scalar operand</param>
        /// <param name="result">Result of the multiplication</param>
        public static void Mult(ref Vec2d a, double d, out Vec2d result)
        {
            result.X = a.X * d;
            result.Y = a.Y * d;
        }

        #endregion

        #region Div

        /// <summary>
        /// Divide a vector by a scalar
        /// </summary>
        /// <param name="a">Vector operand</param>
        /// <param name="d">Scalar operand</param>
        /// <returns>Result of the division</returns>
        public static Vec2d Div(Vec2d a, double d)
        {
            var mult = 1.0 / d;
            a.X *= mult;
            a.Y *= mult;
            return a;
        }

        /// <summary>
        /// Divide a vector by a scalar
        /// </summary>
        /// <param name="a">Vector operand</param>
        /// <param name="d">Scalar operand</param>
        /// <param name="result">Result of the division</param>
        public static void Div(ref Vec2d a, double d, out Vec2d result)
        {
            var mult = 1.0 / d;
            result.X = a.X * mult;
            result.Y = a.Y * mult;
        }

        #endregion

        #region Min

        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise minimum</returns>
        public static Vec2d Min(Vec2d a, Vec2d b)
        {
            a.X = a.X < b.X ? a.X : b.X;
            a.Y = a.Y < b.Y ? a.Y : b.Y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise minimum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise minimum</param>
        public static void Min(ref Vec2d a, ref Vec2d b, out Vec2d result)
        {
            result.X = a.X < b.X ? a.X : b.X;
            result.Y = a.Y < b.Y ? a.Y : b.Y;
        }

        #endregion

        #region Max

        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The component-wise maximum</returns>
        public static Vec2d Max(Vec2d a, Vec2d b)
        {
            a.X = a.X > b.X ? a.X : b.X;
            a.Y = a.Y > b.Y ? a.Y : b.Y;
            return a;
        }

        /// <summary>
        /// Calculate the component-wise maximum of two vectors
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <param name="result">The component-wise maximum</param>
        public static void Max(ref Vec2d a, ref Vec2d b, out Vec2d result)
        {
            result.X = a.X > b.X ? a.X : b.X;
            result.Y = a.Y > b.Y ? a.Y : b.Y;
        }

        #endregion

        #region Clamp

        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <returns>The clamped vector</returns>
        public static Vec2d Clamp(Vec2d vec, Vec2d min, Vec2d max)
        {
            vec.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            vec.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
            return vec;
        }

        /// <summary>
        /// Clamp a vector to the given minimum and maximum vectors
        /// </summary>
        /// <param name="vec">Input vector</param>
        /// <param name="min">Minimum vector</param>
        /// <param name="max">Maximum vector</param>
        /// <param name="result">The clamped vector</param>
        public static void Clamp(ref Vec2d vec, ref Vec2d min, ref Vec2d max, out Vec2d result)
        {
            result.X = vec.X < min.X ? min.X : vec.X > max.X ? max.X : vec.X;
            result.Y = vec.Y < min.Y ? min.Y : vec.Y > max.Y ? max.Y : vec.Y;
        }

        #endregion

        #region Normalize

        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <returns>The normalized vector</returns>
        public static Vec2d Normalize(Vec2d vec)
        {
            var scale = 1.0f / vec.Length;
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        /// <summary>
        /// Scale a vector to unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <param name="result">The normalized vector</param>
        public static void Normalize(ref Vec2d vec, out Vec2d result)
        {
            var scale = 1.0f / vec.Length;
            result.X = vec.X * scale;
            result.Y = vec.Y * scale;
        }

        #endregion

        #region NormalizeFast

        /// <summary>
        /// Scale a vector to approximately unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <returns>The normalized vector</returns>
        public static Vec2d NormalizeFast(Vec2d vec)
        {
            var scale = Functions.InverseSqrtFast(vec.X * vec.X + vec.Y * vec.Y);
            vec.X *= scale;
            vec.Y *= scale;
            return vec;
        }

        /// <summary>
        /// Scale a vector to approximately unit length
        /// </summary>
        /// <param name="vec">The input vector</param>
        /// <param name="result">The normalized vector</param>
        public static void NormalizeFast(ref Vec2d vec, out Vec2d result)
        {
            var scale = Functions.InverseSqrtFast(vec.X * vec.X + vec.Y * vec.Y);
            result.X = vec.X * scale;
            result.Y = vec.Y * scale;
        }

        #endregion

        #region Dot

        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <returns>The dot product of the two inputs</returns>
        public static double Dot(Vec2d left, Vec2d right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        /// <summary>
        /// Calculate the dot (scalar) product of two vectors
        /// </summary>
        /// <param name="left">First operand</param>
        /// <param name="right">Second operand</param>
        /// <param name="result">The dot product of the two inputs</param>
        public static void Dot(ref Vec2d left, ref Vec2d right, out double result)
        {
            result = left.X * right.X + left.Y * right.Y;
        }

        #endregion

        #region Lerp

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
        public static Vec2d Lerp(Vec2d a, Vec2d b, double blend)
        {
            a.X = blend * (b.X - a.X) + a.X;
            a.Y = blend * (b.Y - a.Y) + a.Y;
            return a;
        }

        /// <summary>
        /// Returns a new Vector that is the linear blend of the 2 given Vectors
        /// </summary>
        /// <param name="a">First input vector</param>
        /// <param name="b">Second input vector</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
        public static void Lerp(ref Vec2d a, ref Vec2d b, double blend, out Vec2d result)
        {
            result.X = blend * (b.X - a.X) + a.X;
            result.Y = blend * (b.Y - a.Y) + a.Y;
        }

        #endregion

        #region Barycentric

        /// <summary>
        /// Interpolate 3 Vectors using Barycentric coordinates
        /// </summary>
        /// <param name="a">First input Vector</param>
        /// <param name="b">Second input Vector</param>
        /// <param name="c">Third input Vector</param>
        /// <param name="u">First Barycentric Coordinate</param>
        /// <param name="v">Second Barycentric Coordinate</param>
        /// <returns>a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</returns>
        public static Vec2d BaryCentric(Vec2d a, Vec2d b, Vec2d c, double u, double v)
        {
            return a + u * (b - a) + v * (c - a);
        }

        /// <summary>Interpolate 3 Vectors using Barycentric coordinates</summary>
        /// <param name="a">First input Vector.</param>
        /// <param name="b">Second input Vector.</param>
        /// <param name="c">Third input Vector.</param>
        /// <param name="u">First Barycentric Coordinate.</param>
        /// <param name="v">Second Barycentric Coordinate.</param>
        /// <param name="result">Output Vector. a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</param>
        public static void BaryCentric(ref Vec2d a, ref Vec2d b, ref Vec2d c, float u, float v, out Vec2d result)
        {
            result = a; // copy

            var temp = b; // copy
            temp.Sub(ref a);
            temp.Mult(u);
            result.Add(ref temp);

            temp = c; // copy
            temp.Sub(ref a);
            temp.Mult(v);
            result.Add(ref temp);
        }

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator +(Vec2d left, Vec2d right)
        {
            left.X += right.X;
            left.Y += right.Y;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator -(Vec2d left, Vec2d right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            return left;
        }

        /// <summary>
        /// Negates an instance.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator -(Vec2d vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            return vec;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator *(Vec2d vec, double f)
        {
            vec.X *= f;
            vec.Y *= f;
            return vec;
        }

        /// <summary>
        /// Multiply an instance by a scalar.
        /// </summary>
        /// <param name="f">The scalar.</param>
        /// <param name="vec">The instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator *(double f, Vec2d vec)
        {
            vec.X *= f;
            vec.Y *= f;
            return vec;
        }

        /// <summary>
        /// Divides an instance by a scalar.
        /// </summary>
        /// <param name="vec">The instance.</param>
        /// <param name="f">The scalar.</param>
        /// <returns>The result of the operation.</returns>
        public static Vec2d operator /(Vec2d vec, double f)
        {
            var mult = 1.0f / f;
            vec.X *= mult;
            vec.Y *= mult;
            return vec;
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>True, if both instances are equal; false otherwise.</returns>
        public static bool operator ==(Vec2d left, Vec2d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for ienquality.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>True, if the instances are not equal; false otherwise.</returns>
        public static bool operator !=(Vec2d left, Vec2d right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Overrides

        #region public override string ToString()

        /// <summary>
        /// Returns a System.String that represents the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        #endregion

        #region public override int GetHashCode()

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        #endregion

        #region public override bool Equals(object obj)

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vec2d))
                return false;

            return Equals((Vec2d)obj);
        }

        #endregion

        #endregion

        #endregion

        #region IEquatable<Vector2d> Members

        /// <summary>Indicates whether the current vector is equal to another vector.</summary>
        /// <param name="other">A vector to compare with this vector.</param>
        /// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
        public bool Equals(Vec2d other)
        {
            return
                X == other.X &&
                Y == other.Y;
        }

        #endregion
    }

    public static class Functions
    {
        public static float InverseSqrtFast(float x)
        {
            unsafe
            {
                var xhalf = 0.5f * x;
                var i = *(int*)&x;              // Read bits as integer.
                i = 0x5f375a86 - (i >> 1);      // Make an initial guess for Newton-Raphson approximation
                x = *(float*)&i;                // Convert bits back to float
                x = x * (1.5f - xhalf * x * x); // Perform left single Newton-Raphson step.
                return x;
            }
        }

        /// <summary>
        /// Returns an approximation of the inverse square root of left number.
        /// </summary>
        /// <param name="x">A number.</param>
        /// <returns>An approximation of the inverse square root of the specified number, with an upper error bound of 0.001</returns>
        /// <remarks>
        /// This is an improved implementation of the the method known as Carmack's inverse square root
        /// which is found in the Quake III source code. This implementation comes from
        /// http://www.codemaestro.com/reviews/review00000105.html. For the history of this method, see
        /// http://www.beyond3d.com/content/articles/8/
        /// </remarks>
        public static double InverseSqrtFast(double x)
        {
            return InverseSqrtFast((float) x);
        }
    }

    public struct Angle
    {
        public double Rad { get; private set; }
        public double Deg
        {
            get
            {
                var deg = Rad * (180 / Math.PI);
                return deg < 0 ? deg + 360 : deg;
            }
        }

        public static Angle FromRad(double rad)
        {
            return new() {Rad = rad};
        }

        public static Angle FromDeg(double deg)
        {
            return new() {Rad = deg * (Math.PI / 180)};
        }
        
        
        public static Angle operator +(Angle left, Angle right)
        {
            left.Rad += right.Rad;
            return left;
        }
        
        public static Angle operator -(Angle left, Angle right)
        {
            left.Rad -= right.Rad;
            return left;
        }
    }
}