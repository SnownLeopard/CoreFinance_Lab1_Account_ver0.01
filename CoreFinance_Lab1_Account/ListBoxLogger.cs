using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoreFinance_Lab1_Account
{
    internal class ListBoxLogger : ILogger
    {
        private ListBox listBox;

        public ListBoxLogger(ListBox listBoxTarget)
        {
            this.listBox = listBoxTarget;
        }

        public void Log(string message)
        {
            if (listBox != null)
            {
                listBox.Items.Add(message);
            }
        }
    }
}
