using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Study.爬虫;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;

namespace Study.爬虫_搜图
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", EntryPoint = "GetScrollPos")]
        public static extern int GetScrollPos(IntPtr hwnd, int nBar);
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            comboBox1.SelectedIndex = 0;
            this.BackgroundImage = Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.listView1.BeginUpdate();
            this.listView1.LargeImageList = ImageList;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(listView1, $"双击下载到 {Program.DownloadPath}");
            ImageList.ImageSize = new Size(256, 200);
        }
        private ImageList ImageList = new ImageList();
        private List<string> images = new List<string>();
        private int SearchIndex = 0;
        private int LastSearchEngine = 0;
        private string IG = "";
        /// <summary>
        /// 搜索按钮
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            LastSearchEngine = comboBox1.SelectedIndex;
            if (progressBar1.Visible) return;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            textBox1.Text = textBox1.Text.Trim();
            SearchString = textBox1.Text;
            SearchIndex = 1;
            _searchNextIndex = 0;
            this.ImageList.Images.Clear();
            this.listView1.Items.Clear();
            this.images.Clear();
            if (textBox1.Text.Replace(" ", "").Length == 0)
            {
                progressBar1.Visible = false;
                return;
            }
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    BingSearchPage();
                    break;
                case 1:
                    GoogleSearchPage();
                    break;
                case 2:
                    BaiduSearchPage();
                    break;
                case 3:
                    DuckSearchPage();
                    break;
                case 4:
                    SougouSearchPage();
                    break;
                case 5:
                    SearchPageWith360();
                    break;
                case 6:
                    EcosiaSearchPage();
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Text = "搜图神器 - " + comboBox1.Text;
        }
        private string CheckImage(Image image)
        {
            if (image.Equals(ImageFormat.Jpeg))
            {
                return ".jpeg";
            }
            else if (image.Equals(ImageFormat.Png))
            {
                return ".png";
            }
            else if (image.Equals(ImageFormat.Bmp))
            {
                return ".Bmp";
            }
            else
            {
                return ".jpeg";
            }
        }
        /// <summary>
        /// 下一页按钮
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (progressBar1.Visible) return;
            if (LastSearchEngine != comboBox1.SelectedIndex)
            {
                if (SearchString.Replace(" ", "").Length == 0) return;
                button1_Click(null, null);
                return;
            }
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            textBox1.Text = textBox1.Text.Trim();
            SearchIndex++;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    BingNextPage();
                    break;
                case 1:
                    GoogleNextPage();
                    break;
                case 2:
                    BaiduNextPage();
                    break;
                case 3:
                    DuckNextPage();
                    break;
                case 4:
                    SougouNextPage();
                    break;
                case 5:
                    NextPageWith360();
                    break;
                case 6:
                    EcosiaNextPage();
                    break;
            }
        }
        private int _searchNextIndex;
        private string SearchString = "";
        private async void BingSearchPage()
        {
            await Task.Run(() =>
             {
                 string html = WebFile.GetHtmlString(WebFile.GetStream($"https://cn.bing.com/images/search?q={SearchString}"));
                 IG = new Regex("IG:\"(.*?)\"").Match(html).Groups[1].Value;
                 IEnumerator enumerator = new Regex("<a class=\"inflnk\" aria-label=\"(.*?)\" href=\"(.*?)\" h=\"(.*?)\">(.*?)</a>").Matches(html).GetEnumerator();
                 List<ImageKeys> keys = new List<ImageKeys>();
                 while (enumerator.MoveNext() && enumerator.Current != null)
                 {
                     ImageKeys imageKeys = new ImageKeys
                     {
                         title = ((Match)enumerator.Current).Groups[1].Value,
                         image = new Regex("mediaurl=(.*?)&").Match(WebUtility.UrlDecode(((Match)enumerator.Current).Groups[2].Value)).Groups[1].Value
                     };
                     keys.Add(imageKeys);
                 }
                 progressBar1.Maximum = keys.Count;
                 foreach (ImageKeys item in keys)
                 {
                     try
                     {
                         Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                         ImageList.Images.Add(image);
                         images.Add(item.image);
                         ListViewItem listViewItem = new ListViewItem
                         {
                             ImageIndex = ImageList.Images.Count - 1,
                             Text = item.title
                         };
                         this.listView1.Items.Add(listViewItem);
                     }
                     catch { }
                     progressBar1.PerformStep();
                 }
             });
            progressBar1.Visible = false;
        }
        private async void BingNextPage()
        {
            await Task.Run(() =>
            {
                _searchNextIndex += 50;
                string html = WebFile.GetHtmlString(WebFile.GetStream($"https://cn.bing.com/images/async?q={SearchString}&first={_searchNextIndex}&count=35&layout=RowBased_Landscape&mmasync=1&IG={IG}&SFX={SearchIndex}"));
                IG = new Regex("IG:\"(.*?)\"").Match(html).Groups[1].Value;
                IEnumerator enumerator = new Regex("<a class=\"inflnk\" aria-label=\"(.*?)\" href=\"(.*?)\" h=\"(.*?)\">(.*?)</a>").Matches(html).GetEnumerator();
                List<ImageKeys> keys = new List<ImageKeys>();
                while (enumerator.MoveNext() && enumerator.Current != null)
                {
                    ImageKeys imageKeys = new ImageKeys
                    {
                        title = ((Match)enumerator.Current).Groups[1].Value,
                        image = new Regex("mediaurl=(.*?)&").Match(WebUtility.UrlDecode(((Match)enumerator.Current).Groups[2].Value)).Groups[1].Value
                    };
                    keys.Add(imageKeys);
                }
                progressBar1.Maximum = keys.Count;
                foreach (ImageKeys item in keys)
                {
                    try
                    {
                        Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                        ImageList.Images.Add(image);
                        images.Add(item.image);
                        ListViewItem listViewItem = new ListViewItem
                        {
                            ImageIndex = ImageList.Images.Count - 1,
                            Text = item.title
                        };
                        this.listView1.Items.Add(listViewItem);
                    }
                    catch { }
                    progressBar1.PerformStep();
                }
            });
            progressBar1.Visible = false;
        }
        private void GoogleSearchPage()
        {
            try
            {
                IPData iPData = Json<IPData>.ReadJson(WebFile.GetHtmlString(WebFile.GetStream("https://ip.cn/api/index?ip=&type=0")));
                if (iPData.address.Contains("中国"))
                {
                    progressBar1.Visible = false;
                    MessageBox.Show($"检测到您所在的 IP 位置：{iPData.address}。\r\n由于网络政策问题，请尝试切换 IP（非中国大陆）或搜索引擎。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                progressBar1.Visible = false;
                return;
            }
            progressBar1.Visible = false;
            MessageBox.Show("Google 搜索引擎严厉打击爬虫，无法搜索图片。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void GoogleNextPage(bool isRun = false)
        {
            try
            {
                IPData iPData = Json<IPData>.ReadJson(WebFile.GetHtmlString(WebFile.GetStream("https://ip.cn/api/index?ip=&type=0")));
                if (iPData.address.Contains("中国"))
                {
                    progressBar1.Visible = false;
                    MessageBox.Show($"检测到您所在的 IP 位置：{iPData.address}。\r\n由于网络政策问题，请尝试切换 IP（非中国大陆）或搜索引擎。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                progressBar1.Visible = false;
                return;
            }
            progressBar1.Visible = false;
            MessageBox.Show("Google 搜索引擎严厉打击爬虫，无法搜索图片。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private async void BaiduSearchPage()
        {
            await Task.Run(() =>
             {
                 string html = WebFile.GetHtmlString(WebFile.GetStream($"https://image.baidu.com/search/index?tn=baiduimage&word={SearchString}", accept: "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"));
                 IG = new Regex("data.logid = \"(.*?)\"").Match(html).Groups[1].Value;
                 BaiduNextPage();
             });
        }
        private async void BaiduNextPage()
        {
            await Task.Run(() =>
            {
                try
                {
                    _searchNextIndex += 30;
                    Stream stream = WebFile.GetStream($"https://image.baidu.com/search/acjson?tn=resultjson_com&logid={IG}&ipn=rj&is=&fp=result&fr=&word={SearchString}&queryWord={SearchString}&cl=&lm=&ie=utf-8&oe=utf-8&adpicid=&st=&z=&ic=&hd=&latest=&copyright=&s=&se=&tab=&width=&height=&face=&istype=&qc=&nc=&expermode=&nojc=&isAsync=&pn={_searchNextIndex}&rn=30&gsm=1e&1652009910428=", accept: "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    if (stream == null)
                    {
                        return;
                    }
                    string html = WebFile.GetHtmlString(stream);
                    BaiduDataRoot jsonRead = Json<BaiduDataRoot>.ReadJson(html);
                    List<ImageKeys> keys = new List<ImageKeys>();
                    foreach (BaiduData item in jsonRead.data)
                    {
                        keys.Add(new ImageKeys()
                        {
                            title = item.fromPageTitleEnc,
                            image = item.thumbURL
                        });
                    }
                    progressBar1.Maximum = keys.Count;
                    foreach (ImageKeys item in keys)
                    {
                        try
                        {
                            Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                            ImageList.Images.Add(image);
                            images.Add(item.image);
                            ListViewItem listViewItem = new ListViewItem
                            {
                                ImageIndex = ImageList.Images.Count - 1,
                                Text = item.title
                            };
                            this.listView1.Items.Add(listViewItem);
                        }
                        catch { }
                        progressBar1.PerformStep();
                        Application.DoEvents();
                    }
                }
                catch { }
            });
            progressBar1.Visible = false;
        }

        private async void DuckSearchPage()
        {
            await Task.Run(() =>
            {
                try
                {
                    IPData iPData = Json<IPData>.ReadJson(WebFile.GetHtmlString(WebFile.GetStream("https://ip.cn/api/index?ip=&type=0")));
                    if (iPData.address.Contains("中国"))
                    {
                        progressBar1.Visible = false;
                        MessageBox.Show($"检测到您所在的 IP 位置：{iPData.address}。\r\n由于网络政策问题，请尝试切换 IP（非中国大陆）或搜索引擎。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch
                {
                    progressBar1.Visible = false;
                    return;
                }
                string html = WebFile.GetHtmlString(WebFile.GetStream($"https://duckduckgo.com/?q={SearchString}&iax=images&ia=images"));
                IG = new Regex("vqd='(.*?)'").Match(html).Groups[1].Value;
                DuckDataRoot jsonRead = Json<DuckDataRoot>.ReadJson(WebFile.GetHtmlString(WebFile.GetStream($"https://duckduckgo.com/i.js?l=wt-wt&o=json&q=夏洛特&vqd={IG}&p=1")));
                List<ImageKeys> keys = new List<ImageKeys>();
                foreach (DuckData item in jsonRead.results)
                {
                    keys.Add(new ImageKeys()
                    {
                        title = item.title,
                        image = item.image
                    });
                }
                progressBar1.Maximum = keys.Count;
                foreach (ImageKeys item in keys)
                {
                    try
                    {
                        Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                        ImageList.Images.Add(image);
                        images.Add(item.image);
                        ListViewItem listViewItem = new ListViewItem
                        {
                            ImageIndex = ImageList.Images.Count - 1,
                            Text = item.title
                        };
                        this.listView1.Items.Add(listViewItem);
                    }
                    catch { }
                    progressBar1.PerformStep();
                    Application.DoEvents();
                }
            });
            progressBar1.Visible = false;
        }
        private async void DuckNextPage()
        {
            await Task.Run(() =>
            {
                try
                {
                    IPData iPData = Json<IPData>.ReadJson(WebFile.GetHtmlString(WebFile.GetStream("https://ip.cn/api/index?ip=&type=0")));
                    if (iPData.address.Contains("中国"))
                    {
                        progressBar1.Visible = false;
                        MessageBox.Show($"检测到您所在的 IP 位置：{iPData.address}。\r\n由于网络政策问题，请尝试切换 IP（非中国大陆）或搜索引擎。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                catch
                {
                    progressBar1.Visible = false;
                    return;
                }
                try
                {
                    _searchNextIndex += 100;
                    Stream stream = WebFile.GetStream($"https://duckduckgo.com/i.js?q={SearchString}&o=json&p=1&s={_searchNextIndex}&u=bing&f=,,,,,&l=wt-wt&vqd={IG}");
                    if (stream == null)
                    {
                        return;
                    }
                    string html = WebFile.GetHtmlString(stream);
                    DuckDataRoot jsonRead = Json<DuckDataRoot>.ReadJson(html);
                    List<ImageKeys> keys = new List<ImageKeys>();
                    foreach (DuckData item in jsonRead.results)
                    {
                        keys.Add(new ImageKeys()
                        {
                            title = item.title,
                            image = item.image
                        });
                    }
                    progressBar1.Maximum = keys.Count;
                    foreach (ImageKeys item in keys)
                    {
                        try
                        {
                            Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                            ImageList.Images.Add(image);
                            images.Add(item.image);
                            ListViewItem listViewItem = new ListViewItem
                            {
                                ImageIndex = ImageList.Images.Count - 1,
                                Text = item.title
                            };
                            this.listView1.Items.Add(listViewItem);
                        }
                        catch { }
                        progressBar1.PerformStep();
                        Application.DoEvents();
                    }
                }
                catch { }
            });
            progressBar1.Visible = false;
        }
        private void SougouSearchPage()
        {
            _searchNextIndex = 0;
            SougouNextPage();
        }
        private async void SougouNextPage()
        {
            await Task.Run(() =>
            {
                _searchNextIndex += 48;
                string html = WebFile.GetHtmlString(WebFile.GetStream($"https://pic.sogou.com/napi/pc/searchList?mode=1&start={_searchNextIndex}&xml_len=48&query={SearchString}"));
                SougouRoot jsonRead = Json<SougouRoot>.ReadJson(html);
                List<ImageKeys> keys = new List<ImageKeys>();
                foreach (SougouData.SougouData3 item in jsonRead.data.items)
                {
                    keys.Add(new ImageKeys()
                    {
                        title = item.title,
                        image = item.oriPicUrl
                    });
                }
                progressBar1.Maximum = keys.Count;
                foreach (ImageKeys item in keys)
                {
                    try
                    {
                        Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                        ImageList.Images.Add(image);
                        images.Add(item.image);
                        ListViewItem listViewItem = new ListViewItem
                        {
                            ImageIndex = ImageList.Images.Count - 1,
                            Text = item.title
                        };
                        this.listView1.Items.Add(listViewItem);
                    }
                    catch { }
                    progressBar1.PerformStep();
                    Application.DoEvents();
                }
            });
            progressBar1.Visible = false;
        }
        private void SearchPageWith360()
        {
            _searchNextIndex = 0;
            NextPageWith360();
        }
        private async void NextPageWith360()
        {
            await Task.Run(() =>
            {
                _searchNextIndex += 44;
                string html = WebFile.GetHtmlString(WebFile.GetStream($"https://image.so.com/j?callback=jQuery{DateTime.Now.Ticks}&q={SearchString}&qtag=&pd=1&pn=60&correct={SearchString}&adstar=0&tab=all&sid=8ac62b4ea05ac46f2526bb3eb4922f65&ras=0&cn=0&gn=0&kn=50&crn=0&bxn=20&cuben=0&pornn=0&manun=16&src=srp&sn={_searchNextIndex}&ps={_searchNextIndex}&pc=60&_={DateTime.Now.Ticks}"));
                DataWith360Root jsonRead = Json<DataWith360Root>.ReadJson(new Regex("jQuery(.*?)\\((.*?)\\);").Match(html).Groups[2].Value);
                List<ImageKeys> keys = new List<ImageKeys>();
                foreach (DataWith360 item in jsonRead.list)
                {
                    keys.Add(new ImageKeys()
                    {
                        title = item.title.Replace("<em>", "").Replace("</em>", ""),
                        image = item.img
                    });
                }
                progressBar1.Maximum = keys.Count;
                foreach (ImageKeys item in keys)
                {
                    try
                    {
                        Image image = Image.FromStream(WebFile.GetStream(item.image, 4500));
                        ImageList.Images.Add(image);
                        images.Add(item.image);
                        ListViewItem listViewItem = new ListViewItem
                        {
                            ImageIndex = ImageList.Images.Count - 1,
                            Text = item.title
                        };
                        this.listView1.Items.Add(listViewItem);
                    }
                    catch { }
                    progressBar1.PerformStep();
                    Application.DoEvents();
                }
            });
            progressBar1.Visible = false;
        }
        private void EcosiaSearchPage()
        {
            progressBar1.Visible = false;
            MessageBox.Show("Ecosia 搜索引擎严厉打击爬虫，无法搜索图片。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void EcosiaNextPage()
        {
            progressBar1.Visible = false;
            MessageBox.Show("Ecosia 搜索引擎严厉打击爬虫，无法搜索图片。", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                WebFile.DownloadFile(images[listView1.SelectedItems[0].Index], $"{Program.DownloadPath + SearchString} - {DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}{CheckImage(ImageList.Images[listView1.SelectedItems[0].Index])}");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) button1_Click(null, null);
        }
    }
    public class IPData
    {
        public string address { get; set; }
        public int code { get; set; }
        public string ip { get; set; }
        public int isDomain { get; set; }
        public int rs { get; set; }
    }
    public class DuckData
    {
        public int height { get; set; }
        public string image { get; set; }
        public string source { get; set; }
        public string thumbnail { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }
    public class DuckDataRoot
    {
        public string ads { get; set; }
        public string next { get; set; }
        public string query { get; set; }
        public string queryEncoded { get; set; }
        public string response_type { get; set; }
        public List<DuckData> results { get; set; }
    }
    public class BaiduData
    {
        public string adType { get; set; }
        public string hasAspData { get; set; }
        public string thumbURL { get; set; }
        public string commodityInfo { get; set; }
        public int isCommodity { get; set; }
        public string middleURL { get; set; }
        public string shituToken { get; set; }
        public string largeTnImageUrl { get; set; }
        public int hasLarge { get; set; }
        public string hoverURL { get; set; }
        public int pageNum { get; set; }
        public string objURL { get; set; }
        public string fromURL { get; set; }
        public string fromJumpUrl { get; set; }
        public string fromURLHost { get; set; }
        public string currentIndex { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string type { get; set; }
        public int is_gif { get; set; }
        public int isCopyright { get; set; }
        public string resourceInfo { get; set; }
        public string strategyAssessment { get; set; }
        public string filesize { get; set; }
        public string bdSrcType { get; set; }
        public string di { get; set; }
        public string pi { get; set; }
        public string @is { get; set; }
        public string imgCollectionWord { get; set; }
        public string hasThumbData { get; set; }
        public int bdSetImgNum { get; set; }
        public int partnerId { get; set; }
        public int spn { get; set; }
        public string bdImgnewsDate { get; set; }
        public string fromPageTitle { get; set; }
        public string fromPageTitleEnc { get; set; }
        public string bdSourceName { get; set; }
        public string bdFromPageTitlePrefix { get; set; }
        public int isAspDianjing { get; set; }
        public string token { get; set; }
        public string imgType { get; set; }
        public string cs { get; set; }
        public string os { get; set; }
        public string simid { get; set; }
        public string personalized { get; set; }
        public string simid_info { get; set; }
        public string face_info { get; set; }
        public string xiangshi_info { get; set; }
        public string adPicId { get; set; }
        public string source_type { get; set; }
    }
    public class SougouData
    {
        public class SougouData2
        {
            public string key { get; set; }
            public string value { get; set; }
            public string active { get; set; }
        }
        public class SougouData3
        {
            public string author { get; set; }
            public string author_name { get; set; }
            public string author_pageurl { get; set; }
            public string author_picurl { get; set; }
            public string author_thumbUrl { get; set; }
            public string author_thumb_mfid { get; set; }
            public int biaoqing { get; set; }
            public string ch_site_name { get; set; }
            public string cutBoardInputSkin { get; set; }
            public string docId { get; set; }
            public int docidx { get; set; }
            public int gifpic { get; set; }
            public int grouppic { get; set; }
            public int height { get; set; }
            public int https_convert { get; set; }
            public int index { get; set; }
            public string lastModified { get; set; }
            public string like_num { get; set; }
            public string link { get; set; }
            public string locImageLink { get; set; }
            public string mf_id { get; set; }
            public string name { get; set; }
            public string oriPicUrl { get; set; }
            public string painter_year { get; set; }
            public string picUrl { get; set; }
            public string publishmodified { get; set; }
            public int size { get; set; }
            public string summarytype { get; set; }
            public int thumbHeight { get; set; }
            public string thumbUrl { get; set; }
            public int thumbWidth { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public string wapLink { get; set; }
            public int width { get; set; }
            public double scale { get; set; }
            public int did { get; set; }
            public string imgTag { get; set; }
            public string bgColor { get; set; }
            public string imgDefaultUrl { get; set; }
        }
        public class SougouData4
        {
            public string docId { get; set; }
            public int index { get; set; }
            public string mfid { get; set; }
            public int thumbHeight { get; set; }
            public int thumbWidth { get; set; }
        }
        public List<SougouData4> adPic { get; set; }
        public int blackLevel { get; set; }
        public int cacheDocNum { get; set; }
        public int hasPicsetRes { get; set; }
        public string isQcResult { get; set; }
        public int is_strong_style { get; set; }
        public List<SougouData3> items { get; set; }
        public int maxEnd { get; set; }
        public int painter_doc_count { get; set; }
        public string parity { get; set; }
        public string qo_info { get; set; }
        public string queryCorrection { get; set; }
        public string shopQuery { get; set; }
        public List<List<string>> tag { get; set; }
        public List<string> tagWords { get; set; }
        public List<string> tagWords_feed { get; set; }
        public List<List<string>> tag_feed { get; set; }
        public int totalItems { get; set; }
        public int totalNum { get; set; }
        public string uuid { get; set; }
        public string query { get; set; }
        public List<SougouData2> tagList { get; set; }
    }
    public class SougouRoot
    {
        public int status { get; set; }
        public string info { get; set; }
        public SougouData data { get; set; }
    }
    public class BaiduDataRoot
    {
        public string queryEnc { get; set; }
        public string queryExt { get; set; }
        public int listNum { get; set; }
        public int displayNum { get; set; }
        public string gsm { get; set; }
        public string bdFmtDispNum { get; set; }
        public string bdSearchTime { get; set; }
        public int isNeedAsyncRequest { get; set; }
        public string bdIsClustered { get; set; }
        public List<BaiduData> data { get; set; }
    }
    public class DataWith360
    {
        public string id { get; set; }
        public string qqface_down_url { get; set; }
        public string downurl { get; set; }
        public string downurl_true { get; set; }
        public string grpmd5 { get; set; }
        public int type { get; set; }
        public string src { get; set; }
        public int color { get; set; }
        public int index { get; set; }
        public string title { get; set; }
        public string litetitle { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string imgsize { get; set; }
        public string imgtype { get; set; }
        public string key { get; set; }
        public string dspurl { get; set; }
        public string link { get; set; }
        public int source { get; set; }
        public string img { get; set; }
        public string thumb_bak { get; set; }
        public string thumb { get; set; }
        public string _thumb_bak { get; set; }
        public string _thumb { get; set; }
        public string imgkey { get; set; }
        public int thumbWidth { get; set; }
        public string dsptime { get; set; }
        public int thumbHeight { get; set; }
        public string grpcnt { get; set; }
        public string fixedSize { get; set; }
        public string fnum { get; set; }
        public string comm_purl { get; set; }
    }
    public class DataWith360Root
    {
        public int total { get; set; }
        public string end { get; set; }
        public string sid { get; set; }
        public int ran { get; set; }
        public int ras { get; set; }
        public int cuben { get; set; }
        public int manun { get; set; }
        public int pornn { get; set; }
        public int kn { get; set; }
        public int cn { get; set; }
        public int gn { get; set; }
        public int ps { get; set; }
        public int pc { get; set; }
        public int adstar { get; set; }
        public int lastindex { get; set; }
        public int ceg { get; set; }
        public List<DataWith360> list { get; set; }
        public string boxresult { get; set; }
        public string wordguess { get; set; }
        public int prevsn { get; set; }
    }
    public struct ImageKeys
    {
        public string title;
        public string image;
    }
}
