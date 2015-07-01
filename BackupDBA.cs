﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using sqlbackup.Core;
using System.IO;
using System.Collections;
namespace sqlbackup 
{
    class BackupDB
    {
        //SqlConnection sqlCon = new SqlConnection();
        //sqlCon.ConnectionString = ConnectionString;
        //string connection = sqlCon.ConnectionString;
        //sqlCon.Open();
        public static string Server;
        public static string UserName;
        public static string PassWord;
        public static bool WindowsIdentity;
        public static string ConnectionString;
        /// <summary>
        /// 获取数据库链接参数 命名空间smsSys.Backup内部使用
        /// </summary>
        private static void BackupDBname()//获取数据库链接参数
        {
            //Server = INIFile.ContentValue("SqlConnect", "Server");
            //UserName = INIFile.ContentValue("SqlConnect", "UserName");
            //PassWord = INIFile.ContentValue("SqlConnect", "PassWord");

            Server = SysVisitor.Current.of_GetMySysSet("SqlConnect", "Server");
            UserName = SysVisitor.Current.of_GetMySysSet("SqlConnect", "UserName");
            PassWord = SysVisitor.Current.of_GetMySysSet("SqlConnect", "PassWord");

            WindowsIdentity = Convert.ToBoolean(SysVisitor.Current.of_GetMySysSet("SqlConnect", "WindowsIdentity"));
            if (!WindowsIdentity)
            {
                ConnectionString = "Data Source=" + Server + ";Initial Catalog=;User Id=" + UserName + ";Password=" + PassWord + ";";
            }
            else
            ConnectionString = "Data Source=.;Initial Catalog=;Integrated Security=True";
        }
        /// <summary>
        /// 返回除不备份以外的数据库列表
        /// </summary>
        /// <returns>List类型</returns>
        public static List<string> BackupDBList()
        {
            List<string> str = new List<string>();
            BackupDBname();
            string cmdStirng = "EXEC  sp_helpdb";
            SqlConnection connect = new SqlConnection(ConnectionString);
            SqlCommand cmd = new SqlCommand(cmdStirng, connect);
            try
            {
                if (connect.State == ConnectionState.Closed)
                {
                    connect.Open();
                    IDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        str.Add(dr["name"].ToString());
                    }
                    dr.Close();
                }
            }
            catch //(SqlException e)
            {
                //MessageBox.Show(e.Message);  
            }
            finally
            {
                if (connect != null && connect.State == ConnectionState.Open)
                {
                    connect.Dispose();
                }
            }
            string aa = SysVisitor.Current.of_GetMySysSet("notbackupdb", "notbackupdb");
            string[] bb = aa.Split(',');
            if (bb.Length > 0)
            {
                List<System.String> listS = new List<System.String>(bb);
                List<string> b = new List<string>();
                foreach (var item in str)
                {
                    if (!listS.Contains(item))
                        b.Add(item);
                }
                return b;
            }
            return str;
        }
        /// <summary>
        /// 执行备份
        /// </summary>
        /// <param name="DBName">数据库名</param>
        /// <param name="way">备份方式 1 完整 2 差异 3 不备份</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static string BackupDBNow(string DBName, int way)
        {
            BackupDBname();
            string Path = SysVisitor.Current.of_GetMySysSet("localbackup", "LocalPath") + @"\" + DateTime.Now.ToString("yyyyMMdd_HH");//获取ini备份设置的路径
            string FileName = DateTime.Now.ToString("yyyyMMddHHmm_") + DBName + ".bak";
            string PathFileName = Path + @"\" + FileName;
            DirectoryInfo mydir = new DirectoryInfo(Path);
            if (!mydir.Exists)
            {
                Directory.CreateDirectory(Path);
            }
            int Way = way;
            SqlCommand cmd = new SqlCommand();
            SqlConnection connect = new SqlConnection(ConnectionString);
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connect;
            string CommandText = "";
            if (way == 1)//完整备份
            {
                CommandText = @"Backup database " + DBName + " to disk='" + PathFileName + "' with init,format";
            }
            if (way == 2)//差异备份
            {
                CommandText = @"backup database " + DBName + " to disk='" + PathFileName + "' WITH DIFFERENTIAL,format";
            }
            if (way == 0)//不备份
            {
                return "";
            }
            if (way != 0 && way != 1 && way != 2)//非法参数
            {
                return "";
            }
            cmd.CommandText = CommandText;
            try
            {
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Tools.WriteLog("error", "备份数据库 "+DBName+" 失败，原因"+ex.Message);
                PathFileName = "";
            }
            finally
            {
                connect.Close();
                connect.Dispose();
            }
            return PathFileName;
        }
        /// <summary>
        /// 压缩备份文件
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
         /***--------------------------------------------------
            rar.exe返回值
            255   用户中断         用户中断操作
              9   创建错误         创建文件错误
              8   内存错误         没有足够的内存进行操作
              7   用户错误         命令行选项错误
              6   打开错误         打开文件错误
              5   写错误           写入磁盘错误
              4   被锁定压缩文件   试图修改先前使用 'k' 命令锁定的压缩文件
              3   CRC 错误         解压缩时发生一个 CRC 错误
              2   致命错误         发生一个致命错误
              1   警告             没有发生致命错误
              0   成功             操作成功
         -----------------------------------------------------***/
        
        public static string BakTORar(string FileName,string DBName)
        {
            string strrarPathName = FileName + ".zip";
            try
            {
                Zip.Current.ZipFile(FileName, strrarPathName);
                if (SysVisitor.Current.of_GetMySysSet("localbackup", "WhetherDel").ToLower() == "true")//压缩后删除原文件
                {
                    File.Delete(FileName);
                }
                return FileName + ".zip"; ;
            }
            catch
            {
                return "";
            }
            //不再使用调用rar.exe压缩
            //if (FileName == "")
            //{
            //    return "";
            //}
            //string time = DateTime.Now.ToString("yyyyMMdd_HH");
            //string strtbakPath = FileName;
            //string strrarPathName = FileName + ".rar";

            //////string[] files = Directory.GetFiles(SysVisitor.Current.of_GetMySysSet("localbackup", "LocalPath") + @"\" + time + @"\");
            //////if (files.Length <= 0)
            //////{
            //////    //Directory.CreateDirectory(Path);
            //////    return "";
            //////}
            //System.Diagnostics.Process Process1 = new System.Diagnostics.Process();
            //Process1.StartInfo.FileName = "rar.exe";
            //Process1.StartInfo.CreateNoWindow = true;
            //string Command = "";
            //if (SysVisitor.Current.of_GetMySysSet("localbackup", "WhetherDel").ToLower() == "false")//压缩后保留原文件
            //{
            //    Command = "a -ep -r -ibck -inul " + strrarPathName + " " + strtbakPath;
            //}
            //if (SysVisitor.Current.of_GetMySysSet("localbackup", "WhetherDel").ToLower() == "true")//压缩后删除原文件
            //{
            //    Command = "a -ep -df -r -ibck -inul " + strrarPathName + " " + strtbakPath;
            //}
            //try
            //{
            //    Process1.StartInfo.Arguments = Command;
            //    Process1.Start();
            //    Process1.WaitForExit();
            //    int ret = Process1.ExitCode;
            //    if (Process1.HasExited)
            //    {
            //        int iExitCode = Process1.ExitCode;
            //        if (iExitCode == 0)
            //        {
            //            //正常完成
            //        }
            //        else
            //        {
            //            //有错
            //            Tools.WriteLog("error", "压缩文件失败，rar返回值：" + iExitCode);
            //        }
            //    }
            //    Process1.Close();
            //}
            //catch (Exception ex)
            //{
            //    Tools.WriteLog("error", "压缩失败 " + ex.Message + "文件路径：" + strtbakPath);
            //    strrarPathName = "";
            //}
            //return strrarPathName;
        }
    }
}
