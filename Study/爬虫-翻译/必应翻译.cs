using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.翻译
{
    public partial class 必应翻译 : Form
    {
        List<Task> tasks = new List<Task>();
        bool complete = false;
        public 必应翻译()
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
            this.Disposed += 必应翻译_Disposed;
            tasks.Add(Task.Run(() =>
           {
               LoadLanguage();
               int i = 0;
               foreach (var item in _languagesDict)
               {
                   if (complete)
                   {
                       return;
                   }
                   comboBox1.Items.Add(item.Key);
                   comboBox2.Items.Add(item.Key);
                   if (complete)
                   {
                       return;
                   }
                   comboBox1.SelectedIndex = 0;
                   if (item.Key == "日语")
                   {
                       comboBox2.SelectedIndex = i;
                   }
                   i++;
               }
               comboBox2.Items.RemoveAt(0);
               while (!complete)
               {
                   try
                   {
                       if (textBox1.Text.Replace(" ", "").Length != 0)
                       {
                           textBox2.Text = Post(textBox1.Text, _languagesDict[comboBox2.Text]);
                       }
                       if (comboBox1.SelectedIndex == 0)
                       {
                           foreach (var item in _languagesDict)
                           {
                               if (item.Value == fromLanguage)
                               {
                                   if (!string.IsNullOrEmpty(textBox1.Text.Replace(" ", "")))
                                   {
                                       label1.Text = $"检测到 {item.Key}";
                                   }
                                   else
                                   {
                                       label1.Text = "";
                                   }
                               }
                           }
                       }
                       else
                       {
                           label1.Text = "";
                       }
                       if (textBox1.Text.Replace(" ", "").Length == 0)
                       {
                           textBox2.Text = "";
                           speechFromTranslated = "";
                       }
                   }
                   catch { }
                   Thread.Sleep(1000);
               }
           }));
        }

        private void 必应翻译_Disposed(object sender, EventArgs e)
        {
            complete = true;
            foreach (var item in tasks)
            {
                item.Wait();
                item.Dispose();
            }
        }

        Dictionary<string, string> _languagesDict = new Dictionary<string, string>();
        private void LoadLanguage()
        {
            _languagesDict.Add("自动选择", "auto-detect");
            _languagesDict.Add("阿尔巴尼亚", "sq");
            _languagesDict.Add("阿拉伯语", "ar");
            _languagesDict.Add("阿姆哈拉语", "am");
            _languagesDict.Add("阿萨姆语", "as");
            _languagesDict.Add("阿塞拜疆语", "az");
            _languagesDict.Add("保加利亚语", "bg");
            _languagesDict.Add("波兰语", "pl");
            _languagesDict.Add("波斯尼亚语", "bs");
            _languagesDict.Add("波斯语", "fa");
            _languagesDict.Add("藏语", "bo");
            _languagesDict.Add("达里语", "prs");
            _languagesDict.Add("鞑靼语", "tt");
            _languagesDict.Add("丹麦语", "da");
            _languagesDict.Add("德语", "de");
            _languagesDict.Add("迪维希语", "dv");
            _languagesDict.Add("俄语", "ru");
            _languagesDict.Add("法语", "fr");
            _languagesDict.Add("法语 (加拿大)", "fr-CA");
            _languagesDict.Add("菲律宾语", "fil");
            _languagesDict.Add("斐济语", "fj");
            _languagesDict.Add("芬兰语", "fi");
            _languagesDict.Add("高棉语", "km");
            _languagesDict.Add("格鲁吉亚语", "ka");
            _languagesDict.Add("古吉拉特语", "gu");
            _languagesDict.Add("哈萨克语", "kk");
            _languagesDict.Add("海地克里奥尔语", "ht");
            _languagesDict.Add("韩语", "ko");
            _languagesDict.Add("荷兰语", "nl");
            _languagesDict.Add("加泰罗尼亚语", "ca");
            _languagesDict.Add("捷克语", "cs");
            _languagesDict.Add("卡纳达语", "kn");
            _languagesDict.Add("克雷塔罗奥托米语", "otq");
            _languagesDict.Add("克林贡语 (拉丁文)", "tlh-Latn");
            _languagesDict.Add("克罗地亚语", "hr");
            _languagesDict.Add("库尔德语 (北)", "kmr");
            _languagesDict.Add("库尔德语 (中)", "ku");
            _languagesDict.Add("拉脱维亚语", "lv");
            _languagesDict.Add("老挝语", "lo");
            _languagesDict.Add("立陶宛语", "lt");
            _languagesDict.Add("罗马尼亚语", "ro");
            _languagesDict.Add("马耳他语", "mt");
            _languagesDict.Add("马拉地语", "mr");
            _languagesDict.Add("马拉加斯语", "mg");
            _languagesDict.Add("马拉雅拉姆语", "ml");
            _languagesDict.Add("马来语", "ms");
            _languagesDict.Add("马其顿语", "mk");
            _languagesDict.Add("毛利语", "mi");
            _languagesDict.Add("孟加拉语", "bn");
            _languagesDict.Add("缅甸语", "my");
            _languagesDict.Add("苗语", "mww");
            _languagesDict.Add("南非荷兰语", "af");
            _languagesDict.Add("尼泊尔语", "ne");
            _languagesDict.Add("旁遮普语", "pa");
            _languagesDict.Add("葡萄牙语 (巴西)", "pt");
            _languagesDict.Add("葡萄牙语 (葡萄牙)", "pt-PT");
            _languagesDict.Add("普什图语", "ps");
            _languagesDict.Add("日语", "ja");
            _languagesDict.Add("瑞典语", "sv");
            _languagesDict.Add("萨摩亚语", "sm");
            _languagesDict.Add("塞尔维亚语 (拉丁文)", "sr-Latn");
            _languagesDict.Add("塞尔维亚语 (西里尔文)", "sr-Cyrl");
            _languagesDict.Add("书面挪威语", "nb");
            _languagesDict.Add("斯洛伐克语", "sk");
            _languagesDict.Add("斯洛文尼亚语", "sl");
            _languagesDict.Add("斯瓦希里语", "sw");
            _languagesDict.Add("塔希提语", "ty");
            _languagesDict.Add("泰卢固语", "te");
            _languagesDict.Add("泰米尔语", "ta");
            _languagesDict.Add("泰语", "th");
            _languagesDict.Add("汤加语", "to");
            _languagesDict.Add("提格利尼亚语", "ti");
            _languagesDict.Add("土耳其语", "tr");
            _languagesDict.Add("土库曼语", "tk");
            _languagesDict.Add("威尔士语", "cy");
            _languagesDict.Add("维吾尔语", "ug");
            _languagesDict.Add("乌尔都语", "ur");
            _languagesDict.Add("乌克兰语", "uk");
            _languagesDict.Add("乌兹别克语", "uz");
            _languagesDict.Add("西班牙语", "es");
            _languagesDict.Add("希伯来语", "he");
            _languagesDict.Add("希腊语", "el");
            _languagesDict.Add("匈牙利语", "hu");
            _languagesDict.Add("亚美尼亚语", "hy");
            _languagesDict.Add("意大利语", "it");
            _languagesDict.Add("因纽特语", "iu");
            _languagesDict.Add("印地语", "hi");
            _languagesDict.Add("印度尼西亚语", "id");
            _languagesDict.Add("英语", "en");
            _languagesDict.Add("尤卡特克玛雅语", "yua");
            _languagesDict.Add("粤语 (繁体)", "yue");
            _languagesDict.Add("越南语", "vi");
            _languagesDict.Add("中文 (繁体)", "zh-Hant");
            _languagesDict.Add("中文 (简体)", "zh-Hans");
            _languagesDict.Add("祖鲁语", "zu");
            _languagesDict.Add("Bashkir", "ba");
            _languagesDict.Add("Basque", "eu");
            _languagesDict.Add("Chinese (Literary)", "lzh");
            _languagesDict.Add("Faroese", "fo");
            _languagesDict.Add("Galician", "gl");
            _languagesDict.Add("Inuinnaqtun", "ikt");
            _languagesDict.Add("Inuktitut (Latin)", "iu-Latn");
            _languagesDict.Add("Kyrgyz", "ky");
            _languagesDict.Add("Mongolian (Cyrillic)", "mn-Cyrl");
            _languagesDict.Add("Mongolian (Traditional)", "mn-Mong");
            _languagesDict.Add("Somali", "so");
            _languagesDict.Add("Upper Sorbian", "hsb");
        }
        private string Post(string translateText, string to = "en")
        {
            string html = SendWithGet($"https://cn.bing.com/translator?ref=TThis&from={_languagesDict[comboBox1.Text]}&to={_languagesDict[comboBox2.Text]}&isTTRefreshQuery=1");
            Regex regex = new Regex("var params_RichTranslateHelper = (.*?);");
            string params_RichTranslateHelper = regex.Match(html).Value;
            string ig = new Regex("IG:\"(.*?)\"").Match(html).Groups[1].Value;
            string token = new Regex("\"(.*?)\"").Match(params_RichTranslateHelper).Groups[1].Value;
            string key = new Regex("[\\d]*,").Match(params_RichTranslateHelper).Value.Replace(",", "");

            string json = GetString(SendWithPost($"https://cn.bing.com/ttranslatev3?isVertical=1&&IG={ig}&IID=translator.5022", $"&fromLang={_languagesDict[comboBox1.Text]}&text={translateText}&to={to}&token={token}&key={key}"));

            regex = new Regex("\"text\":\"(.*?)\"");
            string result = regex.Match(json).Groups[1].Value.Replace("\\r\\n", "\r\n");

            regex = new Regex($"\"language\":\"(.*?)\"");
            fromLanguage = regex.Match(json).Groups[1].Value;

            regex = new Regex($"\"{_languagesDict[comboBox2.Text]}\":\"(.*?)\"");
            string fromScript = regex.Match(html).Groups[1].Value;
            string toScript = regex.Match(html).NextMatch().Groups[1].Value;

            json = GetString(SendWithPost($"https://cn.bing.com/ttransliteratev3?isVertical=1&&IG={ig}&IID=translator.5022", $"&fromLang={_languagesDict[comboBox2.Text]}&fromScript={fromScript}&toScript={toScript}&text={result}&token={token}&key={key}"));

            regex = new Regex("\"text\":\"(.*?)\"");
            speechFromTranslated = result;
            return result + "\r\n" + regex.Match(json).Groups[1].Value;
        }
        string speechFromTranslated = "";
        string fromLanguage = "";
        CookieContainer cookieContainer = new CookieContainer();
        /// <summary>
        /// 发送GET请求
        /// </summary>
        public string SendWithGet(string uri)
        {
            //创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.CookieContainer = cookieContainer;
            request.KeepAlive = true;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.4985.0 Safari/537.36 Edg/102.0.1235.0";
            request.Timeout = 5000;
            //读取返回消息
            string res = string.Empty;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                res = reader.ReadToEnd();
                reader.Close();
            }
            catch { }
            return res;
        }
        /// <summary>
        /// 发送POST请求
        /// </summary>
        public Stream SendWithPost(string uri, string postData, string contentType = "application/x-www-form-urlencoded")
        {
            byte[] dataArray = Encoding.UTF8.GetBytes(postData);
            //创建请求
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.CookieContainer = cookieContainer;
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            request.ContentType = contentType;
            request.Timeout = 5000;
            //创建输入流
            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch
            {
                return null;
            }
            //发送请求
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            //读取返回消息
            try
            {
                return request.GetResponse().GetResponseStream();
            }
            catch { }
            return null;
        }
        private string GetString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string res = reader.ReadToEnd();
            reader.Close();
            return res;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Play(textBox1.Text, fromLanguage);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox2.Items.Count < _languagesDict.Count - 1)
            {
                Play(textBox2.Text, "zh-Hans");
            }
            else
            {
                Play(speechFromTranslated, _languagesDict[comboBox2.Text]);
            }
        }
        private void Play(string text, string lang)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
                    speechSynthesizer.SelectVoice("Microsoft Huihui Desktop");
                    if (lang == "en")
                    {
                        speechSynthesizer.SelectVoice("Microsoft Zira Desktop");
                    }
                    if (lang == "ja")
                    {
                        speechSynthesizer.SelectVoice("Microsoft Haruka Desktop");
                    }
                    speechSynthesizer.SpeakAsync(text);
                }
                catch { }
            }
        }

        bool isAuto = false;
        private void button3_Click(object sender, EventArgs e)
        {
            int index1 = comboBox1.SelectedIndex;
            int index2 = comboBox2.SelectedIndex;
            if (index1 == 0)
            {
                isAuto = true;
                if (!string.IsNullOrEmpty(fromLanguage))
                {
                    comboBox1.SelectedIndex = (index2 > 0) ? index2 + 1 : 1;
                    for (int i = 0; i < comboBox2.Items.Count; i++)
                    {
                        if (_languagesDict[comboBox2.Items[i].ToString()] == fromLanguage)
                        {
                            comboBox2.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (isAuto)
                {
                    isAuto = false;
                    comboBox1.SelectedIndex = 0;
                    comboBox2.SelectedIndex = (index1 > 0) ? index1 - 1 : 1;
                }
                else
                {
                    comboBox1.SelectedIndex = (index2 > 0) ? index2 + 1 : 1;
                    comboBox2.SelectedIndex = (index1 > 0) ? index1 - 1 : 1;
                }
            }
            textBox1.Text = speechFromTranslated;
            textBox2.Text = Post(textBox1.Text, _languagesDict[comboBox2.Text]);
        }
    }
}
