using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Migrations;

namespace FormsUser {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void buttonMigrate_Click(object sender, EventArgs e) {
            MigrationVivado m = new MigrationVivado();
            m.writeLog = textBoxLog.AppendText;
            m.projectFileFA = textBoxXprFile.Text;
            m.targetDir = textBoxTargetDir.Text;

            m.MigrateProject();
        }
    }
}
