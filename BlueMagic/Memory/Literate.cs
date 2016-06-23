using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueMagic.Memory
{
    public static class Literate
    {
        public static byte[] ReadBytes(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            Native.Methods.ReadProcessMemory(processHandle, address, buffer, size);
            return buffer;
        }

        public static T Read<T>(SafeMemoryHandle processHandle, IntPtr address) where T : struct
        {
            return TypeConverter.BytesToGenericType<T>(ReadBytes(processHandle, address, MarshalType<T>.Size));
        }

        public static string ReadString(SafeMemoryHandle processHandle, IntPtr address, int size, Encoding encoding)
        {
            byte[] buffer = ReadBytes(processHandle, address, size);
            string s = encoding.GetString(buffer);
            if (s.IndexOf('\0') != -1)
                s = s.Remove(s.IndexOf('\0'));
            return s;
        }

        public static bool WriteBytes(SafeMemoryHandle processHandle, IntPtr address, byte[] bytes)
        {
            using (new ManagedProtection(processHandle, address, bytes.Length))
                return Native.Methods.WriteProcessMemory(processHandle, address, bytes, bytes.Length) == bytes.Length;
        }

        public static bool Write<T>(SafeMemoryHandle processHandle, IntPtr address, T value) where T : struct
        {
            byte[] bytes = TypeConverter.GenericTypeToBytes(value);
            int size = MarshalType<T>.Size;
            IntPtr hObj = Marshal.AllocHGlobal(MarshalType<T>.Size);
            try
            {
                Marshal.StructureToPtr(value, hObj, false);
                bytes = new byte[size];
                Marshal.Copy(hObj, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }
            return WriteBytes(processHandle, address, TypeConverter.GenericTypeToBytes(value));
        }

        public static bool WriteString(SafeMemoryHandle processHandle, IntPtr address, string value, Encoding encoding)
        {
            if (value[value.Length - 1] != '\0')
                value += '\0';

            byte[] bytes = encoding.GetBytes(value);
            return WriteBytes(processHandle, address, bytes);
        }
    }
}
