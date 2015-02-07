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
    class Step3 : ITrainerItem
    {
        HotKey hotkey;
        TrainerBase trainer;
        IntPtr address;
        int[] pointerOffsets;

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

        public Keys HotKeys
        {
            get
            {
                if (hotkey == null)
                    return Keys.NumPad3;
                else
                    return hotkey.Keys;
            }
        }

        public bool IsActive { get; private set; }

        public void Activate()
        {
            if (IsActive)
                return;

            trainer.Memory.WriteMemory(address, 5000, pointerOffsets);

            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        public void Initialize(TrainerBase trainerBase, HotKey hotkey)
        {
            trainer = trainerBase;
            this.hotkey = hotkey;
            // Uncomment this to support handling hotkeys
            // this.hotkey.OnHotKeyPressed += OnHotKeyPressed;

            address = new IntPtr(0x00645310);
            pointerOffsets = new int[] { 0x484 };
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
                    // TODO: dispose managed state (managed objects).          
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
