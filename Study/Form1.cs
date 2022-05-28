using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Study.蓝屏;
using Study.删除文件;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Runtime.InteropServices;

namespace Study
{
    public partial class Form1 : Form
    {
        private long FL
        {
            get
            {
                return 9904128;
            }
        }
        /// <summary>
        /// 窗体初始化
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MessageBox.Show("该程序只能运行在 Windows 平台。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Form1_Disposed1(null, null);
                Environment.Exit(1);
            }
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Form1_Disposed1(null, null);
                Environment.Exit(1);
            }
            notifyIcon1.DoubleClick += (o, e) =>
            {
                FileInfo fileInfo1 = new FileInfo(Application.ExecutablePath);
                if (fileInfo1.Length < FL || fileInfo1.Length > FL)
                {
                    MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Form1_Disposed1(null, null);
                    Environment.Exit(1);
                }
                this.Show();
                this.Activate();
            };
            notifyIcon1.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon1.ContextMenuStrip.Items.Add("重启");
            notifyIcon1.ContextMenuStrip.Items.Add("退出");
            notifyIcon1.ContextMenuStrip.Items[0].Click += (o, e) =>
            {
                Process.Start(Application.ExecutablePath);
                this.Dispose();
            };
            notifyIcon1.ContextMenuStrip.Items[1].Click += (o, e) =>
            {
                this.Dispose();
            };
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            #region AboutInstance
            panel1.Size = new Size(panel1.Size.Width, panel1.Height + 140);
            label1.Visible = false;
            textBox1.Visible = false;
            button12.Visible = false;
            #endregion
            #region AboutVirus
            if (!Program.CanRunVirus)
            {
                button1.Enabled = false;
                button4.Enabled = false;
            }
            else
            {
                button1.ForeColor = Color.Red;
                button4.ForeColor = Color.Red;
                启用病毒模式ToolStripMenuItem.Text = "禁用病毒模式";
            }
            #endregion
            if (Program.Register)
            {
                menuStrip1.Items.RemoveAt(0);
                注册ToolStripMenuItem.Enabled = false;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        /// <summary>
        /// 当窗体启动时调用
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.Update();
            this.Disposed += Form1_Disposed1;
        }

        private void Form1_Disposed1(object sender, EventArgs e)
        {
            NotepadProcess?.Kill();
            Program.ShowWindow(Program.DesktopIntptr, 0);
            Program.FreeLibrary(Program.DSDll);
        }

        /// <summary>
        /// 蓝屏按钮
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.IsAdministrator)
            {
            a: Program.Adm();
                DialogResult dialogResult = MessageBox.Show("管理员权限已被取消。", "权限请求", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                // 重试、终止、忽略
                if (dialogResult == DialogResult.Retry)
                {
                    goto a;
                }
                else if (dialogResult == DialogResult.Abort)
                {
                    Environment.Exit(1);
                }
                else
                {
                    return;
                }
            }
            // 弹出消息框，并接收返回
            var mbResult = MessageBox.Show("确定蓝屏吗？", "蓝屏", MessageBoxButtons.OKCancel);
            // 点击“确定”时发生
            if (mbResult == DialogResult.OK)
            {
                BlueScreen.Run();
                // 退出程序
                Environment.Exit(1);
            }
        }
        /// <summary>
        /// 提取抖音视频按钮
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.Register && !Program.TryRegister)
            {
                MessageBox.Show("您尚未注册，该功能不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (DouyinVideosForm == null || DouyinVideosForm.IsDisposed)
            {
                DouyinVideosForm = new 爬虫_抓取抖音视频.Form1();
                DouyinVideosForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(DouyinVideosForm.Handle, true);
        }
        /// <summary>
        /// C#中文编程按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (ChineseProgramming == null || ChineseProgramming.IsDisposed)
            {
                ChineseProgramming = new 中文编程.Form1();
                ChineseProgramming.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(ChineseProgramming.Handle, true);
        }
        /// <summary>
        /// 惊喜按钮
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.IsAdministrator)
            {
            a: Program.Adm();
                DialogResult dialogResult = MessageBox.Show("管理员权限已被取消。", "权限请求", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                // 重试、终止、忽略
                if (dialogResult == DialogResult.Retry)
                {
                    goto a;
                }
                else if (dialogResult == DialogResult.Abort)
                {
                    Environment.Exit(1);
                }
                else
                {
                    return;
                }
            }
            DialogResult dialogResult2 = MessageBox.Show("确定要删除某文件吗？", "特级警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            if (dialogResult2 == DialogResult.OK)
            {
                Delete.Downloads();
                Delete.Programe();
                Delete.Programe86();
                Delete.DDisk();
                this.Disposed += Form1_Disposed;
            }
        }
        /// <summary>
        /// 必应翻译窗口
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.Register && !Program.TryRegister)
            {
                MessageBox.Show("您尚未注册，该功能不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (BingTranslate == null || BingTranslate.IsDisposed)
            {
                BingTranslate = new 翻译.必应翻译();
                BingTranslate.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(BingTranslate.Handle, true);
        }
        /// <summary>
        /// 软件下载按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.Register && !Program.TryRegister)
            {
                MessageBox.Show("您尚未注册，该功能不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (SoftwareForm == null || SoftwareForm.IsDisposed)
            {
                SoftwareForm = new 软件下载.Form1();
                SoftwareForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(SoftwareForm.Handle, true);
        }
        /// <summary>
        /// 加密文件按钮
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (EncryptionForm == null || EncryptionForm.IsDisposed)
            {
                EncryptionForm = new 加密与解密.Form1();
                EncryptionForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(EncryptionForm.Handle, true);
        }
        /// <summary>
        /// 搜图神器按钮
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.Register && !Program.TryRegister)
            {
                MessageBox.Show("您尚未注册，该功能不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (SearchImageForm == null || SearchImageForm.IsDisposed)
            {
                SearchImageForm = new 爬虫_搜图.Form1();
                SearchImageForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(SearchImageForm.Handle, true);
        }
        /// <summary>
        /// 声明搜图神器窗口
        /// </summary>
        爬虫_搜图.Form1 SearchImageForm = null;
        /// <summary>
        /// 声明加密文件窗口
        /// </summary>
        加密与解密.Form1 EncryptionForm = null;
        /// <summary>
        /// 声明软件下载窗口
        /// </summary>
        软件下载.Form1 SoftwareForm = null;
        /// <summary>
        /// 声明必应翻译窗口
        /// </summary>
        翻译.必应翻译 BingTranslate = null;
        /// <summary>
        /// 声明提取抖音视频窗口
        /// </summary>
        爬虫_抓取抖音视频.Form1 DouyinVideosForm = null;
        /// <summary>
        /// 声明中文编程窗口
        /// </summary>
        中文编程.Form1 ChineseProgramming = null;
        /// <summary>
        /// 声明中文编程窗口
        /// </summary>
        加密与解密.图片转换 ImageConvertForm = null;

        /// <summary>
        /// 病毒复苏
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void Form1_Disposed(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Application.ExecutablePath
            };
            Process.Start(startInfo);
        }
        /// <summary>
        /// 记事本按钮
        /// </summary>
        private void button9_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (NotepadProcess == null || NotepadProcess.HasExited)
            {
                try
                {
                    NotepadProcess = Process.Start("notepad.exe");
                }
                catch { }
            }
            else
            {
                Program.SwitchToThisWindow(NotepadProcess.MainWindowHandle, true);
            }
        }
        Process NotepadProcess = null;
        /// <summary>
        /// 图片转换按钮
        /// </summary>
        private void button10_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!Program.Register && !Program.TryRegister)
            {
                MessageBox.Show("您尚未注册，该功能不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (ImageConvertForm == null || ImageConvertForm.IsDisposed)
            {
                ImageConvertForm = new 加密与解密.图片转换();
                ImageConvertForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(ImageConvertForm.Handle, true);
        }

        private void 注册ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (Program.Register)
            {
                MessageBox.Show("程序已经注册了，无需操作。", "注册");
            }
            else
            {
                new 注册().ShowDialog();
            }
        }

        private void 复制注册信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (Program.Register)
            {
                MessageBox.Show("程序已经注册了，无需操作。", "复制");
                return;
            }
            string dese = 注册.DESEncrypt(Program.RegistrantMachineCode, "46372839", "46372839");
            dese = 注册.DESEncrypt(dese, "46272838", "46272838");
            dese = 注册.DESEncrypt(dese, "46372817", "46372817");
            Clipboard.SetText(dese);
        }

        private void 启用病毒模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!button1.Enabled)
            {
                if (MessageBox.Show("病毒模式有一定的危险性，确定要启用它吗？", "病毒模式", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    启用病毒模式ToolStripMenuItem.Text = "禁用病毒模式";
                    button1.ForeColor = Color.Red;
                    button4.ForeColor = Color.Red;
                    button1.Enabled = true;
                    button4.Enabled = true;
                }
            }
            else
            {
                启用病毒模式ToolStripMenuItem.Text = "启用病毒模式";
                button1.ForeColor = Color.Black;
                button4.ForeColor = Color.Black;
                button1.Enabled = false;
                button4.Enabled = false;
            }
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            MessageBox.Show("本软件由 特别中二的Jal君 制作。\nCopyright ©Jal  2022", $"关于 {Program.Version}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        /// <summary>
        /// 学习资料按钮
        /// </summary>
        private void button11_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (StudyFilesForm == null || StudyFilesForm.IsDisposed)
            {
                StudyFilesForm = new Learning.Form1();
                StudyFilesForm.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(StudyFilesForm.Handle, true);
        }
        Learning.Form1 StudyFilesForm = null;

        private void 更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            Process.Start("https://pan.baidu.com/s/17zhhSvvwn_4omKtbyAio3A?pwd=6666");
        }
        private void 终端ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (!label1.Visible && !textBox1.Visible && !button12.Visible)
            {
                label1.Visible = true;
                textBox1.Visible = true;
                button12.Visible = true;
                panel1.Size = new Size(panel1.Size.Width, panel1.Height - 140);
                Process cmdProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe")
                };
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.LoadUserProfile = true;
                cmdProcess.StartInfo.RedirectStandardInput = true;
                cmdProcess.StartInfo.RedirectStandardOutput = true;
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.Start();
                cmdProcess.StandardInput.WriteLine("exit");
                cmdProcess.StandardInput.AutoFlush = true;
                cmdStringBuilder.Clear();
                cmdStringBuilder.Append("==============================\r\n");
                for (int i = 0; i < 2; i++)
                {
                    cmdStringBuilder.Append(cmdProcess.StandardOutput.ReadLine() + "\r\n");
                }
                cmdStringBuilder.AppendLine("==============================");
                textBox1.Text = cmdStringBuilder.ToString() + "> ";
                cmdStringBuilder.Clear();
                cmdStringBuilder.Append(textBox1.Text);
                textBox1.Focus();
            }
        }
        StringBuilder cmdStringBuilder = new StringBuilder();

        private void button12_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            label1.Visible = false;
            textBox1.Visible = false;
            button12.Visible = false;
            panel1.Size = new Size(panel1.Size.Width, panel1.Height + 140);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            if (textBox1.SelectedText.Length > 0)
            {
                if (e.KeyChar != '\u0003')
                {
                    e.Handled = true;
                    return;
                }
                return;
            }
            if (e.KeyChar == '\r')
            {
                string cmd = textBox1.Text.Substring(cmdStringBuilder.Length);
                Process cmdProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe")
                };
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.LoadUserProfile = true;
                cmdProcess.StartInfo.RedirectStandardInput = true;
                cmdProcess.StartInfo.RedirectStandardOutput = true;
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.Start();
                if (string.IsNullOrEmpty(cmd))
                {
                    cmdProcess.StandardInput.WriteLine($"exit");
                }
                else
                {
                    cmdProcess.StandardInput.WriteLine($"{cmd}&exit");
                }
                cmdProcess.StandardInput.AutoFlush = true;
                string output = cmdProcess.StandardOutput.ReadToEnd();
                textBox1.Text += output.Substring(output.IndexOf("exit")).Remove(0, 4) + "\r\n> ";
                textBox1.Select(textBox1.Text.Length, 0);
                textBox1.ScrollToCaret();
                cmdStringBuilder.Clear();
                cmdStringBuilder.Append(textBox1.Text);
                e.Handled = true;
            }
            else if (e.KeyChar == '\b')
            {
                if (textBox1.SelectionStart <= cmdStringBuilder.Length)
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (textBox1.SelectionStart < cmdStringBuilder.Length)
                {
                    e.Handled = true;
                }
            }
        }

        private void 选项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            using (工具.选项 form = new 工具.选项())
            {
                form.ShowDialog();
            }
        }

        private void menuStrip1_KeyDown(object sender, KeyEventArgs e)
        {
            menuStrip1.Select();
            if (e.Alt)
            {
                switch (e.KeyCode)
                {
                    case Keys.X:
                        if (注册ToolStripMenuItem.Visible)
                        {
                            注册ToolStripMenuItem.ShowDropDown();
                        }
                        break;
                    case Keys.V:
                        视图ToolStripMenuItem.ShowDropDown();
                        break;
                    case Keys.T:
                        工具ToolStripMenuItem.ShowDropDown();
                        break;
                    case Keys.H:
                        帮助ToolStripMenuItem.ShowDropDown();
                        break;
                }
            }
        }
        /// <summary>
        /// 关闭时执行
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            notifyIcon1.ShowBalloonTip(1000, "小鸟游六花", "程序正在后台运行。", ToolTipIcon.None);
        }
        防撤回.Form1 WeChatQQModify = null;
        private void button13_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo = new FileInfo(Application.ExecutablePath);
            if (fileInfo.Length < FL || fileInfo.Length > FL)
            {
                MessageBox.Show("您修改了程序数据，无法验证文件完整性。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            // 检查窗体是否为空或者已被释放资源，则实例化并显示
            if (WeChatQQModify == null || WeChatQQModify.IsDisposed)
            {
                WeChatQQModify = new 防撤回.Form1();
                WeChatQQModify.Show();
            }
            // 给予窗体焦点
            Program.SwitchToThisWindow(WeChatQQModify.Handle, true);
        }
    }
}
