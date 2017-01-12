using BlueMagic.Native;
using System;
using System.Text;

namespace BlueMagic.Memory
{
    public static class Literate
    {
        public static byte[] Read(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            Methods.ReadProcessMemory(processHandle, address, buffer, size);
            return buffer;
        }

        public static T Read<T>(SafeMemoryHandle processHandle, IntPtr address) where T : struct
        {
            return TypeConverter.BytesToGenericType<T>(Read(processHandle, address, MarshalType<T>.Size));
        }

        public static string Read(SafeMemoryHandle processHandle, IntPtr address, int size, Encoding encoding)
        {
            byte[] buffer = Read(processHandle, address, size);
            string s = encoding.GetString(buffer);
            int i = s.IndexOf('\0');
            if (i != -1)
                s = s.Remove(i);
            return s;
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, byte[] bytes)
        {
            using (new Protection(processHandle, address, bytes.Length))
                return Methods.WriteProcessMemory(processHandle, address, bytes, bytes.Length) == bytes.Length;
        }

        public static bool Write<T>(SafeMemoryHandle processHandle, IntPtr address, T value) where T : struct
        {
            return Write(processHandle, address, TypeConverter.GenericTypeToBytes(value));
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, string value, Encoding encoding)
        {
            if (value[value.Length - 1] != '\0')
                value += '\0';

            byte[] bytes = encoding.GetBytes(value);
            return Write(processHandle, address, bytes);
        }
    }
}
