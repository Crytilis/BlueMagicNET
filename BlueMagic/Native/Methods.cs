using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueMagic
{
    public static unsafe class NativeMethods
    {
        public static SafeMemoryHandle OpenProcess(int pId, ProcessAccessRights accessRights = ProcessAccessRights.PROCESS_ALL_ACCESS)
        {
            SafeMemoryHandle processHandle = Imports.OpenProcess(accessRights, false, pId);
            if (processHandle == null || processHandle.IsInvalid || processHandle.IsClosed)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to open process {1} with access {2}",
                    Marshal.GetLastWin32Error(), pId, accessRights.ToString("X")));
            return processHandle;
        }

        public static int GetProcessId(SafeMemoryHandle processHandle)
        {
            int pId = Imports.GetProcessId(processHandle);
            if (pId == 0)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to get Id from process handle 0x{1}",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X")));
            return pId;
        }

        public static bool Is64BitProcess(SafeMemoryHandle processHandle)
        {
            bool Is64BitProcess;
            if (!Imports.IsWow64Process(processHandle, out Is64BitProcess))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to determine if process handle 0x{1} is 64 bit",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X")));
            return !Is64BitProcess;
        }

        public static string GetClassName(IntPtr windowHandle)
        {
            StringBuilder stringBuilder = new StringBuilder(char.MaxValue);
            if (Imports.GetClassName(windowHandle, stringBuilder, stringBuilder.Capacity) == 0)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to get class name from window handle 0x{1}",
                    Marshal.GetLastWin32Error(), windowHandle.ToString("X")));
            return stringBuilder.ToString();
        }

        public static bool CloseHandle(IntPtr handle)
        {
            if (!Imports.CloseHandle(handle))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to close handle 0x{1}",
                    Marshal.GetLastWin32Error(), handle.ToString("X")));
            return true;
        }

        public static int ReadProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, int size)
        {
            int bytesRead = 0;
            if (!Imports.ReadProcessMemory(processHandle, address, buffer, size, out bytesRead))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to read memory from 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size));
            return bytesRead;
        }

        public static int WriteProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, int size)
        {
            int bytesWritten = 0;
            if (!Imports.WriteProcessMemory(processHandle, address, buffer, size, out bytesWritten))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to write memory at 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size));
            return bytesWritten;
        }

        public static IntPtr Allocate([Optional] IntPtr address, int size, MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            IntPtr ret = Imports.VirtualAlloc(address, size, MemoryAllocationState.MEM_COMMIT, protect);
            if (ret.Equals(0))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to allocate memory at 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size));
            return ret;
        }

        public static IntPtr Allocate(SafeMemoryHandle processHandle, [Optional] IntPtr address, int size, MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            IntPtr ret = Imports.VirtualAllocEx(processHandle, address, size, MemoryAllocationState.MEM_COMMIT, protect);
            if (ret.Equals(0))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to allocate memory to process handle 0x{1} at 0x{2}[Size: {3}]",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size));
            return ret;
        }

        public static bool Free(IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
        {
            if (!Imports.VirtualFree(address, size, free))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to free memory at 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size));
            return true;
        }

        public static bool Free(SafeMemoryHandle processHandle, IntPtr address, int size = 0, MemoryFreeType free = MemoryFreeType.MEM_RELEASE)
        {
            if (!Imports.VirtualFreeEx(processHandle, address, size, free))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to free memory from process handle 0x{1} at 0x{2}[Size: {3}]",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size));
            return true;
        }

        public static void Copy(void* destination, void* source, int size)
        {
            try
            {
                Imports.MoveMemory(destination, source, size);
            }
            catch
            {
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to copy memory to {0} from {1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), (*(ulong*)(destination)).ToString($"X{IntPtr.Size}"), (*(ulong*)(source)).ToString($"X{IntPtr.Size}"), size));
            }
        }

        public static MemoryProtectionType ChangeMemoryProtection(IntPtr address, int size, MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            MemoryProtectionType oldProtect;
            if (!Imports.VirtualProtect(address, size, newProtect, out oldProtect))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to change memory protection at 0x{1}[Size: {2}] to {3}",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size, newProtect.ToString("X")));
            return oldProtect;
        }

        public static MemoryProtectionType ChangeMemoryProtection(SafeMemoryHandle processHandle, IntPtr address, int size, MemoryProtectionType newProtect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            MemoryProtectionType oldProtect;
            if (!Imports.VirtualProtectEx(processHandle, address, size, newProtect, out oldProtect))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to change memory protection of process handle 0x{1} at 0x{2}[Size: {3}] to {4}",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size, newProtect.ToString("X")));
            return oldProtect;
        }

        public static MemoryBasicInformation Query(IntPtr address, int size)
        {
            MemoryBasicInformation memInfo;
            if (Imports.VirtualQuery(address, out memInfo, size) == 0)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to retrieve memory information from 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString($"X{IntPtr.Size}"), size));
            return memInfo;
        }

        public static MemoryBasicInformation Query(SafeMemoryHandle processHandle, IntPtr address, int size)
        {
            MemoryBasicInformation memInfo;
            if (Imports.VirtualQueryEx(processHandle, address, out memInfo, size) == 0)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to retrieve memory information of process handle 0x{1} from 0x{2}[Size: {3}]",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString($"X{IntPtr.Size}"), size));
            return memInfo;
        }
    }
}
