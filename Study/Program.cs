/***********************************************************************/
//  Creation: 4/24/2022 10:38 AM
//  Repo: Study
/***********************************************************************/
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Study.爬虫;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Reflection;
using Study.数据库;

namespace Study
{
    internal static class Program
    {
        public static Image BackgroundImage = null;
        public static bool Register = false;
        public static bool TryRegister = false;
        public static string RegisterCode = "";
        public static string RegistrantContent = "";
        public static string RegistrantMachineCode = "";
        public static bool CanRunVirus = false;
        public static bool CanContinue = true;
        public static int FormRunCout = 0;
        public const string VERSION = "13.42.8.2477";
        public const string Version = "v" + VERSION;
        public static 工具.BackgroundJson BackgroundJsonSetting = null;
        public static IntPtr DesktopIntptr = IntPtr.Zero;
        public static IntPtr DSDll = IntPtr.Zero;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //SqlClient sqlClient = new SqlClient("Server=124.222.27.9;Database=hackerover_xyfi.Uid=hackerover_xyfi;Password=6SmaPYX6Mn");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Directory.CreateDirectory(Path.GetTempPath() + "Study");
            try
            {
                #region AboutDLL
                try
                {
                    AddFile("C:\\Windows\\System32\\ucrtbased.dll", Properties.Resources.ucrtbased);
                    AddFile("C:\\Windows\\System32\\vcruntime140d.dll", Properties.Resources.vcruntime140d);
                    AddFile(Path.GetTempPath() + "\\Study\\GetHandleNative.dll", Convert.FromBase64String(Resources.FILE_DS));
                }
                catch
                {
                    MessageBox.Show("您的设备上缺少C++运行库，请以管理员权限启动以补全相关运行库。","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
                if (File.Exists(Path.GetTempPath() + "\\Study\\GetHandleNative.dll"))
                {
                    DSDll = LoadLibrary(Path.GetTempPath() + "\\Study\\GetHandleNative.dll");
                    DesktopIntptr = GetDesktopWindowHandle();
                    if (DesktopIntptr == IntPtr.Zero) DesktopIntptr = GetDesktopWindow();
                }
                IntPtr mainWindowHandle = FindWindow(null, "工具大全 - C#");
                if (mainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(mainWindowHandle, 9);
                    SwitchToThisWindow(mainWindowHandle, true);
                    return;
                }
                #endregion
                #region AboutRegedit
                Check();
                #endregion
                #region AboutBackground
                if (BackgroundJsonSetting == null)
                {
                    if (!File.Exists(Path.GetTempPath() + "Study\\Settings.json"))
                    {
                        工具.BackgroundJson backgroundJson = new 工具.BackgroundJson
                        {
                            BackgroundMode = 1,
                            PetMode = 1
                        };
                        File.WriteAllText(Path.GetTempPath() + "Study\\Settings.json", Json<工具.BackgroundJson>.WriteJson(backgroundJson));
                    }
                }
                BackgroundJsonSetting = Json<工具.BackgroundJson>.ReadJson(File.ReadAllText(Path.GetTempPath() + "Study\\Settings.json"));
                switch (BackgroundJsonSetting.PetMode)
                {
                    case 0:
                        new 宠物.鱼().Show();
                        break;
                    case 1:
                        break;
                    default:
                        new 宠物.鱼().Show();
                        break;
                }
                switch (BackgroundJsonSetting.BackgroundMode)
                {
                    case 0:
                        using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(Resources.ICON_BACKGROUND)))
                        {
                            BackgroundImage = Image.FromStream(memoryStream);
                            memoryStream.Close();
                        }
                        break;
                    case 1:
                        if (!File.Exists(Path.GetTempPath() + "Study\\bg.jpeg.tmp"))
                        {
                            File.WriteAllText(Path.GetTempPath() + "Study\\bg.jpeg.tmp", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString())), Encoding.Unicode);
                        }
                        long.TryParse(Encoding.UTF8.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(Path.GetTempPath() + "Study\\bg.jpeg.tmp"))), out long ticks);
                        DateTime lastTime = new DateTime(ticks);
                        if (!File.Exists(Path.GetTempPath() + "Study\\bg.jpeg") || lastTime.Day != DateTime.Now.Day)
                        {
                            File.WriteAllText(Path.GetTempPath() + "Study\\bg.jpeg.tmp", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString())), Encoding.Unicode);
                            try
                            {
                                WebRequest request = WebRequest.Create("https://cn.bing.com");
                                request.Method = "GET";
                                StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream());
                                request = WebRequest.Create(new Regex("style=\"background-image: url\\((.*?)\\)").Match(streamReader.ReadToEnd()).Groups[1].Value);
                                request.Method = "GET";
                                streamReader.Close();
                                streamReader.Dispose();
                                BackgroundImage = Image.FromStream(request.GetResponse().GetResponseStream());
                                BackgroundImage.Save(Path.GetTempPath() + "Study\\bg.jpeg");
                                request.Abort();
                            }
                            catch
                            {
                                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(Resources.ICON_BACKGROUND)))
                                {
                                    BackgroundImage = Image.FromStream(memoryStream);
                                    memoryStream.Close();
                                }
                            }
                        }
                        if (File.Exists(Path.GetTempPath() + "Study\\bg.jpeg"))
                        {
                            BackgroundImage = Image.FromFile(Path.GetTempPath() + "Study\\bg.jpeg");
                        }
                        break;
                    case 2:
                        if (File.Exists(BackgroundJsonSetting.BackgroundPath))
                        {
                            BackgroundImage = Image.FromFile(BackgroundJsonSetting.BackgroundPath);
                        }
                        else
                        {
                            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(Resources.ICON_BACKGROUND)))
                            {
                                BackgroundImage = Image.FromStream(memoryStream);
                                memoryStream.Close();
                            }
                        }
                        break;
                    default:
                        using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(Resources.ICON_BACKGROUND)))
                        {
                            BackgroundImage = Image.FromStream(memoryStream);
                            memoryStream.Close();
                        }
                        break;
                }
                #endregion
                #region AboutDW
                if (!BackgroundJsonSetting.DWPath.Trim().StartsWith("无"))
                {
                    if (File.Exists(BackgroundJsonSetting.DWPath))
                    {
                        动态壁纸.VideoWindow videoWindow = new 动态壁纸.VideoWindow();
                        videoWindow.SetPosition(Screen.PrimaryScreen.Bounds);
                        videoWindow.Show();
                        SetParent(videoWindow.GetHandle(), DesktopIntptr);
                        videoWindow.Source = new Uri(BackgroundJsonSetting.DWPath);
                        videoWindow.Volume = BackgroundJsonSetting.DWVolume;
                        videoWindow.Play();
                        ShowWindow(DesktopIntptr, 1);
                    }
                }
                #endregion
                #region AboutArgs
                List<Form> formList = new List<Form>();
                foreach (var item in args)
                {
                    if (item.Contains("-病毒模式"))
                    {
                        CanRunVirus = true;
                        Adm();
                    }
                    if (item.Contains("-提取抖音视频"))
                    {
                        if (Register || TryRegister)
                        {
                            formList.Add(new 爬虫_抓取抖音视频.Form1());
                        }
                        else
                        {
                            MessageBox.Show("您尚未注册，提取抖音视频不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        CanContinue = false;
                    }
                    if (item.Contains("-C#中文编程"))
                    {
                        formList.Add(new 中文编程.Form1());
                        CanContinue = false;
                    }
                    if (item.Contains("-必应翻译"))
                    {
                        if (Register || TryRegister)
                        {
                            formList.Add(new 翻译.必应翻译());
                        }
                        else
                        {
                            MessageBox.Show("您尚未注册，必应翻译不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        CanContinue = false;
                    }
                    if (item.Contains("-正版软件下载"))
                    {
                        if (Register || TryRegister)
                        {
                            formList.Add(new 软件下载.Form1());
                        }
                        else
                        {
                            MessageBox.Show("您尚未注册，正版软件下载不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        CanContinue = false;
                    }
                    if (item.Contains("-加密文件"))
                    {
                        formList.Add(new 加密与解密.Form1());
                        CanContinue = false;
                    }
                    if (item.Contains("-搜图神器"))
                    {
                        if (Register || TryRegister)
                        {
                            formList.Add(new 爬虫_搜图.Form1());
                        }
                        else
                        {
                            MessageBox.Show("您尚未注册，搜图神器不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        CanContinue = false;
                    }
                    if (item.Contains("-记事本"))
                    {
                        Process.Start("notepad.exe");
                        CanContinue = false;
                    }
                    if (item.Contains("-图片转换"))
                    {
                        if (Register || TryRegister)
                        {
                            formList.Add(new 加密与解密.图片转换());
                        }
                        else
                        {
                            MessageBox.Show("您尚未注册，图片转换不允许使用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        CanContinue = false;
                    }
                    if (item.Contains("-学习资源"))
                    {
                        formList.Add(new Learning.Form1());
                        CanContinue = false;
                    }
                }
                foreach (var item in formList)
                {
                    FormRunCout++;
                    item.Disposed += (s, e) =>
                    {
                        FormRunCout--;
                        if (FormRunCout <= 0)
                        {
                            Environment.Exit(1);
                        }
                    };
                    MenuStrip menuStrip = new MenuStrip();
                    ToolStripMenuItem menuItem = new ToolStripMenuItem()
                    {
                        Text = "帮助"
                    };
                    ToolStripMenuItem menuItem2 = new ToolStripMenuItem()
                    {
                        Text = "关于"
                    };
                    menuItem2.Click += (o, e) =>
                    {
                        MessageBox.Show("本软件由 特别中二的Jal君 制作。\nCopyright ©Jal  2022", $"关于 {Version}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };
                    menuItem.DropDownItems.Add(menuItem2);
                    menuStrip.Items.Add(menuItem);
                    bool canAdd = true;
                    foreach (var fc in item.Controls)
                    {
                        if (fc.GetType() == typeof(MenuStrip))
                        {
                            canAdd = false;
                            break;
                        }
                    }
                    if (canAdd) item.Controls.Add(menuStrip);
                    item.Show();
                }
                if (!CanContinue)
                {
                    if (formList.Count == 0) return;
                    Application.Run();
                }
                #endregion
                #region AboutInternet
                ServicePointManager.DefaultConnectionLimit = 1000;
                #endregion
            }
            catch (Exception ex)
            {
                ShowWindow(DesktopIntptr, 0);
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #region AboutMainWindows
            Form1 form1 = new Form1();
            form1.panel1.BackColor = Color.Transparent;
            form1.BackgroundImage = BackgroundImage;
            form1.BackgroundImageLayout = ImageLayout.Stretch;
            Application.ThreadException += Application_ThreadException;
            Application.Run(form1);
            #endregion
        }
        private static void AddFile(string path, byte[] bytes)
        {
            if (!File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(Application.ExecutablePath);
            Process.Start(processStartInfo);
        }

        private static void Check()
        {
            if (Registry.GetValue(Registry.CurrentUser.Name + "\\Software\\Study", "Registrant1", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(@"\F\a\l\s\e"))) == null)
            {
                MessageBox.Show("程序未注册，请联系 @特别中二的Jal君 管理员进行注册。\r\n若没注册，您将获得 1 天的试用期，试用结束后，无法使用更多功能。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Registry.SetValue(Registry.CurrentUser.Name + "\\Software\\Study", "Registrant1", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(@"\F\a\l\s\e")));
                Registry.SetValue(Registry.CurrentUser.Name + "\\Software\\Study", "Registrant2", Instant(DateTime.Now.Ticks.ToString("X")));
                Registry.SetValue(Registry.CurrentUser.Name + "\\Software\\Study", "Registrant3", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(注册.GetComputerBit(""))));
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Study");
                RegistrantContent = Encoding.UTF8.GetString(Encoding.Unicode.GetBytes((string)registryKey.GetValue("Registrant2")));
                RegistrantMachineCode = Encoding.UTF8.GetString(Encoding.Unicode.GetBytes((string)registryKey.GetValue("Registrant3")));
                registryKey.Close();
                registryKey.Dispose();
            }
            else
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Study", true);
                if (registryKey.GetValue("Registrant1") == null || registryKey.GetValue("Registrant2") == null || registryKey.GetValue("Registrant3") == null)
                {
                    MessageBox.Show("程序未注册，请联系 @特别中二的Jal君 管理员进行注册。\r\n若没注册，您将获得 1 天的试用期，试用结束后，无法使用更多功能。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    registryKey.SetValue("Registrant1", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(@"\F\a\l\s\e")));
                    registryKey.SetValue("Registrant2", Instant(DateTime.Now.Ticks.ToString("X")));
                    registryKey.SetValue("Registrant3", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(注册.GetComputerBit(""))));
                }
                if (!bool.TryParse(Encoding.UTF8.GetString(Encoding.Unicode.GetBytes((string)registryKey.GetValue("Registrant1"))).Replace("\\", ""), out Register))
                {
                    MessageBox.Show("您修改了程序数据，程序强制退出。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(1);
                }
                RegistrantContent = Encoding.UTF8.GetString(Encoding.Unicode.GetBytes((string)registryKey.GetValue("Registrant2")));
                RegistrantMachineCode = Encoding.UTF8.GetString(Encoding.Unicode.GetBytes((string)registryKey.GetValue("Registrant3")));
                registryKey.Close();
                registryKey.Dispose();
            }
            if (RegistrantMachineCode != 注册.GetComputerBit(""))
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Study", true);
                MessageBox.Show("您的注册码与您的本机注册码不匹配，程序强制退出。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                registryKey.SetValue("Registrant3", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(注册.GetComputerBit(""))));
                Environment.Exit(1);
            }
            long time = Convert.ToInt64(Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(GetBytes(RegistrantContent)))), 16);
            RegisterCode = $"{time}";
            DateTime dateTime = new DateTime(time);
            if (DateTime.Now.Ticks > dateTime.AddDays(1).Ticks && !Register)
            {
                Register = false;
                MessageBox.Show("程序试用已过期，无法使用更多功能。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (!Register)
                {
                    TryRegister = true;
                    MessageBox.Show("程序正在试用。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private static byte[] GetBytes(string content)
        {
            content = new Regex("[/\\d]*").Match(content).Groups[0].Value;
            byte[] to = new byte[content.Split('/').Length];
            for (int i = 0; i < content.Split('/').Length; i++)
            {
                byte.TryParse(content.Split('/')[i], out to[i]);
            }
            return to;
        }
        private static string Instant(string content)
        {
            byte[] vs = Encoding.ASCII.GetBytes(content);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in vs)
            {
                stringBuilder.Append(item + "/");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        }
        /// <summary>
        /// 开启管理员权限
        /// </summary>
        public static void Adm()
        {
            // 检查是否为管理员，如果是则退出，否则继续执行
            if (IsAdministrator)
            {
                return;
            }
            // 配置管理员权限
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = Application.ExecutablePath,
                Arguments = "-病毒模式",
                Verb = "runas"
            };
            try
            {
                // 启动管理员权限
                Process.Start(startInfo);
                // 退出程序，但由于管理员权限是另一进程的，所以退出的只是本程序
                Environment.Exit(1);
            }
            catch
            {
                // 如果抛出异常，也就是当你选择否的时候，则退出程序
                //Environment.Exit(1);
            }
        }
        /// <summary>
        /// 检查是否为管理员权限
        /// </summary>
        internal static bool IsAdministrator
        {
            get
            {
                // 声明Windows用户
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                // 运行identity组成成员
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                // 返回是否与指定的权限匹配
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        /// <summary>
        /// 当前执行文件的详细路径
        /// </summary>
        internal static string CurrentExecutablePath
        {
            get { return Application.ExecutablePath; }
        }
        /// <summary>
        /// 当前执行文件的路径
        /// </summary>
        internal static string CurrentPath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }
        /// <summary>
        /// 当前用户的桌面路径
        /// </summary>
        internal static string DesktopPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\"; }
        }
        /// <summary>
        /// 当前用户的下载路径
        /// </summary>
        internal static string DownloadPath
        {
            get
            {
                SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, IntPtr.Zero, out string downloads);
                return downloads + "\\";
            }
        }
        /// <summary>
        /// 当前用户的下载路径
        /// </summary>
        internal static string VideosPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\"; }
        }
        /// <summary>
        /// Windows没有为“Downloads”文件夹定义CSIDL，所以无法通过Environment.SpecialFolder枚举来获取。
        /// 请注意，这是一个Vista和更高版本的API，不要试图在XP / 2003或更低版本上调用它。
        /// 导入shell32.dll，获取Downloads文件夹起源。
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);
        /// <summary>
        /// 激活窗口
        /// </summary>
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(HandleRef hWnd, GetWindowCmd uCmd);
        [DllImport("user32.dll")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("User32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc hWnd, IntPtr lParam);
        [DllImport("User32.dll")]
        public static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint timeout,out IntPtr lpdwResult);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll")]
        public extern static IntPtr LoadLibrary(string path);
        [DllImport("GetHandleNative.dll", EntryPoint = "DS2_GetDesktopWindowHandle")]
        public static extern IntPtr GetDesktopWindowHandle();
        public static IntPtr GetHandle(this Window @this)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(@this);
            return windowInteropHelper.Handle;
        }

        public static void FullScreen(this Window @this)
        {
            @this.WindowStyle = WindowStyle.None;
            @this.ResizeMode = ResizeMode.NoResize;
            @this.Left = 0;
            @this.Top = 0;
            @this.Width = SystemParameters.PrimaryScreenWidth;
            @this.Height = SystemParameters.PrimaryScreenHeight;
        }

        public static void SetPosition(this Window @this, Rectangle rect)
        {
            @this.WindowStyle = WindowStyle.None;
            @this.ResizeMode = ResizeMode.NoResize;
            @this.Left = rect.Left;
            @this.Top = rect.Top;
            @this.Width = rect.Width;
            @this.Height = rect.Height;
        }
    }
    public enum GetWindowCmd : uint
    {
        /// <summary>
        /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
        /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
        /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
        /// </summary>
        GW_HWNDFIRST = 0,
        /// <summary>
        /// 返回的句柄标识了在z序最低端的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
        /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
        /// </summary>
        GW_HWNDLAST = 1,
        /// <summary>
        /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
        /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
        /// </summary>
        GW_HWNDNEXT = 2,
        /// <summary>
        /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
        /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
        /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
        /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
        /// </summary>
        GW_HWNDPREV = 3,
        /// <summary>
        /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
        /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
        /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
        /// </summary>
        GW_OWNER = 4,
        /// <summary>
        /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
        /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
        /// </summary>
        GW_CHILD = 5,
        /// <summary>
        /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
        /// 如果无使能窗口，则获得的句柄与指定窗口相同。
        /// </summary>
        GW_ENABLEDPOPUP = 6
    }
}
