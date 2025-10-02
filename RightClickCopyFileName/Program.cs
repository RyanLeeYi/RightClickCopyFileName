using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace RightclickCopyFileName
{
    class Program
    {
        private const string MenuName = "*\\shell\\RightclickCopyFileName";
        private const string Command = "*\\shell\\RightclickCopyFileName\\command";

        private const string MenuName2 = "*\\shell\\RightclickCopyFullFileName";
        private const string Command2 = "*\\shell\\RightclickCopyFullFileName\\command";

        private const string MenuName3 = "dllfile\\shell\\CheckDllFileBase";
        private const string Command3 = "dllfile\\shell\\CheckDllFileBase\\command";

        private const string MenuName_DecodeCsFile = "*\\shell\\DecodeCsFile";
        private const string Command_DecodeCsFile = "*\\shell\\DecodeCsFile\\command";

        private const string MenuName_DecodeCsFile_Folder = "Directory\\shell\\DecodeCsFileFolder";
        private const string Command_DecodeCsFile_Folder = "Directory\\shell\\DecodeCsFileFolder\\command";

        private const string MenuName_DecodeCsFile_SubFolder = "Directory\\shell\\DecodeCsFileSubFolder";
        private const string Command_DecodeCsFile_SubFolder = "Directory\\shell\\DecodeCsFileSubFolder\\command";

        private static string fileName = "";
        private static string fileFullName = "";
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                建立系統右鍵選單();
                建立系統右鍵選單_GetFullName();
                建立系統右鍵選單_CheckDllFileBase();
                建立系統右鍵選單_DecodeCsFile();
                建立系統右鍵選單_DecodeCsFile_Folder();
                建立系統右鍵選單_DecodeCsFile_SubFolder();
            }
            else
            {
                if (args.Contains("-fp"))
                {
                    var filefullPath = args.Last();
                    fileName = filefullPath;
                    try
                    {
                        // 要使用thread，並使用STA模式才能執行
                        Thread thread = new Thread(new ThreadStart(SetCopyText));

                        thread.SetApartmentState(ApartmentState.STA); //重點

                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadKey();
                    }
                }

                if (args.Contains("-checkfilebit"))
                {
                    var filefullPath = args.Last();
                    fileFullName = filefullPath;
                    fileName = fileFullName.Split('\\').LastOrDefault();
                    try
                    {
                        // 要使用thread，並使用STA模式才能執行
                        Thread thread = new Thread(new ThreadStart(ShowFileBitVersionByPowershell));

                        thread.SetApartmentState(ApartmentState.STA); //重點

                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadKey();
                    }
                }

                if (args.Contains("-decodefile"))
                {
                    var filefullPath = args.Last();
                    解譯單一檔案(filefullPath);
                }

                if (args.Contains("-decodefolder"))
                {
                    var folderPath = args.Last();
                    解譯資料夾(folderPath);
                }

                if (args.Contains("-decodesubfolder"))
                {
                    var folderPath = args.Last();
                    解譯資料夾及子資料夾(folderPath);
                }

                if (args.Contains("-p"))
                {
                    var filefullPath = args.Last();
                    fileName = filefullPath.Split('\\').LastOrDefault();
                    try
                    {
                        // 要使用thread，並使用STA模式才能執行
                        Thread thread = new Thread(new ThreadStart(SetCopyText));

                        thread.SetApartmentState(ApartmentState.STA); //重點

                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadKey();
                    }
                }
            }
        }

        private static void 建立系統右鍵選單()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                if (regmenu != null)
                    regmenu.SetValue("", "複製檔案名稱");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -p \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }

        }

        private static void 建立系統右鍵選單_GetFullName()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName2);
                if (regmenu != null)
                    regmenu.SetValue("", "複製檔案完整名稱");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command2);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -fp \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }

        }

        private static void 建立系統右鍵選單_CheckDllFileBase()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName3);
                if (regmenu != null)
                    regmenu.SetValue("", "檢查dll檔案的位元版本");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command3);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -checkfilebit \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }

        }
        private static void 建立系統右鍵選單_DecodeCsFile()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName_DecodeCsFile);
                if (regmenu != null)
                    regmenu.SetValue("", "解譯單一CS檔案");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command_DecodeCsFile);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -decodefile \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }
        private static void 建立系統右鍵選單_DecodeCsFile_Folder()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName_DecodeCsFile_Folder);
                if (regmenu != null)
                    regmenu.SetValue("", "解譯資料夾內CS檔案");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command_DecodeCsFile_Folder);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -decodefolder \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }
        private static void 建立系統右鍵選單_DecodeCsFile_SubFolder()
        {
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            string exe = Assembly.GetExecutingAssembly().Location;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName_DecodeCsFile_SubFolder);
                if (regmenu != null)
                    regmenu.SetValue("", "解譯資料夾(含子資料夾)內CS檔案");//設定右鍵顯示名稱
                regcmd = Registry.ClassesRoot.CreateSubKey(Command_DecodeCsFile_SubFolder);
                if (regcmd != null)
                    regcmd.SetValue("", $"\"{exe}\" -decodesubfolder \"%1\"");//設定cmd指令
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }
        private static void SetCopyText()
        {
            Clipboard.SetText(fileName);
        }

        private static void 解譯單一檔案(string filePath)
        {
            try
            {
                // 取得程式所在目錄
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = Path.GetDirectoryName(exePath);
                string mainJsPath = Path.Combine(exeDir, "main.js");

                if (!File.Exists(mainJsPath))
                {
                    MessageBox.Show($"找不到 main.js 檔案於: {mainJsPath}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 建立 PowerShell 命令
                string command = $"node \"{mainJsPath}\" -f \"{filePath}\"";
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = exeDir
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"執行解譯單一檔案時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void 解譯資料夾(string folderPath)
        {
            try
            {
                // 取得程式所在目錄
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = Path.GetDirectoryName(exePath);
                string mainJsPath = Path.Combine(exeDir, "main.js");

                if (!File.Exists(mainJsPath))
                {
                    MessageBox.Show($"找不到 main.js 檔案於: {mainJsPath}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 建立 PowerShell 命令
                string command = $"node \"{mainJsPath}\" -s \"{folderPath}\"";
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = exeDir
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"執行解譯資料夾時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void 解譯資料夾及子資料夾(string folderPath)
        {
            try
            {
                // 取得程式所在目錄
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = Path.GetDirectoryName(exePath);
                string mainJsPath = Path.Combine(exeDir, "main.js");

                if (!File.Exists(mainJsPath))
                {
                    MessageBox.Show($"找不到 main.js 檔案於: {mainJsPath}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 建立 PowerShell 命令（加入 -r 參數啟用遞迴模式）
                string command = $"node \"{mainJsPath}\" -s \"{folderPath}\" -r";
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = exeDir
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"執行解譯資料夾及子資料夾時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowFileBitVersionByPowershell()
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(fileFullName);

                // PE Header 偏移位置 (位於第 60 位元組)
                int peHeaderOffset = BitConverter.ToInt32(bytes, 60);

                // 機器類型位置：PE Header 開頭 + 4 位元組（表示 Machine Type）
                ushort machineType = BitConverter.ToUInt16(bytes, peHeaderOffset + 4);

                if (machineType == 0x8664)
                {
                    Console.WriteLine("64-bit (x64)");
                    MessageBox.Show($"\"{fileFullName}\" 為 64 位元", "檢查dll位元版本", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (machineType == 0x14C)
                {
                    Console.WriteLine("32-bit (x86)");
                    MessageBox.Show($"\"{fileFullName}\" 為 32 位元", "檢查dll位元版本", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Console.WriteLine("Unknown architecture");
                    MessageBox.Show($"\"{fileFullName}\" 無法判斷架構", "檢查dll位元版本", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"無法載入 DLL: {ex.Message}");
                MessageBox.Show($"\"{fileFullName}\" 無法載入 DLL: {ex.Message}", "檢查dll位元版本", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}