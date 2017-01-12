﻿using BlueMagic.Native;
using System;

namespace BlueMagic.Memory
{
    public class Protection : IDisposable
    {
        public static SafeMemoryHandle ProcessHandle { get; private set; }
        public static IntPtr Address { get; private set; }
        public static int Size { get; private set; }
        public static MemoryProtectionType OldProtection { get; private set; }
        public static MemoryProtectionType NewProtection { get; private set; }

        public Protection(SafeMemoryHandle processHandle, IntPtr address, int size, MemoryProtectionType protection = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            ProcessHandle = processHandle;
            Address = address;
            Size = size;
            NewProtection = protection;
            OldProtection = Methods.ChangeMemoryProtection(ProcessHandle, Address, Size, NewProtection);
        }

        ~Protection()
        {
            Dispose();
        }

        public void Dispose()
        {
            Methods.ChangeMemoryProtection(ProcessHandle, Address, Size, OldProtection);
            GC.SuppressFinalize(this);
        }
    }
}