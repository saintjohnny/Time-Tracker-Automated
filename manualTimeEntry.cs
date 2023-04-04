using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeTracker
{
    public partial class manualTimeEntry : Form
    {
        private TimeTracker form1;
        public manualTimeEntry(TimeTracker form1)
        {
            InitializeComponent();
            this.form1 = form1;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

          
        }

        private void idleAwayLogTimeButton_Click(object sender, EventArgs e)
        {

            if (manualTimeEntryTextBox.Text == "")
            {
                MessageBox.Show("Please enter what the task was in the textbox before logging time.", "Time Tracker Automation");
            }
            else if (numericUpDownCustomTime.Text == "0")
            {
                MessageBox.Show("Please enter a time in minutes other than zero", "Time Tracker Automation");
            }
            else
            {
                 double customMinutesToDouble = double.Parse(numericUpDownCustomTime.Text);

                TimeSpan customTime = TimeSpan.FromMinutes(customMinutesToDouble);
                string customMinutesString = customTime.ToString(@"hh\:mm\:ss");

              

                Icon IEIconzz2 = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

                Bitmap stopIcon = new Bitmap(IEIconzz2.ToBitmap(), 17, 17);

               

                form1.dataGridView1.Rows.Add(stopIcon, "Manual time entry", manualTimeEntryTextBox.Text, customMinutesString);

                form1.swList.Add(new Stopwatch());
                form1.swList.Last().Start();
               
           

             //   form1.easierOnEyesIdleTime = "";
              //  form1.idleTimeReturnStringHandOff = "";

                this.Close();
            }
        }

        private void manualTimeEntry_Load(object sender, EventArgs e)
        {
            if (form1.radioButtonPopUpOnTopYes.Checked == true)
            {
                this.TopMost = true;
            }
            if (form1.radioButtonPopUpOnTopNo.Checked == true)
            {
                this.TopMost = false;
            }

           
        }

        private void logIdleTimeCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void numericUpDownCustomTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
