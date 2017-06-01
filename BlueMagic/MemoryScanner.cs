using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlueMagic
{
    public static class MemoryScanner
    {
        public static List<IntPtr> ScanForBytes(byte[] bytes, byte[] buffer)
        {
            List<IntPtr> results = new List<IntPtr>();
            int bytesLength = bytes.Length;
            int bufferlength = buffer.Length - bytesLength;
            int i, j, k;
            bool f;

            for (i = 0; i <= bufferlength; ++i)
            {
                if (buffer[i] == bytes[0])
                {
                    for (j = i, k = 1, f = true; k < bytesLength; ++i, ++k)
                    {
                        if (buffer[j + k] != bytes[k])
                        {
                            f = false;
                            break;
                        }
                    }

                    if (f)
                        results.Add(new IntPtr(j));
                }
            }

            return results;
        }

        public static List<IntPtr> ScanForGeneric<T>(T value, byte[] buffer) where T : struct
        {
            return ScanForBytes(TypeConverter.GenericTypeToBytes(value), buffer);
        }

        public static List<IntPtr> ScanForSignature(Signature signature, byte[] buffer)
        {
            if (signature.Bytes != null)
                return ScanForBytes(signature.Bytes, buffer);

            List<IntPtr> results = new List<IntPtr>();
            int signatureStringLength = signature.String.Length;
            int signatureBytesLength = signatureStringLength / 2;
            int bufferLength = buffer.Length - signatureBytesLength;
            string s = signature.String;
            int i, j, k, l, m, x = -1, y, z;
            bool f;
            string b;

            for (i = 0; x < 0 && i < signatureStringLength; i += 2)
                if (s.Substring(i, 2) != "??")
                    x = i / 2;

            int bytesLength = signatureBytesLength - x;

            for (i = x, y = x * 2, z = y + 1; i <= bufferLength; ++i)
            {
                b = buffer[i].ToString("X2");

                if ((s[y] == b[0] && s[z] == b[1]) || (s[y] == '?' && s[z] == '?') || (s[y] == '?' && s[z] == b[1]) || (s[z] == '?' && s[y] == b[0]))
                {
                    for (j = i, k = 1, l = (k * 2) + y, m = l + 1, f = true; k < bytesLength; ++i, ++k, l += 2, m = l + 1)
                    {
                        b = buffer[j + k].ToString("X2");

                        if ((s[l] != '?' && s[l] != b[0]) || (s[m] != '?' && s[m] != b[1]))
                        {
                            f = false;
                            break;
                        }
                    }

                    if (f)
                        results.Add(new IntPtr(j - x));
                }
            }

            return results;
        }

        public static List<IntPtr> ScanRegionForBytes(SafeMemoryHandle processHandle, byte[] bytes, MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForBytes(bytes, MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionForGeneric<T>(SafeMemoryHandle processHandle, T value, MemoryBasicInformation region) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForGeneric(value, MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionForSignature(SafeMemoryHandle processHandle, Signature signature, MemoryBasicInformation region)
        {
            List<IntPtr> results = new List<IntPtr>();
            List<IntPtr> addresses = ScanForSignature(signature, MemoryLiterate.Read(processHandle, region.BaseAddress, region.RegionSize.ToInt32()));
            foreach (IntPtr address in addresses)
                results.Add(new IntPtr(region.BaseAddress.ToInt64() + address.ToInt64()));

            return results;
        }

        public static List<IntPtr> ScanRegionsForBytes(SafeMemoryHandle processHandle, byte[] bytes, List<MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForBytes(processHandle, bytes, region));

            return results;
        }

        public static List<IntPtr> ScanRegionsForGeneric<T>(SafeMemoryHandle processHandle, T value, List<MemoryBasicInformation> regions) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForGeneric(processHandle, value, region));

            return results;
        }

        public static List<IntPtr> ScanRegionsForSignature(SafeMemoryHandle processHandle, Signature signature, List<MemoryBasicInformation> regions)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (MemoryBasicInformation region in regions)
                results.AddRange(ScanRegionForSignature(processHandle, signature, region));

            return results;
        }

        public static List<IntPtr> ScanModuleForBytes(Process process, SafeMemoryHandle processHandle, byte[] bytes, ProcessModule module)
        {
            return ScanRegionsForBytes(processHandle, bytes, MemoryRegions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanModuleForGeneric<T>(Process process, SafeMemoryHandle processHandle, T value, ProcessModule module) where T : struct
        {
            return ScanRegionsForGeneric(processHandle, value, MemoryRegions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanModuleForSignature(Process process, SafeMemoryHandle processHandle, Signature signature, ProcessModule module)
        {
            return ScanRegionsForSignature(processHandle, signature, MemoryRegions.Load(process, processHandle, module));
        }

        public static List<IntPtr> ScanAllModulesForBytes(Process process, SafeMemoryHandle processHandle, byte[] bytes)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForBytes(process, processHandle, bytes, module))
                    results.Add(address);

            return results;
        }

        public static List<IntPtr> ScanAllModulesForGeneric<T>(Process process, SafeMemoryHandle processHandle, T value) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForGeneric(process, processHandle, value, module))
                    results.Add(address);

            return results;
        }

        public static List<IntPtr> ScanAllModulesForSignature(Process process, SafeMemoryHandle processHandle, Signature signature)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (ProcessModule module in process.Modules)
                foreach (IntPtr address in ScanModuleForSignature(process, processHandle, signature, module))
                    results.Add(address);

            return results;
        }

        public static IntPtr RescanForBytes(SafeMemoryHandle processHandle, byte[] bytes, IntPtr address)
        {
            if (MemoryLiterate.Read(processHandle, address, bytes.Length) == bytes)
                return address;

            return IntPtr.Zero;
        }

        public static IntPtr RescanForGeneric<T>(SafeMemoryHandle processHandle, T value, IntPtr address) where T : struct
        {
            return RescanForBytes(processHandle, TypeConverter.GenericTypeToBytes(value), address);
        }

        public static IntPtr RescanForSignature(SafeMemoryHandle processHandle, Signature signature, IntPtr address)
        {
            if (signature.Bytes != null)
                return RescanForBytes(processHandle, signature.Bytes, address);

            int signatureLength = signature.String.Length;
            int signatureBytesLength = signatureLength / 2;
            byte[] buffer = MemoryLiterate.Read(processHandle, address, signature.String.Length / 2);
            string s = signature.String;
            int i, j = -1;
            string b;

            for (i = 0; j < 0 && i < signatureLength; i += 2)
                if (s.Substring(i, 2) != "??")
                    j = i / 2;

            int bytesLength = signatureBytesLength - j;

            for (i = j, j = i + 1; i < bytesLength; ++i, ++j)
            {
                b = buffer[i].ToString("X2");

                if ((s[i] != '?' && s[i] != b[0]) || (s[j] != '?' && s[j] != b[1]))
                    return IntPtr.Zero;
            }

            return address;
        }

        public static List<IntPtr> RescanForBytes(SafeMemoryHandle processHandle, byte[] bytes, List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForBytes(processHandle, bytes, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForGeneric<T>(SafeMemoryHandle processHandle, T value, List<IntPtr> addresses) where T : struct
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForGeneric(processHandle, value, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }

        public static List<IntPtr> RescanForSignature(SafeMemoryHandle processHandle, Signature signature, List<IntPtr> addresses)
        {
            List<IntPtr> results = new List<IntPtr>();
            foreach (IntPtr address in addresses)
            {
                IntPtr result = RescanForSignature(processHandle, signature, address);
                if (result != IntPtr.Zero)
                    results.Add(result);
            }

            return results;
        }
    }
}
