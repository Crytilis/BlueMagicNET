using System;

namespace BlueMagic.Memory
{
    public class Protection : IDisposable
    {
        public static SafeMemoryHandle ProcessHandle { get; private set; }
        public static IntPtr Address { get; private set; }
        public static IntPtr Size { get; private set; }
        public static Native.MemoryProtectionType OldProtection { get; private set; }
        public static Native.MemoryProtectionType NewProtection { get; private set; }

        public Protection(SafeMemoryHandle processHandle, IntPtr address, IntPtr size, Native.MemoryProtectionType protection = Native.MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            ProcessHandle = processHandle;
            Address = address;
            Size = size;
            NewProtection = protection;
            OldProtection = Native.Methods.ChangeMemoryProtection(ProcessHandle, Address, Size, NewProtection);
        }

        ~Protection()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Native.Methods.ChangeMemoryProtection(ProcessHandle, Address, Size, OldProtection);
            GC.SuppressFinalize(this);
        }
    }
}
