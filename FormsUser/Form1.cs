using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Migrations;

namespace FormsUser {
    public partial class Form1 : Form {
        private Thread migrationThread;
        
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void buttonBrowseXpr_Click(object sender, EventArgs e) {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Vivado Project Files (*.xpr)|*.xpr|All Files (*.*)|*.*";
                ofd.Title = "Select Vivado Project File";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    textBoxXprFile.Text = ofd.FileName;
                }
            }
        }

        private void buttonBrowseTarget_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                fbd.Description = "Select Target Directory";
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.OK) {
                    textBoxTargetDir.Text = fbd.SelectedPath;
                }
            }
        }

        private void AppendLog(string text) {
            if (textBoxLog.InvokeRequired) {
                textBoxLog.Invoke(new Action<string>(AppendLog), text);
            } else {
                textBoxLog.AppendText(text);
            }
        }

        private void SetButtonEnabled(bool enabled) {
            if (buttonMigrate.InvokeRequired) {
                buttonMigrate.Invoke(new Action<bool>(SetButtonEnabled), enabled);
            } else {
                buttonMigrate.Enabled = enabled;
            }
        }

        private void ShowResult(int result) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<int>(ShowResult), result);
            } else {
                if (result == 0) {
                    MessageBox.Show("Migration completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    MessageBox.Show("Migration completed with errors. Check the log for details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowError(string message) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<string>(ShowError), message);
            } else {
                MessageBox.Show("Migration failed: " + message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonMigrate_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(textBoxXprFile.Text)) {
                MessageBox.Show("Please select a source XPR file.", "Missing Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxTargetDir.Text)) {
                MessageBox.Show("Please select a target directory.", "Missing Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            textBoxLog.Clear();
            buttonMigrate.Enabled = false;

            string xprFile = textBoxXprFile.Text;
            string targetDir = textBoxTargetDir.Text;

            migrationThread = new Thread(() => {
                try {
                    MigrationVivado m = new MigrationVivado();
                    m.writeLog = AppendLog;
                    m.projectFileFA = xprFile;
                    m.targetDir = targetDir;
                    m.createGitIgnore = true;

                    int result = m.MigrateProject();
                    ShowResult(result);
                }
                catch (Exception ex) {
                    AppendLog("ERROR: " + ex.Message + Environment.NewLine);
                    ShowError(ex.Message);
                }
                finally {
                    SetButtonEnabled(true);
                }
            });

            migrationThread.IsBackground = true;
            migrationThread.Start();
        }
    }
}
