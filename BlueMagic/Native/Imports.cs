using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BlueMagic.Native
{
    internal unsafe class Imports
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeMemoryHandle OpenProcess(
            AccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetProcessId(
            SafeMemoryHandle handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process(
            SafeMemoryHandle hProcess,
            [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetClassName(
            IntPtr hWnd,
            StringBuilder lpClassName,
            int nMaxCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadProcessMemory(
            SafeMemoryHandle hProcess,
            IntPtr dwAddress,
            [Out] byte[] lpBuffer,
            IntPtr nSize,
            out IntPtr lpBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WriteProcessMemory(
            SafeMemoryHandle hProcess,
            IntPtr dwAddress,
            [Out] byte[] lpBuffer,
            IntPtr nSize,
            out IntPtr iBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        internal static extern void MoveMemory(
            void* destination,
            void* source,
            IntPtr nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            IntPtr nSize,
            MemoryAllocationType dwAllocationType,
            MemoryProtectionType dwProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFreeEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            IntPtr nSize,
            MemoryFreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int VirtualQueryEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            out MemoryBasicInformation lpBuffer,
            IntPtr dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtectEx(
            SafeMemoryHandle hProcess,
            IntPtr lpAddress,
            IntPtr nSize,
            MemoryProtectionType flNewProtect,
            out MemoryProtectionType lpflOldProtect);
    }
}
