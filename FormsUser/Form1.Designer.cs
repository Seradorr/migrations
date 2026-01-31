namespace FormsUser {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.buttonMigrate = new System.Windows.Forms.Button();
            this.textBoxXprFile = new System.Windows.Forms.TextBox();
            this.textBoxTargetDir = new System.Windows.Forms.TextBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonMigrate
            // 
            this.buttonMigrate.Location = new System.Drawing.Point(12, 76);
            this.buttonMigrate.Name = "buttonMigrate";
            this.buttonMigrate.Size = new System.Drawing.Size(75, 23);
            this.buttonMigrate.TabIndex = 2;
            this.buttonMigrate.Text = "Migrate";
            this.buttonMigrate.UseVisualStyleBackColor = true;
            this.buttonMigrate.Click += new System.EventHandler(this.buttonMigrate_Click);
            // 
            // textBoxXprFile
            // 
            this.textBoxXprFile.Location = new System.Drawing.Point(100, 13);
            this.textBoxXprFile.Name = "textBoxXprFile";
            this.textBoxXprFile.Size = new System.Drawing.Size(304, 20);
            this.textBoxXprFile.TabIndex = 0;
            this.textBoxXprFile.Tag = "XprFile";
            // 
            // textBoxTargetDir
            // 
            this.textBoxTargetDir.AcceptsTab = true;
            this.textBoxTargetDir.Location = new System.Drawing.Point(100, 40);
            this.textBoxTargetDir.Name = "textBoxTargetDir";
            this.textBoxTargetDir.Size = new System.Drawing.Size(304, 20);
            this.textBoxTargetDir.TabIndex = 1;
            this.textBoxTargetDir.Tag = "TargetDir";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Location = new System.Drawing.Point(13, 119);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(391, 255);
            this.textBoxLog.TabIndex = 3;
            this.textBoxLog.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source xpr file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Target directory";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 386);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.textBoxTargetDir);
            this.Controls.Add(this.textBoxXprFile);
            this.Controls.Add(this.buttonMigrate);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonMigrate;
        private System.Windows.Forms.TextBox textBoxXprFile;
        private System.Windows.Forms.TextBox textBoxTargetDir;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

