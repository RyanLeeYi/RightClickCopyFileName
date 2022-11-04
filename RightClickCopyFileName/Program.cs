using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace RightclickCopyFileName
{
    class Program
    {
        private const string MenuName = "*\\shell\\RightclickCopyFileName";
        private const string Command = "*\\shell\\RightclickCopyFileName\\command";
        private static string fileName = "";
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                建立系統右鍵選單();
            }
            else
            {

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
        private static void SetCopyText()
        {
            Clipboard.SetText(fileName);
        }
    }
}