using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JeremyAnsel.Xwa.Opt
{
    public class PlayabilityMessage : IComparable, IComparable<PlayabilityMessage>
    {
        public PlayabilityMessage(PlayabilityMessageLevel level, string category, string format, params object[] args)
        {
            this.Level = level;
            this.Category = category;
            this.Message = string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public PlayabilityMessageLevel Level { get; private set; }

        public string Category { get; private set; }

        public string Message { get; private set; }

        public static bool operator ==(PlayabilityMessage left, PlayabilityMessage right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(PlayabilityMessage left, PlayabilityMessage right)
        {
            return !(left == right);
        }

        public static bool operator <(PlayabilityMessage left, PlayabilityMessage right)
        {
            return PlayabilityMessage.Compare(left, right) < 0;
        }

        public static bool operator >(PlayabilityMessage left, PlayabilityMessage right)
        {
            return PlayabilityMessage.Compare(left, right) > 0;
        }

        public static int Compare(PlayabilityMessage left, PlayabilityMessage right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }

            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }

            return left.CompareTo(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}] {1}: {2}", this.Level, this.Category, this.Message);
        }

        public override bool Equals(object obj)
        {
            var other = obj as PlayabilityMessage;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return this.Equals(other);
        }

        public bool Equals(PlayabilityMessage other)
        {
            return this.CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return new
            {
                this.Level,
                this.Category,
                this.Message
            }
            .GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var other = obj as PlayabilityMessage;

            if (other == null)
            {
                throw new ArgumentException("A PlayabilityMessage object is required for comparison.", "obj");
            }

            return this.CompareTo(other);
        }

        public int CompareTo(PlayabilityMessage other)
        {
            if (other == null)
            {
                return 1;
            }

            int diff = this.Level.CompareTo(other.Level);

            if (diff != 0)
            {
                return diff;
            }

            diff = string.Compare(this.Category, other.Category, StringComparison.Ordinal);

            if (diff != 0)
            {
                return diff;
            }

            return string.Compare(this.Message, other.Message, StringComparison.Ordinal);
        }
    }
}
