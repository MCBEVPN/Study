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
using Study.爬虫;

namespace Study.工具
{
    public partial class 选项 : Form
    {
        private ToolTip toolTip = new ToolTip();
        private List<Panel> panelList = new List<Panel>();
        public 选项()
        {
            InitializeComponent();
            panelList.Add(panel1);
            panelList.Add(panel2);
            panelList.Add(panel3);
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            #region 背景
            this.radioButton2.Checked = true;
            this.radioButton1.CheckedChanged += R_CheckedChanged;
            this.radioButton2.CheckedChanged += R_CheckedChanged;
            this.radioButton3.CheckedChanged += R_CheckedChanged;
            #endregion
            #region 宠物
            comboBox1.SelectedIndex = 0;
            #endregion
            #region 动态壁纸
            if (Program.BackgroundJsonSetting.DWPath.Trim().StartsWith("无"))
            {
                comboBox2.SelectedIndex = 0;
            }
            else
            {
                comboBox2.Text = Program.BackgroundJsonSetting.DWPath;
            }
            trackBar1.Value = (int)(Program.BackgroundJsonSetting.DWVolume * 100);
            #endregion
            this.treeView1.Nodes.Add("主题");
            this.treeView1.Nodes[0].Nodes.Add(new TreeNode("背景"));
            this.treeView1.Nodes[0].Nodes.Add(new TreeNode("桌面宠物"));
            this.treeView1.Nodes[0].Nodes.Add(new TreeNode("动态壁纸"));
            this.treeView1.ExpandAll();
        }

        private void R_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = radioButton3.Checked;
            button3.Enabled = radioButton3.Checked;
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode treeNode = this.treeView1.SelectedNode;
            textBox1.Text = Program.BackgroundJsonSetting.BackgroundPath;
            comboBox1.SelectedIndex = Program.BackgroundJsonSetting.PetMode;
            textBox1.Enabled = false;
            button3.Enabled = false;
            switch (treeNode.Text)
            {
                case "背景":
                    switch (Program.BackgroundJsonSetting.BackgroundMode)
                    {
                        case 0:
                            radioButton1.Checked = true;
                            break;
                        case 1:
                            radioButton2.Checked = true;
                            break;
                        case 2:
                            radioButton3.Checked = true;
                            textBox1.Enabled = true;
                            button3.Enabled = true;
                            break;
                        default:
                            radioButton2.Checked = true;
                            break;
                    }
                    PanelsSetVisible(panel1);
                    break;
                case "桌面宠物":
                    PanelsSetVisible(panel2);
                    break;
                case "动态壁纸":
                    PanelsSetVisible(panel3);
                    break;
                default:
                    PanelsSetVisible();
                    break;
            }
        }

        private void PanelsSetVisible(Panel panel = null)
        {
            PanelWhere(panel);
        }
        private void PanelWhere(Panel panel)
        {
            if (panel == null)
            {
                foreach (Panel item in panelList)
                {
                    item.Visible = false;
                }
                return;
            }
            foreach (Panel item in panelList)
            {
                if (panel == item)
                {
                    item.Visible = true;
                }
                else
                {
                    item.Visible = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BackgroundJson backgroundJson = new BackgroundJson();
            #region 背景
            if (radioButton1.Checked)
            {
                backgroundJson.BackgroundMode = 0;
            }
            else if (radioButton2.Checked)
            {
                backgroundJson.BackgroundMode = 1;
            }
            else if (radioButton3.Checked)
            {
                backgroundJson.BackgroundMode = 2;
            }
            backgroundJson.BackgroundPath = textBox1.Text.Trim();
            Program.BackgroundJsonSetting = backgroundJson;
            #endregion
            #region 宠物
            backgroundJson.PetMode = comboBox1.SelectedIndex;
            #endregion
            #region 动态壁纸
            backgroundJson.DWPath = comboBox2.Text;
            backgroundJson.DWVolume = trackBar1.Value / 100;
            #endregion
            File.WriteAllText(Path.GetTempPath() + "Study\\Settings.json", Json<BackgroundJson>.WriteJson(backgroundJson));
            MessageBox.Show("需重启程序生效。", "选项", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Filter = "图片|*.png;*.jpeg;*.jpg;*.gif;*.bmp;*.tiff;*.wmf;*.exif;*.ico",
                Title = "浏览",
                SupportMultiDottedExtensions = false
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Filter = "视频|*.mp4;*.mov",
                Title = "浏览",
                SupportMultiDottedExtensions = false
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                comboBox2.Text = fileDialog.FileName;
            }
        }

        private bool isMouseDown = false;
        private void trackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                toolTip.SetToolTip(trackBar1, trackBar1.Value.ToString());
            }
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            toolTip.SetToolTip(trackBar1, trackBar1.Value.ToString());
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            toolTip.RemoveAll();
        }
    }
    public class BackgroundJson
    {
        public int BackgroundMode = 0;
        public string BackgroundPath = "";
        public int PetMode = 0;
        public string DWPath = "无";
        public double DWVolume = 0.4;
    }
}
