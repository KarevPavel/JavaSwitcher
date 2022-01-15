using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JavaSwapper
{
    public partial class Form1 : Form
    {
        private static readonly string FILE_NAME = "TMP_JDK_LIST.txt";
        private static readonly string ABSOLUTE_FILE_PATH = System.IO.Path.GetTempPath() + FILE_NAME;
        private static readonly string JAVA_HOME = "JAVA_HOME";
        private static readonly string PATH = "Path";
        public static Dictionary<string, string> jdkKvp = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            loadDictionaryFromFile();
            FUCK_C_SHARP();
        }

        private void loadDictionaryFromFile()
        {
            if (System.IO.File.Exists(ABSOLUTE_FILE_PATH))
            {
                string[] jdkLines = System.IO.File.ReadAllLines(ABSOLUTE_FILE_PATH);
                foreach (string jdkLine in jdkLines)
                {
                    var regex = new System.Text.RegularExpressions.Regex("\"(.*)\"\\:\"(.*)\"");
                    var match = regex.Match(jdkLine);
                    if (match.Success)
                    {
                        jdkKvp.Add(match.Groups[1].Value, match.Groups[2].Value);
                    }
                }
            }
        }

        private void FUCK_C_SHARP()
        {
            this.dataGridView1.CellClick += this.CellClick;
            this.Resize += Form1_Resize;
            this.gridBtn.Text = "Удалить";
            this.gridBtn.UseColumnTextForButtonValue = false;

            foreach (var kv in jdkKvp)
            {
                this.insertValueToDG(kv.Key, kv.Value);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon1.ContextMenu = new System.Windows.Forms.ContextMenu();
                this.notifyIcon1.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Развернуть",
                    new System.EventHandler((object objc, System.EventArgs args) => { this.WindowState = System.Windows.Forms.FormWindowState.Normal; })));

                foreach (var kvp in JavaSwapper.Form1.jdkKvp)
                {
                    this.notifyIcon1.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(kvp.Key,
                         new System.EventHandler((object objc, System.EventArgs args) => {
                             var currentJavaHome = System.Environment.GetEnvironmentVariable(JAVA_HOME, EnvironmentVariableTarget.Machine);
                             var newJavaHome = kvp.Value;
                             var currentPath = System.Environment.GetEnvironmentVariable(PATH, EnvironmentVariableTarget.Machine);
                             var newPath = currentPath.Replace(String.Format("{0}\\bin;", currentJavaHome), String.Format("{0}\\bin;", newJavaHome));
                             System.Environment.SetEnvironmentVariable(PATH, newPath, System.EnvironmentVariableTarget.Machine);
                             System.Environment.SetEnvironmentVariable(JAVA_HOME, newJavaHome, System.EnvironmentVariableTarget.Machine);
                         })));
                }
            }
        }

        private string javaVersion(string jdkPath)
        {
            var cmd = new System.Diagnostics.Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = false;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();
            cmd.StandardInput.WriteLine(String.Format("\"{0}\\bin\\java\" -version", jdkPath));
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            string result = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            var regex = new System.Text.RegularExpressions.Regex("(\\w+)\\sversion\\s\"(.*)\"");
            var match = regex.Match(result);
            if (match.Success)
            {
                var JdkName = match.Groups[1].Value;
                var firstElement = JdkName.ElementAt(0);
                JdkName = JdkName.Replace(firstElement, Char.ToUpper(firstElement));
                var jdkVersion = match.Groups[2].Value;
                return String.Format("{0} {1}", JdkName, jdkVersion);
            }
            return "idianahooi;";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialogResult = this.folderBrowserDialog1.ShowDialog();
            dialogResult.ToString();
            if (dialogResult == DialogResult.OK)
            {
                var jdkPath = this.folderBrowserDialog1.SelectedPath;
                var shortName = this.javaVersion(jdkPath);
                jdkKvp.Add(shortName, jdkPath);
                this.insertValueToDG(shortName, jdkPath);
            }
        }

        private void saveToFile()
        {
            var fileName = System.IO.Path.GetTempPath() + FILE_NAME;
            List<string> content = new List<string>();
            foreach (var kvp in jdkKvp)
            {
                content.Add(String.Format("\"{0}\":\"{1}\"", kvp.Key, kvp.Value));
            }
            System.IO.File.WriteAllLines(fileName, content);
        }

        private void insertValueToDG(string shortName, string jdkPath)
        {
            var rowIndex = this.dataGridView1.Rows.Count;
            this.dataGridView1.Rows.Insert(rowIndex, shortName, jdkPath, "Удалить");
        }

        private void CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.dataGridView1.Columns.Count - 1)
            {
                jdkKvp.Remove(this.dataGridView1.Rows[e.RowIndex].Cells["shortName"].Value.ToString());
                this.dataGridView1.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (jdkKvp.Count > 0)
            {
                saveToFile();
            }
        }

        private void button4_MouseClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}