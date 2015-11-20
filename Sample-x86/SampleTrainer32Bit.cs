using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Sample_x86
{
    public partial class SampleTrainer32bit : Form
    {
        Trainer trainer;
        public SampleTrainer32bit()
        {
            InitializeComponent();
        }
        private void cheatEngineSupport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }

        private void SampleTrainer32bit_Load(object sender, EventArgs e)
        {
            var link = new LinkLabel.Link();
            link.LinkData = "http://www.cheatengine.org/";
            cheatEngineSupport.Links.Add(link);
        }

        private void SampleTrainer32bit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (trainer != null)
                trainer.Dispose();
        }

        private void step1_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer != null)
                return;

            trainer = new Trainer("Tutorial-i386");  // Opens the process
            //trainer = new Trainer("Tutorial-x86_64");

            // Assoicate the cheats with the trainer
            trainer.AddTrainerItem(new TrainerItems.Step2());
            trainer.AddTrainerItem(new TrainerItems.Step3());
            trainer.AddTrainerItem(new TrainerItems.Step4());
            trainer.AddTrainerItem(new TrainerItems.Step5());
            trainer.AddTrainerItem(new TrainerItems.Step6());
            trainer.AddTrainerItem(new TrainerItems.Step7());
            trainer.AddTrainerItem(new TrainerItems.Step8());
            trainer.AddTrainerItem(new TrainerItems.Step9());

            trainer.RegisterHotKeys(); // Register all the hotkeys

            step1.Enabled = false;
        }

        private void step2_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;
            
            trainer.EmulateHotkey(2);
        }

        private void step3_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(3);
        }

        private void step4_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(4);
        }

        private void step5_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(5);
        }

        private void step6_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(6);
        }

        private void step7_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(7);
        }

        private void step8_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(8);
        }

        private void step9_CheckedChanged(object sender, EventArgs e)
        {
            if (!step1.Checked || trainer == null)
                return;

            trainer.EmulateHotkey(9);
        }
    }
}
