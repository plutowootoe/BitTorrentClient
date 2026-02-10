using System;
using System.IO;


namespace BitTorrent
{

    // source Fredrik Mörk (http://stackoverflow.com/a/4015634)

    public static class MemoryStreamExtensions
    {
        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
}