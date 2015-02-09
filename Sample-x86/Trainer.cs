using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hollow.Trainer.Framework;
using Hollow.Trainer.Framework.HotKeys;
namespace Sample_x86
{
    class Trainer : TrainerBase
    {
        List<ITrainerItem> trainerItems = new List<ITrainerItem>();

        public Trainer(string processName)
        {
            this.OpenProcess(processName);
        }

        public override void AddTrainerItem(ITrainerItem item)
        {
            trainerItems.Add(item);
        }

        public override void RegisterHotKeys()
        {
            foreach(var item in trainerItems)
            {
                // Uncomment the following two lines to use hotkeys.
                var hotkey = HotKeyFactory.Factory.RegisterHotKey(item.HotKey, item.HotKeyModifers);
                item.Initialize(this, hotkey);
            }
        }

        // This will be used to activate with button press as opposed to hotkey
        public void EmulateHotkey(int stepNum)
        {
            if (stepNum < 2 || stepNum > 9)
                throw new ArgumentException("Invalid argument value must be between 2 and 9", "stepNum");

            var item = trainerItems[stepNum - 2]; // We are subtracting 2 here since its values 2-9

            item.OnHotKeyPressed(this, new HotKeyEventArgs(item.HotKey, item.HotKeyModifers));
        }
    }
}
