using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Study.防撤回
{
    public partial class Form1 : Form
    {
        // 当前使用的修改者
        private AppModifier modifier = null;
        private WechatModifier wechatModifier = null;
        private QQModifier qqModifier = null;
        private TIMModifier timModifier = null;
        private QQLiteModifier qqLiteModifier = null;
        private readonly GAHelper ga = GAHelper.Instance; // Google Analytics 记录
        public Form1()
        {
            InitializeComponent();
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            foreach (var item in this.Controls)
            {
                if (item is RadioButton)
                {
                    ((RadioButton)item).BackColor = Color.Transparent;
                    ((RadioButton)item).ForeColor = Color.White;
                }
                else if (item is Label)
                {
                    ((Label)item).BackColor = Color.Transparent;
                    ((Label)item).ForeColor = Color.White;
                }
            }
            Initialize();
            InitializeControls();
        }
        private void Initialize()
        {
            // 从配置文件中读取配置
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Bag bag = serializer.Deserialize<Bag>(Properties.Resources.PatchJson);
            // 初始化每个应用对应的修改者
            wechatModifier = new WechatModifier(bag.Apps["Wechat"]);
            qqModifier = new QQModifier(bag.Apps["QQ"]);
            timModifier = new TIMModifier(bag.Apps["TIM"]);
            qqLiteModifier = new QQLiteModifier(bag.Apps["QQLite"]);

            radioButton1.Tag = wechatModifier;
            radioButton2.Tag = qqModifier;
            radioButton3.Tag = timModifier;
            radioButton4.Tag = qqLiteModifier;

            // 默认微信
            radioButton1.Enabled = true;
            modifier = wechatModifier;
        }
        private void InitializeControls()
        {
            // 自动获取应用安装路径
            textBox1.Text = modifier.FindInstallPath();
            // 显示是否能够备份还原、版本和功能
            InitEditorsAndUI(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!modifier.IsAllFilesExist(textBox1.Text))
            {
                MessageBox.Show("请选择正确的安装路径!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 记录点了什么应用的防撤回
            string enName = GetCheckedRadioButtonNameEn(); // 应用英文名
            string version = modifier.GetVersion(); // 应用版本
            ga.RequestPageView($"{enName}/{version}/patch", "点击防撤回");
            EnableAllButton(false);
            // a.重新初始化编辑器
            modifier.InitEditors(textBox1.Text);
            // b.获取选择的功能 （精准匹配返回null） // TODO 此处逻辑可以优化 不可完全信任UI信息
            List<string> categories = UIController.GetCategoriesFromPanel(flowLayoutPanel1);
            if (categories != null && categories.Count == 0)
            {
                MessageBox.Show("请至少选择一项功能");
                EnableAllButton(true);
                button3.Enabled = modifier.BackupExists();
                return;
            }
            // c.计算SHA1，验证文件完整性，寻找对应的补丁信息（精确版本、通用特征码两种补丁信息）
            try
            {
                modifier.ValidateAndFindModifyInfo(categories);
            }
            catch (BusinessException ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.ErrorCode}", ex.Message);
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableAllButton(true);
                button3.Enabled = modifier.BackupExists();
                return;
            }
            catch (IOException ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.HResult:x4}", ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认当前应用（微信/QQ/TIM）处于关闭状态。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableAllButton(true);
                button3.Enabled = modifier.BackupExists();
                return;
            }
            catch (Exception ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/sha1/ex/{ex.HResult:x4}", ex.Message);
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableAllButton(true);
                button3.Enabled = modifier.BackupExists();
                return;
            }
            try
            {
                modifier.Patch();
                ga.RequestPageView($"{enName}/{version}/patch/succ", "补丁安装成功。");
                MessageBox.Show("安装补丁成功。", "安装补丁");
            }
            catch (BusinessException ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/ex/{ex.ErrorCode}", ex.Message);
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                ga.RequestPageView($"{enName}/{version}/patch/ex/{ex.HResult.ToString("x4")}", ex.Message);
                MessageBox.Show(ex.Message + " 请以管理员权限启动本程序，并确认当前应用（微信/QQ/TIM）处于关闭状态。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                InitEditorsAndUI(textBox1.Text);
            }
        }
        private void InitEditorsAndUI(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                EnableAllButton(false);
                // 清空界面元素
                label5.Text = "";
                flowLayoutPanel1.Controls.Clear();
                // 重新计算并修改界面元素
                modifier.InitEditors(path);
                modifier.SetVersionLabelAndCategoryCategories(label5, flowLayoutPanel1);
                EnableAllButton(true);
                // 重新显示备份状态
                button3.Enabled = false;
                button3.Enabled = modifier.BackupExists();
                List<string> categories = UIController.GetCategoriesFromPanel(flowLayoutPanel1);
                if (categories != null && categories.Count == 0)
                {
                    button2.Enabled = false;
                }
            }
        }
        private string GetCheckedRadioButtonNameEn()
        {
            if (radioButton1.Checked)
            {
                return "wechat";
            }
            else if (radioButton2.Checked)
            {
                return "qq";
            }
            else if (radioButton3.Checked)
            {
                return "tim";
            }
            else if (radioButton4.Checked)
            {
                return "qqlite";
            }
            return "none";
        }
        private void EnableAllButton(bool state)
        {
            foreach (Control c in this.Controls)
            {
                if (c is Button)
                {
                    c.Enabled = state;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.SelectedPath;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (modifier.IsAllFilesExist(textBox1.Text))
            {
                InitEditorsAndUI(textBox1.Text);
            }
            else
            {
                label5.ForeColor = Color.Red;
                UIController.AddMsgToPanel(flowLayoutPanel1, "请输入正确的应用路径");
                label5.Text = "未能检测";
                button2.Enabled = false;
                button3.Enabled = false;
            }
        }
    }
}
