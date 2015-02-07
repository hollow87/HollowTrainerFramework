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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Hollow.Trainer.Framework.HotKeys
{
    public class HotKeyFactory : IDisposable
    {
        private class HotKeyWndProc : NativeWindow
        {
            public HotKeyWndProc()
            {
                this.CreateHandle(new CreateParams());
            }

            private const int WM_HOTKEY = 0x0312;
            private const int WM_CLOSE = 0x0010;
            private const int WM_DESTROY = 0x0002;
            private const int WM_QUIT = 0x0012;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    lock (Factory._lockObj)
                    {
                        if (Factory.hotKeyItems.ContainsKey(m.LParam))
                        {
                            HotKey hotkey = HotKeyFactory.Factory.hotKeyItems[m.LParam];
                            hotkey.OnHotKeyPressedHandler(
                               // (KeyModifier)((int)m.LParam & 0xFFFF),
                                //(Keys)(((int)m.LParam >> 16) & 0xFFFF)
                            );

                        }
                    }
                }
                base.WndProc(ref m);
            }
        }

        public static HotKeyFactory Factory { get; private set; }

        static HotKeyFactory()
        {
            Factory = new HotKeyFactory();
        }

        private HotKeyFactory()
        {
        }

        private Dictionary<IntPtr, HotKey> hotKeyItems = new Dictionary<IntPtr, HotKey>();
        private object _lockObj = new object();

        private HotKeyWndProc hotKeyWnd= new HotKeyWndProc();

        private IntPtr MakeLParamForHotKey(Keys keys, KeyModifier modifiers)
        {
            return (IntPtr)(((int)keys << 16) | ((int)modifiers & 0xFFFF));
        }

        public HotKey RegisterHotKey(KeyModifier modifiers, Keys keys)
        {
            HotKey item = new HotKey(modifiers, keys, hotKeyWnd.Handle);

            lock (_lockObj)
            {
                hotKeyItems.Add(MakeLParamForHotKey(keys, modifiers), item);
            }

            try
            {
                item.Register();
            }
            catch
            {
                throw;
            }

            return item;
        }

        internal void RemoveHotKey(HotKey hotKey)
        {
            if (hotKey.IsRegistered)
                throw new Exception("TODO: Hotkey has not been unregistered");

            lock(_lockObj)
            {
                IntPtr key = MakeLParamForHotKey(hotKey.Keys, hotKey.Modifiers);
                if (hotKeyItems.ContainsKey(key))
                {
                    hotKeyItems.Remove(key);
                }
            }
        }

        #region IDisposable Support
        public bool IsDisposed { get; private set; } // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                        
                }

                while (hotKeyItems.Count > 0)
                {
                    hotKeyItems.First().Value.Unregister();
                }

                hotKeyWnd.DestroyHandle();
                _lockObj = null;

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
         ~HotKeyFactory() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion
    }
}
