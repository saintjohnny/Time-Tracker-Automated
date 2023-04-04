namespace TimeTracker
{
    partial class pomodoroTimer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.resetTimerButton = new System.Windows.Forms.Button();
            this.setBreakTimerButton = new System.Windows.Forms.Button();
            this.closePomodoroButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(263, 63);
            this.label1.TabIndex = 0;
            this.label1.Text = "Time\'s up!  Your x minute timer has finished.  Take a break! Make sure to stand u" +
    "p and stretch for 5 min or so :)  Would you like to set another timer now?";
            // 
            // resetTimerButton
            // 
            this.resetTimerButton.Location = new System.Drawing.Point(12, 104);
            this.resetTimerButton.Name = "resetTimerButton";
            this.resetTimerButton.Size = new System.Drawing.Size(260, 23);
            this.resetTimerButton.TabIndex = 1;
            this.resetTimerButton.Text = "Reset Timer";
            this.resetTimerButton.UseVisualStyleBackColor = true;
            this.resetTimerButton.Click += new System.EventHandler(this.resetTimerButton_Click);
            // 
            // setBreakTimerButton
            // 
            this.setBreakTimerButton.Location = new System.Drawing.Point(12, 75);
            this.setBreakTimerButton.Name = "setBreakTimerButton";
            this.setBreakTimerButton.Size = new System.Drawing.Size(126, 23);
            this.setBreakTimerButton.TabIndex = 2;
            this.setBreakTimerButton.Text = "5 Min Break";
            this.setBreakTimerButton.UseVisualStyleBackColor = true;
            this.setBreakTimerButton.Click += new System.EventHandler(this.setBreakTimerButton_Click);
            // 
            // closePomodoroButton
            // 
            this.closePomodoroButton.Location = new System.Drawing.Point(146, 75);
            this.closePomodoroButton.Name = "closePomodoroButton";
            this.closePomodoroButton.Size = new System.Drawing.Size(126, 23);
            this.closePomodoroButton.TabIndex = 3;
            this.closePomodoroButton.Text = "Close";
            this.closePomodoroButton.UseVisualStyleBackColor = true;
            this.closePomodoroButton.Click += new System.EventHandler(this.closePomodoroButton_Click);
            // 
            // pomodoroTimer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 136);
            this.Controls.Add(this.closePomodoroButton);
            this.Controls.Add(this.setBreakTimerButton);
            this.Controls.Add(this.resetTimerButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "pomodoroTimer";
            this.Text = "Pomodoro Timer";
            this.Load += new System.EventHandler(this.pomodoroTimer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button resetTimerButton;
        private System.Windows.Forms.Button setBreakTimerButton;
        private System.Windows.Forms.Button closePomodoroButton;
    }
}