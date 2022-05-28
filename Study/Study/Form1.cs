using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Study.Learning
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            foreach (Control item in this.Controls)
            {
                if (item.GetType() != typeof(Button) && item.GetType() != typeof(TextBox) && item.GetType() != typeof(ComboBox) && item.GetType() != typeof(MenuStrip))
                {
                    ((Control)item).ForeColor = Color.White;
                    ((Control)item).BackColor = Color.Transparent;
                }
            }
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26RAv0NfiitOJ_hWbs9E1qA1?pwd=49xg");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26RLKhydVznfSZERv2d-qyA1?pwd=qwcc");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26RRPp_AYaORchT9CUQWe_A1?pwd=ibxt");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26Rfa8OdIQlQCp339jSwP_A1?pwd=zv93");
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("制        作：           特别中二的Jal君\n\n资源提供：           Hacker Over信\n\n资源审核：           黑客零", "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26RntUydVznfSZERv2d8nVA1?pwd=tz7i");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26RxKK-Dxux0XnOZAixLfGA1?pwd=nzia");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26S2gYOdIQlQCp339jT1O-A1?pwd=uu4w");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Process.Start("https://pan.xunlei.com/s/VN26S8KrcfZub4LjtknGC-6AA1?pwd=a7vj");
        }
    }
}
