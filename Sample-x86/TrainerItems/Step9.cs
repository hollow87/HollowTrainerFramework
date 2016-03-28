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
    class Step9 : ITrainerItem
    {
        HotKey hotkey;
        TrainerBase trainer;
        IntPtr address;
        byte[] orginalBytes = new byte[] { 0x89, 0x43, 0x04,                // mov [ebx+04], eax
                                           0xD9, 0xEE                       // fldz
                                         };

        byte[] newBytes = new byte[] { 0x83, 0x7B, 0x10, 0x02,              // cmp [ebx+10], 02
                                       0x0F, 0x84, 0x08, 0x00, 0x00, 0x00,  // je team2
                                       0x8B, 0x43, 0x04,                    // mov eax, [ebx+04]
                                       0xE9, 0x02, 0x00, 0x00, 0x00,        // jmp orgCode
                                       // team2:
                                       0x31, 0xC0,                          // xor eax, eax
                                       // orgCode:
                                       0x89, 0x43, 0x04,                    // mov [ebx+04], eax
                                       0xD9, 0xEE                           // fldz
                                     };
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

        public Keys HotKey
        {
            get
            {
                if (hotkey == null)
                    return Keys.NumPad9;
                else
                    return hotkey.Key;
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

            address = IntPtr.Add(trainer.Process.TargetProcess.MainModule.BaseAddress, 0x261D7);
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
