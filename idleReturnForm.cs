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
    public partial class idleReturnForm : Form
    {

        private TimeTracker form1;
        public idleReturnForm(TimeTracker form1)
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.form1 = form1;

            //public idleReturnForm()
            //{
            //    InitializeComponent();
            //}
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                //Stop your infinite loop
                // this.Close();

            }


            else
            {
                form1.idleOnePopupOnly = false;
            }
        }

        string recallSetting;
        private void idleReturnForm_Load(object sender, EventArgs e)
        {

            if (form1.radioButtonPopUpOnTopYes.Checked == true)
            {
                this.TopMost = true;
            }
            if (form1.radioButtonPopUpOnTopNo.Checked == true)
            {
                this.TopMost = false;
            }


            recallSetting = form1.numericUpDownHowLongIdleTillPopup.Text;
            idleAwayLabel.Text = "We've been idle for " + form1.easierOnEyesIdleTime + @".  If you'd like to save this time please enter the task in the textbox below and hit the ""Log Time"" button.";


        }

     

        private void button1_Click_1(object sender, EventArgs e)
        {

            if (idleAwayLogTimeTextBox.Text == "")
            {
                MessageBox.Show("Please enter what the task was in the textbox before logging time.", "Time Tracker Automation");
            }
            else
            {

                Icon IEIconzz2 = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

                Bitmap stopIcon = new Bitmap(IEIconzz2.ToBitmap(), 17, 17);

                // MessageBox.Show(form1.idleTimeReturnStringHandOff);

                form1.dataGridView1.Rows.Add(stopIcon, "Idle away time log", idleAwayLogTimeTextBox.Text, form1.idleTimeReturnStringHandOff);

                form1.swList.Add(new Stopwatch());
                form1.swList.Last().Start();
                //form1.swList.Last().Stop();
                form1.idleOnePopupOnly = false;

                form1.easierOnEyesIdleTime = "";
                form1.idleTimeReturnStringHandOff = "";

                this.Close();
            }
        }

        private void logIdleTimeCancelButton_Click(object sender, EventArgs e)
        {
            form1.easierOnEyesIdleTime = "";
            form1.idleTimeReturnStringHandOff = "";
            form1.idleOnePopupOnly = false;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox1.Checked == true)
            {
                form1.numericUpDownHowLongIdleTillPopup.Text = "0";
                form1.writeToOptionsFile();
            }
            else
            {
                form1.numericUpDownHowLongIdleTillPopup.Text = recallSetting;
                form1.writeToOptionsFile();
            }
        }
    }
}

