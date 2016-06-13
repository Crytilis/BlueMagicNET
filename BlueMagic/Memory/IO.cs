using System;
using System.Text;

namespace BlueMagic.Memory
{
    public static class IO
    {
        public static byte[] ReadBytes(SafeMemoryHandle processHandle, IntPtr address, IntPtr length)
        {
            byte[] buffer = new byte[length.ToInt64()];
            Native.Methods.ReadProcessMemory(processHandle, address, buffer, length);
            return buffer;
        }

        public static T Read<T>(SafeMemoryHandle processHandle, IntPtr address)
        {
            return MarshalType<T>.BytesToGenericType(ReadBytes(processHandle, address, MarshalType<T>.Size));
        }

        public static string ReadString(SafeMemoryHandle processHandle, IntPtr address, IntPtr length, Encoding encoding)
        {
            byte[] buffer = ReadBytes(processHandle, address, length);
            string s = encoding.GetString(buffer);
            if (s.IndexOf('\0') != -1)
                s = s.Remove(s.IndexOf('\0'));
            return s;
        }

        public static void WriteBytes(SafeMemoryHandle processHandle, IntPtr address, byte[] bytes)
        {
            using (new Protection(processHandle, address, new IntPtr(bytes.Length)))
                Native.Methods.WriteProcessMemory(processHandle, address, bytes, new IntPtr(bytes.Length));
        }

        public static void Write<T>(SafeMemoryHandle processHandle, IntPtr address, T value)
        {
            WriteBytes(processHandle, address, MarshalType<T>.GenericTypeToBytes(value));
        }

        public static void WriteString(SafeMemoryHandle processHandle, IntPtr address, string value, Encoding encoding)
        {
            if (value[value.Length - 1] != '\0')
                value += '\0';

            byte[] bytes = encoding.GetBytes(value);
            WriteBytes(processHandle, address, bytes);
        }
    }
}
