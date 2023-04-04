namespace TimeTracker
{
    partial class manualTimeEntry
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
            this.numericUpDownCustomTime = new System.Windows.Forms.NumericUpDown();
            this.logIdleTimeCancelButton = new System.Windows.Forms.Button();
            this.idleAwayLogTimeButton = new System.Windows.Forms.Button();
            this.manualTimeEntryTextBox = new System.Windows.Forms.TextBox();
            this.idleAwayLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCustomTime)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Time in minutes";
            // 
            // numericUpDownCustomTime
            // 
            this.numericUpDownCustomTime.Location = new System.Drawing.Point(15, 56);
            this.numericUpDownCustomTime.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.numericUpDownCustomTime.Name = "numericUpDownCustomTime";
            this.numericUpDownCustomTime.Size = new System.Drawing.Size(73, 20);
            this.numericUpDownCustomTime.TabIndex = 11;
            this.numericUpDownCustomTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericUpDownCustomTime_KeyPress);
            // 
            // logIdleTimeCancelButton
            // 
            this.logIdleTimeCancelButton.Location = new System.Drawing.Point(128, 108);
            this.logIdleTimeCancelButton.Name = "logIdleTimeCancelButton";
            this.logIdleTimeCancelButton.Size = new System.Drawing.Size(110, 23);
            this.logIdleTimeCancelButton.TabIndex = 10;
            this.logIdleTimeCancelButton.Text = "Cancel";
            this.logIdleTimeCancelButton.UseVisualStyleBackColor = true;
            this.logIdleTimeCancelButton.Click += new System.EventHandler(this.logIdleTimeCancelButton_Click);
            // 
            // idleAwayLogTimeButton
            // 
            this.idleAwayLogTimeButton.Location = new System.Drawing.Point(12, 108);
            this.idleAwayLogTimeButton.Name = "idleAwayLogTimeButton";
            this.idleAwayLogTimeButton.Size = new System.Drawing.Size(110, 23);
            this.idleAwayLogTimeButton.TabIndex = 9;
            this.idleAwayLogTimeButton.Text = "Log Time";
            this.idleAwayLogTimeButton.UseVisualStyleBackColor = true;
            this.idleAwayLogTimeButton.Click += new System.EventHandler(this.idleAwayLogTimeButton_Click);
            // 
            // manualTimeEntryTextBox
            // 
            this.manualTimeEntryTextBox.Location = new System.Drawing.Point(12, 82);
            this.manualTimeEntryTextBox.Name = "manualTimeEntryTextBox";
            this.manualTimeEntryTextBox.Size = new System.Drawing.Size(226, 20);
            this.manualTimeEntryTextBox.TabIndex = 8;
            // 
            // idleAwayLabel
            // 
            this.idleAwayLabel.Location = new System.Drawing.Point(12, 9);
            this.idleAwayLabel.Name = "idleAwayLabel";
            this.idleAwayLabel.Size = new System.Drawing.Size(228, 70);
            this.idleAwayLabel.TabIndex = 7;
            this.idleAwayLabel.Text = "Enter how long in minutes, and the task in the box below and press \"Log Time\"";
            // 
            // manualTimeEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(253, 143);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownCustomTime);
            this.Controls.Add(this.logIdleTimeCancelButton);
            this.Controls.Add(this.idleAwayLogTimeButton);
            this.Controls.Add(this.manualTimeEntryTextBox);
            this.Controls.Add(this.idleAwayLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "manualTimeEntry";
            this.Text = "Manual Time Entry";
            this.Load += new System.EventHandler(this.manualTimeEntry_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCustomTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownCustomTime;
        private System.Windows.Forms.Button logIdleTimeCancelButton;
        private System.Windows.Forms.Button idleAwayLogTimeButton;
        private System.Windows.Forms.TextBox manualTimeEntryTextBox;
        private System.Windows.Forms.Label idleAwayLabel;
    }
}