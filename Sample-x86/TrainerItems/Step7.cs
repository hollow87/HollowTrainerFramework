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
    class Step7 : ITrainerItem
    {
        HotKey hotkey;
        TrainerBase trainer;
        IntPtr address = new IntPtr(0x00426C40);
        byte[] orginalBytes = new byte[] { 0xFF, 0x8B, 0x78, 0x04, 0x00, 0x00 };
        byte[] newBytes = new byte[] { 0x83, 0x83, 0x78, 0x04, 0x00, 0x00, 0x02 };
        IntPtr codeCave;
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
                    return Keys.NumPad7;
                else
                    return hotkey.Keys;
            }
        }

        public bool IsActive { get; private set; }

        public void Activate()
        {
            if (IsActive)
                return;

            codeCave = trainer.Memory.InjectCode(address, newBytes, orginalBytes);

            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            trainer.Memory.RemoveCode(address, codeCave, orginalBytes);

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
        public bool disposedValue = false; // To detect redundant calls

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
