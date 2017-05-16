﻿using System;
using System.Collections.Generic;

namespace BlueMagic.Memory
{
    public class Pointer : IDisposable
    {
        public IntPtr BaseAddress { get; private set; }
        public List<int> Offsets { get; private set; } = new List<int>();

        public Pointer(IntPtr baseAddress, params int[] offsets)
        {
            BaseAddress = baseAddress;

            foreach (int i in offsets)
                Offsets.Add(i);
        }

        ~Pointer()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
