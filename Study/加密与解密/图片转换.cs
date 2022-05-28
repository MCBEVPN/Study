using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.加密与解密
{
    public partial class 图片转换 : Form
    {
        public 图片转换()
        {
            InitializeComponent();
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("图片不存在。", "转换", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Bitmap bitmap = new Bitmap(Image.FromFile(textBox1.Text));
                ConvertImage(textBox1.Text, comboBox1.Text, bitmap);
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "转换", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ConvertImage(string dir, string filter, Bitmap bmp)
        {
            string filePath = $"{Path.GetDirectoryName(dir)}\\{Path.GetFileNameWithoutExtension(dir)}.{filter.ToLower()}";
            switch (filter)
            {
                case "JPG":
                    bmp.Save(filePath, ImageFormat.Jpeg);
                    break;
                case "PNG":
                    bmp.Save(filePath, ImageFormat.Png);
                    break;
                case "GIF":
                    bmp.Save(filePath, ImageFormat.Gif);
                    break;
                case "BMP":
                    bmp.Save(filePath, ImageFormat.Bmp);
                    break;
                case "TIFF":
                    bmp.Save(filePath, ImageFormat.Tiff);
                    break;
                case "WMF":
                    bmp.Save(filePath, ImageFormat.Wmf);
                    break;
                case "EXIF":
                    bmp.Save(filePath, ImageFormat.Exif);
                    break;
                case "ICO":
                    using (Stream stream = File.Create(filePath))
                    {
                        Icon icon = Icon.FromHandle(bmp.GetHicon());
                        icon.Save(stream);
                        stream.Close();
                        icon.Dispose();
                    }
                    break;
            }
            bmp.Dispose();
            MessageBox.Show($"已成功转换到\n{filePath}", "转换");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Filter = "图片|*.png;*.jpeg;*.jpg;*.gif;*.bmp;*.tiff;*.wmf;*.exif;*.ico",
                Title = "文件",
                SupportMultiDottedExtensions = false
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.FileName;
            }
        }
    }
}
