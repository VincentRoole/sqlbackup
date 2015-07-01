﻿using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Configuration.Install;
using System.Windows.Forms;


namespace sqlbackup
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)//string[] args
        {
            // 运行服务
            if (args.Length == 0)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new Servicesqlbackup() };
                ServiceBase.Run(ServicesToRun);


            }
            // 安装服务
            else if (args[0].ToLower() == "/i" || args[0].ToLower() == "-i")
            {
                try
                {
                    string[] cmdline = { };
                    string serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Install(new System.Collections.Hashtable());

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            // 删除服务
            else if (args[0].ToLower() == "/u" || args[0].ToLower() == "-u")
            {
                try
                {
                    string[] cmdline = { };
                    string serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Uninstall(null);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
            //以窗口方式运行服务
            else if (args[0].ToLower() == "/f" || args[0].ToLower() == "-f" || args[0].ToLower() == "/form" || args[0].ToLower() == "-form")
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }


        }

    }
}
