using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Study.防撤回
{
    internal class App
    {
        public string Name { get; set; }

        public Dictionary<string, TargetInfo> FileTargetInfos { get; set; }

        public Dictionary<string, List<ModifyInfo>> FileModifyInfos { get; set; }

        /// <summary>
        /// 通用的修改特征
        /// </summary>
        public Dictionary<string, List<CommonModifyInfo>> FileCommonModifyInfos { get; set; }

        public HashSet<string> GetSupportVersions()
        {
            // 使用 HashSet 防重
            HashSet<string> versions = new HashSet<string>();
            // 精准
            if (FileModifyInfos != null)
            {
                foreach (List<ModifyInfo> modifyInfos in FileModifyInfos.Values)
                {
                    foreach (ModifyInfo modifyInfo in modifyInfos)
                    {
                        versions.Add(modifyInfo.Version);
                    }
                }
            }
            // 模糊 范围
            if (FileCommonModifyInfos != null)
            {
                foreach (List<CommonModifyInfo> commonModifyInfos in FileCommonModifyInfos.Values)
                {
                    foreach (CommonModifyInfo commonModifyInfo in commonModifyInfos)
                    {
                        string end = string.IsNullOrEmpty(commonModifyInfo.EndVersion) ? "最新版" : commonModifyInfo.EndVersion;
                        versions.Add(commonModifyInfo.StartVersion + "~" + end);
                    }
                }
            }
            return versions;
        }

        public string GetSupportVersionStr()
        {
            string str = "";
            foreach (string v in GetSupportVersions())
            {
                str += v + "、";
            }
            if (str.Length > 1)
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }
    internal abstract class AppModifier
    {
        protected App config;

        public App Config { set { config = value; } get { return config; } }

        protected List<FileHexEditor> editors;

        public string InstallPath { get; set; }

        /// <summary>
        /// 自动搜索应用安装路径
        /// </summary>
        /// <returns>应用安装路径</returns>
        public abstract string FindInstallPath();

        //public abstract bool ValidateAndInitialize(string installPath);

        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        public abstract string GetVersion();

        /// <summary>
        /// 操作版本号显示控件的内容和样式
        /// </summary>
        /// <param name="label">显示版本的控件</param>
        public void SetVersionLabelAndCategoryCategories(Label label, System.Windows.Forms.Panel panel)
        {
            string version = GetVersion();
            // 补丁信息中是否都有对应的版本
            int i = 0, j = 0;

            // 精确版本匹配
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 精确版本匹配
                bool haven = false;
                foreach (ModifyInfo modifyInfo in config.FileModifyInfos[editor.FileName]) // 多个版本信息
                {
                    if (editor.FileVersion == modifyInfo.Version)
                    {
                        haven = true;
                        break;
                    }
                }
                if (haven)
                {
                    i++;
                }
            }

            if (i == editors.Count)
            {
                label.Text = version + "（已支持）";
                label.ForeColor = Color.White;
                UIController.AddMsgToPanel(panel, "只有基于特征的补丁才能选择功能");
                return;
            }

            // 模糊版本匹配（特征码）
            // 特征码匹配的时候的可选功能项
            SortedSet<string> categories = new SortedSet<string>();
            SortedSet<string> installed = new SortedSet<string>();
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 匹配出对应版本是否有可以使用的特征
                if (config.FileCommonModifyInfos != null)
                {
                    bool inRange = false;
                    foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
                    {
                        // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                        if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                        {
                            // 取出特征码的功能类型
                            foreach (string c in commonModifyInfo.GetCategories())
                            {
                                if (c != null)
                                {
                                    categories.Add(c);
                                }
                            }
                            // 获取已经安装过的功能类型
                            SortedSet<string> replaced = ModifyFinder.FindReplacedFunction(editor.FilePath, commonModifyInfo.ReplacePatterns);
                            foreach (string c in replaced)
                            {
                                installed.Add(c);
                            }
                            inRange = true;
                            break;
                        }
                    }
                    if (inRange)
                    {
                        j++;
                    }
                }
            }

            // 全部都有对应匹配的版本
            if (j == editors.Count)
            {
                label.Text = version + "（支持防撤回）";
                label.ForeColor = Color.White;
                UIController.AddCategoryCheckBoxToPanel(panel, categories.ToArray(), installed.ToArray());
            }
            else
            {
                label.Text = version + "（不支持防撤回）";
                label.ForeColor = Color.Red;
                UIController.AddMsgToPanel(panel, "无功能选项");
            }

        }

        /// <summary>
        /// 判断APP安装路径内是否都存在要修改的文件
        /// </summary>
        /// <param name="installPath">APP安装路径</param>
        /// <returns></returns>
        public bool IsAllFilesExist(string installPath)
        {
            if (string.IsNullOrEmpty(installPath))
            {
                return false;
            }
            int success = 0;
            foreach (TargetInfo info in config.FileTargetInfos.Values)
            {
                string filePath = Path.Combine(installPath, info.RelativePath);
                if (File.Exists(filePath))
                {
                    success++;
                }
            }
            if (success == config.FileTargetInfos.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断版本是否处于版本范围，特殊版本的可以重载此方法
        /// </summary>
        /// <param name="version">当前版本</param>
        /// <param name="start">起始版本</param>
        /// <param name="end">结束版本,为空为不限制</param>
        /// <returns></returns>
        public bool IsInVersionRange(string version, string start, string end)
        {
            try
            {
                if (VersionUtil.Compare(version, start) == 1 && VersionUtil.Compare(version, end) <= 0)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("判断版本范围时出错：" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// 寻找版本对应的特征码信息
        /// </summary>
        /// <param name="editor">文件编辑器</param>
        private CommonModifyInfo FindCommonModifyInfo(FileHexEditor editor)
        {
            foreach (CommonModifyInfo commonModifyInfo in config.FileCommonModifyInfos[editor.FileName])
            {
                // editor.FileVersion 在 StartVersion 和 EndVersion 之间
                if (IsInVersionRange(editor.FileVersion, commonModifyInfo.StartVersion, commonModifyInfo.EndVersion))
                {
                    Console.WriteLine($"{commonModifyInfo.StartVersion}<{editor.FileVersion}<={commonModifyInfo.EndVersion}");
                    return commonModifyInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// 文件修改器是否已经有对应的特征码修改替换信息
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public bool EditorsHasCommonModifyInfos()
        {
            int i = 0;
            for (i = 0; i < editors.Count; i++) // 多种文件
            {
                if (editors[i].FileCommonModifyInfo == null)
                {
                    break;
                }
            }
            if (i == editors.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// a.初始化修改器
        /// </summary>
        /// <param name="installPath">APP安装路径</param>
        public void InitEditors(string installPath)
        {
            // 初始化文件修改器
            editors = new List<FileHexEditor>();
            foreach (TargetInfo info in config.FileTargetInfos.Values)
            {
                FileHexEditor editor = new FileHexEditor(installPath, info);
                editors.Add(editor);
            }
        }

        /// <summary>
        /// b.验证文件完整性，寻找对应的补丁信息
        /// </summary>
        /// <param name="categories">操作类型（防撤回或者多开等）,为空则是所有操作</param>
        public void ValidateAndFindModifyInfo(List<string> categories)
        {
            // 寻找对应文件版本与SHA1的修改信息
            foreach (FileHexEditor editor in editors) // 多种文件
            {
                // 通过SHA1和文件版本判断是否可以打补丁 根据不同结果返回不同的提示
                ModifyInfo matchingSHA1Before = null, matchingSHA1After = null, matchingVersion = null;
                foreach (ModifyInfo modifyInfo in config.FileModifyInfos[editor.FileName]) // 多个版本信息
                {
                    if (modifyInfo.Name == editor.FileName) // 保险用的无用判断
                    {
                        if (editor.FileSHA1 == modifyInfo.SHA1After)
                        {
                            matchingSHA1After = modifyInfo;
                        }
                        else if (editor.FileSHA1 == modifyInfo.SHA1Before)
                        {
                            matchingSHA1Before = modifyInfo;
                        }

                        if (editor.FileVersion == modifyInfo.Version)
                        {
                            matchingVersion = modifyInfo;
                        }
                    }
                }

                // 补丁前SHA1匹配上，肯定是正确的dll
                if (matchingSHA1Before != null)
                {
                    editor.FileModifyInfo = matchingSHA1Before;
                    editor.TargetChanges = matchingSHA1Before.Changes;
                    continue;
                }
                // 补丁后SHA1匹配上，肯定已经打过补丁
                if (matchingSHA1After != null)
                {
                    throw new BusinessException("installed", $"你已经安装过此补丁！");
                }

                // SHA1不匹配说明精准替换肯定不支持
                if (matchingSHA1Before == null && matchingSHA1After == null)
                {
                    // 尝试使用特征码替换
                    // 多个版本范围，匹配出对应版本可以使用的特征
                    if (config.FileCommonModifyInfos != null)
                    {
                        editor.FileCommonModifyInfo = FindCommonModifyInfo(editor);
                    }

                    // 存在对应的特征时不报错
                    if (editor.FileCommonModifyInfo != null && editor.FileCommonModifyInfo.ReplacePatterns != null)
                    {
                        List<ReplacePattern> replacePatterns = editor.FileCommonModifyInfo.ReplacePatterns;
                        // 根据需要操作的功能类型（防撤回或者多开等）筛选特征码
                        if (categories != null && categories.Count > 0)
                        {
                            replacePatterns = editor.FileCommonModifyInfo.ReplacePatterns.Where(info => categories.Contains(info.Category)).ToList();
                        }
                        // 如果能顺利得到 TargetChanges 不报错则可以使用特征替换方式
                        editor.TargetChanges = ModifyFinder.FindChanges(editor.FilePath, replacePatterns);
                        continue;
                    }
                    else
                    {
                        // SHA1不匹配，连版本也不匹配，说明完全不支持
                        if (matchingVersion == null)
                        {
                            throw new BusinessException("not_support", $"不支持此版本：{editor.FileVersion}！");
                        }
                        // SHA1不匹配，但是版本匹配，可能dll已经被其他补丁程序修改过
                        if (matchingVersion != null)
                        {
                            throw new BusinessException("maybe_modified", $"程序支持此版本：{editor.FileVersion}。但是文件校验不通过，请确认是否使用过其他补丁程序！");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// c.根据补丁信息，安装补丁
        /// 两种打补丁的方式：精准（指定位置替换）、通用（特征码替换）
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            // 首先验证文件修改器是否没问题
            foreach (FileHexEditor editor in editors)
            {
                if (editor == null)
                {
                    throw new Exception("补丁安装失败，原因：文件修改器初始化失败！");
                }
            }
            // 再备份所有文件
            foreach (FileHexEditor editor in editors)
            {
                editor.Backup();
            }
            // 打补丁！
            List<FileHexEditor> done = new List<FileHexEditor>(); // 已经打上补丁的
            try
            {
                foreach (FileHexEditor editor in editors)
                {
                    bool success = editor.Patch();
                    if (!success)
                    {
                        // 此处还原逻辑不可能进入
                        editor.Restore();
                    }
                    else
                    {
                        done.Add(editor);
                    }
                }
            }
            catch (Exception ex)
            {
                // 恢复所有已经打上补丁的文件
                foreach (FileHexEditor editor in done)
                {
                    editor.Restore();
                }
                throw ex;
            }
            return true;
        }
        public bool BackupExists()
        {
            foreach (FileHexEditor editor in editors)
            {
                if (!File.Exists(editor.FileBakPath) || editor.FileVersion != editor.BackupFileVersion)
                {
                    return false;
                }
            }
            return true;
        }
        public bool Restore()
        {
            if (BackupExists())
            {
                foreach (FileHexEditor editor in editors)
                {
                    string currVersion = editor.FileVersion;
                    string bakVersion = editor.BackupFileVersion;
                    if (currVersion != bakVersion)
                    {
                        DialogResult dr = MessageBox.Show($"当前文件：{editor.FilePath}，版本：{currVersion}；" + Environment.NewLine + $"备份文件：{editor.FileBakPath}，版本：{bakVersion}；" + Environment.NewLine + $"两者版本不一致，是否继续还原？", "提示 ", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (dr == DialogResult.OK)
                        {
                            editor.Restore();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        editor.Restore();
                    }
                }
                return true;
            }
            else
            {
                throw new Exception("备份文件不存在，还原失败！");
            }
        }
    }
    internal class Bag
    {
        public Dictionary<string, App> Apps { get; set; }
        public string LatestVersion { get; set; }
        public string Notice { get; set; }
    }
    internal class BoyerMooreMatcher
    {
        private static int AlphabetSize = 256;
        private static int Max(int a, int b) { return (a > b) ? a : b; }
        static int[] PreprocessToBuildBadCharactorHeuristic(byte[] pattern)
        {
            int m = pattern.Length;
            int[] badCharactorShifts = new int[AlphabetSize];
            for (int i = 0; i < AlphabetSize; i++)
            {
                badCharactorShifts[i] = m;
            }
            for (int i = 0; i < m; i++)
            {
                badCharactorShifts[(int)pattern[i]] = m - 1 - i;
            }
            return badCharactorShifts;
        }
        static int[] PreprocessToBuildGoodSuffixHeuristic(byte[] pattern)
        {
            int m = pattern.Length;
            int[] goodSuffixShifts = new int[m];
            int[] suffixLengthArray = GetSuffixLengthArray(pattern);
            for (int i = 0; i < m; ++i)
            {
                goodSuffixShifts[i] = m;
            }
            int j = 0;
            for (int i = m - 1; i >= -1; --i)
            {
                if (i == -1 || suffixLengthArray[i] == i + 1)
                {
                    for (; j < m - 1 - i; ++j)
                    {
                        if (goodSuffixShifts[j] == m)
                        {
                            goodSuffixShifts[j] = m - 1 - i;
                        }
                    }
                }
            }
            for (int i = 0; i < m - 1; ++i)
            {
                goodSuffixShifts[m - 1 - suffixLengthArray[i]] = m - 1 - i;
            }
            return goodSuffixShifts;
        }
        static int[] GetSuffixLengthArray(byte[] pattern)
        {
            int m = pattern.Length;
            int[] suffixLengthArray = new int[m];
            int f = 0, g = 0, i = 0;
            suffixLengthArray[m - 1] = m;
            g = m - 1;
            for (i = m - 2; i >= 0; --i)
            {
                if (i > g && suffixLengthArray[i + m - 1 - f] < i - g)
                {
                    suffixLengthArray[i] = suffixLengthArray[i + m - 1 - f];
                }
                else
                {
                    if (i < g)
                    {
                        g = i;
                    }
                    f = i;
                    while (g >= 0 && pattern[g] == pattern[g + m - 1 - f])
                    {
                        --g;
                    }
                    suffixLengthArray[i] = f - g;
                }
            }
            return suffixLengthArray;
        }

        public static bool TryMatch(byte[] text, byte[] pattern, out int firstShift)
        {
            firstShift = -1;
            int n = text.Length;
            int m = pattern.Length;
            int s = 0;
            int j = 0;
            int[] badCharShifts = PreprocessToBuildBadCharactorHeuristic(pattern);
            int[] goodSuffixShifts = PreprocessToBuildGoodSuffixHeuristic(pattern);

            while (s <= (n - m))
            {
                j = m - 1;
                while (j >= 0 && pattern[j] == text[s + j])
                {
                    j--;
                }
                if (j < 0)
                {
                    firstShift = s;
                    return true;
                }
                else
                {
                    s += Max(goodSuffixShifts[j], badCharShifts[(int)text[s + j]] - (m - 1) + j);
                }
            }

            return false;
        }

        public static int[] MatchAll(byte[] text, byte[] pattern)
        {
            int n = text.Length;
            int m = pattern.Length;
            int s = 0;
            int j = 0;
            int[] shiftIndexes = new int[n - m + 1];
            int c = 0;
            int[] badCharShifts = PreprocessToBuildBadCharactorHeuristic(pattern);
            int[] goodSuffixShifts = PreprocessToBuildGoodSuffixHeuristic(pattern);
            while (s <= (n - m))
            {
                j = m - 1;
                while (j >= 0 && pattern[j] == text[s + j])
                {
                    j--;
                }
                if (j < 0)
                {
                    shiftIndexes[c] = s;
                    c++;
                    s += goodSuffixShifts[0];
                }
                else
                {
                    s += Max(goodSuffixShifts[j], badCharShifts[(int)text[s + j]] - (m - 1) + j);
                }
            }
            int[] shifts = new int[c];
            for (int y = 0; y < c; y++)
            {
                shifts[y] = shiftIndexes[y];
            }
            return shifts;
        }
    }
    internal class Change
    {
        public long Position { get; set; }
        public byte[] Content { get; set; }
        public Change() { }
        public Change(long position, byte[] content)
        {
            Position = position;
            Content = content;
        }
        public Change Clone()
        {
            Change o = new Change
            {
                Position = Position,
                Content = Content
            };
            return o;
        }
    }
    internal class CommonModifyInfo
    {
        public string Name { get; set; }
        public string StartVersion { get; set; }
        public string EndVersion { get; set; }
        public List<ReplacePattern> ReplacePatterns { get; set; }
        public CommonModifyInfo Clone()
        {
            CommonModifyInfo o = new CommonModifyInfo
            {
                Name = Name,
                StartVersion = StartVersion,
                EndVersion = EndVersion
            };
            List<ReplacePattern> cs = new List<ReplacePattern>();
            foreach (ReplacePattern c in ReplacePatterns)
            {
                cs.Add(c.Clone());
            }
            o.ReplacePatterns = cs;
            return o;
        }
        public List<string> GetCategories()
        {
            if (ReplacePatterns != null && ReplacePatterns.Count > 0)
            {
                return ReplacePatterns.Select(p => p.Category).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
    public class Device
    {
        private static string macID = null;
        private static string osVersion = null;
        private static string fingerPrint = null;
        public static string MacID
        {
            get
            {
                if (macID == null)
                {
                    macID = ObtainMacID();
                }
                return macID;
            }
        }
        public static string OSVersion
        {
            get
            {
                if (osVersion == null)
                {
                    var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                                select x.GetPropertyValue("Caption")).FirstOrDefault();
                    osVersion = name != null ? name.ToString() : "Unknown";
                }
                return osVersion;
            }
        }
        /// <summary>
        /// Calculate GUID
        /// </summary>
        /// <returns>GUID</returns>
        public static string Value()
        {
            try
            {
                if (fingerPrint == null)
                {
                    fingerPrint = GetHash(
                        "MAC >> " + MacID
                        );
                }
                return fingerPrint;
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }
        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }
        public static string ObtainMacID()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            try
            {
                ManagementClass mc = new ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (mo[wmiMustBeTrue].ToString() == "True")
                    {
                        if (result == "")
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            try
            {
                ManagementClass mc = new ManagementClass(wmiClass);
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    //Only get the first one
                    if (result == "")
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                }
            }
            catch
            {
            }
            return result;
        }
    }
    internal class FileHexEditor
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string FileBakPath { get; set; }

        private string fileReplacedPath;

        private string version;
        public string FileVersion
        {
            get
            {
                if (version == null)
                {
                    version = FileUtil.GetFileVersion(FilePath);
                }
                return version;
            }
        }

        public string BackupFileVersion
        {
            get
            {
                return FileUtil.GetFileVersion(FileBakPath);
            }
        }

        public string sha1;
        public string FileSHA1
        {
            get
            {
                if (sha1 == null)
                {
                    sha1 = FileUtil.ComputeFileSHA1(FilePath);
                }
                return sha1;
            }
        }

        public TargetInfo FileTargetInfo { get; set; }

        /// <summary>
        /// 通过比对SHA1得到的特定版本的修改信息
        /// </summary>
        public ModifyInfo FileModifyInfo { get; set; }

        /// <summary>
        /// 通过比对版本范围得到的通用查找替换的修改信息（特征码替换信息）
        /// </summary>
        public CommonModifyInfo FileCommonModifyInfo { get; set; }

        /// <summary>
        /// 将要执行的修改
        /// </summary>
        public List<Change> TargetChanges { get; set; }

        public FileHexEditor(string installPath, TargetInfo target)
        {
            FileTargetInfo = target.Clone();
            FileName = FileTargetInfo.Name;
            FilePath = Path.Combine(installPath, FileTargetInfo.RelativePath);
            FileBakPath = FilePath + ".bak";
            fileReplacedPath = FilePath + ".process";
        }

        /// <summary>
        /// 备份
        /// </summary>
        public void Backup()
        {
            // 不覆盖同版本的备份文件
            if (File.Exists(FileBakPath))
            {
                if (FileVersion != BackupFileVersion)
                {
                    File.Copy(FilePath, FileBakPath, true);
                }
            }
            else
            {
                File.Copy(FilePath, FileBakPath, true);
            }

        }

        /// <summary>
        /// 打补丁
        /// </summary>
        /// <returns></returns>
        public bool Patch()
        {
            if (TargetChanges == null)
            {
                throw new BusinessException("change_null", "在安装补丁时，变更的内容为空！");
            }

            FileUtil.EditMultiHex(FilePath, TargetChanges);
            return true;
        }

        /// <summary>
        /// 还原
        /// </summary>
        public void Restore()
        {
            File.Copy(FileBakPath, FilePath, true);
        }
    }
    internal class FileUtil
    {
        /// <summary>
        /// 获取文件版本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileVersion(string path)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
            return fileVersionInfo.FileVersion;
        }

        /// <summary>
        /// 计算文件SHA1
        /// </summary>
        /// <param name="s">文件路径</param>
        /// <returns></returns>
        public static string ComputeFileSHA1(string s)
        {
            FileStream file = new FileStream(s, FileMode.Open);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retval = sha1.ComputeHash(file);
            file.Close();
            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++)
            {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }

        /// <summary>
        /// 修改文件指定位置的字节
        /// </summary>
        /// <param name="path">文件对象的路径</param>
        /// <param name="position">偏移位置</param>
        /// <param name="after">修改后的值</param>
        /// <returns></returns>
        public static bool EditHex(string path, long position, byte after)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = position;
                stream.WriteByte(after);
            }
            return true;
        }

        /// <summary>
        /// 修改文件多个指定位置的多个字节
        /// </summary>
        /// <param name="path">文件对象的路径</param>
        /// <param name="changes">需要修改的位置和内容</param>
        public static void EditMultiHex(string path, List<Change> changes)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                foreach (Change change in changes)
                {
                    stream.Seek(change.Position, SeekOrigin.Begin);
                    foreach (byte b in change.Content)
                    {
                        // 跳过通配符
                        if (b == 0x3F)
                        {
                            stream.ReadByte();
                        }
                        else
                        {
                            stream.WriteByte(b);
                        }
                    }
                }
            }
        }
    }
    public class FuzzyMatcher
    {
        public const byte wildcard = 0x3F; // 通配符
        /// <summary>
        /// 通配符匹配所有符合查找串的位置
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="pattern">查找串</param>
        /// <returns></returns>
        public static int[] MatchAll(byte[] content, byte[] pattern)
        {
            byte[] head = GetHead(pattern);
            int[] indexs = BoyerMooreMatcher.MatchAll(content, head);
            // 头串和查找串相同则直接返回，不同则继续判断是否符合查询串
            if (head.Length == pattern.Length)
            {
                return indexs;
            }
            else
            {
                List<int> res = new List<int>();
                foreach (int index in indexs)
                {
                    if (IsEqual(content, index, pattern))
                    {
                        res.Add(index);
                    }
                }
                return res.ToArray();
            }
        }
        /// <summary>
        /// 通配符匹配所有符合查找串的位置，并排除已经替换的情况
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="searchBytes">查找串</param>
        /// <param name="replaceBytes">替换串</param>
        /// <returns></returns>
        public static int[] MatchNotReplaced(byte[] content, byte[] searchBytes, byte[] replaceBytes)
        {
            byte[] head = GetHead(searchBytes);
            int[] indexs = BoyerMooreMatcher.MatchAll(content, head);
            // 头串和查找串相同则直接返回，不同则继续判断是否符合查询串
            List<int> res = new List<int>();
            if (head.Length != searchBytes.Length)
            {
                foreach (int index in indexs)
                {
                    if (IsEqual(content, index, searchBytes))
                    {
                        res.Add(index);
                    }
                }
                indexs = res.ToArray();
            }
            // 判断是否与替换串相同
            res = new List<int>();
            foreach (int index in indexs)
            {
                if (!IsEqual(content, index, replaceBytes))
                {
                    res.Add(index);
                }
            }
            return res.ToArray();
        }
        /// <summary>
        /// 获取头串
        /// </summary>
        /// <param name="whole">完整查找串</param>
        /// <returns></returns>
        private static byte[] GetHead(byte[] whole)
        {
            int len = whole.Length;
            for (int i = 0; i < whole.Length; i++)
            {
                if (whole[i] == wildcard)
                {
                    len = i;
                    break;
                }
            }
            if (len == 0)
            {
                throw new Exception("不正确的通配符位置!");
            }
            return whole.Take(len).ToArray();
        }
        /// <summary>
        /// 确认整个查找串是否匹配
        /// </summary>
        /// <param name="content">被查找对象</param>
        /// <param name="start">头串匹配位置</param>
        /// <param name="whole">完整查找串</param>
        /// <returns></returns>
        public static bool IsEqual(byte[] content, int start, byte[] whole)
        {
            int i = 0;
            for (i = 0; i < whole.Length; i++)
            {
                if (whole[i] == wildcard)
                {
                    continue;
                }
                if (content[start + i] != whole[i])
                {
                    break;
                }
            }
            if (i == whole.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class HttpUtil
    {
        public static HttpClient Client { get; } = new HttpClient();

        static HttpUtil()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        /// <summary>
        /// 补丁路径
        /// 已经弃用的路径
        /// </summary>
        private static readonly string[] urls = new string[]
        {
            "https://gitee.com/huiyadanli/RevokeMsgPatcher/raw/master/RevokeMsgPatcher.Assistant/Data/1.2/patch.json",
            "https://huiyadanli.coding.net/p/RevokeMsgPatcher/d/RevokeMsgPatcher/git/raw/master/RevokeMsgPatcher.Assistant/Data/1.2/patch.json",
            "https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/RevokeMsgPatcher.Assistant/Data/1.2/patch.json"
        };
        private static int i = 0;
        public static async Task<string> GetPatchJsonAsync()
        {
            try
            {
                return await Client.GetStringAsync(urls[i]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("第" + (i + 1) + "次请求异常:[" + ex.Message + "]\nURL:" + urls[i]);
                GAHelper.Instance.RequestPageView($"/main/json/request_ex/{i + 1}/{ex.Message}", "第" + (i + 1) + "次请求异常:[" + ex.Message + "]");
                i++;
                if (i >= urls.Length)
                {
                    i = 0;
                    return null;
                }
                else
                {
                    return await GetPatchJsonAsync();
                }
            }
        }
    }
    internal class ReplacePattern
    {
        public byte[] Search { get; set; }
        public byte[] Replace { get; set; }
        public string Category { get; set; }
        public ReplacePattern Clone()
        {
            ReplacePattern o = new ReplacePattern();
            o.Search = Search;
            o.Replace = Replace;
            return o;
        }
    }
    public class GAHelper
    {
        private static GAHelper instance = null;
        private static readonly object obj = new object();
        public static GAHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GAHelper();
                }
                return instance;
            }
        }
        // 根据实际情况修改
        private static readonly HttpClient client = HttpUtil.Client;
        private const string GAUrl = "https://www.google-analytics.com/collect";
        // 根据实际使用分析账号设置
        private const string tid = "UA-80358493-2"; // GA Tracking ID / Property ID.
        private static readonly string cid = Device.Value(); // Anonymous Client ID. // Guid.NewGuid().ToString()
        // 屏幕分辨率(可选)
        private static readonly string sr = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;
        public string UserAgent { get; set; }
        public GAHelper()
        {
            UserAgent = string.Format("Hui Google Analytics Tracker/1.0 ({0}; {1}; {2})", Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString);
        }
        public async Task RequestPageViewAsync(string page, string title = null)
        {
            try
            {
                if (!page.StartsWith("/"))
                {
                    page = "/" + page;
                }
                // 请求参数
                var values = new Dictionary<string, string>
                {
                    { "v", "1" }, // 当前必填1
                    { "tid", tid },
                    { "cid", cid },
                    { "ua", UserAgent },
                    { "t", "pageview" },
                    { "sr", sr },
                    { "dp", page },
                    { "dt", title },
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(GAUrl, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GAHelper:" + ex.Message);
            }
        }
        public void RequestPageView(string page, string title = null)
        {
            Task.Run(() => RequestPageViewAsync(page, title));
        }
    }
    internal class WechatModifier : AppModifier
    {
        public WechatModifier(App config)
        {
            this.config = config;
        }
        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("Wechat");
            string realPath = GetRealInstallPath(installPath);
            if (string.IsNullOrEmpty(realPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\Wechat"))
                {
                    realPath = GetRealInstallPath(defaultPath);
                    if (!string.IsNullOrEmpty(realPath))
                    {
                        return defaultPath;
                    }
                }
            }
            else
            {
                return realPath;
            }
            return null;
        }
        /// <summary>
        /// 微信 3.5.0.4 改变了目录结构
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private string GetRealInstallPath(string basePath)
        {
            if (IsAllFilesExist(basePath))
            {
                return basePath;
            }
            if (string.IsNullOrEmpty(basePath))
            {
                return null;
            }
            DirectoryInfo[] directories = new DirectoryInfo(basePath).GetDirectories();
            PathUtil.SortByLastWriteTimeDesc(ref directories); // 按修改时间倒序
            foreach (DirectoryInfo folder in directories)
            {
                if (IsAllFilesExist(folder.FullName))
                {
                    return folder.FullName;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取整个APP的当前版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "WeChatWin.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }
    }
    internal class ModifyInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string SHA1Before { get; set; }
        public string SHA1After { get; set; }
        public List<Change> Changes { get; set; }
        public ModifyInfo Clone()
        {
            ModifyInfo o = new ModifyInfo();
            o.Name = Name;
            o.Version = Version;
            o.SHA1Before = SHA1Before;
            o.SHA1After = SHA1After;
            List<Change> cs = new List<Change>();
            foreach (Change c in Changes)
            {
                cs.Add(c.Clone());
            }
            o.Changes = cs;
            return o;
        }
    }
    internal class ModifyFinder
    {
        // TODO 该逻辑需要优化！
        public static List<Change> FindChanges(string path, List<ReplacePattern> replacePatterns)
        {
            // 读取整个文件(dll)
            byte[] fileByteArray = File.ReadAllBytes(path);

            List<Change> changes = new List<Change>(); // 匹配且需要替换的地方

            // 查找所有替换点
            int matchNum = 0; // 匹配数量
            foreach (ReplacePattern pattern in replacePatterns)
            {
                // 所有的匹配点位
                int[] matchIndexs = FuzzyMatcher.MatchAll(fileByteArray, pattern.Search);
                if (matchIndexs.Length == 1)
                {
                    matchNum++;
                    // 与要替换的串不一样才需要替换（当前的特征肯定不一样）
                    if (!FuzzyMatcher.IsEqual(fileByteArray, matchIndexs[0], pattern.Replace))
                    {
                        changes.Add(new Change(matchIndexs[0], pattern.Replace));
                    }
                }
            }

            // 匹配数和期望的匹配数不一致时报错（当前一个特征只会出现一次）
            if (matchNum != replacePatterns.Count)
            {
                Tuple<bool, SortedSet<string>> res = IsAllReplaced(fileByteArray, replacePatterns);
                if (res.Item1)
                {
                    throw new BusinessException("match_already_replace", "特征比对：当前应用已经安装了对应功能的补丁！");
                }
                else
                {
                    if (res.Item2.Count > 0)
                    {
                        throw new BusinessException("match_inconformity", $"特征比对：以下功能补丁已经安装，请取消勾选！\n已安装功能：【{string.Join("、", res.Item2)}】");
                    }
                    else
                    {
                        throw new BusinessException("match_inconformity", $"特征比对：当前特征码匹配数[{matchNum}]和期望的匹配数[{replacePatterns.Count}]不一致。\n" +
                            $"出现此种情况的一般有如下可能：\n" +
                            $"1. 你可能已经安装了某个功能的补丁，请选择未安装功能进行安装。\n" +
                            $"2. 如果当前版本为最新版本，特征码可能出现变化（可能性比较低），请联系作者处理。");
                    }
                }
            }
            else
            {
                // 匹配数和需要替换的数量不一致时，可能时部分/所有特征已经被替换
                if (matchNum != changes.Count)
                {
                    // 此逻辑在当前特征配置下不会进入，因为查找串和替换串当前全部都是不相同的
                    if (changes.Count == 0)
                    {
                        throw new BusinessException("match_already_replace", "特征比对：当前应用已经安装了所选功能补丁！");
                    }
                    else
                    {
                        throw new BusinessException("match_part_replace", "特征比对：部分特征已经被替换，请确认是否有使用过其他防撤回/多开补丁！");
                    }

                }
                else
                {
                    // 匹配数和需要替换的数量一致时才是正常状态
                    return changes;
                }
            }
        }

        public static SortedSet<string> FindReplacedFunction(string path, List<ReplacePattern> replacePatterns)
        {
            byte[] fileByteArray = File.ReadAllBytes(path);
            Tuple<bool, SortedSet<string>> res = IsAllReplaced(fileByteArray, replacePatterns);
            return res.Item2;
        }

        private static Tuple<bool, SortedSet<string>> IsAllReplaced(byte[] fileByteArray, List<ReplacePattern> replacePatterns)
        {
            int matchNum = 0;
            SortedSet<string> alreadyReplaced = new SortedSet<string>(); // 已经被替换特征的功能
            foreach (ReplacePattern pattern in replacePatterns)
            {
                // 所有的匹配点位
                int[] matchIndexs = FuzzyMatcher.MatchAll(fileByteArray, pattern.Replace);
                if (matchIndexs.Length == 1)
                {
                    matchNum++;
                    alreadyReplaced.Add(pattern.Category);
                }
            }
            return new Tuple<bool, SortedSet<string>>(matchNum == replacePatterns.Count, alreadyReplaced);
        }
    }
    internal class PathUtil
    {
        public static void DisplayAllProgram()
        {
            RegistryKey uninstallKey, programKey;
            uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string[] programKeys = uninstallKey.GetSubKeyNames();
            foreach (string keyName in programKeys)
            {
                programKey = uninstallKey.OpenSubKey(keyName);
                Console.WriteLine(keyName + " , " + programKey.GetValue("DisplayName") + " , " + programKey.GetValue("InstallLocation"));
                programKey.Close();
            }
            uninstallKey.Close();
        }
        /// <summary>
        /// 从注册表中寻找安装路径
        /// </summary>
        /// <param name="uninstallKeyName">
        /// 安装信息的注册表键名
        /// 微信：WeChat
        /// QQ：{052CFB79-9D62-42E3-8A15-DE66C2C97C3E} 
        /// TIM：TIM
        /// </param>
        /// <returns>安装路径</returns>
        public static string FindInstallPathFromRegistry(string uninstallKeyName)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey($@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{uninstallKeyName}");
                if (key == null)
                {
                    return null;
                }
                object installLocation = key.GetValue("InstallLocation");
                key.Close();
                if (installLocation != null && !string.IsNullOrEmpty(installLocation.ToString()))
                {
                    return installLocation.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        /// <summary>
        /// 获取所有可能的默认安装路径
        /// </summary>
        /// <param name="relativePath">Tencent\*</param>
        /// <returns></returns>
        public static List<string> GetDefaultInstallPaths(string relativePath)
        {
            List<string> list = new List<string>();
            // 从默认安装目录查找
            string[] drives = Environment.GetLogicalDrives(); //获取当前计算机逻辑磁盘名称列表
            foreach (string d in drives)
            {
                string path = Path.Combine(d, $@"Program Files (x86)\{relativePath}");
                if (Directory.Exists(path))
                {
                    list.Add(path);
                }
            }
            return list;
        }
        /// <summary>
        /// 按文件夹修改时间倒序
        /// </summary>
        /// <param name="dirs"></param>
        public static void SortByLastWriteTimeDesc(ref DirectoryInfo[] dirs)
        {
            Array.Sort(dirs, delegate (DirectoryInfo x, DirectoryInfo y) { return y.LastWriteTime.CompareTo(x.LastWriteTime); });
        }
    }
    class QQLiteModifier : AppModifier
    {
        public QQLiteModifier(App config)
        {
            this.config = config;
        }
        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("QQLite");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\QQLite"))
                {
                    if (IsAllFilesExist(defaultPath))
                    {
                        return defaultPath;
                    }
                }
            }
            else
            {
                return installPath;
            }
            return null;
        }
        /// <summary>
        /// 获取整个APP的当前版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "IM.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }
    }
    class QQModifier : AppModifier
    {
        public QQModifier(App config)
        {
            this.config = config;
        }
        /// <summary>
        /// 自动寻找获取QQ安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("{052CFB79-9D62-42E3-8A15-DE66C2C97C3E}");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\QQ"))
                {
                    if (IsAllFilesExist(defaultPath))
                    {
                        return defaultPath;
                    }
                }
            }
            else
            {
                return installPath;
            }
            return null;
        }
        /// <summary>
        /// 获取整个APP的当前版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "IM.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }
    }
    internal class VersionUtil
    {
        /// <summary>
        /// 版本比较
        /// 0  相等
        /// 1  大与
        /// -1 小于
        /// 为空的版本最大
        /// </summary>
        /// <param name="v1">版本1</param>
        /// <param name="v2">版本2</param>
        /// <returns></returns>
        public static int Compare(string v1, string v2)
        {
            if (string.IsNullOrEmpty(v1))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(v2))
            {
                return -1;
            }
            string[] v1s = v1.Split('.');
            string[] v2s = v2.Split('.');
            int len = Math.Max(v1s.Length, v2s.Length);
            for (int i = 0; i < len; i++)
            {
                int i1 = 0, i2 = 0;
                if (i < v1s.Length)
                {
                    i1 = Convert.ToInt32(v1s[i]);
                }
                if (i < v2s.Length)
                {
                    i2 = Convert.ToInt32(v2s[i]);
                }
                if (i1 == i2)
                {
                    continue;
                }
                else if (i1 > i2)
                {
                    return 1;
                }
                else if (i1 < i2)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
    internal class UIController
    {
        public static void AddCategoryCheckBoxToPanel(System.Windows.Forms.Panel panel, string[] categories, string[] installed)
        {
            if (categories != null && categories.Length != 0)
            {
                panel.Controls.Clear();
                for (int i = 0; i < categories.Length; i++)
                {
                    CheckBox chk = new CheckBox
                    {
                        Text = categories[i],
                        Font = new Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))),
                        Name = "chkCategoriesIndex" + i,
                        Checked = true,
                        AutoSize = true
                    };
                    if (installed.Contains(categories[i]))
                    {
                        chk.Text += "（已安装）";
                        chk.Enabled = false;
                    }
                    panel.Controls.Add(chk);
                }
            }
            else
            {
                AddMsgToPanel(panel, "无功能选项");
            }
        }
        public static void AddMsgToPanel(System.Windows.Forms.Panel panel, string msg)
        {
            panel.Controls.Clear();
            Label label = new Label
            {
                Name = "lblCategoriesMsg",
                Font = new Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))),
                ForeColor = Color.Red,
                Text = msg,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };
            panel.Controls.Add(label);
        }
        public static List<string> GetCategoriesFromPanel(System.Windows.Forms.Panel panel)
        {
            List<string> categories = new List<string>();
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is CheckBox checkBox)
                {
                    if (checkBox.Enabled && checkBox.Checked)
                    {
                        categories.Add(checkBox.Text);
                    }
                }
                else if (ctrl is Label label)
                {
                    return null; // 如果是标签, 说明是精准匹配, 直接返回null
                }
            }
            return categories;
        }
    }
    internal class TargetInfo
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string Memo { get; set; }
        public TargetInfo Clone()
        {
            TargetInfo o = new TargetInfo();
            o.Name = Name;
            o.RelativePath = RelativePath;
            o.Memo = Memo;
            return o;
        }
    }
    class TIMModifier : AppModifier
    {
        public TIMModifier(App config)
        {
            this.config = config;
        }
        /// <summary>
        /// 自动寻找获取微信安装路径
        /// </summary>
        /// <returns></returns>
        public override string FindInstallPath()
        {
            string installPath = PathUtil.FindInstallPathFromRegistry("TIM");
            if (!IsAllFilesExist(installPath))
            {
                foreach (string defaultPath in PathUtil.GetDefaultInstallPaths(@"Tencent\TIM"))
                {
                    if (IsAllFilesExist(defaultPath))
                    {
                        return defaultPath;
                    }
                }
            }
            else
            {
                return installPath;
            }
            return null;
        }
        /// <summary>
        /// 获取整个APP的当前版本
        /// </summary>
        /// <returns></returns>
        public override string GetVersion()
        {
            if (editors != null && editors.Count > 0)
            {
                foreach (FileHexEditor editor in editors)
                {
                    if (editor.FileName == "IM.dll")
                    {
                        return editor.FileVersion;
                    }
                }
            }
            return "";
        }
    }

    internal class BusinessException : ApplicationException
    {
        public string ErrorCode { get; protected set; }
        public BusinessException(string errcode, string message) : base(message)
        {
            ErrorCode = errcode;
        }
    }
}