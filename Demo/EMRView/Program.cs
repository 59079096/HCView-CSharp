/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace EMRView
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandleException);  // 全局异常捕获
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);  // 动态指定程序集目录
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmDoctorStation());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("未处理的线程异常：" + e.Exception.Message);
        }

        private static void CurrentDomain_UnhandleException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("未处理的异常：" + e.ExceptionObject.ToString());
        }

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    string vPath = "";
        //    string vAssembly = args.Name.Substring(0, args.Name.IndexOf(","));
        //    if (vAssembly == "ICSharpCode.AvalonEdit")
        //        vPath = AppDomain.CurrentDomain.BaseDirectory + "SharpCode\\ICSharpCode.AvalonEdit.dll";
        //    else
        //    if (vAssembly == "ICSharpCode.CodeCompletion")
        //        vPath = AppDomain.CurrentDomain.BaseDirectory + "SharpCode\\ICSharpCode.CodeCompletion.dll";

        //    return string.IsNullOrWhiteSpace(vPath) ? null : Assembly.LoadFrom(vPath);
        //}
    }
}
