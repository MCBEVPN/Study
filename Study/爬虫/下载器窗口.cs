using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.爬虫
{
    public partial class 下载器窗口 : Form
    {
        public 下载器窗口(int max)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            foreach (Control item in this.Controls)
            {
                try
                {
                    if (item.GetType() != typeof(Button) && item.GetType() != typeof(TextBox) && item.GetType() != typeof(ComboBox))
                    {
                        ((Control)item).ForeColor = Color.White;
                        ((Control)item).BackColor = Color.Transparent;
                    }
                }
                catch { }
            }
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            CheckForIllegalCrossThreadCalls = false;
            base.Text = "下载";
            label1.Text = $"正在下载到：{Program.DownloadPath}";
            progressBar1.Maximum = max;
        }
        public int Value
        {
            get
            {
                return progressBar1.Value;
            }
            set
            {
                progressBar1.Value = value;
                PerformStep();
            }
        }
        private void PerformStep()
        {
            progressBar1.PerformStep();
        }
        public new string Text
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }
        public string SpeedText
        {
            get
            {
                return label2.Text;
            }
            set
            {
                label2.Text = value;
            }
        }
    }
}
