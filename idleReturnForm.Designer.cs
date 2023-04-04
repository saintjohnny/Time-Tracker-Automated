namespace TimeTracker
{
    partial class idleReturnForm
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
            this.idleAwayLabel = new System.Windows.Forms.Label();
            this.idleAwayLogTimeTextBox = new System.Windows.Forms.TextBox();
            this.idleAwayLogTimeButton = new System.Windows.Forms.Button();
            this.logIdleTimeCancelButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // idleAwayLabel
            // 
            this.idleAwayLabel.Location = new System.Drawing.Point(12, 9);
            this.idleAwayLabel.Name = "idleAwayLabel";
            this.idleAwayLabel.Size = new System.Drawing.Size(228, 70);
            this.idleAwayLabel.TabIndex = 1;
            this.idleAwayLabel.Text = "Enter the task and press \"Log Time\"";
            // 
            // idleAwayLogTimeTextBox
            // 
            this.idleAwayLogTimeTextBox.Location = new System.Drawing.Point(12, 82);
            this.idleAwayLogTimeTextBox.Name = "idleAwayLogTimeTextBox";
            this.idleAwayLogTimeTextBox.Size = new System.Drawing.Size(226, 20);
            this.idleAwayLogTimeTextBox.TabIndex = 2;
            // 
            // idleAwayLogTimeButton
            // 
            this.idleAwayLogTimeButton.Location = new System.Drawing.Point(12, 108);
            this.idleAwayLogTimeButton.Name = "idleAwayLogTimeButton";
            this.idleAwayLogTimeButton.Size = new System.Drawing.Size(110, 23);
            this.idleAwayLogTimeButton.TabIndex = 3;
            this.idleAwayLogTimeButton.Text = "Log Time";
            this.idleAwayLogTimeButton.UseVisualStyleBackColor = true;
            this.idleAwayLogTimeButton.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // logIdleTimeCancelButton
            // 
            this.logIdleTimeCancelButton.Location = new System.Drawing.Point(128, 108);
            this.logIdleTimeCancelButton.Name = "logIdleTimeCancelButton";
            this.logIdleTimeCancelButton.Size = new System.Drawing.Size(110, 23);
            this.logIdleTimeCancelButton.TabIndex = 4;
            this.logIdleTimeCancelButton.Text = "Cancel";
            this.logIdleTimeCancelButton.UseVisualStyleBackColor = true;
            this.logIdleTimeCancelButton.Click += new System.EventHandler(this.logIdleTimeCancelButton_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 137);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(189, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Don\'t show these popups anymore";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // idleReturnForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 161);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.logIdleTimeCancelButton);
            this.Controls.Add(this.idleAwayLogTimeButton);
            this.Controls.Add(this.idleAwayLogTimeTextBox);
            this.Controls.Add(this.idleAwayLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "idleReturnForm";
            this.Text = "Welcome Back";
            this.Load += new System.EventHandler(this.idleReturnForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label idleAwayLabel;
        private System.Windows.Forms.TextBox idleAwayLogTimeTextBox;
        private System.Windows.Forms.Button idleAwayLogTimeButton;
        private System.Windows.Forms.Button logIdleTimeCancelButton;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}