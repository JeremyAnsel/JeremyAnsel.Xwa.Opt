// -----------------------------------------------------------------------
// <copyright file="Indices.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    public struct Indices : IEquatable<Indices>
    {
        private int a;

        private int b;

        private int c;

        private int d;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c")]
        public Indices(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = -1;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
        public Indices(int a, int b, int c, int d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
        public int A
        {
            get { return this.a; }
            set { this.a = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
        public int B
        {
            get { return this.b; }
            set { this.b = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "C")]
        public int C
        {
            get { return this.c; }
            set { this.c = value; }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "D")]
        public int D
        {
            get { return this.d; }
            set { this.d = value; }
        }

        public static readonly Indices Empty = new Indices(-1, -1, -1, -1);

        public bool IsTriangle
        {
            get
            {
                return this.d < 0;
            }
        }

        public bool IsQuadrangle
        {
            get
            {
                return this.d >= 0;
            }
        }

        /// <summary>
        /// Compares two <see cref="Indices"/> objects. The result specifies whether the values of the two objects are equal.
        /// </summary>
        /// <param name="left">The left <see cref="Indices"/> to compare.</param>
        /// <param name="right">The right <see cref="Indices"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right are equal; otherwise, <value>false</value>.</returns>
        public static bool operator ==(Indices left, Indices right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="Indices"/> objects. The result specifies whether the values of the two objects are unequal.
        /// </summary>
        /// <param name="left">The left <see cref="Indices"/> to compare.</param>
        /// <param name="right">The right <see cref="Indices"/> to compare.</param>
        /// <returns><value>true</value> if the values of left and right differ; otherwise, <value>false</value>.</returns>
        public static bool operator !=(Indices left, Indices right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} ; {1} ; {2} ; {3}", this.A, this.B, this.C, this.D);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Indices))
            {
                return false;
            }

            return this.Equals((Indices)obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns><value>true</value> if the specified object is equal to the current object; otherwise, <value>false</value>.</returns>
        public bool Equals(Indices other)
        {
            return this.a == other.a
                && this.b == other.b
                && this.c == other.c
                && this.d == other.d;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return new
            {
                this.a,
                this.b,
                this.c,
                this.d
            }
            .GetHashCode();
        }

        public static Indices Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] data = value.Split(';');

            if (data.Length == 3)
            {
                return new Indices(
                    int.Parse(data[0], CultureInfo.InvariantCulture),
                    int.Parse(data[1], CultureInfo.InvariantCulture),
                    int.Parse(data[2], CultureInfo.InvariantCulture),
                    -1);
            }
            else
            {
                return new Indices(
                    int.Parse(data[0], CultureInfo.InvariantCulture),
                    int.Parse(data[1], CultureInfo.InvariantCulture),
                    int.Parse(data[2], CultureInfo.InvariantCulture),
                    int.Parse(data[3], CultureInfo.InvariantCulture));
            }
        }

        internal static Indices FromByteArray(byte[] buffer, int start)
        {
            Indices i = new Indices
            {
                a = BitConverter.ToInt32(buffer, start + 0),
                b = BitConverter.ToInt32(buffer, start + 4),
                c = BitConverter.ToInt32(buffer, start + 8),
                d = BitConverter.ToInt32(buffer, start + 12)
            };

            return i;
        }

        internal byte[] ToByteArray()
        {
            byte[] buffer = new byte[16];

            BitConverter.GetBytes(this.a).CopyTo(buffer, 0);
            BitConverter.GetBytes(this.b).CopyTo(buffer, 4);
            BitConverter.GetBytes(this.c).CopyTo(buffer, 8);
            BitConverter.GetBytes(this.d).CopyTo(buffer, 12);

            return buffer;
        }

        public Indices SetA(int value)
        {
            return new Indices(value, this.b, this.c, this.d);
        }

        public Indices SetB(int value)
        {
            return new Indices(this.a, value, this.c, this.d);
        }

        public Indices SetC(int value)
        {
            return new Indices(this.a, this.b, value, this.d);
        }

        public Indices SetD(int value)
        {
            return new Indices(this.a, this.b, this.c, value);
        }

        public Indices InvertOrder()
        {
            if (this.d < 0)
            {
                return new Indices(this.c, this.b, this.a);
            }
            else
            {
                return new Indices(this.d, this.c, this.b, this.a);
            }
        }
    }
}
