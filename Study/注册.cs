using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows.Forms;

namespace Study
{
    public partial class 注册 : Form
    {
        public 注册()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (DESDecrypt(textBox1.Text, "46372839", "46372839") == Program.RegistrantMachineCode)
                {
                    Registry.SetValue(Registry.CurrentUser.Name + "\\Software\\Study", "Registrant1", Encoding.Unicode.GetString(Encoding.UTF8.GetBytes(@"\T\r\u\e")));
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                        FileName = Application.ExecutablePath,
                    };
                    try
                    {
                        Process.Start(startInfo);
                        Environment.Exit(1);
                    }
                    catch { }
                }
                else
                {
                    MessageBox.Show("验证失败。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("验证失败。", "注册", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static string DESEncrypt(string data, string key, string iv)
        {
            byte[] byKey = Encoding.ASCII.GetBytes(key);
            byte[] byIV = Encoding.ASCII.GetBytes(iv);
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }
        public static string DESDecrypt(string data, string key, string iv)
        {
            byte[] byKey = Encoding.ASCII.GetBytes(key);
            byte[] byIV = Encoding.ASCII.GetBytes(iv);
            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
        public static string GetComputerBit(string softname)
        {
            string bIOSSerialNumber = GetBIOSSerialNumber();
            string hardDiskSerialNumber = GetHardDiskSerialNumber();
            string netCardMACAddress = GetNetCardMACAddress();
            string cpuID = GetCpuID();

            var ComputerBit = bIOSSerialNumber + hardDiskSerialNumber + netCardMACAddress + cpuID;
            MD5 md4 = new MD5CryptoServiceProvider();
            ComputerBit = BitConverter.ToString(md4.ComputeHash(Encoding.Default.GetBytes(ComputerBit))).Replace("-", "").ToUpper().Substring(8, 0x10);
            return softname + ComputerBit;
        }
        public static string GetCpuID()
        {
            try
            {
                ManagementObjectCollection instances = new ManagementClass("Win32_Processor").GetInstances();
                string str = null;
                foreach (ManagementObject obj2 in instances)
                {
                    str = obj2.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                return str;
            }
            catch
            {
                return "";
            }
        }
        public static string GetHardDiskSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                string str = "";
                foreach (ManagementObject obj2 in searcher.Get())
                {
                    str = obj2["SerialNumber"].ToString().Trim();
                    break;
                }
                return str;
            }
            catch
            {
                return "";
            }
        }
        public static string GetNetCardMACAddress()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE ((MACAddress Is Not NULL) AND (Manufacturer <> 'Microsoft'))");
                string str = "";
                foreach (ManagementObject obj2 in searcher.Get())
                {
                    str = obj2["MACAddress"].ToString().Trim();
                }
                return str;
            }
            catch
            {
                return "";
            }
        }
        public static string GetBIOSSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select SerialNumber From Win32_BIOS");
                string str = "";
                foreach (ManagementObject obj2 in searcher.Get())
                {
                    str = obj2["SerialNumber"].ToString().Trim();
                }
                return str;
            }
            catch
            {
                return "";
            }
        }
    }
}
