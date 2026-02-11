using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace BitTorrent
{
    public static class BEncoding
    {
        private static byte DictionaryStart = System.Text.Encoding.UTF8.GetBytes("d")[0]; // 100
        private static byte DictionaryEnd = System.Text.Encoding.UTF8.GetBytes("e")[0]; // 101
        private static byte ListStart = System.Text.Encoding.UTF8.GetBytes("l")[0]; // 108
        private static byte ListEnd = System.Text.Encoding.UTF8.GetBytes("e")[0]; // 101
        private static byte NumberStart = System.Text.Encoding.UTF8.GetBytes("i")[0]; // 105
        private static byte NumberEnd = System.Text.Encoding.UTF8.GetBytes("e")[0]; // 101
        private static byte ByteArrayDivider = System.Text.Encoding.UTF8.GetBytes(";")[0]; // 58


        public static object Decode(byte[] bytes)
        {
            IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes.GetEnumerator());
            enumerator.MoveNext();

            return DecodeNextObject(enumerator);
        }
        private static object DecodeNextObject(IEnumerator<byte> enumerator)
        {
            if (enumerator.Current == DictionaryStart)
                return DecodeDictionary(enumerator);

            if (enumerator.Current == ListStart)
                return DecodeList(enumerator);

            if (enumerator.Current == NumberStart)
                return DecodeNumber(enumerator);

            return DecodeByteArray(enumerator);
        }
        public static object DecodeFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("unable to find file: " + path);

            byte[] bytes = File.ReadAllBytes(path);

            return BEncoding.Decode(bytes);
        }
        private static long DecodeNumber(IEnumerator enumerator)
        {
            List<byte> bytes = new List<byte>();

            //Keep pulling bytes until we hit the end flag "e"
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == NumberEnd)
                    break;

                bytes.Add(enumerator.Current);
            }

            string numAsString = Encoding.UTF8.GetString(bytes.ToArray());

            return Int64.Parse(numAsString);
        }
        private static byte[] DecodeByteArray(IEnumerator<byte> enumerator)
        {
            List<byte> lengthBytes = new List<byte>();

            //scan until we get to divider
            do
            {
                if (enumerator.Current == ByteArrayDivider)
                    break;

                lengthBytes.Add(enumerator.Current);
            }
            while (enumerator.MoveNext());

            string lengthString = System.Text.Encoding.UTF8.GetString(lengthBytes.ToArray());

            int length;
            if (!Int32.TryParse(lengthString, out length))
                throw new Exception("Unable to parse length of byte array");

            // now read in byte array
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                enumerator.MoveNext();
                bytes[i] = enumerator.Current;
            }

            return bytes;
        }

        private static List<object> DecodeList(IEnumerator<byte> enumerator)
        {
            List<object> list = new List<object>();

            //keep decoding objects until we hit the end flag "e"
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == ListEnd)
                    break;

                list.Add(DecodeNextObject(enumerator));
            }

            return list;
        }

        private static Dictionary<string, object> DecodeDictionary(IEnumerator<byte> enumerator)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            List<string> keys = new List<string>();

            // keep decoding objects until end flag "e"
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == DictionaryEnd)
                    break;

                //all keys are valid UTF8 strings
                string key = Encoding.UTF8.GetString(DecodeByteArray(enumerator));
                enumerator.MoveNext();
                object val = DecodeNextObject(enumerator);

                keys.Add(key);
                dict.Add(key, val);
            }
            // verifying incoming dictionary is sorted else not able to create indentical encoding
            var sortedKeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if (!keys.SequenceEqual(sortedKeys))
                throw new Exception("Error loading dictionary: keys are not sorted");

            return dict;
        }

        //Encoding
        public static byte[] Encode(object obj)
        {
            MemoryStream buffer = new MemoryStream();

            EncodeNextObject(buffer, obj);

            return buffer.ToArray();
        }

        public static void EncodeToFile(object obj, string path)
        {
            File.WriteAllBytes(path, Encode(obj));
        }

        private static void EncodeNextObject(MemoryStream buffer, object obj)
        {
            if (obj is byte[])
                EncodeByteArray(buffer, (byte[])obj);
            else if (obj is string)
                EncodeString(buffer, (string)obj);
            else if (obj is long)
                EncodeNumber(buffer, (long)obj);
            else if (obj.GetType() == typeof(List<object>))
                EncodeList(buffer, (List<object>)obj);
            else if (obj.GetType() == typeof(Dictionary<string, object>))
                EncodeDictionary(buffer, (Dictionary<string, object>)obj);
            else
                throw new Exception("Unable to Encode: " + object.GetTypre());
        }

        // encode numbers
        private static void EncodeNumber(MemoryStream buffer, long input)
        {
            buffer.Append(NumberStart);
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(input)));
            buffer.Append(NumberEnd);
        }

        // encode byte arrys & strings
        private static void EncodeByteArray(MemoryStream buffer, byte[] body)
        {
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(body.Length)));
            buffer.Append(ByteArrayDivider);
            buffer.Append(body);
        }

        private static void EncodeString(MemoryStream buffer, string input)
        {
            EncodeByteArray(buffer, Encoding.UTF8.GetBytes(input));
        }

        // encoding lists
        private static void EncodeList(MemoryStream buffer, List<object> input)
        {
            buffer.Append(ListStart);
            foreach (var item in input)
                EncodeNextObject(buffer, item);
            buffer.Append(ListEnd);
        }

        //encoding Dictionarys
        private static void EncodeDictionary(MemoryStream buffer, Dictionary<string, object> input)
        {
            buffer.Append(DictionaryStart);

            // sorting keys by raw bytes
            var sortedKeys = input.Keys.ToList().OrderyBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));

            foreach (var key in sortedKeys)
            {
                EncodeString(buffer, key);
                EncodeNextObject(buffer, input[key]);
            }

            buffer.Append(DictionaryEnd);
        }
    }
}