using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace renderXdemo
{
    public partial class displaySize : Form
    {
        public displaySize()
        {
            InitializeComponent();
        }

        private void displaySize_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            appTerminated = true;
            Application.Exit();
        }

        public static int W;
        public static int H;
        public static bool fScreen;
        public static bool appTerminated = false;
        bool ov = false;

        private void button1_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out W) | !int.TryParse(textBox2.Text, out H)){
                MessageBox.Show("Please Input Valid Resolution");
            }
            else{
                fScreen = checkBox1.Checked;
                ov = true;
                this.Close();
            }

        }

        private void displaySize_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ov)
            {
                appTerminated = true;
            }
        }
    }
}
