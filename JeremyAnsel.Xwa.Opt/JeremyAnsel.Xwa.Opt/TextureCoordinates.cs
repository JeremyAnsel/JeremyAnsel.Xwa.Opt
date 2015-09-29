// -----------------------------------------------------------------------
// <copyright file="TextureCoordinates.cs" company="">
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

    public struct TextureCoordinates : IEquatable<TextureCoordinates>
    {
        private float u;

        private float v;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "u")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "v")]
        public TextureCoordinates(float u, float v)
        {
            this.u = u;
            this.v = v;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "U")]
        public float U
        {
            get { return this.u; }
            set { this.u = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "V")]
        public float V
        {
            get { return this.v; }
            set { this.v = value; }
        }

        public static readonly TextureCoordinates Empty = new TextureCoordinates(0.0f, 0.0f);

        /// <summary>
        /// Compares two <see cref="TextureCoordinates"/> objects. The result specifies whether the values of the two objects are equal.
        /// </summary>
        /// <param name="left">The left <see cref="TextureCoordinates"/> to compare.</param>
        /// <param name="right">The right <see cref="TextureCoordinates"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right are equal; otherwise, <value>false</value>.</returns>
        public static bool operator ==(TextureCoordinates left, TextureCoordinates right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="TextureCoordinates"/> objects. The result specifies whether the values of the two objects are unequal.
        /// </summary>
        /// <param name="left">The left <see cref="TextureCoordinates"/> to compare.</param>
        /// <param name="right">The right <see cref="TextureCoordinates"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right differ; otherwise, <value>false</value>.</returns>
        public static bool operator !=(TextureCoordinates left, TextureCoordinates right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:R} ; {1:R}", this.U, this.V);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TextureCoordinates))
            {
                return false;
            }

            return this.Equals((TextureCoordinates)obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public bool Equals(TextureCoordinates other)
        {
            return this.u == other.u
                && this.v == other.v;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return new
            {
                this.u,
                this.v
            }
            .GetHashCode();
        }

        public static TextureCoordinates Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            string[] data = value.Split(';');

            return new TextureCoordinates(
                float.Parse(data[0], CultureInfo.InvariantCulture),
                float.Parse(data[1], CultureInfo.InvariantCulture));
        }

        internal static TextureCoordinates FromByteArray(byte[] buffer, int start)
        {
            TextureCoordinates v = new TextureCoordinates();

            v.u = BitConverter.ToSingle(buffer, start + 0);
            v.v = BitConverter.ToSingle(buffer, start + 4);

            return v;
        }

        internal byte[] ToByteArray()
        {
            byte[] buffer = new byte[8];

            BitConverter.GetBytes(this.u).CopyTo(buffer, 0);
            BitConverter.GetBytes(this.v).CopyTo(buffer, 4);

            return buffer;
        }

        public TextureCoordinates SetU(float value)
        {
            return new TextureCoordinates(value, this.v);
        }

        public TextureCoordinates SetV(float value)
        {
            return new TextureCoordinates(this.u, value);
        }
    }
}
