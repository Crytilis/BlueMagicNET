using System;
using System.Runtime.InteropServices;

namespace BlueMagic.Memory
{
    public static class Manager
    {
        public static IntPtr Allocate(int size, [Optional] IntPtr address)
        {
            return Native.Methods.Allocate(address, size, Native.MemoryProtectionType.PAGE_EXECUTE_READWRITE);
        }

        public static IntPtr Allocate(int size, SafeMemoryHandle processHandle, [Optional] IntPtr address)
        {
            return Native.Methods.Allocate(processHandle, address, size, Native.MemoryProtectionType.PAGE_EXECUTE_READWRITE);
        }

        public static void Free(IntPtr address)
        {
            Native.Methods.Free(address, 0, Native.MemoryFreeType.MEM_RELEASE);
        }

        public static void Free(SafeMemoryHandle processHandle, IntPtr address)
        {
            Native.Methods.Free(processHandle, address, 0, Native.MemoryFreeType.MEM_RELEASE);
        }

        public static unsafe void Copy(void* destination, void* source, int size)
        {
            Native.Imports.MoveMemory(destination, source, size);
        }
    }
}
