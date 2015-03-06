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
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Reflection;


namespace Hollow.Trainer.Framework
{
    public abstract class TrainerBase : IDisposable
    {
        public abstract void RegisterHotKeys();

        public abstract void AddTrainerItem(ITrainerItem item);


        public ProcessManager Process { get; private set; }
        public MemoryOperations Memory { get; private set; }

        protected void OpenProcess(string processName)
        {
            Process = new ProcessManager(processName);
            Memory = new MemoryOperations(Process);
        }

        protected bool InjectManagedDll(string fullPath, string fullClassName, string methodName, string argument)
        {
            if (!Process.AdjustPrivilege(Win32Api.Advapi32.SE_DEBUG_NAME, true))
                return false; // Unable to adjust process token

            IntPtr fnLoadLibrary = Win32Api.Kernel32.GetProcAddress(
                Win32Api.Kernel32.GetModuleHandle("kernel32"),
                "LoadLibraryW");

            Memory.InjectThread(fnLoadLibrary, GetManagedBootstrapPath());

            ProcessModuleCollection modules = Process.TargetProcess.Modules;
            ProcessModule bootStrapModule = null;

            foreach (ProcessModule module in modules)
            {
                if (Path.GetFileName(module.FileName).ToLower() == "managedbootstrap.dll")
                {
                    bootStrapModule = module;
                }
            }

            IntPtr procOffset = GetFunctionOffset("ManagedBootstrap.dll", "ImplantDotNetAssembly");

            IntPtr procAddress;
            if(Process.Is64Bit)
                procAddress = new IntPtr(bootStrapModule.BaseAddress.ToInt64() + procOffset.ToInt64());
            else
                procAddress = new IntPtr(bootStrapModule.BaseAddress.ToInt32() + procOffset.ToInt32());

            string combinedArguments = fullPath + ";" + fullClassName + ";" + methodName + ";" + argument;

            Memory.InjectThread(procAddress, combinedArguments);

            IntPtr fnFreeLibrary = Win32Api.Kernel32.GetProcAddress(
                Win32Api.Kernel32.GetModuleHandle("kernel32"),
                "FreeLibrary");

            Win32Api.Kernel32.CreateRemoteThread(Process.TargetProcessHandle,
                IntPtr.Zero,
                0,
                fnFreeLibrary,
                bootStrapModule.BaseAddress,
                0,
                IntPtr.Zero);

            return true;
        }

        internal string GetManagedBootstrapPath()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.Combine(path, "ManagedBootstrap.dll");

            return path;

        }

        internal IntPtr GetFunctionOffset(string library, string procName)
        {
            IntPtr module = Win32Api.Kernel32.LoadLibrary(library);

            IntPtr procAddress = Win32Api.Kernel32.GetProcAddress(module, procName);

            IntPtr ret;

            if(Process.Is64Bit)
                ret = new IntPtr(procAddress.ToInt64() - module.ToInt64());
            else
                ret = new IntPtr(procAddress.ToInt32() - module.ToInt32());

            Win32Api.Kernel32.FreeLibrary(module);

            return ret;
        }

        protected void ReleaseProcess()
        {
            Process.Dispose();
        }
        

        #region IDisposable Support
        private bool IsDisposing { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposing)
            {
                if (disposing)
                {
                    Process.Dispose();
                    HotKeys.HotKeyFactory.Factory.Dispose();
                }

                Memory = null;

                IsDisposing = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}