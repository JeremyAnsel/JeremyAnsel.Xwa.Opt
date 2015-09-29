// -----------------------------------------------------------------------
// <copyright file="Vector.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public struct Vector : IEquatable<Vector>
    {
        private float x;

        private float y;

        private float z;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "z")]
        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        public float X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public float Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Z")]
        public float Z
        {
            get { return this.z; }
            set { this.z = value; }
        }

        public static readonly Vector Empty = new Vector(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// Compares two <see cref="Vector"/> objects. The result specifies whether the values of the two objects are equal.
        /// </summary>
        /// <param name="left">The left <see cref="Vector"/> to compare.</param>
        /// <param name="right">The right <see cref="Vector"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right are equal; otherwise, <value>false</value>.</returns>
        public static bool operator ==(Vector left, Vector right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="Vector"/> objects. The result specifies whether the values of the two objects are unequal.
        /// </summary>
        /// <param name="left">The left <see cref="Vector"/> to compare.</param>
        /// <param name="right">The right <see cref="Vector"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right differ; otherwise, <value>false</value>.</returns>
        public static bool operator !=(Vector left, Vector right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:R} ; {1:R} ; {2:R}", this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector))
            {
                return false;
            }

            return this.Equals((Vector)obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public bool Equals(Vector other)
        {
            return this.x == other.x
                && this.y == other.y
                && this.z == other.z;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return new
            {
                this.x,
                this.y,
                this.z
            }
            .GetHashCode();
        }

        public static Vector Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            string[] data = value.Split(';');

            return new Vector(
                float.Parse(data[0], CultureInfo.InvariantCulture),
                float.Parse(data[1], CultureInfo.InvariantCulture),
                float.Parse(data[2], CultureInfo.InvariantCulture));
        }

        internal static Vector FromByteArray(byte[] buffer, int start)
        {
            Vector v = new Vector();

            v.x = BitConverter.ToSingle(buffer, start + 0);
            v.y = BitConverter.ToSingle(buffer, start + 4);
            v.z = BitConverter.ToSingle(buffer, start + 8);

            return v;
        }

        internal byte[] ToByteArray()
        {
            byte[] buffer = new byte[12];

            BitConverter.GetBytes(this.x).CopyTo(buffer, 0);
            BitConverter.GetBytes(this.y).CopyTo(buffer, 4);
            BitConverter.GetBytes(this.z).CopyTo(buffer, 8);

            return buffer;
        }

        public Vector SetX(float value)
        {
            return new Vector(value, this.y, this.z);
        }

        public Vector SetY(float value)
        {
            return new Vector(this.x, value, this.z);
        }

        public Vector SetZ(float value)
        {
            return new Vector(this.x, this.y, value);
        }

        public Vector Abs()
        {
            return new Vector(Math.Abs(this.x), Math.Abs(this.y), Math.Abs(this.z));
        }

        public Vector Scale(float scaleX, float scaleY, float scaleZ)
        {
            return new Vector(this.x * scaleX, this.y * scaleY, this.z * scaleZ);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a", Justification = "Reviewed")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b", Justification = "Reviewed")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c", Justification = "Reviewed")]
        public static Vector Normal(Vector a, Vector b, Vector c)
        {
            Vector ba = new Vector(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vector ca = new Vector(c.X - a.X, c.Y - a.Y, c.Z - a.Z);

            Vector n = new Vector(
                ba.Z * ca.Y - ba.Y * ca.Z,
                ba.X * ca.Z - ba.Z * ca.X,
                ba.Y * ca.X - ba.X * ca.Y);

            float length = (float)Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);

            if (length > 0.0f)
            {
                n = new Vector(n.X / length, n.Y / length, n.Z / length);
            }

            return n;
        }
    }
}
