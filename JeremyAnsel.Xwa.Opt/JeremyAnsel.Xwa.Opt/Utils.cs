// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal static class Utils
    {
        public static string GetNullTerminatedString(byte[] value, int startIndex)
        {
            int count = 0;

            while (value[startIndex + count] != 0)
            {
                count++;
            }

            string text = Encoding.ASCII.GetString(value, startIndex, count + 1);

            int index = text.IndexOf('\0');

            if (index == -1)
            {
                return text;
            }

            return text.Substring(0, index);
        }

        public static string GetNullTerminatedString(BinaryReader file, int startIndex)
        {
            var bytes = new List<byte>(256);
            file.BaseStream.Position = startIndex;

            while (true)
            {
                byte b = file.ReadByte();

                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
            }

            string text = Encoding.ASCII.GetString(bytes.ToArray());
            return text;
        }
    }
}
