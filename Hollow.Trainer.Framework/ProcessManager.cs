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
using System.Diagnostics;

namespace Hollow.Trainer.Framework
{
    public class ProcessManager : IDisposable
    {
        public Process TargetProcess { get; set; }
        public IntPtr TargetProcessHandle { get; set; }

        public bool Is64Bit
        {
            get
            {
                if ((Environment.OSVersion.Version.Major > 5)
                    || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
                {
                    try
                    {
                        bool retVal;
                        return !(Win32Api.Kernel32.IsWow64Process(TargetProcess.Handle, out retVal) && retVal);
                    }
                    catch
                    {
                        return false; // access is denied to the process
                    }
                }

                return false; // not on 64-bit Windows
            }
        }

        internal ProcessManager(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
                throw new Exception("TODO: Error no process found");

            if (processes.Length > 1)
                throw new Exception("TODO: more than 1 process found");

            TargetProcess = processes[0];


            IntPtr handle = Win32Api.Kernel32.OpenProcess(
                Win32Api.Kernel32.ProcessAccessFlags.CreateThread |
                Win32Api.Kernel32.ProcessAccessFlags.QueryInformation |
                Win32Api.Kernel32.ProcessAccessFlags.VirtualMemoryOperation |
                Win32Api.Kernel32.ProcessAccessFlags.VirtualMemoryRead |
                Win32Api.Kernel32.ProcessAccessFlags.VirtualMemoryWrite,
                false, TargetProcess.Id);

            if (handle == IntPtr.Zero)
                throw new Exception("TODO: Error opening process");

            TargetProcessHandle = handle;
        }

        public bool AdjustPrivilege(string privilegeName, bool enable)
        {
            if (TargetProcessHandle == IntPtr.Zero) // Maybe throw an exception instead?
                return false;

            IntPtr token;
            if (!Win32Api.Advapi32.OpenProcessToken(TargetProcessHandle,
                Win32Api.Advapi32.TOKEN_ADJUST_PRIVILEGES | Win32Api.Advapi32.TOKEN_QUERY,
                out token))
                return false; // Unable to open Process token

            Win32Api.Advapi32.TOKEN_PRIVILEGES privileges;
            privileges.PrivilegeCount = 1;
            privileges.Privileges = new Win32Api.Advapi32.LUID_AND_ATTRIBUTES[1];
            privileges.Privileges[0].Attributes = (enable) ? Win32Api.Advapi32.SE_PRIVILEGE_ENABLED : 0;
            if (!Win32Api.Advapi32.LookupPrivilegeValue(null, privilegeName, out privileges.Privileges[0].Luid))
            {
                // Close handle to the token
                Win32Api.Kernel32.CloseHandle(token);
                return false; // unable to lookup privilege
            }
                

            bool result = Win32Api.Advapi32.AdjustTokenPrivileges(token, 
                false, 
                ref privileges, 
                0, 
                IntPtr.Zero, 
                IntPtr.Zero);

            // close handle to token
            Win32Api.Kernel32.CloseHandle(token);

            return result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                    // Managed cleanup
                    //
                    TargetProcess.Dispose();      
                }

                //
                // Unmanaged cleanup
                //

                Win32Api.Kernel32.CloseHandle(TargetProcessHandle);

                //
                // Set large fields to null
                //

                disposedValue = true;
            }
        }

        ~ProcessManager()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
