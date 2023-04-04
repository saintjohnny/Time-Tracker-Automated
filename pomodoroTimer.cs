using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeTracker
{
    public partial class pomodoroTimer : Form
    {

        public TimeTracker form1;
        public pomodoroTimer(TimeTracker form1)
      
        {


            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.StartPosition = FormStartPosition.CenterScreen;
         //   this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.form1 = form1;

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void pomodoroTimer_Load(object sender, EventArgs e)
        {
            if (form1.radioButtonPopUpOnTopYes.Checked == true)
            {
                this.TopMost = true;
            }
            if (form1.radioButtonPopUpOnTopNo.Checked == true)
            {
                this.TopMost = false;
            }

            //  manualTimeEntry f3 = new manualTimeEntry(this);

            // f3.ShowDialog(this);
            resetTimerButton.Text = "Reset " +form1.timeRecall + " timer";

            form1.Show();
            form1.WindowState = FormWindowState.Normal;
            form1.notifyIcon1.Visible = false;

           //not working yet this.StartPosition = FormStartPosition.CenterParent;

            label1.Text = "Time's up!  Your " + form1.timeRecall + " minute timer has finished.  Take a break! Make sure to stand up and stretch for 5 min or so :)  Would you like to set another timer now?";

            
           

        }

        private void resetTimerButton_Click(object sender, EventArgs e)
        {
           

            form1.PomodoroButton.PerformClick();
            this.Close();
        }

        private void setBreakTimerButton_Click(object sender, EventArgs e)
        {
            form1.PomodoroTextBox.Text = "5";
            form1.timerActivatedButtonPush = true;
            form1.PomodoroButton.PerformClick();
            this.Close();
        }

        private void closePomodoroButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
