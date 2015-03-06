/*

Copyright (c) 2015, Michael Combs
All rights reserved.

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, 
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, 
this list of conditions and the following disclaimer in the documentation 
and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace Hollow.Trainer.Framework
{
    public class MemoryOperations
    {
        private ProcessManager Process { get; set; }

        internal MemoryOperations(ProcessManager process)
        {
            Process = process;
        }

        public byte[] ReadMemory(IntPtr address, int length)
        {
            byte[] buffer = new byte[length];

            bool ret = Win32Api.Kernel32.ReadProcessMemory(
                Process.TargetProcessHandle, 
                address, 
                buffer, 
                length, 
                IntPtr.Zero);

            if (!ret)
                throw new Exception("TODO: Error reading process memory");

            return buffer;
        }

        public byte[] ReadMemory(IntPtr address, int length, params int[] offsets)
        {
            int pointerCount = offsets.Length - 1;

            if (offsets.Length > 0)
            {
                IntPtr pointer = ReadPointer(address);
                for (var i = 0; i < pointerCount; i++)
                {
                    pointer = IntPtr.Add(pointer, offsets[i]);
                    pointer = ReadPointer(pointer);
                }
                address = IntPtr.Add(pointer, offsets[pointerCount]);
            }

            return ReadMemory(address, length);
        }

        public byte ReadByte(IntPtr address, params int[] offsets)
        {
            byte[] data = ReadMemory(address, 1, offsets);
            return data[0];
        }

        public short ReadInt16(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToInt16(ReadMemory(address, 2, offsets), 0);
        }

        public ushort ReadUInt16(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToUInt16(ReadMemory(address, 2, offsets), 0);
        }

        public int ReadInt32(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToInt32(ReadMemory(address, 4, offsets), 0);
        }

        public uint ReadUInt32(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToUInt32(ReadMemory(address, 4, offsets), 0);
        }

        public float ReadFloat(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToSingle(ReadMemory(address, 4, offsets), 0);
        }

        public double ReadDouble(IntPtr address, params int[] offsets)
        {
            return BitConverter.ToDouble(ReadMemory(address, 8, offsets), 0);
        }

        public IntPtr ReadPointer(IntPtr address)
        {
            int length = Process.Is64Bit ? 8 : 4;

            byte[] pointer = ReadMemory(address, length);

            if (Process.Is64Bit)
                return new IntPtr(BitConverter.ToInt64(pointer, 0));

            return new IntPtr(BitConverter.ToInt32(pointer, 0));

        }

        public void WriteMemory(IntPtr address, byte[] data)
        {
            bool ret = Win32Api.Kernel32.WriteProcessMemory(
                Process.TargetProcessHandle, 
                address,
                data,
                data.Length,
                IntPtr.Zero);

            if (!ret)
                throw new Exception("TODO: Error writing process memory");
        }

        public void WriteMemory(IntPtr address, byte[] data, params int[] offsets)
        {
            int pointerCount = offsets.Length - 1;

            if (offsets.Length > 0)
            {
                IntPtr pointer = ReadPointer(address);
                for (var i = 0; i < pointerCount; i++)
                {
                    pointer = IntPtr.Add(pointer, offsets[i]);
                    pointer = ReadPointer(pointer);
                }
                address = IntPtr.Add(pointer, offsets[pointerCount]);
            }

            WriteMemory(address, data);
        }

        public void WriteMemory(IntPtr address, byte data, params int[] offsets)
        {
            WriteMemory(address, new byte[] { data }, offsets);
        }

        public void WriteMemory(IntPtr address, short data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }

        public void WriteMemory(IntPtr address, ushort data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }

        public void WriteMemory(IntPtr address, int data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }

        public void WriteMemory(IntPtr address, uint data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }

        public void WriteMemory(IntPtr address, float data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }

        public void WriteMemory(IntPtr address, double data, params int[] offsets)
        {
            WriteMemory(address, BitConverter.GetBytes(data), offsets);
        }


        public IntPtr AllocateMemory(uint size)
        {

            IntPtr ret = Win32Api.Kernel32.VirtualAllocEx(
                Process.TargetProcessHandle,
                IntPtr.Zero,
                size,
                Win32Api.Kernel32.MemoryAllocateType.Commit | Win32Api.Kernel32.MemoryAllocateType.Reserve,
                Win32Api.Kernel32.PageProtection.ExecuteReadWrite);

            if (ret == IntPtr.Zero)
                throw new Exception("TODO: Error allocating memory");

            return ret;
        }

        public bool FreeMemory(IntPtr address)
        {
            return Win32Api.Kernel32.VirtualFreeEx(
                Process.TargetProcessHandle,
                address,
                0,
                Win32Api.Kernel32.MemoryFreeType.Release);
        }

        public IntPtr InjectCode(IntPtr address, byte[] newBytes, byte[] orginalBytes)
        {
            if (orginalBytes.Length < 5)
                throw new Exception("TODO: Please ensure orginalbytes is at least 5 bytes");

            IntPtr caveAddress = AllocateMemory((uint)(newBytes.Length + 5));
            int nopsNeeded = orginalBytes.Length > 5 ? orginalBytes.Length - 5 : 0;
            // (to - from - 5)
            int offset = caveAddress.ToInt32() - address.ToInt32() - 5;

            // Write the new jump
            WriteMemory(address, (byte)0xE9);
            address = IntPtr.Add(address, 1); // increment address by 1
            WriteMemory(address, BitConverter.GetBytes(offset));
            address = IntPtr.Add(address, 4); // increment by 4

            // Write the nops
            if (nopsNeeded > 0)
            {
                byte[] nops = new byte[nopsNeeded];

                for (var i = 0; i < nopsNeeded; i++)
                {
                    nops[i] = 0x90;
                }

                WriteMemory(address, nops);

                address = IntPtr.Add(address, nopsNeeded); // incrementing by number of nops we added
            }

            // Write the bytes to the cave address
            WriteMemory(caveAddress, newBytes);

            // Write the return jmp
            IntPtr returnJmpAddress = IntPtr.Add(caveAddress, newBytes.Length);
            offset = address.ToInt32() - returnJmpAddress.ToInt32() - 5;
            WriteMemory(returnJmpAddress, (byte)0xE9);
            WriteMemory(IntPtr.Add(returnJmpAddress, 1), BitConverter.GetBytes(offset));
            

            return caveAddress;
        }

        public bool RemoveCode(IntPtr orginalAddress, IntPtr codeCave, byte[] orginalBytes)
        {
            WriteMemory(orginalAddress, orginalBytes);
            return FreeMemory(codeCave);
        }

        public uint InjectThread(IntPtr fnFunction, string argument)
        {
            char[] nullChar = { '\0' };
            int argumentSize = Encoding.Unicode.GetByteCount(argument) + Encoding.Unicode.GetByteCount(nullChar);
            byte[] argumentBytes = Encoding.Unicode.GetBytes(argument);
            byte[] nullBytes = Encoding.Unicode.GetBytes(nullChar);

            IntPtr baseAddress = AllocateMemory((uint)argumentSize);
            WriteMemory(baseAddress, argumentBytes);
            WriteMemory((baseAddress + argumentBytes.Length), nullBytes);

            IntPtr threadHandle = Win32Api.Kernel32.CreateRemoteThread(Process.TargetProcessHandle,
                IntPtr.Zero,
                0,
                fnFunction,
                baseAddress,
                0,
                IntPtr.Zero);

            Win32Api.Kernel32.WaitForSingleObject(threadHandle, uint.MaxValue);

            FreeMemory(baseAddress);

            uint exitCode = 0;
            Win32Api.Kernel32.GetExitCodeThread(threadHandle, out exitCode);

            Win32Api.Kernel32.CloseHandle(threadHandle);

            return exitCode;
        }
    }
}
