using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace InjectTest
{
    class Program
    {

        static int EntryPoint(string argument)
        {
            System.Media.SystemSounds.Exclamation.Play();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("I am a managed application");
            sb.AppendLine();
            sb.AppendFormat("I am runnning inside of {0}", Process.GetCurrentProcess().ProcessName);
            sb.AppendLine();
            sb.AppendFormat("I was given the following argument: {0}", string.IsNullOrEmpty(argument) ? "" : argument);
            sb.AppendLine();
            sb.AppendLine("I will continue to run (doing nothing) after this method returns.");
            sb.AppendLine("Maybe you should make me do more.");

            MessageBox.Show(sb.ToString());

            sb.Clear();
            sb = null;

            return 0;
        }
        static void Main(string[] args)
        {
        }
    }
}
