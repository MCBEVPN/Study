using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Study.爬虫;
using System.IO;
using System.Web;
using System.Collections;
using System.Drawing;

namespace Study.爬虫_抓取抖音视频
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 视频地址列表
        /// </summary>
        private List<string> UriList = new List<string>();
        /// <summary>
        /// 抖音标题
        /// </summary>
        private string title = "- 抖音";
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
        }
        /// <summary>
        /// 解析按钮
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // 检测输入框是否准确
            if (!textBox1.Text.StartsWith("https://www.douyin.com/video/"))
            {
                return;
            }
            // 声明正则
            Regex regex = new Regex("src(.*?)vr%3D");
            // 获取字节流
            Stream stream = WebFile.GetStream(textBox1.Text);
            // 如果字节流为空，则返回
            if (stream == null)
            {
                return;
            }
            // 声明字节流，请求输入框的网址
            StreamReader sr = new StreamReader(stream);
            // 读取字节流的内容
            string html = sr.ReadToEnd();
            // 正则匹配，用匹配集合来循环
            MatchCollection match = regex.Matches(html);
            // 创建循环
            IEnumerator enumerator = match.GetEnumerator();
            // 清空地址列表
            UriList.Clear();
            // 必须满足当前不能为空才能循环，MoveNext就是下一个目标的意思
            while (enumerator.MoveNext() && enumerator.Current != null)
            {
                UriList.Remove(HttpUtility.UrlDecode(((Match)enumerator.Current).Value.Remove(0, 3)).Remove(0, 3).Insert(0, "https:"));
                UriList.Add(HttpUtility.UrlDecode(((Match)enumerator.Current).Value.Remove(0, 3)).Remove(0, 3).Insert(0, "https:"));
            }
            // 重新声明正则
            regex = new Regex("<title data-react-helmet=\"(.*?)\">(.*?)</title>");
            try
            {
                // 将多余的删掉，只留真正的标题
                title = regex.Match(html).Value.Remove(0, 35);
                title = title.Remove(title.Length - 8, 8);
            }
            catch
            {
                // 抛出异常时退出
                return;
            }
            // 如果视频地址列表数量是否为0，并处理下载按钮事件
            if (UriList.Count == 0)
            {
                button2.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
            }
            button2.Text = $"下载全部（{UriList.Count}）";
        }
        /// <summary>
        /// 下载按钮
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // 次数
            int i = 0;
            // 创建文件夹
            Directory.CreateDirectory(Program.VideosPath + "抖音");
            // 遍历地址列表
            foreach (var item in UriList)
            {
            retry:
                try
                {
                    // 如果列表等于1，则不加（），否则相反
                    if (UriList.Count == 1)
                    {
                        WebFile.DownloadFile(item, Program.VideosPath + "抖音\\" + title + ".mp4");
                    }
                    else
                    {
                        WebFile.DownloadFile(item, Program.VideosPath + "抖音\\" + title + $"({i}).mp4");
                    }
                }
                catch (Exception ex)
                {
                    // 抛出异常时，重试与取消的弹窗
                    DialogResult dialogResult = MessageBox.Show(ex.Message, "下载失败", MessageBoxButtons.RetryCancel);
                    // 如果点重试，则goto，否则跳出循环
                    if (dialogResult == DialogResult.Retry)
                    {
                        goto retry;
                    }
                    else
                    {
                        break;
                    }
                }
                // 每成功下载加1
                i++;
            }
            // 处理下载按钮事件
            button2.Text = $"下载全部（{UriList.Count}）";
            button2.Enabled = false;
            // 如果次数不等于0的时候弹出来
            if (i != 0)
            {
                MessageBox.Show($"全部下载成功。\n已将全部视频录入 {Program.VideosPath + "抖音"}。");
            }
        }
    }
}
