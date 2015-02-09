using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hollow.Trainer.Framework;
using Hollow.Trainer.Framework.HotKeys;

namespace Sample_x86.TrainerItems
{
    class Step8 : ITrainerItem
    {
        HotKey hotkey;
        TrainerBase trainer;
        Timer timer;
        IntPtr address = new IntPtr(0x00645390);

        public Step8()
        {
            timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            // This is writing to a pointer to a pointer to a pointer to a pointer
            trainer.Memory.WriteMemory(address, 5000, 0x0C, 0x14, 0x0, 0x18);
        }

        public KeyModifier HotKeyModifers
        {
            get
            {
                if (hotkey == null)
                    return KeyModifier.None;
                else
                    return hotkey.Modifiers;
            }
        }

        public Keys HotKey
        {
            get
            {
                if (hotkey == null)
                    return Keys.NumPad8;
                else
                    return hotkey.Key;
            }
        }

        public bool IsActive { get; private set; }

        public void Activate()
        {
            if (IsActive)
                return;

            timer.Start();

            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            timer.Stop();

            IsActive = false;
        }

        public void Initialize(TrainerBase trainerBase, HotKey hotkey)
        {
            trainer = trainerBase;
            this.hotkey = hotkey;
            // Uncomment this to support handling hotkeys
            // this.hotkey.OnHotKeyPressed += OnHotKeyPressed;
        }

        public void OnHotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (!IsActive)
                Activate();
            else
                Deactivate();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Stop();
                    timer.Dispose();      
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources. 
        // ~Step7() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
