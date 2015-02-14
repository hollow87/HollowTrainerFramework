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
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace Hollow.Trainer.Framework.HotKeys
{
    public class HotKeyFactory : IDisposable
    {
        public static HotKeyFactory Factory { get; private set; }


        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys key);

        static HotKeyFactory()
        {
            Factory = new HotKeyFactory();
        }

        private HotKeyFactory()
        {
            thread = new Thread(new ThreadStart(ThreadProc));
            thread.Start();
        }

        private Thread thread;
        private bool shutdown = false;
        private Dictionary<Keys, HotKey> hotKeyItems = new Dictionary<Keys, HotKey>();
        private object _lockObj = new object();

        public HotKey RegisterHotKey(Keys key, KeyModifier modfiers = KeyModifier.None)
        {
            HotKey item = new HotKey(key, modfiers);

            lock (_lockObj)
            {
                hotKeyItems.Add(key, item);
            }

            return item;
        }

        internal void RemoveHotKey(HotKey hotKey)
        {
            lock (_lockObj)
            {
                if (hotKeyItems.ContainsKey(hotKey.Key))
                {
                    hotKeyItems.Remove(hotKey.Key);
                }
            }
        }

        private void ThreadProc()
        {
            while (!shutdown)
            {
                lock(_lockObj)
                {
                    foreach (var item in hotKeyItems.Values)
                    {
                        if ((GetAsyncKeyState(item.Key) & 0x8000) == 0x8000)
                        {
                            item.IsKeyDown = true;
                        }

                        if(item.Modifiers != KeyModifier.None)
                        {
                            if((item.Modifiers & KeyModifier.Alt) == KeyModifier.Alt)
                            {
                                if ((GetAsyncKeyState(Keys.Menu) & 0x8000) == 0x8000)
                                    item.ModifersDown |= KeyModifier.Alt;
                                else
                                    item.ModifersDown -= (item.ModifersDown & KeyModifier.Alt);
                            }

                            if ((item.Modifiers & KeyModifier.Ctrl) == KeyModifier.Ctrl)
                            {
                                if ((GetAsyncKeyState(Keys.ControlKey) & 0x8000) == 0x8000)
                                    item.ModifersDown |= KeyModifier.Ctrl;
                                else
                                    item.ModifersDown -= (item.ModifersDown & KeyModifier.Ctrl);
                            }

                            if ((item.Modifiers & KeyModifier.Shift) == KeyModifier.Shift)
                            {
                                if ((GetAsyncKeyState(Keys.ShiftKey) & 0x8000) == 0x8000)
                                    item.ModifersDown |= KeyModifier.Shift;
                                else
                                    item.ModifersDown -= (item.ModifersDown & KeyModifier.Shift);
                            }

                            if ((item.Modifiers & KeyModifier.Win) == KeyModifier.Win)
                            {
                                if ((GetAsyncKeyState(Keys.LWin) & 0x8000) == 0x8000)
                                    item.ModifersDown |= KeyModifier.Win;
                                else
                                    item.ModifersDown -= (item.ModifersDown & KeyModifier.Win);

                                if ((item.ModifersDown & KeyModifier.Win) != KeyModifier.Win)
                                {
                                    if ((GetAsyncKeyState(Keys.RWin) & 0x8000) == 0x8000)
                                        item.ModifersDown |= KeyModifier.Win;
                                    else
                                        item.ModifersDown -= (item.ModifersDown & KeyModifier.Win);
                                }

                            }
                        }

                        if ((GetAsyncKeyState(item.Key) & 0x8000) != 0x8000 & item.IsKeyDown)
                        {
                            if (item.ModifersDown == item.Modifiers)
                            {
                                item.IsKeyDown = false;
                                item.OnHotKeyPressedHandler();
                            }
                        }

                    }
                }

                Thread.Sleep(15);
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
                    shutdown = true;
                    thread.Join();

                    hotKeyItems.Clear();
                }

                

                _lockObj = null;
                hotKeyItems = null;
                thread = null;

                IsDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
