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
using System.Windows.Forms;

namespace Hollow.Trainer.Framework.HotKeys
{
    public class HotKey
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event EventHandler<HotKeyEventArgs> OnHotKeyPressed;

        public KeyModifier Modifiers { private set; get; }
        public Keys Keys { private set; get; }

        private IntPtr hWnd;
        private int id;

        public bool IsRegistered { get; private set; }

        internal HotKey(KeyModifier modifiers, Keys keys, IntPtr hWnd)
        {
            this.Modifiers = modifiers;
            this.Keys = keys;
            this.hWnd = hWnd;
            id = this.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (int)Modifiers ^ (int)Keys ^ hWnd.ToInt32();
        }

        internal void Register()
        {
            if (IsRegistered)
                throw new Exception("TODO: Error already registered");

            bool ret = RegisterHotKey(hWnd, id, (int)Modifiers, (int)Keys);

            if (!ret)
                throw new Exception("TODO: Error registering hotkey");

            IsRegistered = true;
        }

        public void Unregister()
        {
            if (!IsRegistered)
                throw new Exception("TODO: Error not registered");

            bool ret = UnregisterHotKey(hWnd, id);

            if (!ret)
                throw new Exception(string.Format("TODO: Error unregistering hotkey\r\nError:{0}", Marshal.GetLastWin32Error()));

            IsRegistered = false;
            HotKeyFactory.Factory.RemoveHotKey(this);
        }

        internal void OnHotKeyPressedHandler()
        {
            EventHandler<HotKeyEventArgs> handler = OnHotKeyPressed;

            if (handler != null)
            {
                handler(this, new HotKeyEventArgs(Modifiers, Keys));
            }
        }

    }
}
