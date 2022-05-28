using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.蓝屏
{
    public partial class BlueScreen
    {
        /// <summary>
        /// 蓝屏起源
        /// </summary>
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
        /// <summary>
        /// 开始蓝屏
        /// </summary>
        public static void Run()
        {
            if (!Program.IsAdministrator)
            {
            a: Program.Adm();
                DialogResult dialogResult = MessageBox.Show("管理员权限已被取消。", "蓝屏", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
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
            int isCritical = 1;
            int BreakOnTermination = 0x1D;
            // 启用输出模式
            Process.EnterDebugMode();
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
        }
    }
}
