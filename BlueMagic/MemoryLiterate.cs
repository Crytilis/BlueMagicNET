using System;
using System.Text;

namespace BlueMagic
{
    public static class MemoryLiterate
    {
        public static byte[] Read(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            NativeMethods.ReadProcessMemory(processHandle, address, buffer, size);
            return buffer;
        }

        public static byte[] Read(SafeMemoryHandle processHandle, Pointer pointer, int size)
        {
            byte[] buffer;

            if (pointer.Offsets.Count == 0)
            {
                buffer = new byte[size];
                NativeMethods.ReadProcessMemory(processHandle, pointer.BaseAddress, buffer, size);
                return buffer;
            }

            int addressSize = MarshalType<IntPtr>.Size;
            buffer = new byte[addressSize];
            NativeMethods.ReadProcessMemory(processHandle, pointer.BaseAddress, buffer, addressSize);
            IntPtr address = TypeConverter.BytesToGenericType<IntPtr>(buffer);
            int offsetsCount = pointer.Offsets.Count - 1;

            for (int i = 0; i < offsetsCount; ++i)
            {
                NativeMethods.ReadProcessMemory(processHandle, address + pointer.Offsets[i], buffer, addressSize);
                address = TypeConverter.BytesToGenericType<IntPtr>(buffer);
            }

            buffer = new byte[size];
            NativeMethods.ReadProcessMemory(processHandle, address + pointer.Offsets[offsetsCount], buffer, size);
            return buffer;
        }

        public static T Read<T>(SafeMemoryHandle processHandle, IntPtr address) where T : struct
        {
            return TypeConverter.BytesToGenericType<T>(Read(processHandle, address, MarshalType<T>.Size));
        }

        public static T Read<T>(SafeMemoryHandle processHandle, Pointer pointer) where T : struct
        {
            return TypeConverter.BytesToGenericType<T>(Read(processHandle, pointer, MarshalType<T>.Size));
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

        public static string Read(SafeMemoryHandle processHandle, Pointer pointer, int size, Encoding encoding)
        {
            byte[] buffer = Read(processHandle, pointer, size);
            string s = encoding.GetString(buffer);
            int i = s.IndexOf('\0');
            if (i != -1)
                s = s.Remove(i);
            return s;
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, byte[] bytes)
        {
            using (new MemoryProtection(processHandle, address, bytes.Length))
                return NativeMethods.WriteProcessMemory(processHandle, address, bytes, bytes.Length) == bytes.Length;
        }

        public static bool Write(SafeMemoryHandle processHandle, Pointer pointer, byte[] bytes)
        {
            if (pointer.Offsets.Count == 0)
                using (new MemoryProtection(processHandle, pointer.BaseAddress, bytes.Length))
                    return NativeMethods.WriteProcessMemory(processHandle, pointer.BaseAddress, bytes, bytes.Length) == bytes.Length;

            int addressSize = MarshalType<IntPtr>.Size;
            IntPtr address = TypeConverter.BytesToGenericType<IntPtr>(Read(processHandle, pointer.BaseAddress, addressSize));
            int offsetsCount = pointer.Offsets.Count - 1;

            for (int i = 0; i < offsetsCount; ++i)
                address = TypeConverter.BytesToGenericType<IntPtr>(Read(processHandle, address + pointer.Offsets[i], addressSize));

            address += pointer.Offsets[offsetsCount];
            using (new MemoryProtection(processHandle, address, bytes.Length))
                return NativeMethods.WriteProcessMemory(processHandle, address, bytes, bytes.Length) == bytes.Length;
        }

        public static bool Write<T>(SafeMemoryHandle processHandle, IntPtr address, T value) where T : struct
        {
            return Write(processHandle, address, TypeConverter.GenericTypeToBytes(value));
        }

        public static bool Write<T>(SafeMemoryHandle processHandle, Pointer pointer, T value) where T : struct
        {
            return Write(processHandle, pointer, TypeConverter.GenericTypeToBytes(value));
        }

        public static bool Write(SafeMemoryHandle processHandle, IntPtr address, string value, Encoding encoding)
        {
            if (value[value.Length - 1] != '\0')
                value += '\0';

            byte[] bytes = encoding.GetBytes(value);
            return Write(processHandle, address, bytes);
        }

        public static bool Write(SafeMemoryHandle processHandle, Pointer pointer, string value, Encoding encoding)
        {
            if (value[value.Length - 1] != '\0')
                value += '\0';

            byte[] bytes = encoding.GetBytes(value);
            return Write(processHandle, pointer, bytes);
        }
    }
}
