using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.加密与解密
{
    public partial class Form1 : Form
    {
        public Form1()
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
            comboBox1.SelectedIndex = 0;
        }
        private string path = "";
        /// <summary>
        /// 加载按钮
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(textBox1.Text))
                {
                    using (Stream stream = streamReader.BaseStream)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        textBox4.Text = Encoding.UTF8.GetString(memoryStream.ToArray());
                        stream.Close();
                        memoryStream.Close();
                        memoryStream.Dispose();
                        streamReader.Close();
                        path = textBox1.Text.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                textBox4.Text = ex.Message;
                return;
            }
        }
        /// <summary>
        /// 解密按钮
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            string from = "";
            try
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        from = Encoding.UTF8.GetString(Convert.FromBase64String(textBox2.Text));
                        break;
                    case 1:
                        byte[] to = new byte[textBox2.Text.Split(' ').Length];
                        for (int i = 0; i < textBox2.Text.Split(' ').Length; i++)
                        {
                            Application.DoEvents();
                            byte.TryParse(textBox2.Text.Split(' ')[i], out to[i]);
                        }
                        from = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Encoding.ASCII.GetString(to)));
                        break;
                    case 2:
                        from = Encoding.UTF8.GetString(Encoding.Unicode.GetBytes(textBox2.Text));
                        break;
                }
            }
            catch (Exception ex)
            {
                textBox3.Text = ex.Message;
                return;
            }
            textBox3.Text = from;
        }
        /// <summary>
        /// 浏览文件按钮
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "(所有文件)|*.*";
            fileDialog.Title = "文件";
            fileDialog.SupportMultiDottedExtensions = false;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.FileName;
            }
        }
        /// <summary>
        /// 加密按钮
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text.Replace(" ", ""))) return;
            if (!File.Exists(textBox4.Text))
            {
                MessageBox.Show("无法找到文件", "应用", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确定应用吗？应用后无法撤回。", "应用", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                if (File.Exists(path))
                {
                    File.WriteAllText(path, textBox2.Text);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StringBuilder to = new StringBuilder();
            try
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        to.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(textBox4.Text)));
                        break;
                    case 1:
                        byte[] array = Encoding.ASCII.GetBytes(textBox4.Text);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (byte item in array)
                        {
                            stringBuilder.Append(Convert.ToString(item) + " ");
                        }
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        to.Append(stringBuilder.ToString());
                        byte[] from = new byte[stringBuilder.ToString().Split(' ').Length];
                        for (int i = 0; i < stringBuilder.ToString().Split(' ').Length; i++)
                        {
                            Application.DoEvents();
                            from[i] = byte.Parse(stringBuilder.ToString().Split(' ')[i]);
                        }
                        break;
                    case 2:
                        to.Append(Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(textBox4.Text)));
                        break;
                }
            }
            catch (Exception ex)
            {
                textBox2.Text = ex.Message;
                return;
            }
            if (to.Length > textBox2.MaxLength)
            {
                Clipboard.SetText(to.ToString());
                MessageBox.Show("由于数据过大，已自动复制到剪切板。", "数据过载");
            }
            else
            {
                Application.DoEvents();
                textBox2.Text = to.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text)) return;
            StringBuilder to = new StringBuilder();
            try
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        to.Append(Convert.ToBase64String(File.ReadAllBytes(textBox1.Text)));
                        break;
                    case 1:
                        byte[] array = Encoding.ASCII.GetBytes(Encoding.UTF8.GetString(File.ReadAllBytes(textBox1.Text)));
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (byte item in array)
                        {
                            stringBuilder.Append(Convert.ToString(item) + " ");
                        }
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        to.Append(stringBuilder.ToString());
                        byte[] from = new byte[stringBuilder.ToString().Split(' ').Length];
                        for (int i = 0; i < stringBuilder.ToString().Split(' ').Length; i++)
                        {
                            Application.DoEvents();
                            from[i] = byte.Parse(stringBuilder.ToString().Split(' ')[i]);
                        }
                        break;
                    case 2:
                        to.Append(Encoding.Unicode.GetString(File.ReadAllBytes(textBox1.Text)));
                        break;
                }
            }
            catch (Exception ex)
            {
                textBox2.Text = ex.Message;
                return;
            }
            if (to.Length > textBox2.MaxLength)
            {
                Clipboard.SetText(to.ToString());
                MessageBox.Show("由于数据过大，已自动复制到剪切板。", "数据过载");
            }
            else
            {
                Application.DoEvents();
                textBox2.Text = to.ToString();
            }
        }
    }
}
