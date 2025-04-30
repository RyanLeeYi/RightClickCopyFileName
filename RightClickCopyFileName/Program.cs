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

        private static string fileName = "";
        private static string fileFullName = "";
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                建立系統右鍵選單();
                建立系統右鍵選單_GetFullName();
                建立系統右鍵選單_CheckDllFileBase();
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
        private static void SetCopyText()
        {
            Clipboard.SetText(fileName);
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