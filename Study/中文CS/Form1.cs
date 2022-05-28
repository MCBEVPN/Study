using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace 中文编程
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            textBox1.Text = "/******************** 示例代码 ********************/\r\n公共成员 类 Program\r\n{\r\n    公共成员 静态成员 void Main()\r\n        {\r\n            字符串 outputStr = \"Hello World!\";\r\n            消息.弹出(outputStr);\r\n         }\r\n}";
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
            this.BackgroundImage = Study.Program.BackgroundImage;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            _Form1 = this;
            RePlace.Start();
        }

        public static Form1 _Form1;
        StringBuilder StringBuilder = new StringBuilder();
        private void button1_Click(object sender, EventArgs e)
        {
            // 编译器处理段
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParameters.GenerateExecutable = true;
            compilerParameters.GenerateInMemory = true;
            // 代码
            StringBuilder.Append(textBox1.Text);
            StringBuilder.Insert(0, "using 中文编程;\n");
            StringBuilder.Insert(0, "using System.Windows.Forms;\n");
            StringBuilder.Insert(0, "using System;\n");
            // 开始编译
            CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParameters, RePlace.Execute(StringBuilder.ToString(), false));
            // 检测是否有误
            Regex regexNamespace = new Regex("namespace\\s+[\\S\\r\\n]*[\\s]*{");
            Regex regexClass = new Regex("class\\s+[\\S\\r\\n]*[\\s]*{");
            string oneNamespace = regexNamespace.Match(RePlace.Execute(StringBuilder.ToString(), false)).Groups[0].Value.Replace("namespace", "").Replace(" ", "").Replace("{", "").Replace("\r\n", "");
            string oneClass = regexClass.Match(RePlace.Execute(StringBuilder.ToString(), false)).Groups[0].Value.Replace("class", "").Replace(" ", "").Replace("{", "").Replace("\r\n", "");
            if (compilerResults.Errors.HasErrors)
            {
                foreach (CompilerError item in compilerResults.Errors)
                {
                    OutPut(item.ErrorText);
                }
            }
            else
            {
                Assembly assembly = compilerResults.CompiledAssembly;
                try
                {
                    object obj = assembly.CreateInstance($"{oneNamespace}.{oneClass}");
                    MethodInfo methodInfo = obj.GetType().GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    methodInfo.Invoke(obj, null);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("未找到类型"))
                    {
                        OutPut("第一个类不能静态");
                    }
                    else
                    {
                        OutPut(ex.Message);
                    }
                }
                foreach (string item in compilerResults.Output)
                {
                    OutPut(item);
                }
            }
            StringBuilder.Clear();
        }
        /// <summary>
        /// 输出
        /// </summary>
        public static void OutPut(string text)
        {
            _Form1.textBox2.Text += $"[{DateTime.Now}]\t{text}\r\n";
            _Form1.button2.Enabled = true;
        }
        /// <summary>
        /// 当代码输入框输入时发生
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox _textBox = (TextBox)sender;
            if (_textBox.Text.Length != 0)
            {
                // 如果可以，则序列化代码
                if (canChange)
                {
                    canChange = false;
                    StringBuilder.Append(RePlace.Execute(_textBox.Text, false));
                    _textBox.Text = RePlace.Execute(_textBox.Text, true);
                    StringBuilder.Clear();
                }
            }
        }
        bool canChange = false;
        /// <summary>
        /// 当代码输入的时后执行
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 检测符号
            if (e.KeyChar == '}' || e.KeyChar == ';')
            {
                canChange = true;
            }
        }
        /// <summary>
        /// 清空按钮
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            _Form1.textBox2.Clear();
            _Form1.button2.Enabled = false;
        }
    }
    static class RePlace
    {
        /// <summary>
        /// 声明序列化关键字集合
        /// </summary>
        public static List<CharKeys> charKeys1 = new List<CharKeys>();
        /// <summary>
        /// 声明反序列化关键字集合
        /// </summary>
        public static List<CharKeys> charKeys2 = new List<CharKeys>();
        public static void Start()
        {
            // 添加关键字（序列化）
            charKeys1.Add(new CharKeys() { _old = "命名空间 ", _new = "namespace " });
            charKeys1.Add(new CharKeys() { _old = "类 ", _new = "class " });
            charKeys1.Add(new CharKeys() { _old = "非公共成员 ", _new = "private " });
            charKeys1.Add(new CharKeys() { _old = "公共成员 ", _new = "public " });
            charKeys1.Add(new CharKeys() { _old = "静态成员 ", _new = "static " });
            charKeys1.Add(new CharKeys() { _old = "新实例 ", _new = "new " });
            charKeys1.Add(new CharKeys() { _old = "项目内成员 ", _new = "internal " });
            charKeys1.Add(new CharKeys() { _old = "此类 ", _new = "this " });
            charKeys1.Add(new CharKeys() { _old = "字符 ", _new = "char " });
            charKeys1.Add(new CharKeys() { _old = "字符串 ", _new = "string " });
            charKeys1.Add(new CharKeys() { _old = "布尔 ", _new = "bool " });
            charKeys1.Add(new CharKeys() { _old = "字符[", _new = "char[" });
            charKeys1.Add(new CharKeys() { _old = "字符串[", _new = "string[" });
            charKeys1.Add(new CharKeys() { _old = "布尔[", _new = "bool[" });
            charKeys1.Add(new CharKeys() { _old = "清单<", _new = "List<" });
            charKeys1.Add(new CharKeys() { _old = "//  目前不支持  Console.Read(", _new = "Console.Read(" });
            charKeys1.Add(new CharKeys() { _old = "//  目前不支持  Console.ReadKey(", _new = "Console.ReadKey(" });
            charKeys1.Add(new CharKeys() { _old = "//  目前不支持  Console.ReadLine(", _new = "Console.ReadLine(" });
            // 反序列化
            foreach (var item in charKeys1)
            {
                charKeys2.Add(new CharKeys() { _old = item._new, _new = item._old });
            }
        }
        public static string Execute(string text, bool nat)
        {
            // 表示是否序列化
            if (!nat)
            {
                foreach (var item in charKeys1)
                {
                    text = text.Replace(item._old, item._new);
                }
            }
            else
            {
                foreach (var item in charKeys2)
                {
                    text = text.Replace(item._old, item._new);
                }
            }
            return text;
        }
    }
    /// <summary>
    /// 序列化结构
    /// </summary>
    struct CharKeys
    {
        public string _old;
        public string _new;
    }
}
namespace 中文编程
{
    public class 消息
    {
        public static void 输出(string text)
        {
            Form1.OutPut(text);
        }
        public static void 弹出(string text)
        {
            MessageBox.Show(text);
        }
    }
}
