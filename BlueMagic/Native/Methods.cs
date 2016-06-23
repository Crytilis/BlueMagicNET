using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueMagic.Native
{
    public static class Methods
    {
        public static SafeMemoryHandle OpenProcess(int pId, AccessFlags accessFlags = AccessFlags.PROCESS_ALL_ACCESS)
        {
            SafeMemoryHandle processHandle = Imports.OpenProcess(accessFlags, false, pId);
            if (processHandle == null || processHandle.IsInvalid || processHandle.IsClosed)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to open process {1} with access {2}",
                    Marshal.GetLastWin32Error(), pId, accessFlags.ToString("X")));
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

        public static IntPtr ReadProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, IntPtr sizeBytes)
        {
            IntPtr bytesRead;
            if (!Imports.ReadProcessMemory(processHandle, address, buffer, sizeBytes, out bytesRead))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to read memory from 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString("X8"), sizeBytes));
            return bytesRead;
        }

        public static IntPtr WriteProcessMemory(SafeMemoryHandle processHandle, IntPtr address, [Out] byte[] buffer, IntPtr sizeBytes)
        {
            IntPtr bytesWritten;
            if (!Imports.WriteProcessMemory(processHandle, address, buffer, sizeBytes, out bytesWritten))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to write memory at 0x{1}[Size: {2}]",
                    Marshal.GetLastWin32Error(), address.ToString("X8"), sizeBytes));
            return bytesWritten;
        }

        public static MemoryProtectionType ChangeMemoryProtection(SafeMemoryHandle processHandle, IntPtr address, IntPtr sizeBytes, MemoryProtectionType newProtect)
        {
            MemoryProtectionType oldProtect;
            if (!Imports.VirtualProtectEx(processHandle, address, sizeBytes, newProtect, out oldProtect))
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to change memory protection of process handle 0x{1} at 0x{2}[Size: {3}] to {4}",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString("X8"), sizeBytes, newProtect.ToString("X")));
            return oldProtect;
        }

        public static MemoryBasicInformation Query(SafeMemoryHandle processHandle, IntPtr address, IntPtr sizeBytes)
        {
            MemoryBasicInformation memInfo;
            if (Imports.VirtualQueryEx(processHandle, address, out memInfo, sizeBytes) == 0)
                throw new Win32Exception(string.Format("[Error Code: {0}] Unable to retrieve memory information of process handle 0x{1} from 0x{2}[Size: {3}]",
                    Marshal.GetLastWin32Error(), processHandle.DangerousGetHandle().ToString("X"), address.ToString("X8"), sizeBytes));
            return memInfo;
        }
    }
}
