// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace JeremyAnsel.Xwa.Opt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
    }
}
