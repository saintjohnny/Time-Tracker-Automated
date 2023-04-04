using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.Windows.Forms;



namespace TimeTracker
{
    public partial class TimeTracker : Form
    {

        public bool IsCancelled { get; set; }

        public TimeTracker()
        {
            InitializeComponent();
            mouseIdleTimer = new Stopwatch();
            activeTime = new Stopwatch();

            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
           // this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);


            //to protect crashes, other half is under form load to allow resizing again
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewFilterView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

           // this.dataGridViewSaveBeforeUndo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

           backgroundWorker1.WorkerReportsProgress = true;
           backgroundWorker1.WorkerSupportsCancellation = true;
           backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
           backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            
        }






        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        //stores stopwatch window names
        public List<Stopwatch> swList = new List<Stopwatch>();

        //stores strings to reference to ignore or use later
        List<string> checkStrings = new List<string>();

        //stores strings to ignore form names
        List<string> ignoreStrings = new List<string>();

        List<string> tempFileNameStore = new List<string>();

        List<string> tempFileNameStoreToday = new List<string>();

        // The GetWindowThreadProcessId function retrieves the identifier of the thread
        // that created the specified window and, optionally, the identifier of the
        // process that created the window.



        /* First, we start a timer, and once each second, we check and see what window has focus
            * Then, we check and see if the window is on the datagrid.
            * If the window is not on the datagrid, we add it with a new time value; time needs to start at zero.
            * Then we add the timer value to the datagrid.
            * 
            * */

        StringBuilder Buff;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing && !CloseRequested)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        Boolean notifyMinimizedOnceOnly;
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                //Stop your infinite loop
                // this.Close();

            }


            else
            {
                if (FormWindowState.Minimized == this.WindowState)
                {
                    //  this.Close();
                    //  MessageBox.Show("Test");

                }

                else
                {
                    this.WindowState = FormWindowState.Minimized;



                    notifyIcon1.BalloonTipTitle = "Time Tracker Automation";
                    notifyIcon1.BalloonTipText = "Minimized to System Tray.  This popup will only happen once per session.";


                    if (FormWindowState.Minimized == this.WindowState)
                    {
                        notifyIcon1.Visible = true;
                        if (notifyMinimizedOnceOnly == false)
                        {
                            notifyMinimizedOnceOnly = true;
                            notifyIcon1.ShowBalloonTip(100);
                        }
                        this.Hide();

                    }
                    else if (FormWindowState.Normal == this.WindowState)
                    {
                        notifyIcon1.Visible = false;
                    }
                    e.Cancel = true;


                }
                //  e.Cancel = true;

            }
        }





        private void tally()
        {


            try
            {
                double seconds = 0;
                seconds = dataGridView1.Rows.Cast<DataGridViewRow>()
                    .AsEnumerable()
                    .Sum(x => TimeSpan.Parse((x.Cells[3].Value.ToString())).TotalSeconds);//2 is time column currently  //icon fix

                // Assign to textbox
                string timeYo = TimeSpan.FromSeconds(seconds).TotalHours + ":" + TimeSpan.FromSeconds(seconds).Minutes
                         + ":" + TimeSpan.FromSeconds(seconds).Seconds.ToString();






                // tally label
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                string timeYoConverted = time.ToString(@"hh\:mm\:ss");
                //  MessageBox.Show(timeYoConverted.ToString());

                timeYoConverted = string.Format("{0}hr {1}mn {2}sec",
                           (int)time.TotalHours,
                           time.Minutes,
                           time.Seconds);

                tallyLabel.Text = "Tracked Time: " + timeYoConverted.ToString();
            }
            catch (Exception)
            {

            }
            
            
                
               
                   // MessageBox.Show("Test");
                     


                    TimeSpan at = activeTime.Elapsed;

                    // Format and display the TimeSpan value.
                    elapsedTime = String.Format("{0}hr {1}mn {2}sec",//minutes/seconds for label
                       at.Hours, at.Minutes, at.Seconds,
                       at.Milliseconds / 10);

                    //string elapsedTimePreDecimalString = String.Format("{1} ",//minutes only for compare
                    //  at.Hours, at.Minutes, at.Seconds,
                    //  at.Milliseconds / 10);

                    //  MessageBox.Show(elapsedTimePreDecimalString);

                    // int elapsedTimeInteger = Int32.Parse(elapsedTimePreDecimalString);


                    activeTimeLabel.Text = "Active Time: " + elapsedTime;
                
            
           

        }
        
        Boolean stopAllWatches;
        Boolean defaultIconTopRightErrorFix;
        int index;
        string fullpath;
        string processIDString;
        string currentWindowName;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        // int duplicateCounter = 0;
        //  bool isFirstRun = true;
   


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                if (notifyIcon1.Visible == false)
                {
                    notifyIcon1.Visible = true;
                   // MessageBox.Show("Test");
                }

            }


            if (filterTextBoxIgnore.Text == "")
            {
                filterTextBoxIgnore.Text = "No filtering Selected";
            }

            tally(); //keep tally of non idle time
            if (stopAllWatches == true)
            {

            }

            else
            {




                // Returns the name of the process owning the foreground window.

                IntPtr hhnD = GetForegroundWindow();
                //hhnD = a number value
                // The foreground window can be NULL in certain circumstances, 
                // such as when a window is losing activation.
                if (hhnD == null)

                { }

                uint pid;
                GetWindowThreadProcessId(hhnD, out pid);

              //  MessageBox.Show("pid is "+ pid.ToString());  numerical value
               // MessageBox.Show("hhnD is "+ hhnD.ToString()); numerical value

                foreach (System.Diagnostics.Process processName in System.Diagnostics.Process.GetProcesses())
                {

                    processIDString = "";
                    currentWindowName = "";


                    processIDString = processName.ProcessName + ".exe";

                    if (processName.Id == pid)
                    {


                       // processIDString = processName.ProcessName + ".exe";
                        // MessageBox.Show(processName.ProcessName);


                        //grag icon
                        try
                        {
                            var process = processName; // Or whatever method you are using
                            fullpath = process.MainModule.FileName;

                         //   MessageBox.Show("process is " +process.ToString());  sgiws form text in parenthesis
                          //  MessageBox.Show("full path is " + fullpath.ToString());  path to exe
                        }
                        catch (Exception)
                        {

                            //if process is admin ran then default to app executable icon
                            fullpath = Application.ExecutablePath;

                           // MessageBox.Show(fullpath);

                        }
                        // MessageBox.Show(fullPath.ToString());

                        dataGridView1.ClearSelection();
                        dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                        dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                        dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                        dataGridView1.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;

                        //get processID start


                        const int nChars = 256;
                        Buff = new StringBuilder(nChars);
                        IntPtr handle = GetForegroundWindow();
                        // MessageBox.Show(handle.ToString());//number value

                        if (GetWindowText(handle, Buff, nChars) > -1)
                        {



                             currentWindowName = Buff.ToString();
                          //  MessageBox.Show(currentWindowName);

                            if (currentWindowName == "")
                            {
                              //  currentWindowName = "No Form ID " + hhnD.ToString();
                            }

                            // return Buff.ToString();

                            bool windowIsInGrid = false;
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {

                                //this code doesn't seem to change anything but I'm leaving it for now to prevent potential errors
                                if (row.Index == dataGridView1.Rows.Count - 1 && Convert.ToString(row.Cells[0].Value) == "")
                                {
                                    break;
                                }
                                DataGridViewCell iconCell = row.Cells[0];//icon fix all
                                DataGridViewCell nameCellProcess = row.Cells[1];//namecell to reference process ID cell
                                DataGridViewCell nameCell = row.Cells[2];//namecell to reference current cell
                                DataGridViewCell timeCell = row.Cells[3];//namecell to reference current cell

                                index = row.Index;//row index
                                string nameOfWindowFromCell = Convert.ToString(nameCell.Value);//get window name from cell value
                                string nameOfProcessFromCell = Convert.ToString(nameCellProcess.Value);//get window name from cell value
                                Stopwatch stopWatchFromRow = swList[index];//create stopwatch/list
                                if (((nameOfWindowFromCell == currentWindowName)) || (nameOfWindowFromCell == stopWatchTextBox.Text))
                                {

                                    nameCell.Style.BackColor = colorDialog1.Color;
                                    nameCellProcess.Style.BackColor = colorDialog1.Color;
                                    timeCell.Style.BackColor = colorDialog1.Color;
                                    //iconCell.Style.BackColor = colorDialog1.Color;

                                    // MessageBox.Show(nameCell..ToString());

                                    this.dataGridView1.CurrentCell = this.dataGridView1[0, index];

                                    // If this row contains the name of the current window, make sure the stopwatch is running.
                                    windowIsInGrid = true;
                                    if (!stopWatchFromRow.IsRunning)
                                    {
                                        stopWatchFromRow.Start();
                                    }
                                }
                                else
                                {
                                    nameCell.Style.BackColor = Color.White;
                                    nameCellProcess.Style.BackColor = Color.White;
                                    timeCell.Style.BackColor = Color.White;
                                    //  iconCell.Style.BackColor = Color.White;
                                    // Otherwise, make sure the stopwatch is stopped.
                                    if (stopWatchFromRow.IsRunning)
                                    {
                                        stopWatchFromRow.Stop();
                                    }
                                }

                                if (inUseNotIdle == false)
                                {
                                    if (stopWatchFromRow.IsRunning)
                                    {
                                        stopWatchFromRow.Stop();
                                    }

                                }
                                else if ((inUseNotIdle == true) && (nameOfWindowFromCell == currentWindowName))
                                {
                                    if (!stopWatchFromRow.IsRunning)
                                    {
                                        stopWatchFromRow.Start();
                                    }
                                }


                                string time = stopWatchFromRow.Elapsed.ToString();

                            }
                           

                            bool b = ((checkStrings.Any(currentWindowName.ToUpper().Contains)) || (checkStrings.Any(processIDString.ToUpper().Contains)));
                            bool ignoreb = ((ignoreStrings.Any(currentWindowName.ToUpper().Contains)) || (ignoreStrings.Any(processIDString.ToUpper().Contains)));//check /store strings for ignore group

                            if ((!windowIsInGrid) && (b == true) && (ignoreb == false))   //if boolean window in grid is false, meaning it needs to be added
                            {

                                if (currentWindowName == "")
                                {
                                   // currentWindowName = "No Window ID " + hhnD.ToString();
                                }

                                Process[] processes = Process.GetProcesses();

                                foreach (Process process in processes)
                                {
                                    if (!string.IsNullOrEmpty(process.MainWindowTitle))
                                    {
                                        //MessageBox.Show(currentWindowName);
                                        if (process.ProcessName.Contains(processName.ProcessName) && (process.MainWindowTitle.Contains(currentWindowName)) && (currentWindowName != ""))
                                        {
                                            //MessageBox.Show("Test");
                                            dataGridView1.Rows.Add(Icon, processIDString, currentWindowName, "00:00.00");
                                            swList.Add(new Stopwatch());
                                            swList.Last().Start();


                                            try
                                            {
                                                DataGridViewImageCell cell = (DataGridViewImageCell)dataGridView1.Rows[index + 1].Cells[0];//icon fix

                                                Icon IEIcon = Icon.ExtractAssociatedIcon(fullpath);


                                                cell.Value = new Bitmap(IEIcon.ToBitmap(), 17, 17);

                                            }
                                            catch (Exception ex)
                                            {
                                                //   MessageBox.Show(ex.ToString());
                                                if (defaultIconTopRightErrorFix == false)
                                                {
                                                    DataGridViewImageCell cellz = (DataGridViewImageCell)dataGridView1.Rows[0].Cells[0];//icon fix

                                                    Icon IEIconz = Icon.ExtractAssociatedIcon(Application.ExecutablePath);


                                                    cellz.Value = new Bitmap(IEIconz.ToBitmap(), 17, 17);
                                                    defaultIconTopRightErrorFix = true;
                                                }
                                                processIDString = "";
                                                currentWindowName = "";
                                                //MessageBox.Show(ex.ToString());
                                            }
                                        }
                                    }
                                }
                                      

                                processIDString = "";
                                currentWindowName = "";


                            }


                        }


                    }

                }



            }
        }

        //idle checker


        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static long GetTickCount()
        {
            return Environment.TickCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();

        static int GetLastInputTime()
        {
            int idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = (int)lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }

        private static uint idle = 0;


        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    base.WndProc(ref m);
                    if ((int)m.Result == HTCLIENT)
                        m.Result = (IntPtr)HTCAPTION;
                    return;
                    //break;
            }
            base.WndProc(ref m);
        }

        void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.ReadOnly == true)
                return;

            // .. whatever code you have in your handler...
        }


        Stopwatch activeTime;

        private void Form1_Load(object sender, EventArgs e)
        {
            labelUpdating.Text = "";

            labelMouseTime.Text = "";


            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFilterView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
           
            
            
            // this.dataGridViewSaveBeforeUndo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;


            //   dataGridView1.Rows.Add("Time Tracker Automation", "Time Tracker Automation", "00:00.00");


            panelFront.BringToFront();
            panelOptionsLayer.SendToBack();
            panelOptionsLayer.Enabled = false;
            panelOptionsLayer.Visible = false;


            dataGridView1.ReadOnly = true;

            labelFilename.Text = "";







            createAppDataFolder();
            // writeToConfigFile();
            readFromConfigFile();
            readFromOptionsFile();
            button2.BackColor = colorDialog1.Color;
            versionLabel.Visible = false;






            Point pos = this.PointToClient(Cursor.Position);




            linkLabel7.Text = "";
            checkIdle.Start();

            logFileCustomInterval();//SET LOG TIMER SAVE
            dateAndLogTimer.Start();


            objInteruptions.Start();

            timer1.Start();//set milliseconds option

            individualTimer.Start();

            tenthsTimer.Start();


            try
            {
                // (Begin Regedit) --> Startup objects
                using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (rkApp.GetValue("Time Tracker") == null)
                    {


                        radioButtonBootWindowsNo.Checked = true;
                    }
                    else
                    {
                        try
                        {
                            radioButtonBootWindowsYes.Checked = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error reading from registry." + ex);
                        }

                    }
                }
            }
            catch (Exception)
            {

            }

            //didn't fix the size issue
            //Size GetDefaultSize(Control ctrl)
            //{
            //    PropertyInfo pi = ctrl.GetType().GetProperty("DefaultSize", BindingFlags.NonPublic | BindingFlags.Instance);
            //    return (Size)pi.GetValue(ctrl, null);
            //}

            //this.Size = GetDefaultSize(this);



            activeTime.Start();


        }






        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }


        Stopwatch objInteruptions = new Stopwatch();

        Stopwatch individualTimer = new Stopwatch();

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tenthsTimer_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Index == dataGridView1.Rows.Count - 1 && Convert.ToString(row.Cells[0].Value) == "")
                {
                    break;
                }
                //where the problem lays


                int index = row.Index;
                DataGridViewCell timeCell = row.Cells[3];//icon fix
                DataGridViewCell idleCheckRow2 = row.Cells[1];//icon fix
                TimeSpan objTimeSpan = swList[index].Elapsed;
                string time = String.Format(CultureInfo.CurrentCulture, "{0:D2}:{1:D2}:{2:D2}", objTimeSpan.Hours, objTimeSpan.Minutes, objTimeSpan.Seconds, objTimeSpan.Milliseconds);
                if ((idleCheckRow2.Value.ToString().Contains("Idle away time log") == false) && (idleCheckRow2.Value.ToString().Contains("Manual time entry") == false))
                {
                    timeCell.Value = time;
                }
            }
        }
        public string idleTimeReturnStringHandOff;
        int idleTimerMinutesInt;
        public Boolean machineIsIdleForIdleResume;
        DateTime start;
        Boolean inUseNotIdle = true;
        public Boolean idleOnePopupOnly;
        public string easierOnEyesIdleTime;

        private void notIdle()
        {

            //   DateTime? start = null;

            inUseNotIdle = true;
            idleLabel.Text = "Not Idle";
            idleLabel.ForeColor = Color.Green;
            idleFrontLabel.Visible = false;
            //  MessageBox.Show("Test");
            activeTime.Start();

            int howLong = int.Parse(numericUpDownHowLongIdleTillPopup.Text);

            if (start.AddMinutes(howLong) < DateTime.UtcNow)
            {


                if ((machineIsIdleForIdleResume == true) && (numericUpDownHowLongIdleTillPopup.Text != "0"))
                {

                    //  MessageBox.Show("Test222");

                    //get difference between when idle started and time now in minutes
                    DateTime theTime = DateTime.UtcNow;

                    TimeSpan interval = theTime.Subtract(start);
                    double idleSecondsReturn = interval.TotalSeconds;

                    TimeSpan time = TimeSpan.FromSeconds(idleSecondsReturn);
                    idleTimeReturnStringHandOff = time.ToString(@"hh\:mm\:ss");

                    easierOnEyesIdleTime = idleTimeReturnStringHandOff;

                    easierOnEyesIdleTime = string.Format("{0}hr {1}mn {2}sec",
                               (int)time.TotalHours,
                               time.Minutes,
                               time.Seconds);
                    machineIsIdleForIdleResume = false;

                    //  MessageBox.Show(idleTimeReturnString.ToString());
                    if (idleOnePopupOnly == false)
                    {
                        idleReturnForm f2 = new idleReturnForm(this);
                        f2.Show();
                        idleOnePopupOnly = true;

                    }

                }

            }

            else//machine is not idle and also not ready for resume popup
            {
                machineIsIdleForIdleResume = false;
            }
        }


        string elapsedTime;
        Stopwatch mouseIdleTimer;
        string xStore = "placeholder";
        int posX;

        private void checkIdle_Tick(object sender, EventArgs e)
        {


            if (stopAllWatches == true)
            {

            }

            else
            {

                try
                {

                    if (radioSystemIdle.Checked == true)
                    {
                        //if machine is idle
                        //  int count = Convert.ToInt32(numericUpDownIdleMinutes.Value);
                        idle = GetIdleTime();
                        if ((idle > (numericUpDownIdleMinutes.Value * 60000)))//convert milliseconds to minutes 60000
                        {
                            idleLabel.ForeColor = Color.Red;
                            idleLabel.Text = "Idle";
                            inUseNotIdle = false;
                            idleFrontLabel.Visible = true;
                            activeTime.Stop();

                            if (machineIsIdleForIdleResume == false)
                            {

                                machineIsIdleForIdleResume = true;
                                idleTimerMinutesInt = int.Parse(numericUpDownIdleMinutes.Text);
                                start = DateTime.UtcNow.AddMinutes(-idleTimerMinutesInt);//get time now but include the time it has been idle so far

                            }

                        }



                        //if machine is NOT idle
                        else
                        {
                            notIdle();
                        }
                    }

                    if (radioMouseIdle.Checked == true)
                    {//if machine is idle

                        try

                        {
                            xStore = posX.ToString();
                         //   this.Cursor = new Cursor(Cursor.Current.Handle); causes lag, not needed anyways
                            posX = Cursor.Position.X;
                            //int posY = Cursor.Position.Y;
                        }
                        catch (Exception)
                        {

                        }

                        if (xStore != posX.ToString())
                        {
                            mouseIdleTimer.Reset();
                            mouseIdleTimer.Start();
                        }

                        TimeSpan ts = mouseIdleTimer.Elapsed;

                        // Format and display the TimeSpan value.
                        elapsedTime = String.Format("{1:00}:{2:00}:{3:00}",//minutes/seconds for label
                           ts.Hours, ts.Minutes, ts.Seconds,
                           ts.Milliseconds / 10);

                       string elapsedTimePreDecimalString = String.Format("{1}",//minutes only for compare
                         ts.Hours, ts.Minutes, ts.Seconds,
                         ts.Milliseconds / 10);

                      //  MessageBox.Show(elapsedTimePreDecimalString);

                        int elapsedTimeInteger = Int32.Parse(elapsedTimePreDecimalString);
                        
                        labelMouseTime.Text = "Mouse Timer sample "+elapsedTime;
                        //MessageBox.Show(elapsedTimeInteger.ToString());
                        // idle = GetIdleTime();

                       // MessageBox.Show(elapsedTimeInteger.ToString() + "  " + (numericUpDownIdleMinutes.Value).ToString());
                        if ((elapsedTimeInteger > (numericUpDownIdleMinutes.Value -1)))//
                        {
                            idleLabel.ForeColor = Color.Red;
                            idleLabel.Text = "Idle";
                            inUseNotIdle = false;
                            idleFrontLabel.Visible = true;
                            activeTime.Stop();

                            if (machineIsIdleForIdleResume == false)
                            {

                                machineIsIdleForIdleResume = true;
                                idleTimerMinutesInt = int.Parse(numericUpDownIdleMinutes.Text);
                                start = DateTime.UtcNow.AddMinutes(-idleTimerMinutesInt);//get time now but include the time it has been idle so far

                            }

                        }



                        //if machine is NOT idle
                        else
                        {
                            notIdle();
                        }
                    }

                }

                catch (Exception ex)
                {
                  //  MessageBox.Show(ex.ToString());
                }
            }

            if (filterTextBoxDontIgnore.Text.Contains(",,"))
            {
                filterTextBoxDontIgnore.Text = filterTextBoxDontIgnore.Text.Replace(",,", ",");
                filterTextBoxDontIgnore.Focus();
                filterTextBoxDontIgnore.Select(filterTextBoxDontIgnore.Text.Length, 0);
            }
            if (filterTextBoxIgnore.Text.Contains(",,"))
            {
                filterTextBoxIgnore.Text = filterTextBoxIgnore.Text.Replace(",,", ",");
                filterTextBoxIgnore.Focus();
                filterTextBoxIgnore.Select(filterTextBoxIgnore.Text.Length, 0);
            }

            //begin the filtering code because this timer updates less rapidly
            //some regex to clean up if users put spaces after commas
            //also some regex to prevent cursor from jumping after regex edit
            Regex rgx = new Regex(@",\s+");
            if (rgx.IsMatch(filterTextBoxDontIgnore.Text))
            {
                // true

                string postRegex = Regex.Replace(filterTextBoxDontIgnore.Text, @",\s+", ",");
                filterTextBoxDontIgnore.Text = postRegex;

                filterTextBoxDontIgnore.Focus();
                filterTextBoxDontIgnore.Select(filterTextBoxDontIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }
            Regex rgx2 = new Regex(@"\s+,");
            if (rgx2.IsMatch(filterTextBoxDontIgnore.Text))
            {
                // true
                string postRegex2 = Regex.Replace(filterTextBoxDontIgnore.Text, @"\s+,", ",");
                filterTextBoxDontIgnore.Text = postRegex2;


                filterTextBoxDontIgnore.Focus();
                filterTextBoxDontIgnore.Select(filterTextBoxDontIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }

            Regex rgx3 = new Regex(@",\s+");


            if (rgx3.IsMatch(filterTextBoxIgnore.Text))
            {
                // true

                string ignorePostRegex = Regex.Replace(filterTextBoxIgnore.Text, @",\s+", ",");
                filterTextBoxIgnore.Text = ignorePostRegex;
                filterTextBoxIgnore.Focus();
                filterTextBoxIgnore.Select(filterTextBoxIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }
            Regex rgx4 = new Regex(@"\s+,");
            if (rgx4.IsMatch(filterTextBoxIgnore.Text))
            {
                // true

                string ignorePostRegex2 = Regex.Replace(filterTextBoxIgnore.Text, @"\s+,", ",");
                filterTextBoxIgnore.Text = ignorePostRegex2;
                filterTextBoxIgnore.Focus();
                filterTextBoxIgnore.Select(filterTextBoxIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }

            //replace first comma if appears
            Regex rgx5 = new Regex(@"^,");
            if (rgx5.IsMatch(filterTextBoxDontIgnore.Text))
            {
                // true

                string postRegex5 = Regex.Replace(filterTextBoxDontIgnore.Text, @"^,", "");
                filterTextBoxDontIgnore.Text = postRegex5;

                filterTextBoxDontIgnore.Focus();
                filterTextBoxDontIgnore.Select(filterTextBoxDontIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }

            Regex rgx6 = new Regex(@"^,");
            if (rgx6.IsMatch(filterTextBoxIgnore.Text))
            {
                // true

                string postRegex6 = Regex.Replace(filterTextBoxIgnore.Text, @"^,", "");
                filterTextBoxIgnore.Text = postRegex6;

                filterTextBoxIgnore.Focus();
                filterTextBoxIgnore.Select(filterTextBoxIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }




            //tidy up strings
            ignoreStrings.Clear();
            checkStrings.Clear();
            //checkStrings.Add("ca5");
            //checkStrings.Add("test");
            //checkStrings.Add("test2");
            string tags = filterTextBoxDontIgnore.Text;
            List<string> TagIds = tags.Split(',').ToList();
            //  List<string> TagIdsNoDupes = TagIds.Distinct().ToList();  //doesn't work

            foreach (string StringTagList in TagIds)
            {

                checkStrings.Add(StringTagList.ToUpper());



            }



            string ignoreTags = filterTextBoxIgnore.Text;
            List<string> ignoreTagIds = ignoreTags.Split(',').ToList();
            foreach (string ignoreStringTagList in ignoreTagIds)
            {

                ignoreStrings.Add(ignoreStringTagList.ToUpper());

                //ignoreStrings.Add(ignoreStringTagList.ToUpper());
            }



            foreach (var ignoredicGrab in ignoreStrings)
            {
                //MessageBox.Show(dicGrab.ToString());

                // MessageBox.Show(ignoredicGrab.ToString());

            }


            foreach (var dicGrab in checkStrings)
            {
                //MessageBox.Show(dicGrab.ToString());

                // MessageBox.Show(dicGrab.ToString());

            }





        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {


            string url = "";

            string business = "saintjohnny@gmail.com";  // your paypal email
            string description = "Saintjohnny's Software Tools";            // '%20' represents a space. remember HTML!
            string country = "US";                  // AU, US, etc.
            string currency = "USD";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.saintjohnny.com");
        }



        private void dataGridView1_DoubleClick_1(object sender, EventArgs e)
        {

            if (dataGridView1.ReadOnly == true)
                return;

            // .. whatever code you have in your handler...
        }

        private void regexRemoveEndswithComma()
        {
            //replace first comma if appears
            Regex rgx51 = new Regex(@",$");
            if (rgx51.IsMatch(filterTextBoxDontIgnore.Text))
            {
                // true

                string postRegex51 = Regex.Replace(filterTextBoxDontIgnore.Text, @",$", "");
                filterTextBoxDontIgnore.Text = postRegex51;

                filterTextBoxDontIgnore.Focus();
                filterTextBoxDontIgnore.Select(filterTextBoxDontIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }

            Regex rgx61 = new Regex(@",$");
            if (rgx61.IsMatch(filterTextBoxIgnore.Text))
            {
                // true

                string postRegex61 = Regex.Replace(filterTextBoxIgnore.Text, @",$", "");
                filterTextBoxIgnore.Text = postRegex61;

                filterTextBoxIgnore.Focus();
                filterTextBoxIgnore.Select(filterTextBoxIgnore.Text.Length, 0);
            }
            else
            {
                // false;
            }




        }
        int logFileCustomIntervalActualSeconds;
        private void logFileCustomInterval()//Set how often in seconds for log file save intervals
        {
            logFileCustomIntervalActualSeconds = int.Parse(numericUpDownLogFileIntervals.Text);
            //var timeSpanInMill = TimeSpan.FromSeconds(logFileCustomIntervalActualSeconds);
            dateAndLogTimer.Interval = logFileCustomIntervalActualSeconds * 1000;//covnert milliseconds to seconds
        }

        int formGrabPollRateInt;
        private void formGrabPollRate()
        {
            formGrabPollRateInt = int.Parse(formGrabPollRateNumericUpDown.Text);
            timer1.Interval = formGrabPollRateInt;

        }

       
        string dateCreated;
      
        string todaysTimeOnStartup = DateTime.Now.ToString("MM/dd/yyyy");///this needs to stay here so it doesn't update unless tomorrow begins which is captured below.
        Boolean firstPass = true; //this keeps track if program is started for first time so that log files can be created or overwritten correctly
        private void dateAndLogTimer_Tick_1(object sender, EventArgs e)
        {
           //separate timer so that midnight check is still utilized if other timers are puased when idle
            
            if (backgroundWorker1.IsBusy != true)
            {

                backgroundWorker1.RunWorkerAsync();
                Application.DoEvents();

            }

        

        }





        private void textBox2_TextChanged(object sender, EventArgs e)//allows user to keep typing before text is formatted from above
        {
            dateAndLogTimer.Stop();
            dateAndLogTimer.Start();

            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"This is for filtering out forms you don't want to track, such as your browsing data, or non work related things.  To filter out chrome browsing and firefox you would enter ""chrome,firefox"" without the quotes into the ""Don't track"" textbox separated by commas as shown.  Alternately, if you wanted to only track instance of adobe photoshop you could add ""photoshop"" to the ""Only Track"" textbox.  The filters are not case sensitive, but if you are not able to filter something, take a look at the text on the top of the form and see if it has distinguishable text you can filter.  If you filtered ""adobe"" into the track filter, you would end up with timers for adobe acrobat and adobe photoshop and nothing else that didn't contain ""adobe"".  The window text can also be seen highlighted in the datagrid table.  You can be as specific of vague as you like, and you can add as many as you like to either textbox filter.  This program only tracks your data into the csv file and does not communicate through the web or transmit/share your data with any parties.  Guaranteed.", "Text Filtering");
        }
        bool resizeBoolean = false;
        private void resizeFiltering()
        {//resize filtering visibility form
            if (resizeBoolean == false)
            {
                groupBox2.BringToFront();
                groupBox2.Width = this.Width;
                groupBox2.Height = this.Height;
                filterTextBoxDontIgnore.Width = this.Width - 50;
                filterTextBoxIgnore.Width = this.Width - 50;

                pictureBoxPlus.SendToBack();
                resizeBoolean = true;

                linkLabel7.Hide();
            }
            else
            {
                groupBox2.SendToBack();
                groupBox2.Width = 190;
                groupBox2.Height = 205;
                filterTextBoxDontIgnore.Width = 168;
                filterTextBoxIgnore.Width = 169;

                pictureBoxNegative.SendToBack();
                resizeBoolean = false;

                linkLabel7.Show();
            }
        }

        bool resizeOptionsBoolean = false;
        private void resizeOptionsFiltering()
        {//resize filtering visibility form
            panelOptionsLayer.SendToBack();
            panelOptionsLayer.Enabled = true;
            panelOptionsLayer.Visible = false;

            //  panelOptionsLayer.Width = this.Width;
            //  panelOptionsLayer.Height = this.Height; 


            if (resizeOptionsBoolean == false)
            {

                panelOptionsLayer.Show();
                panelOptionsLayer.BringToFront();
                // panelOptionsLayer.Width = this.Width;
                //  panelOptionsLayer.Height = this.Height;



                resizeOptionsBoolean = true;
            }
            else
            {
                panelOptionsLayer.SendToBack();
                panelOptionsLayer.Hide();


                resizeOptionsBoolean = false;
                panelOptionsLayer.Width = this.Width;
                panelOptionsLayer.Height = this.Height;
            }
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {
            resizeFiltering();



        }

        private void pictureBoxNegative_Click(object sender, EventArgs e)
        {
            resizeFiltering();
        }

        private void TimeTracker_Resize(object sender, EventArgs e)
        {
            if (resizeBoolean == true)
            {
                groupBox2.Width = this.Width;
                groupBox2.Height = this.Height;
            }

            if (resizeOptionsBoolean == true)
            {
                panelOptionsLayer.BringToFront();
                panelOptionsLayer.Width = this.Width;
                panelOptionsLayer.Height = this.Height;
            }



            //if (FormWindowState.Minimized == this.WindowState)
            //{
            //    notifyIcon1.Visible = true;
            //    notifyIcon1.ShowBalloonTip(500);
            //    this.Hide();
            //}
            //else if (FormWindowState.Normal == this.WindowState)
            //{
            //    notifyIcon1.Visible = false;
            //}

        }



        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                versionLabel.BringToFront();
                versionLabel.Visible = true;


                try
                {
                    Application.DoEvents();

                    WebRequest request = WebRequest.Create("http://www.saintjohnny.com/timetracker/files/versionCheck.html");
                    WebResponse response = request.GetResponse();
                    System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream());
                    var versionCheck = reader.ReadToEnd();

                    System.Version onlineVersion = new System.Version(versionCheck);
                    System.Version thisVersion = new System.Version(labelVersion.Text);




                    if (thisVersion < onlineVersion)
                    {
                        MessageBox.Show("Version " + onlineVersion + " available at www.saintjohnny.com", "New Version Available!");
                        versionLabel.Visible = false;
                    }


                    else if (thisVersion >= onlineVersion)
                    {
                        MessageBox.Show("You are running the current version! :)", "Nice job");
                        versionLabel.Visible = false;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Website not available " + ex.ToString());
                    versionLabel.Visible = false;
                }

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {

                //application data folder code
                createAppDataFolder();

                // MessageBox.Show(appDataTimeTrackerFolder.ToString());
                if ((File.Exists(appDataTimeTrackerFolder + "\\" + "Time Tracker Automation.cfg")) == true)
                {
                    string message = "Are you sure you want to overwrite your previous settings?";
                    string caption = "Save Settings?";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;

                    // Displays the MessageBox.mbox     

                    result = MessageBox.Show(message, caption, buttons);





                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        writeToConfigFile();
                    }

                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        //do nothing
                    }
                }

                else
                {
                    //if file does not exist just create it
                    writeToConfigFile();

                }










            }
        }
        string appDataDirectory;
        string appDataTimeTrackerFolder;
        private void createAppDataFolder()
        {

            try
            {
                appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appDataTimeTrackerFolder = Path.Combine(appDataDirectory, "Time Tracker Automation");


                if (!Directory.Exists(appDataTimeTrackerFolder))
                    Directory.CreateDirectory(appDataTimeTrackerFolder);
            }
            catch
            { }
        }

        string htmlColor;
        public void writeToOptionsFile()
        {

            try
            {
                using (StreamWriter sw1 = File.CreateText(appDataTimeTrackerFolder + "\\" + "Time Tracker Automation Options.cfg"))
                {


                    // System.Drawing.Color col = System.Drawing.ColorTranslator.FromHtml("#FFCC66");

                    htmlColor = ColorTranslator.ToHtml(colorDialog1.Color);

                    //Color color = colorDialog1.Color;
                    //string colorName = color.Name; // this gives you the ability to switch back to Color through Color.FromName()
                    //sameColor = Color.FromName(colorName);


                    sw1.WriteLine("colorDialog1 " + htmlColor);

                    if (radioButtonPomodoroNo.Checked == true)
                    {
                        sw1.WriteLine("radioButtonPomodoroNo " + "yes");
                    }
                    if (radioButtonPomodoroYes.Checked == true)
                    {
                        sw1.WriteLine("radioButtonPomodoroYes " + "yes");
                    }

                    sw1.WriteLine("numericUpDownHowLongIdleTillPopup " + numericUpDownHowLongIdleTillPopup.Text);

                    if (radioButtonPopUpOnTopNo.Checked == true)
                    {
                        sw1.WriteLine("radioButtonPopUpOnTopNo " + "yes");
                    }
                    if (radioButtonPopUpOnTopYes.Checked == true)
                    {
                        sw1.WriteLine("radioButtonPopUpOnTopYes " + "yes");
                    }
                    if (radioPngOpenYes.Checked == true)
                    {
                        sw1.WriteLine("radioPngOpenYes " + "yes");
                    }
                    if (radioPngOpenNo.Checked == true)
                    {
                        sw1.WriteLine("radioPngOpenNo " + "yes");
                    }
                    if (radioMouseIdle.Checked == true)
                    {
                        sw1.WriteLine("radioMouseIdle " + "yes");
                    }
                    if (radioSystemIdle.Checked == true)
                    {
                        sw1.WriteLine("radioSystemIdle " + "yes");
                    }



                    sw1.WriteLine("numericUpDownLogFileIntervals " + numericUpDownLogFileIntervals.Text);

                    
                    sw1.WriteLine("numericUpDownIdleMinutes " + numericUpDownIdleMinutes.Text);

                    sw1.WriteLine("formGrabPollRateNumericUpDown " + formGrabPollRateNumericUpDown.Text);



                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
        private void readFromOptionsFile()
        {



            try
            {

                int counter1 = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader file =
                   new System.IO.StreamReader(appDataTimeTrackerFolder + "\\" + "Time Tracker Automation Options.cfg");
                while ((line = file.ReadLine()) != null)
                {

                    if (line.Contains("radioButtonPomodoroNo"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioButtonPomodoroNo.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    if (line.Contains("radioButtonPomodoroYes"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioButtonPomodoroYes.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    if (line.Contains("radioButtonPopUpOnTopYes"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioButtonPopUpOnTopYes.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    if (line.Contains("radioButtonPopUpOnTopNo"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioButtonPopUpOnTopNo.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    
                            if (line.Contains("radioPngOpenYes"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioPngOpenYes.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    if (line.Contains("radioPngOpenNo"))
                    {
                        // MessageBox.Show("yes");
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);

                        radioPngOpenNo.Checked = true;
                        // filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }

                    if (line.Contains("numericUpDownLogFileIntervals"))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        numericUpDownLogFileIntervals.Text = result;
                        result = "";
                    }

                    if (line.Contains("numericUpDownHowLongIdleTillPopup"))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        numericUpDownHowLongIdleTillPopup.Text = result;
                        result = "";
                    }



                    if (line.Contains("colorDialog1"))
                    {




                        var result = line.Substring(line.IndexOf(' ') + 1);

                        var htmlColorDecode = ColorTranslator.FromHtml(result);
                        var grabbedColor = htmlColorDecode;

                        colorDialog1.Color = grabbedColor;
                        result = "";



                    }
                    if (line.Contains("numericUpDownIdleMinutes"))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        numericUpDownIdleMinutes.Text = result;
                        result = "";
                    }

                    if (line.Contains("numericUpDownHowLongIdleTillPopup "))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        numericUpDownHowLongIdleTillPopup.Text = result;
                        result = "";
                    }
                    if (line.Contains("formGrabPollRateNumericUpDown "))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        formGrabPollRateNumericUpDown.Text = result;
                        result = "";
                    }


                    if (line.Contains("radioMouseIdle "))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        radioMouseIdle.Checked = true;
                        result = "";
                    }

                    if (line.Contains("radioSystemIdle "))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        radioSystemIdle.Checked = true;
                        result = "";
                    }



                   



                    counter1++;
                }

                file.Close();




            }

            catch (Exception ex)
            {
                if (File.Exists(appDataTimeTrackerFolder + "\\Time Tracker Automation Options.cfg"))
                {
                    MessageBox.Show("Unable to load saved options " + Environment.NewLine + ex, "Settings Wizard");
                }
                //  MessageBox.Show(ex.ToString());  useless error message that pops on on first boot
            }

        }




        private void writeToConfigFile()
        {

            try
            {
                using (StreamWriter sw = File.CreateText(appDataTimeTrackerFolder + "\\" + "Time Tracker Automation.cfg"))
                {


                    sw.WriteLine("PomodoroTextBox " + PomodoroTextBox.Text);


                    sw.WriteLine("filterTextBoxDontIgnore " + filterTextBoxDontIgnore.Text);

                    sw.WriteLine("filterTextBoxIgnore " + filterTextBoxIgnore.Text);




                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }





        }

        private void readFromConfigFile()
        {



            try
            {

                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader file =
                   new System.IO.StreamReader(appDataTimeTrackerFolder + "\\" + "Time Tracker Automation.cfg");
                while ((line = file.ReadLine()) != null)
                {


                    if (line.Contains("filterTextBoxDontIgnore"))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        filterTextBoxDontIgnore.Text = result;
                        result = "";
                    }
                    if (line.Contains("filterTextBoxIgnore"))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        filterTextBoxIgnore.Text = result;
                        result = "";
                    }
                    //textboxes start


                    if (line.Contains("PomodoroTextBox "))
                    {
                        //read value after last space
                        var result = line.Substring(line.IndexOf(' ') + 1);
                        PomodoroTextBox.Text = result;
                        result = "";
                    }





                    counter++;
                }

                file.Close();




            }

            catch (Exception ex)
            {
                if (File.Exists(appDataTimeTrackerFolder + "\\Time Tracker Automation.cfg"))
                {
                    MessageBox.Show("Unable to load custom user settings " + Environment.NewLine + ex, "Settings Wizard");
                }
                //  MessageBox.Show(ex.ToString());  useless error message that pops on on first boot
            }

        }

        private void loadYourSavedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readFromConfigFile();
        }

        private void loadDefaultFactorySettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            filterTextBoxIgnore.Text = "";
            filterTextBoxDontIgnore.Text = "";
            numericUpDownIdleMinutes.Text = "3";
            colorDialog1.Color = Color.Lime;
            button2.BackColor = colorDialog1.Color;
            numericUpDownHowLongIdleTillPopup.Text = "5";
            radioButtonPomodoroYes.Checked = true;
            PomodoroTextBox.Text = "25";
            radioButtonPopUpOnTopYes.Checked = true;
            radioButtonPomodoroYes.Checked = true;
            numericUpDownLogFileIntervals.Text = "20";
            formGrabPollRateNumericUpDown.Text = "500";
            radioMouseIdle.Checked = true;

        }



        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            //when clicking system tray icon this brings it back up


            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;


        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            //when clicking system tray icon this brings it back up

            if (e.Button == MouseButtons.Right)
            {
                //do something here
                //MessageBox.Show("Test");
                //contextMenuStrip1.Show(Control.MousePosition);
                //  contextMenuStrip1.Show(Cursor.Position);

            }
            else//left or middle click
            {
                //do something here
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;

            }


        }

        private void updateRegistryAddStartup()
        {
            // RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //  add.SetValue("Time Tracker Automation", "\"" + Application.ExecutablePath.ToString() + "\"");

            try
            {
                // (Begin Regedit) --> Startup objects
                using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (rkApp.GetValue("Time Tracker") == null)
                    {

                        rkApp.SetValue("Time Tracker", Application.ExecutablePath.ToString());
                        rkApp.Close();
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Failed to update registry " + ex);
            }

        }


        private void radioButtonBootWindowsYes_CheckedChanged(object sender, EventArgs e)
        {

            updateRegistryAddStartup();

        }

        private void radioButtonBootWindowsNo_CheckedChanged(object sender, EventArgs e)
        {

            try
            {

                using (RegistryKey rkApp2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (rkApp2.GetValue("Time Tracker") != null)
                    {

                        rkApp2.DeleteValue("Time Tracker", false);
                        rkApp2.Close();
                    }
                    else if (rkApp2.GetValue("Time Tracker") == null)
                    {


                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update registry " + ex);
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            resizeOptionsFiltering();

        }

        private void buttonCloseOptionsMenu_Click(object sender, EventArgs e)
        {
            // panelOptionsLayer.SendToBack();
            //  panelOptionsLayer.Visible = false;
            //  panelOptionsLayer.Enabled = false;
            int idleMinutesInt = Convert.ToInt32(numericUpDownIdleMinutes.Value);
            int logFileIntervalsInt = Convert.ToInt32(numericUpDownLogFileIntervals.Value);

            if ((logFileIntervalsInt) < idleMinutesInt *60)
            {
                writeToOptionsFile();
                resizeOptionsFiltering();
            }
            else
            {
                MessageBox.Show("Log file write intervals in minutes have to be less than idle time in minutes otherwise the log file wont be written to.  Please lower the log file write intervals, or increase the idle timer.",
                      "",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information,
                      MessageBoxDefaultButton.Button1);
            }
        }

        private void startWithWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        // Color customColor;
        private void button2_Click(object sender, EventArgs e)
        {

            colorDialog1.ShowDialog();
            button2.BackColor = colorDialog1.Color;
            colorAjustment();


        }

        private void colorAjustment()
        {
            ////start color adjustments:
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {

                //this code doesn't seem to change anything but I'm leaving it for now to prevent potential errors
                if (row.Index == dataGridView1.Rows.Count - 1 && Convert.ToString(row.Cells[0].Value) == "")
                {
                    break;
                }
                DataGridViewCell iconCell = row.Cells[0];//icon fix all
                DataGridViewCell nameCellProcess = row.Cells[1];//namecell to reference process ID cell
                DataGridViewCell nameCell = row.Cells[2];//namecell to reference current cell
                DataGridViewCell timeCell = row.Cells[3];//namecell to reference current cell

                index = row.Index;//row index
                string nameOfWindowFromCell = Convert.ToString(nameCell.Value);//get window name from cell value
                string nameOfProcessFromCell = Convert.ToString(nameCellProcess.Value);//get window name from cell value
                Stopwatch stopWatchFromRow = swList[index];//create stopwatch/list
                if (nameOfWindowFromCell == stopWatchTextBox.Text)
                {

                    this.dataGridView1.CurrentCell = this.dataGridView1[0, index];


                    nameCell.Style.BackColor = colorDialog1.Color;
                    nameCellProcess.Style.BackColor = colorDialog1.Color;
                    timeCell.Style.BackColor = colorDialog1.Color;
                    //iconCell.Style.BackColor = colorDialog1.Color;

                    // MessageBox.Show(nameCell..ToString());



                    // If this row contains the name of the current window, make sure the stopwatch is running.
                    //windowIsInGrid = true;
                    //if (!stopWatchFromRow.IsRunning)
                    //{
                    //    stopWatchFromRow.Start();
                    //}
                }
                else
                {
                    nameCell.Style.BackColor = Color.White;
                    nameCellProcess.Style.BackColor = Color.White;
                    timeCell.Style.BackColor = Color.White;
                    //  iconCell.Style.BackColor = Color.White;
                    // Otherwise, make sure the stopwatch is stopped.
                    if (stopWatchFromRow.IsRunning)
                    {
                        stopWatchFromRow.Stop();
                    }
                }


            }

        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            //do nothing


        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //do nothing
        }
        string currentMouseOverRow;
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {

            try
            {

                if (e.Button == MouseButtons.Right)
                {

                    if ((e.RowIndex != -1) && (e.ColumnIndex != 0) && (e.ColumnIndex != 3))
                    {
                        // Add this
                        dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        // Can leave these here - doesn't hurt
                        dataGridView1.Rows[e.RowIndex].Selected = true;
                        dataGridView1.Focus();

                        ////

                        int rowSelected = e.RowIndex;

                        if ((e.RowIndex != -1) && (e.ColumnIndex != 0) && (e.ColumnIndex != 3))//dont want to click time column to ignore/add filter
                        {
                            this.dataGridView1.Rows[rowSelected].Selected = true;
                            currentMouseOverRow = dataGridView1.CurrentCell.Value.ToString();



                            ContextMenuStrip contexMenuuu = new ContextMenuStrip();
                            // contexMenuuu.BackColor = Color.Red;

                            contexMenuuu.Show();
                            //contexMenuuu.Items.Add("Delete ");
                            contexMenuuu.Items.Add((@"Add """ + currentMouseOverRow + @""" to the track list"));
                            contexMenuuu.Items.Add((@"Add """ + currentMouseOverRow + @""" to the ignore list"));

                            //   contexMenuuu.Show();
                            contexMenuuu.Show(dataGridView1, new Point(e.X, e.Y));
                            contexMenuuu.ItemClicked += new ToolStripItemClickedEventHandler(
                                contexMenuuu_ItemClicked);

                            if (currentMouseOverRow != null)
                            {
                                // m.MenuItems.Add(new MenuItem(string.Format("Ignore ")));
                            }


                            //make context menu pop up near cursor
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                contexMenuuu.Show(Cursor.Position);
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing so no errors when right clicking the row header area
            }


        }




        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void contexMenuuu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            if (e.ClickedItem.ToString().EndsWith("to the ignore list"))
            {
                //MessageBox.Show("Test");
                try
                {
                    filterTextBoxIgnore.Text += "," + currentMouseOverRow;

                    // MessageBox.Show(@"Added """+currentMouseOverRow+@""" to the ignore list","Added");
                    currentMouseOverRow = "";

                }
                catch (Exception)
                {

                }
            }
            else if (e.ClickedItem.ToString().EndsWith("to the track list"))
            {
                //MessageBox.Show("Test");
                try
                {
                    filterTextBoxDontIgnore.Text += "," + currentMouseOverRow;

                    // MessageBox.Show(@"Added """+currentMouseOverRow+@""" to the ignore list","Added");
                    currentMouseOverRow = "";

                }
                catch (Exception)
                {

                }
            }




        }

        private void commenceUndoFilterView()
        {
           


        }

        private void SaveToundoFilterView()
        {

           
            
        }


        private void updateFilterView()
        {

            //added these two lines on 11/26 to see if fixes jumbled column issue
            this.dataGridViewFilterView.Rows.Clear();
            this.dataGridViewFilterView.Columns.Clear();



            if (this.dataGridViewFilterView.DataSource != null)
            {
                this.dataGridViewFilterView.DataSource = null;
            }
            else
            {
                this.dataGridViewFilterView.Rows.Clear();
                this.dataGridViewFilterView.Columns.Clear();

            }
            //then you can copy the rows values one by one (working on the selectedrows collection)
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                dataGridViewFilterView.Columns.Add(c.Clone() as DataGridViewColumn);
            }

            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                int index = dataGridViewFilterView.Rows.Add(r.Clone() as DataGridViewRow);
                foreach (DataGridViewCell o in r.Cells)
                {
                    dataGridViewFilterView.Rows[index].Cells[o.ColumnIndex].Value = o.Value;


                }
            }



            foreach (DataGridViewRow rs in dataGridViewFilterView.Rows)
            {
                // int index2 = dataGridViewFilterView.Rows.Add(rs.Clone() as DataGridViewRow);
                foreach (DataGridViewCell o2 in rs.Cells)
                {

                    o2.Style.BackColor = Color.White;

                }
            }


            dataGridViewFilterView.ClearSelection();
            dataGridViewFilterView.Columns[0].SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewFilterView.Columns[1].SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewFilterView.Columns[2].SortMode = DataGridViewColumnSortMode.Automatic;
            dataGridViewFilterView.Columns[3].SortMode = DataGridViewColumnSortMode.Automatic;


        }


        private void button3_Click(object sender, EventArgs e)
        {


            saveLoadSettingsToolStripMenuItem.Visible = false;
            //menuStrip1.Hide();

            panel1FilterView.BringToFront();
            // updateFilterView();
            addTime();

            if (refreshMaybe == true)
            {
                string message = "Would you like to refresh this page with your current tracked data from the main display?  This will overwrite any changes you filtered out already on this page.";
                string caption = "Refresh?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;



                // Displays the MessageBox.mbox     

                result = MessageBox.Show(message, caption, buttons);





                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    updateFilterView();
                    addTime();
                }

                if (result == System.Windows.Forms.DialogResult.No)
                {
                    //do nothing
                }
            }



        }

        private void panelFilterCloseButton_Click(object sender, EventArgs e)
        {

            saveLoadSettingsToolStripMenuItem.Visible = true;
            // menuStrip1.Show();
            panel1FilterView.SendToBack();
        }

        Boolean refreshMaybe;
        private void refreshFilterView_Click(object sender, EventArgs e)
        {
            buttonsDisable();

            //  refreshMaybe = true;

            //updateFilterView();
            /////////////////////////////////////////

            
                
           




            dataGridViewFilterView.Columns.Clear();
                dt = new DataTable();
                dt.Columns.Add("Date"); //icon fix
                dt.Columns.Add("Process ID");
                dt.Columns.Add("Form ID");
                dt.Columns.Add("Time");

            FileInfo file = new FileInfo("file.txt");

            if (file.Exists)
            {
                // TO DO
            }

            try
            {

                foreach (string f in Directory.GetFiles(Application.StartupPath + "\\Time Tracker Automated Time Logs\\"))

                {


                    DateTime creation = File.GetCreationTime(f);


                    if (creation.ToString("MMMM dd, yyyy") == DateTime.Today.ToString("MMMM dd, yyyy"))
                    {
                        tempFileNameStoreToday.Add(f);
                        //  MessageBox.Show(f.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "File(s) unable to import");
            }
                //////////////////////////


                foreach (var tempFileLoopThroughToday in tempFileNameStoreToday)
                {

                   


                    using (var sr = File.OpenText(tempFileLoopThroughToday))
                    {
                        Application.DoEvents();
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            line = line.Replace(@"""", "");

                       
                            try
                            {
                            if ((line.Contains("00:00:00") == false) && (line.Contains(":") == true) && (line.Contains("Icon,Process ID,Form") == false))
                            {
                                thisArray = line.Split(',');
                                dt.Rows.Add(thisArray);
                            }


                        }
                            catch (Exception)
                            {
                                blankCellErrorTrackInt++;
                                blankCellErrorTrack = true;
                                if (blankCellErrorTrackInt <= 100)//only track 100 errors otherwise lags cvs importation
                                {
                                    errorMessage += string.Join(Environment.NewLine, thisArray);
                                }

                            }
                        }

                    }



                }


           
                dataGridViewFilterView.DataSource = dt;
            



            blankCellErrorTrack = false;

            if (thisArray != null)
            {
                Array.Clear(thisArray, 0, thisArray.Length);
            }

            //clear filenames to time doesn't double
            tempFileNameStoreToday.Clear();



            if (filesWereOpened == true)
            {
                MessageBox.Show("Import complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information,
         MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                filesWereOpened = false;
            }
            //////////////////////
            buttonsEnable();
                addTime();


        }


        string searchText;


        private void buttonsEnable()
        {
            refreshFilterView.Enabled = true;
            importCSVButton.Enabled = true;
           
           // buttonRevertFilter.Enabled = true;
            button5.Enabled = true;
            button4.Enabled = true;
            buttonSaveToPng.Enabled = true;

            filterTextBoxKeep.Enabled = true;
            filterFormIDButton.Enabled = true;
            panelFilterCloseButton.Enabled = true;
        }
        private void buttonsDisable()
        {
            refreshFilterView.Enabled = false;
            importCSVButton.Enabled = false;
           
            panelFilterCloseButton.Enabled = false;
          //  buttonRevertFilter.Enabled = false;
            button5.Enabled = false;
            button4.Enabled = false;
            buttonSaveToPng.Enabled = false;

            filterTextBoxKeep.Enabled = false;
            filterFormIDButton.Enabled = false;
           
        }



        // int notI = 0;
        //   List<string> storeRow = new List<string>();
        //private void filterRowsKeepButton_Click(object sender, EventArgs e)
        //{
        //    Application.DoEvents();
        //    buttonsDisable();


        //    searchText = filterTextBoxKeep.Text.ToUpper();
        //    while (filterAvailable())
        //        filter();

        //    buttonsEnable();
           


        //    addTime();
        //}

        public Boolean filterAvailable()
        {

          
            Boolean ret = false;
            foreach (DataGridViewRow row in dataGridViewFilterView.Rows)
            {
                
                String s = "";
                if (row.Index != dataGridViewFilterView.Rows.GetLastRow(DataGridViewElementStates.Visible) + 1)
                {
                   
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        
                        if (cell.Value != null)
                            s += cell.Value.ToString();
                    }
                }
                if (s.ToUpper().Contains(searchText) == false && s.Length > 0)
                {
                   

                    // dataGridViewFilterView.Rows.Remove(row);
                    ret = true;
                    break;

                }

            }
            return ret;
        }
        //Thread z;
        //public void filter()
        //{
        //    IsCancelled = false;
        //    z = new Thread(() =>
        //    {

        //        foreach (DataGridViewRow row in dataGridViewFilterView.Rows)
        //        {
        //            dataGridViewFilterView.Invoke((MethodInvoker)delegate
        //            {
        //                String s = "";
        //                if (row.Index != dataGridViewFilterView.Rows.GetLastRow(DataGridViewElementStates.Visible) + 1)
        //                    foreach (DataGridViewCell cell in row.Cells)
        //                        if (cell.Value != null)
        //                            s += cell.Value.ToString();

        //                //Application.DoEvents();
        //                if (s.ToUpper().Contains(searchText) == false && s.Length > 0)
        //                    dataGridViewFilterView.Rows.Remove(row);
        //            });





        //    }
        //    });
        //}
        private void addTime()
        {

            try
            {
                string timeCellData = string.Empty;
                int indexOfYourColumn = 3;//icon fix

                foreach (DataGridViewRow row in dataGridViewFilterView.Rows)
                {
                    Application.DoEvents();
                    Application.DoEvents();
                    DataGridViewCell timeCellData2 = row.Cells[indexOfYourColumn];
                    timeCellData = row.Cells[indexOfYourColumn].Value.ToString();
                    // MessageBox.Show(timeCellData.ToString());


                    Regex regexTime = new Regex(@"\d+:\d+:\d+");
                    if (regexTime.IsMatch(timeCellData))
                    {
                        // MessageBox.Show(timeCellData.ToString()+"is match");
                    }
                    else
                    {
                        Application.DoEvents();
                        // MessageBox.Show(timeCellData.ToString() + "no match");
                        timeCellData = "00:00:00";

                        timeCellData2.Value = timeCellData;
                    }

                   // timeCellData = timeCellData.Trim('"'); ;//remove quotes in time field
                    timeCellData2.Value = timeCellData;

                }
                //trim it up

                //string trimCell = string.Empty;
                //for (int i = dataGridViewFilterView.Rows.Count - 1; i >= 0; i--)
                //{
                //    Application.DoEvents();
                //    DataGridViewRow dataGridViewRow = dataGridViewFilterView.Rows[i];

                //    foreach (DataGridViewCell cell in dataGridViewRow.Cells)
                //    {
                //        try
                //        {
                //            Application.DoEvents();
                //            if (cell.ColumnIndex != 0)//trim all columns except column 0 if has icon in it
                //            {
                //                trimCell = cell.Value.ToString();
                //                trimCell = trimCell.Trim('"');
                //                cell.Value = trimCell;
                //            }

                //        }
                //        catch (Exception)
                //        {
                //            // MessageBox.Show(ex.ToString());
                //        }
                //    }
                //}


                string iconNowDateColumn = string.Empty;
                string timeCellDataColumn2 = string.Empty;

                foreach (DataGridViewRow row2 in dataGridViewFilterView.Rows)
                {
                    Application.DoEvents();
                    DataGridViewCell timeCellData2Column2 = row2.Cells[3];
                    timeCellDataColumn2 = row2.Cells[3].Value.ToString();
                    timeCellDataColumn2 = timeCellDataColumn2.Trim('"'); ;//remove quotes in time field                
                    timeCellData2Column2.Value = timeCellDataColumn2;



                    DataGridViewCell iconNowDateColumn0 = row2.Cells[0];
                    string quoteCheck = iconNowDateColumn0.Value.ToString();

                    if (quoteCheck.Contains("\""))
                    {
                        Application.DoEvents();
                        iconNowDateColumn = row2.Cells[0].Value.ToString();
                        iconNowDateColumn = iconNowDateColumn.Trim('"'); ;//remove quotes in icon field                
                        iconNowDateColumn0.Value = iconNowDateColumn;
                    }
                }

                Application.DoEvents();
                // Retrieve total seconds
                double seconds = 0;
                seconds = dataGridViewFilterView.Rows.Cast<DataGridViewRow>()
                    .AsEnumerable()
                    .Sum(x => TimeSpan.Parse((x.Cells[3].Value.ToString())).TotalSeconds);//2 is time column currently  //icon fix

                // Assign to textbox
                string timeYo = TimeSpan.FromSeconds(seconds).TotalHours + ":" + TimeSpan.FromSeconds(seconds).Minutes
                         + ":" + TimeSpan.FromSeconds(seconds).Seconds.ToString();


                // MessageBox.Show(timeYo.ToString());
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                string timeYoConverted = time.ToString(@"hh\:mm\:ss");
                //  MessageBox.Show(timeYoConverted.ToString());

                timeYoConverted = string.Format("{0}hr {1}mn {2}sec",
                           (int)time.TotalHours,
                           time.Minutes,
                           time.Seconds);

                label5.Text = "Total Time: " + timeYoConverted.ToString();
            }
            catch(Exception)
            {//temporary warning
               // MessageBox.Show("Error somewhere on the entire add time code");
                buttonsEnable();
            }
            }
        

        // buttonsEnable();

        DataTable dt = new DataTable();
        int blankCellErrorTrackInt = 0;
        Boolean filesWereOpened;
        string[] thisArray;
        string[] filterCommaSplit;
        //   List<string[]> rows;
        bool blankCellErrorTrack;
        string errorMessage;
        private void importCSVButton_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            buttonsDisable();
            progressBarPanel.Visible = true;
            progressBarPanel.Enabled = true;
            progressBarPanel.BringToFront();
            progressBar1.Enabled = true;
            progressBar1.Visible = true;
            progressBar1.BringToFront();
             blankCellErrorTrackInt = 0;
            //int size = -1;
            // DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            // if (result == DialogResult.OK) // Test result.

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV files (*.CSV)|*.CSV|All files (*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Application.StartupPath + "\\Time Tracker Automated Time Logs\\");
            openFileDialog1.Title = "Please select one or more CSV time log files to import (use files generated by time tracker only)";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                
                filesWereOpened = true;
                dataGridViewFilterView.Columns.Clear();
                 dt = new DataTable();
                dt.Columns.Add("Date"); //icon fix
                dt.Columns.Add("Process ID");
                dt.Columns.Add("Form ID");
                dt.Columns.Add("Time");

                foreach (var openFile in openFileDialog1.FileNames)
                {
                    
                    tempFileNameStore.Add(openFile);
                    
                }
           
                

                if (backgroundWorkerFileImport.IsBusy != true)
                {

                    backgroundWorkerFileImport.RunWorkerAsync();
                    Application.DoEvents();

                }









            }

            else
            {
                filesWereOpened = false;

                progressBarPanel.Visible = false;
                progressBarPanel.Enabled = false;
                progressBarPanel.SendToBack();
                progressBar1.Enabled = false;
                progressBar1.Visible = false;
                progressBar1.SendToBack();

                buttonsEnable();
            }



            // dataGridViewFilterView.DataSource = null;
            // dataGridViewFilterView.Rows.Clear();

            // dataGridViewFilterView.Rows.Clear();

            //if (rows != null)
            //{
            //    rows.Clear();
            //}

            buttonsEnable();

        }



        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            MessageBox.Show(@"If you want to sort columns, please click the """ + button3.Text + @""" button.", "Sorting");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
           

            // Delete Key - Delete Selected Row!
            if (keyData == Keys.Delete)
            {
                if (dataGridViewFilterView.Focused)
                {
                    labelUpdating.Text = "Deleting...";
                    deleteSelectedRows();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        string currentMouseOverFilterRow;
        private void dataGridViewFilterView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {




            try {

                if (e.Button == MouseButtons.Right)
                {
                    if ((e.RowIndex != -1) && (e.ColumnIndex != 0))

                    //
                    {
                        // Add this
                        dataGridViewFilterView.CurrentCell = dataGridViewFilterView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        // Can leave these here - doesn't hurt
                        dataGridViewFilterView.Rows[e.RowIndex].Selected = true;
                        dataGridViewFilterView.Focus();

                        ////

                        int rowSelected = e.RowIndex;

                        if ((e.RowIndex != -1) && (e.ColumnIndex != 0))//dont want to click time column to ignore/add filter
                        {
                            this.dataGridViewFilterView.Rows[rowSelected].Selected = true;
                            currentMouseOverFilterRow = dataGridViewFilterView.CurrentCell.Value.ToString();



                            ContextMenuStrip contexMenuFilterScreen = new ContextMenuStrip();
                            // contexMenuFilterScreen.BackColor = Color.Red;

                            contexMenuFilterScreen.Show();
                            //contexMenuFilterScreen.Items.Add("Delete ");
                           // contexMenuFilterScreen.Items.Add((@"Only show rows containing """ + currentMouseOverFilterRow));
                           // contexMenuFilterScreen.Items.Add((@"Delete selected rows" + currentMouseOverFilterRow));
                            
                            //   contexMenuFilterScreen.Show();
                            contexMenuFilterScreen.Show(dataGridViewFilterView, new Point(e.X, e.Y));
                            contexMenuFilterScreen.ItemClicked += new ToolStripItemClickedEventHandler(
                                contexMenuFilterScreen_ItemClicked);

                            if (currentMouseOverFilterRow != null)
                            {
                                // m.MenuItems.Add(new MenuItem(string.Format("Ignore ")));
                            }


                            //make context menu pop up near cursor
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                contexMenuFilterScreen.Show(Cursor.Position);
                            }

                        }
                    }
                }
            }
            catch(Exception)
            {
              
            }
        }


        private void contexMenuFilterScreen_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            if (e.ClickedItem.ToString().StartsWith("Remove rows containing"))
            {
                //MessageBox.Show("Test");
                try
                {

                   // filterTextBoxRemove.Text = currentMouseOverFilterRow;

                    // MessageBox.Show(@"Added """+currentMouseOverRow+@""" to the ignore list","Added");
                    currentMouseOverFilterRow = "";
                  //  dataGridFilterFilterButton.PerformClick();
                   // filterTextBoxRemove.Text = "";

                }
                catch (Exception)
                {

                }
            }
            else if (e.ClickedItem.ToString().StartsWith("Only show rows containing"))
            {
                //MessageBox.Show("Test");
                try
                {
                    filterTextBoxKeep.Text = currentMouseOverFilterRow;

                    // MessageBox.Show(@"Added """+currentMouseOverRow+@""" to the ignore list","Added");
                    currentMouseOverFilterRow = "";
                  //  filterRowsKeepButton.PerformClick();
                    filterTextBoxKeep.Text = "";
                }
                catch (Exception)
                {

                }
            }




        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            importCSVButton.PerformClick();
        }

        private void filterTextBoxDontIgnore_TextChanged(object sender, EventArgs e)
        {
            dateAndLogTimer.Stop();
            dateAndLogTimer.Start();
        }

        private void dataGridViewFilterView_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

           
        }

        private void TimeTracker_SizeChanged(object sender, EventArgs e)
        {
            //if(this.WindowState == FormWindowState.Minimized)
            //{

            //    notifyIcon1.Icon = SystemIcons.Application;
            //    notifyIcon1.BalloonTipText = "Minimizedyo";
            //    notifyIcon1.ShowBalloonTip(1000);
            //}
            //else if (this.WindowState == FormWindowState.Normal)
            //{
            //    notifyIcon1.BalloonTipText = "Your form back yo";
            //    notifyIcon1.ShowBalloonTip(1000);
            //}
        }

     
        //custom stopwatch timer starts here
        
        private void stopWatchButton_Click(object sender, EventArgs e)
        {

            if (stopWatchTextBox.Text == "")
            {
                MessageBox.Show("Please enter a name for the stopwatch so it can be tracked.", "Stopwatch");
            }
            else
            {
                stopAllWatches = true;
                swList.Last().Stop();
                foreach (var timer in swList)
                {
                    timer.Stop();
                }

                if (stopWatchTextBox.Enabled == true)
                {


               

                    Icon IEIconzz2 = Icon.ExtractAssociatedIcon(Application.ExecutablePath);                                    

                    Bitmap stopIcon = new Bitmap(IEIconzz2.ToBitmap(), 17, 17);


                    stopWatchButton.Text = "Stop Timer";
                   // var index = dataGridView1.Rows.Count - 1;
                 //   Stopwatch stopWatchFromRow = swList[index];
                    dataGridView1.Rows.Add(stopIcon, "Manual Stopwatch Timer", stopWatchTextBox.Text, "00:00.00");
                    swList.Add(new Stopwatch());
                    swList.Last().Start();
                    stopWatchTextBox.Enabled = false;
                    stopWatchButton.BackColor = Color.Red;

                    MessageBox.Show(@"Stopwatch started for """ + stopWatchTextBox.Text + @""" task.  No applications will be tracked until the stopwatch is stopped.  Also keep in mind the stop watch will keep counting even while the computer is idle so that you can track custom tasks whether you are at your computer or away from it.", "Stopwatch");
                }
                else
                {
                    stopWatchButton.Text = "Start Timer";
                    swList.Last().Stop();
                    stopWatchTextBox.Enabled = true;
                    stopAllWatches = false;
                    stopWatchTextBox.Text = "";
                }


                ////start color adjustments:

                colorAjustment();
               

                stopWatchButton.BackColor = SystemColors.Control;

                //if (!stopWatchFromRow.IsRunning)
                //{
                //    stopWatchFromRow.Start();
                //}

            }
        }

        private void tallyLabel_TextChanged(object sender, EventArgs e)
        {
          if (stopAllWatches == true)
            {
                
                if (stopWatchButton.BackColor == SystemColors.Control)
                {

                    stopWatchButton.BackColor = Color.Red;
                }
                else
                {
                    stopWatchButton.BackColor = SystemColors.Control;
                }

            }

        }

        public Timer tomatoTimer;
        double tomatoDouble;
        DateTime startTime;
        public string timeRecall;
        //   internal object stopWatchFromRow;

        public Boolean timerActivatedButtonPush;
        private void PomodoroButton_Click(object sender, EventArgs e)
        {
            

            if (PomodoroTextBox.Text != "")
            {


                if (PomodoroTextBox.Enabled == true)
                {
                    tomatoPic.Visible = true;

                    if (timerActivatedButtonPush == false)
                    {
                 //       timeRecall = PomodoroTextBox.Text;
                    }

                    if (PomodoroTextBox.Text != "5")//5 min break timer messes it up
                        {
                        timeRecall = PomodoroTextBox.Text;
                    }

                    timerActivatedButtonPush = true;
                    PomodoroTextBox.Enabled = false;
                    PomodoroButton.Text = "Stop";
                    tomatoDouble = double.Parse(PomodoroTextBox.Text);

                    PomodoroTextBox.Text = TimeSpan.FromMinutes(tomatoDouble).ToString();

                    startTime = DateTime.Now;

                    tomatoTimer = new Timer() { Interval = 1000 };


                     tomatoTimer.Tick += new EventHandler(tomatoTimer_Tick);


                   tomatoTimer.Enabled = true;


                }

                else
                {
                    tomatoPic.Visible = false;
                    PomodoroTextBox.Enabled = true;
                    tomatoTimer.Stop();
                    PomodoroButton.Text = "Start";
                    PomodoroTextBox.Text = timeRecall;
                }
            }
            else
            {
                MessageBox.Show("Please enter a number in minutes","Timer");
            }

        }

        private void tomatoTimer_Tick(object sender, EventArgs e)
        {
            PomodoroTextBox.Text =
              (TimeSpan.FromMinutes(tomatoDouble) - (DateTime.Now - startTime))
              .ToString("hh\\:mm\\:ss");


            if (PomodoroTextBox.Text == "00:00:00")
            {
                tomatoTimer.Stop();
                PomodoroTextBox.Enabled = true;

                PomodoroButton.Text = "Start";
                PomodoroTextBox.Text = timeRecall;

                if (radioButtonPomodoroYes.Checked == true)
                {
                    Stream str = Properties.Resources.ding2;
                    SoundPlayer snd = new SoundPlayer(str);
                    snd.Play();
                }

                {
                    pomodoroTimer f3 = new pomodoroTimer(this);
                    f3.StartPosition = FormStartPosition.CenterScreen;
                    f3.ShowDialog(this);

                    //string message = "Time's up!  Your " + timeRecall + " minute timer has finished.  Take a break! Make sure to stand up and stretch for 5 min or so :)  Would you like to restart the timer now?";
                    //string caption = "Pomodoro Timer";
                    //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    //MessageBoxIcon icon = MessageBoxIcon.Information;
                    //DialogResult result;



                    //// Displays the MessageBox.mbox     

                    //result = MessageBox.Show(new Form { TopMost = true }, message, caption, buttons, icon);





                    //if (result == System.Windows.Forms.DialogResult.Yes)
                    //{
                    //  //  PomodoroButton.PerformClick();
                    //}

                    //if (result == System.Windows.Forms.DialogResult.No)
                    //{

                        
                    //    //do nothing
                    //}


                 

                }

            

            }
        }


        private void PomodoroTextBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void PomodoroTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
      //      if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
      //(e.KeyChar != '.'))
      //      {
      //          e.Handled = true;
      //          MessageBox.Show("Numerical characters only: Please enter time in minutes.","Timer");
      //      }
          

            //// only allow one decimal point
            //if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            //{
            //    e.Handled = true;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            idleReturnForm f2 = new idleReturnForm(this);
            f2.Show();
        }

        


       
       
        private bool CloseRequested;
       // private object evtargs;

        private void exitToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            CloseRequested = true;
            this.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           

        }

        

        private void PomodoroTextBox_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }


        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Per Wikipedia:  The Pomodoro Technique is a time management method developed by Francesco Cirillo in the late 1980s. The technique uses a timer to break down work into intervals, traditionally 25 minutes in length, separated by short [~5 minute] breaks. These intervals are called pomodoros, the plural in English of the Italian word pomodoro, which means tomato. The method is based on the idea that frequent breaks can improve mental agility.");
        }

        private void numericUpDownLogFileIntervals_ValueChanged(object sender, EventArgs e)
        {
            logFileCustomInterval();
        }

        private void numericUpDownHowLongIdleTillPopup_KeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void numericUpDownLogFileIntervals_KeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void numericUpDownIdleMinutes_KeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void numericUpDownIdleMinutes_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        
     
        
        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("When the computer is idle this program will temporarily pause the timers until there is activity.  Adjust how long in minutes you want the computer to be able to idle before the timers actually pause until activity is detected on the computer again.  This prevents the timers from counting non-stop on whatever the last window with focus was on your lunch breaks etc!", "Idle Timer");

           
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"If you set this for 5 minutes for example and you are gone and the computer idles for 5 or more minutes, when you get back and move the mouse or type on the keyboard, you'll get a popup with an option to enter whatever activity you were doing when you were gone if it was time you want to track.", "Idle Return");
        }

        private void formGrabPollRateNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            formGrabPollRate();
        }

        private void formGrabPollRateNumericUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"","Form polling");
        }

      private void deleteSelectedRows()
        {

            labelUpdating.Text = "Deleting...";
            foreach (DataGridViewRow row in dataGridViewFilterView.SelectedRows)
            {
                dataGridViewFilterView.Rows.RemoveAt(row.Index);
                
            }


            addTime();
            labelUpdating.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            labelUpdating.Text = "Deleting...";
            deleteSelectedRows();

            //buttonsDisable();
            //refreshFilterView.Enabled = false;
            //importCSVButton.Enabled = false;
            //dataGridFilterFilterButton.Enabled = false;
            //filterRowsKeepButton.Enabled = false;
            //panelFilterCloseButton.Enabled = false;

            //try
            //{

            //    //text filter
            //    for (int i = dataGridViewFilterView.Rows.Count - 1; i >= 0; i--)
            //    {
            //        DataGridViewRow dataGridViewRow = dataGridViewFilterView.Rows[i];

            //        foreach (DataGridViewCell cell in dataGridViewRow.Cells)
            //        {
            //            try
            //            {
            //                string val = cell.Value as string;
            //                if ((val.EndsWith("00:00:00") || (val.EndsWith("00:00:01") || (val.EndsWith("00:00:02") || (val.EndsWith("00:00:03") || (val.EndsWith("00:00:04") || (val.EndsWith("00:00:05") || (val.EndsWith("00:00:06") || (val.EndsWith("00:00:07") || (val.EndsWith("00:00:08") || (val.EndsWith("00:00:09") || (val.EndsWith("00:00:10")))))))))))))
            //                {
            //                    //MessageBox.Show(val);
            //                    if (!dataGridViewRow.IsNewRow)
            //                    {
            //                        // MessageBox.Show("not new?");
            //                        dataGridViewFilterView.Rows.Remove(dataGridViewRow);
            //                        break;
            //                    }

            //                }
            //            }
            //            catch (Exception)
            //            {
            //                // MessageBox.Show(ex.ToString());
            //                buttonsEnable();
            //            }
            //        }
            //    }

            //}

            //catch(Exception)
            //{//temporary warning
            //    MessageBox.Show("Error with removing 10 or less");
            //    buttonsEnable();
            //}

            //Int32 selectedRowCount = dataGridViewFilterView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            //if (selectedRowCount > 0)
            //{
            //    for (int i = 0; i < selectedRowCount; i++)
            //    {
            //        dataGridViewFilterView.Rows.RemoveAt(dataGridViewFilterView.SelectedRows[0].Index);
            //    }
            //}


          



           // buttonsEnable();



        }

        private void midnightLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            midnightLabel.Visible = false;
        }

        private void label2_Click(object sender, EventArgs e)
        {
         
        }

        private void linkLabel7_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start((Application.StartupPath + "\\Time Tracker Automated Time logs\\"));
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void filterTextBoxDontIgnore_MouseLeave(object sender, EventArgs e)
        {
            regexRemoveEndswithComma();
        }

        private void filterTextBoxIgnore_MouseLeave(object sender, EventArgs e)
        {
            regexRemoveEndswithComma();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //  dataGridViewFilterView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            try
            {
                if (dataGridViewFilterView.Rows.Count != 0)
                {
                    dataGridViewFilterView.SelectAll();
                    DataObject dataObj = dataGridViewFilterView.GetClipboardContent();
                    if (dataObj != null)
                        Clipboard.SetDataObject(dataObj);
                    dataGridViewFilterView.ClearSelection();


                    // MessageBox.Show("Contents Copied to clipboard", "Grid to Clipboard");


                    MessageBox.Show("Contents copied to clipboard.",
                        "Grid to Clipboard",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                    

                }
                else
                {
                    MessageBox.Show("Grid is empty.",
                        "Grid to Clipboard",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error copying to clipboard: " + Environment.NewLine + Environment.NewLine + ex.ToString(),
                       "Grid to Clipboard",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Error,
                       MessageBoxDefaultButton.Button1);
               // MessageBox.Show("Error copying to clipboard " + Environment.NewLine + Environment.NewLine + ex.ToString());
            }
        }

        //private void buttonRevertFilter_Click(object sender, EventArgs e)
        //{
        //    Application.DoEvents();
        //    buttonsDisable();
        //   // commenceUndoFilterView();
        //    addTime();
        //    buttonsEnable();
        //}

        private void DrawBitmapWithBorder(Bitmap bmp, Point pos, Graphics g)//not used
        {
            const int borderSize = 20;

            using (Brush border = new SolidBrush(Color.White /* Change it to whichever color you want. */))
            {
                g.FillRectangle(border, pos.X - borderSize, pos.Y - borderSize,
                    bmp.Width + borderSize, bmp.Height + borderSize);
            }

            g.DrawImage(bmp, pos);
        }


        private void buttonSaveToPng_Click(object sender, EventArgs e)
        {
            string dayOfWeekPng = DateTime.Now.DayOfWeek.ToString();
            string dateCreatedPng = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + " " + dayOfWeekPng;
            string dateCreatedPngBasic = " (Created on " +DateTime.Now.ToString("MM/dd/yyyy")+")";
            

            //Resize DataGridView to full height.
            int height = dataGridViewFilterView.Height;
           // int bottom = dataGridViewFilterView.Bottom;
            dataGridViewFilterView.Height = 45+(dataGridViewFilterView.RowCount * dataGridViewFilterView.RowTemplate.Height);

            //Create a Bitmap and draw the DataGridView on it.
            Bitmap bitmap = new Bitmap(this.dataGridViewFilterView.Width, this.dataGridViewFilterView.Height);

            using (Graphics g2 = Graphics.FromImage(bitmap))
            {
                g2.FillRectangle(Brushes.Black, 0, 0, bitmap.Width, bitmap.Height);
            }


            
            dataGridViewFilterView.DrawToBitmap(bitmap, new Rectangle(0, 21, this.dataGridViewFilterView.Width, this.dataGridViewFilterView.Height));

            //Resize DataGridView back to original height.
            dataGridViewFilterView.Height = height;



            // Bitmap image = new Bitmap(img);

            StringFormat strFormat = new StringFormat();

            strFormat.Alignment = StringAlignment.Near;
            strFormat.LineAlignment = StringAlignment.Near;

            Graphics g = Graphics.FromImage(bitmap);


            //start border
           
            //end border

            g.DrawString(label5.Text+ dateCreatedPngBasic, new Font("Arial", 15), Brushes.Red,
                    0,0, strFormat);


            //bottom left when string allignment is center or far or near?  Center I think
            //g.DrawString("123456789 hello mcfly", new Font("Tahoma", 11), Brushes.Red,
            //   0, bitmap.Height - 10, strFormat);


            //Save the Bitmap to folder.
            try
            {
                bitmap.Save(Application.StartupPath + "\\" + dateCreatedPng + "DataGrid Screenshot.png");

                MessageBox.Show("PNG file saved "+ Application.StartupPath + "\\" + dateCreatedPng + "DataGrid Screenshot.png","Export Complete");

                if (radioPngOpenYes.Checked == true)
                {
                    Process.Start(Application.StartupPath + "\\" + dateCreatedPng + "DataGrid Screenshot.png");
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error, unable to save PNG file" + Environment.NewLine+Environment.NewLine+ ex, "Error");
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Application.DoEvents();

            regexRemoveEndswithComma();
            string todayDateWithoutTime = DateTime.Now.ToString("MM/dd/yyyy");
            
            

            // tomorrowCheck = false;
            //  dateAndLogTimer.Stop();



            var sb = new StringBuilder();

            var headers = dataGridView1.Columns.Cast<DataGridViewColumn>();
            sb.AppendLine("Time Log " + todayDateWithoutTime +  "(" + tallyLabel.Text+" / " + activeTimeLabel.Text +")");
            sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //  var cellsEndWithCheck = row.Cells.Cast<DataGridViewCell>();
                // string[] test123 = (string.Join(",", cellsEndWithCheck.Select(cell => "\"" + cell.Value + "\"").ToArray()));


                var cells = row.Cells.Cast<DataGridViewCell>();
                sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
            }
            char a, b, c;
            for (int i = 1; i < sb.Length - 1; i++)
            {
                b = sb[i];
                a = sb[i - 1];
                c = sb[i + 1];
                if (b.ToString().Equals(","))
                {
                    if (!a.ToString().Equals("\"") && !c.ToString().Equals("\""))
                    {
                        //replaces all comma within the cell sentences
                        sb[i] = '-';
                    }
                    //If the comma is the last char in a cell
                    if (a.ToString().Equals("\"") && c.ToString().Equals("\""))
                    {
                        if (sb[i + 2].ToString().Equals(","))
                        {
                            sb[i] = '-';
                        }
                    }
                    else
                    {
                        sb[i] = '-';
                    }
                }
                Application.DoEvents();
                sb = sb.Replace("System.Drawing.Bitmap", DateTime.Now.ToString("yyyy/MM/dd"));

            }
            //using (StreamWriter writer = new StreamWriter(@"lol.csv"))
            //{
            //    writer.Write(sb);
            //}

           

            string dayOfWeek = DateTime.Now.DayOfWeek.ToString();


            //whe nday 

            



           

            //  var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // Directory.CreateDirectory(path + "\\Time Tracker Automated Time logs");
            var pathLogFile = Path.GetDirectoryName(Application.StartupPath + "\\Time Tracker Automated Time logs\\");
            Directory.CreateDirectory(pathLogFile);
            //MessageBox.Show(pathLogFile);

            //var pathLogFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location  +"\\Time Tracker Time Logs");

            // MessageBox.Show(dayOfWeek);


            if (firstPass == true)//to only create one log file per session, and make it easy to create a new one later with a different filename 
            {
                dateCreated = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + " " + dayOfWeek;
                firstPass = false;
            }
            else if (firstPass == false)
            {
                //  dateCreated = dateCreated;//not necessarry code, but makes me feel better :)
            }

            //if the current date doesn't match the date of the program's startup, restart.  This ensures if computer is suspended on a friday and is resumed on a monday, function should still work
            if (todayDateWithoutTime != todaysTimeOnStartup)
            {

                backgroundWorker1.CancelAsync();

                this.Show();
                this.WindowState = FormWindowState.Normal;
                
                //test only 
                //sleep thread to give it time to not be visible before restarting
                inUseNotIdle = false; //so log file is not written to below
                System.Threading.Thread.Sleep(3000);
                notifyIcon1.Visible = false;
               // notifyIcon1.Dispose();
                Application.Restart();
                Environment.Exit(0);

              //  dataGridView1.Rows.Clear();
              //  dataGridView1.Refresh();

                //swList.Clear();

               // midnightLabel.Visible = true;

            }


            try
            {
                if (inUseNotIdle == true)//only write to log file if not idle to prevent dropbox notification updates nonstop updating
                {

                   

                    //  MessageBox.Show("Test");
                    using (StreamWriter writer = new StreamWriter(pathLogFile + "\\" + dateCreated + ".csv"))
                    {
                        Application.DoEvents();
                        writer.Write(sb);

                        linkLabel7.Invoke((MethodInvoker)delegate {
                            linkLabel7.LinkColor = SystemColors.ControlText;
                            linkLabel7.Text = (@"Data above is auto saving to """ + dateCreated + @".csv""" + " in the log folder local to this application path click here to view.").ToString();
                        });


                        
                    }

              
                }

                else
                {

                }

            }
            catch (Exception)
            {
                linkLabel7.Invoke((MethodInvoker)delegate {
                    linkLabel7.LinkColor = Color.Red;
                    linkLabel7.Text = ("Error saving to log file; it may be be locked from some other process.  Dropbox etc can lock files temporarily when syncing.  I'll keep trying to write.  Ignore this message unless the log file is never written to.");
                });


              //  threeTriesWarningFileLock = threeTriesWarningFileLock + 1;



                //if (threeTriesWarningFileLock == 3)//try 3 times before popup in case there is a hiccup with syncing with dropbox etc
                //{
                 
                //    string message = "Time Tracker can't write to log file, it is probably opened or locked.  Here is the error: " + Environment.NewLine + Environment.NewLine + ex.ToString();
                //    string caption = "Error";
                //    MessageBoxButtons buttons = MessageBoxButtons.OK;
                //    DialogResult result;

                //    // Displays the MessageBox.mbox     

                //    result = MessageBox.Show(message, caption, buttons);





                //    if (result == System.Windows.Forms.DialogResult.OK)
                //    {


                //        threeTriesWarningFileLock = 0;

                //    }

                //}
                //if (singleWarningFileLock == false)
                //{
                //    singleWarningFileLock = true;
                //    string message = "Time Tracker can't write to log file, it is probably opened or locked.  Here is the error: " + Environment.NewLine + Environment.NewLine + ex.ToString();
                //    string caption = "Error";
                //    MessageBoxButtons buttons = MessageBoxButtons.OK;
                //    DialogResult result;

                //    // Displays the MessageBox.mbox     

                //    result = MessageBox.Show(message, caption, buttons);





                //    if (result == System.Windows.Forms.DialogResult.OK)
                //    {
                //        singleWarningFileLock = false;
                //    }

                //}
            }


       


            // dateAndLogTimer.Start();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        
        }

        private void backgroundWorkerFileImport_DoWork(object sender, DoWorkEventArgs e)
        {
          
            try
            {

                progressBar1.Invoke((MethodInvoker)delegate {

                    progressBar1.Show();
                    progressBar1.Maximum = tempFileNameStore.Count;
                });

               

               
                int progressCount = tempFileNameStore.Count;
                  int progressDivision = (100 / progressCount);
                int fileCount = 0;
               



                foreach (var tempFileLoopThrough in tempFileNameStore)
                {
              
                    Application.DoEvents();
                    fileCount++;

                    labelFilename.Invoke((MethodInvoker)delegate {

                        labelFilename.Text = "Opening CSV File " + fileCount + " of " + tempFileNameStore.Count;
                    });

                    progressBar1.Invoke((MethodInvoker)delegate {

                        progressBar1.Increment(1);
                    });


                    using (var sr = File.OpenText(tempFileLoopThrough))
                    {
                        Application.DoEvents();
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                           
                            try
                            {
                                line = line.Replace(@"""", "");

                                //MessageBox.Show(line);
                                if ((line.Contains("00:00:00") == false)&&(line.Contains(":") == true) && (line.Contains("Icon,Process ID,Form") == false))
                                {
                                    thisArray = line.Split(',');
                                    dt.Rows.Add(thisArray);
                                }



                            }
                            catch (Exception)
                            {
                                blankCellErrorTrackInt++;
                                blankCellErrorTrack = true;
                                if (blankCellErrorTrackInt <= 100)//only track 100 errors otherwise lags cvs importation
                                {
                                    errorMessage += string.Join(Environment.NewLine, thisArray);
                                }

                            }
                        }
                       
                    }



                }
                fileCount = 0;

                labelFilename.Invoke((MethodInvoker)delegate {

                    labelFilename.Text = "";
                });
                
                if (blankCellErrorTrack == true)
                {
                    MessageBox.Show("Error, could not import the following row(s), this usually means there is an extra comma or something, but this should not happen unless log file is manually edited as far as I know.  Here are some of the lines that did not import (I only show up until 100 errors to avoid system hanging)" + Environment.NewLine + Environment.NewLine + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);

                    // MessageBox.Show("Error, could not import the following row(s), this usually means there is an extra comma or something, but this should not happen unless log file is manually edited as far as I know.  Here are some of the lines that did not import (I only show up until 100 errors to avoid system hanging)" + Environment.NewLine + Environment.NewLine + errorMessage);
                    errorMessage = "";
                    blankCellErrorTrack = false;

                }
            }

            catch (IOException ex)
            {

                MessageBox.Show(ex.ToString());

                progressBarPanel.Visible = false;
                progressBarPanel.Enabled = false;
                progressBarPanel.SendToBack();
                progressBar1.Enabled = false;
                progressBar1.Visible = false;
                progressBar1.SendToBack();

                buttonsEnable();

            }
        }

        private void backgroundWorkerFileImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorkerFileImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {


            dataGridViewFilterView.Invoke((MethodInvoker)delegate {

                dataGridViewFilterView.DataSource = dt;
            });



            blankCellErrorTrack = false;

            if (thisArray != null)
            {
                Array.Clear(thisArray, 0, thisArray.Length);
            }

            //clear filenames to time doesn't double
            tempFileNameStore.Clear();

            labelFilename.ForeColor = Color.Blue;
            labelFilename.Text = "Processing...";




          //SaveToundoFilterView();





            buttonsEnable();
            addTime();

            if (filesWereOpened == true)
            {
                MessageBox.Show("Import complete", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information,
         MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                filesWereOpened = false;
            }


            progressBar1.Value = 0;
            // progressBar1.Hide();          
            progressBar1.Enabled = false;
            progressBar1.Visible = false;
            progressBar1.SendToBack();
            progressBarPanel.Visible = false;
            progressBarPanel.Enabled = false;
            // progressBarPanel.Visible = false;
            progressBarPanel.SendToBack();

            labelFilename.ForeColor = SystemColors.Control;
            labelFilename.Text = "";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            //dataGridViewFilterView.DataSource as DataTable).DefaultView.RowFilter =
              //string.Format("Name LIKE '{0}%' OR Name LIKE '% {0}%'", filterTextBoxKeep.Text);

            BindingSource bs1 = new BindingSource();
            bs1.DataSource = dataGridViewFilterView.DataSource;
           // bs1.Filter = "[Form ID] Like '%" + filterTextBoxKeep.Text + "%'";
            //bs.DataSource = bs.DataSource;
            dataGridViewFilterView.DataSource = bs1;
            addTime();
            // dataGridViewFilterView.DataSource

            //BindingSource bs = new BindingSource();
            //bs.DataSource = dataGridViewFilterView.DataSource;
            //bs.Filter = "[Form ID] Like '%" + filterTextBoxKeep.Text + "%'";
            //bs.DataSource = bs.DataSource; not needed
            //dataGridViewFilterView.DataSource = bs;

        }

        private void filterTextBoxKeep_TextChanged(object sender, EventArgs e)
        {

           
        }
        string combineSeparateFilters;
        private void filterFormIDButton_Click(object sender, EventArgs e)
        {
           


            buttonsDisable();
            labelUpdating.Text = "Updating...";

            try
            {
               
                Application.DoEvents();
                BindingSource bs = new BindingSource();
                bs.DataSource = dataGridViewFilterView.DataSource;
                //bs.Filter = String.Format(@"(Column1 = '{0}') AND (Column2 LIKE '%{1}%' OR Column3 LIKE '%{1}%' OR Column4 LIKE '%{1}%' OR Column5 LIKE '%{1}%' OR Column6 LIKE '%{1}%')", filterTextBoxKeep.Text);

                //works for one column sub string 

                combineSeparateFilters = "";
                //split filter up by commas to filter search by each separated value
                filterCommaSplit = filterTextBoxKeep.Text.Split(',');
                
                
                foreach (var separateFilters in filterCommaSplit)
                {
                    //M
                    combineSeparateFilters += "[Form ID] Like '%"+separateFilters+"%' AND ";

                  
                }
                


                //combine everything and remove last 5 lengths
                string separatefiltersCombined = combineSeparateFilters = combineSeparateFilters.Remove(combineSeparateFilters.Length - 5, 5);

               
                // bs.Filter = "[Form ID] Like '%excel%' AND [Form ID] Like '%book%'";

                bs.Filter = separatefiltersCombined;

             
                dataGridViewFilterView.DataSource = bs;

                

                addTime();
            }
            catch(Exception ex)
            {
                MessageBox.Show("error " + ex.ToString());
                buttonsEnable();
            }
            //////////////////////////////////////////////////////////
            // bs.Filter = "[Form ID] LIKE '{0}%' OR [Process ID] LIKE '{0}%'", filterTextBoxKeep.Text;
            //bs.Filter = "[Form ID] LIKE '{0}%' OR [Process ID] LIKE '% {0}%'"+ filterTextBoxKeep.Text;
            //   bs.DataSource = bs.DataSource;





            //  bs.Filter = String.Format("(Column1 = '{0}') AND (Column2 LIKE '%{1}%' OR Column3 LIKE '%{1}%' OR Column4 LIKE '%{1}%' OR Column5 LIKE '%{1}%' OR Column6 LIKE '%{1}%')", dropdown.Text, searchbox.Text);

            ////works to filter multiple columns but both have to start with text
            //Application.DoEvents();
            //(dataGridViewFilterView.DataSource as DataTable).DefaultView.RowFilter =
            //string.Format("[Form ID] LIKE '{0}%' OR [Process ID] LIKE '{0}%'", filterTextBoxKeep.Text);



            labelUpdating.Text = "";
            buttonsEnable();



            filterTextBoxKeep.Focus();
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            MessageBox.Show(@"Filter multiple strings separated by commas such as ""cat,hat"" (without the quotes) would show all rows containing both cat and hat.  Be careful not to add a space between words if a space is not needed.", "Info",
    MessageBoxButtons.OK, MessageBoxIcon.Information);

           
        }

        private void radioSystemIdle_CheckedChanged(object sender, EventArgs e)
        {
            labelMouseTime.Text = "";
        }

        private void filterTextBoxKeep_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                filterFormIDButton.PerformClick();
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {

            manualTimeEntry f3 = new manualTimeEntry(this);
            f3.StartPosition = FormStartPosition.CenterParent;
            f3.ShowDialog(this);
            //f3.Show();
        }
    }
    }

    

