using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LoadGraphml
{
    public class StreamUtils
    {
        public static void WriteString(MemoryStream stream, string str)
        {
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
            WriteInt32(stream, strBytes.Length);
            stream.Write(strBytes, 0, strBytes.Length);
        }
        public static void WriteInt32(MemoryStream stream,int num) { 
            byte [] numByte = BitConverter.GetBytes(num);
            stream.Write(numByte, 0, numByte.Length);
        }

        public static int ReadInt32(MemoryStream stream)
        {
            byte[] numBytes = new byte[4];
            stream.Read(numBytes, 0, 4);
            return BitConverter.ToInt32(numBytes);
        }
        public static string ReadString(MemoryStream stream)
        {
            int strLen = ReadInt32(stream);
            byte[] strBytes = new byte[strLen];
            stream.Read(strBytes, 0, strBytes.Length);
            return Encoding.UTF8.GetString(strBytes);
      
        }
    }
}
