/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
/*   此单元由呆呆（47920381<mail.james@qq.com>）提供   */
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EMRView
{
    /// <summary>
    /// C#脚本引擎。
    /// </summary>
    public class ScriptEngine : IDisposable
    {
        #region 调用上下文

        class InvodeContext
        {
            /// <summary>
            /// 类型。
            /// </summary>
            public string Type
            {
                get;
                set;
            }

            /// <summary>
            /// 方法。
            /// </summary>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// 方法对象。
            /// </summary>
            public MethodInfo Method
            {
                get;
                set;
            }

            /// <summary>
            /// 委托。
            /// </summary>
            public FastInvokeHandler Handler
            {
                get;
                set;
            }

            /// <summary>
            /// 实例。
            /// </summary>
            public object Instance
            {
                get;
                set;
            }

            public ScriptException Exception
            {
                get;
                set;
            }
        }

        #endregion

        private CodeStruct m_Struct = null;
        private Assembly m_Assembly = null;

        private Dictionary<string, InvodeContext> m_Contexts = new Dictionary<string, InvodeContext>();

        #region 静态方法

        /// <summary>
        /// 获得默认的脚本参考。
        /// </summary>
        /// <returns>教本示例。</returns>
        public static string GetDefaultScript()
        {
            return "";  // EAS.Properties.Resources.DefaultSharpScript;
        }

        /// <summary>
        /// 根据代码创建脚本引擎。
        /// </summary>
        /// <param name="scriptCode">脚本代码。</param>
        /// <returns>脚本引擎实例。</returns>
        public static ScriptEngine Create(string scriptCode)
        {
            return Create(scriptCode, false);
        }

        /// <summary>
        /// 根据代码创建脚本引擎。
        /// </summary>
        /// <param name="scriptCode">脚本代码。</param>
        /// <param name="ignoreErrors">忽略编译错误。</param>
        /// <returns>脚本引擎实例。</returns>
        public static ScriptEngine Create(string scriptCode, bool ignoreErrors)
        {
            ScriptEngine scriptEngine = new ScriptEngine(scriptCode, ignoreErrors);
            return scriptEngine;
        }

        #endregion

        /// <summary>
        /// 是否生成代码文件。
        /// </summary>
        public static bool GenerateCodeFile
        {
            get;
            set;
        }

        /// <summary>
        /// 编译错误。
        /// </summary>
        public ScriptCompileException CompileException
        {
            get;
            private set;
        }

        public ScriptEngine(bool ignoreErrors, string sourcecode, List<string> references)
        {
            m_Struct = new CodeStruct();
            m_Struct.References = references;
            m_Struct.SourceCode = sourcecode;
            try
            {
                this.m_Assembly = this.CreateAssembly();
            }
            catch (ScriptCompileException sce)
            {
                if (ignoreErrors)
                {
                    this.CompileException = sce;
                }
                else
                {
                    throw sce;
                }
            }
        }

        /// <summary>
        /// 使用脚本代码初始化ScriptEngine对象实例。
        /// </summary>
        /// <param name="sourceFromFile">脚本代码文件。</param>
        public ScriptEngine(string sourceFromFile)
            : this(sourceFromFile, false)
        {
        }

        /// <summary>
        /// 使用脚本代码初始化ScriptEngine对象实例。
        /// </summary>
        /// <param name="sourceFromFile">脚本代码文件。</param>
        /// <param name="ignoreErrors">忽略编译错误。</param>
        public ScriptEngine(string sourceFromFile, bool ignoreErrors)
        {
            m_Struct = new CodeStruct(sourceFromFile);
            try
            {
                this.m_Assembly = this.CreateAssembly();
            }
            catch (ScriptCompileException sce)
            {
                if (ignoreErrors)
                {
                    this.CompileException = sce;
                }
                else
                {
                    throw sce;
                }
            }
        }

        /// <summary>
        /// 执行脚本/默认入口。
        /// </summary>
        /// <returns>返回值。</returns>
        public object Execute(string method, params object[] args)
        {
            if (this.m_Assembly == null)
            {
                if (this.CompileException != null)
                    throw this.CompileException;
                else
                    throw new ScriptException("Error:  An exception occurred while parsing the script block", null);
            }

            Module[] mods = this.m_Assembly.GetModules(false);
       
            List<Type> types = mods[0].GetTypes().Where(x => x.IsPublic).ToList();

            foreach (Type type in types)
            {
                MethodInfo mi = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
                if (mi != null)
                {
                    return mi.Invoke(null, args);
                }
            }

            throw new ScriptException("Error:  could not find the public static Main entry function to execute", null);
        }

        /// <summary>
        /// 调用C#脚本函数。
        /// </summary>
        /// <param name="method">方法名称。</param>
        /// <param name="args">调用参数。</param>
        /// <returns>脚本执行结果。</returns>
        public object Invoke(string method, params object[] args)
        {
            return this.Invoke(string.Empty, method, args);
        }

        /// <summary>
        /// 调用C#脚本函数。
        /// </summary>
        /// <param name="type">对象类型。</param>
        /// <param name="method">方法名称。</param>
        /// <param name="args">调用参数。</param>
        /// <returns>脚本执行结果。</returns>
        public object Invoke(string type, string method, params object[] args)
        {
            if (this.m_Assembly == null)
            {
                if (this.CompileException != null)
                    throw this.CompileException;
                else
                    throw new ScriptException("Error: An exception occurred while parsing the script block", null);
            }

            InvodeContext ic = this.GetMethodContext(type, method);
            if (ic.Exception != null)
                throw ic.Exception;

            return ic.Handler(ic.Method, args);
        }

        /// <summary>
        /// 创建快速调用委托以及相关参数。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        InvodeContext GetMethodContext(string type, string method)
        {
            // 生成Key。
            string key = string.Format("{0}.{1}", type, method);

            InvodeContext ic = null;
            if (!this.m_Contexts.TryGetValue(key, out ic))
            {
                ic = new InvodeContext();
                ic.Type = type;
                ic.Name = method;

                // 求方法，求类型。
                System.Type xType = null;
                if (!string.IsNullOrEmpty(type))
                {
                    xType = this.m_Assembly.GetType(type);
                    if (xType != null)
                    {
                        ic.Method = xType.GetMethod(method);
                    }
                    else
                    {
                        ic.Exception = new ScriptException(string.Format("Error:  type {0} not find", type), null);
                    }
                }
                else
                {
                    Module[] mods = this.m_Assembly.GetModules(false);
                    var types = mods[0].GetTypes().Where(x => x.IsPublic).ToList();
                    foreach (Type vType in types)
                    {
                        ic.Method = vType.GetMethod(method);
                        if (ic.Method != null)
                        {
                            xType = vType;
                            break;
                        }
                    }
                }

                // 判断。
                if (ic.Method == null && ic.Exception == null)
                {
                    ic.Exception = new ScriptException(string.Format("Error: method {0} not find", method), null);
                }

                // 生成实例。
                if (!ic.Method.IsStatic)
                {
                    ic.Instance = System.Activator.CreateInstance(xType);
                }

                // 生产委托
                ic.Handler = FastInvoker.CreateMethodInvoker(ic.Method);
                this.m_Contexts.Add(key, ic);
            }

            return ic;
        }

        #region 编译代码

        /// <summary>
        /// The actual workhorse of the script engine.  This function will
        /// create the assembly in memmory and return it to be used
        /// </summary>		
        internal Assembly CreateAssembly()
        {
            if (string.IsNullOrEmpty(this.m_Struct.SourceCode))
            {
                throw new ScriptException("Error: There was no CS script code to compile\r\n", null);
            }

            CodeDomProvider codeProvider = new CSharpCodeProvider();
            //ICodeCompiler compiler = codeProvider.CreateCompiler();

            //add compiler parameters
            CompilerParameters compilerParams = new CompilerParameters();
            //compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.CompilerOptions = "/target:library /warn:0 /nologo /debug";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = true;

            //内部引用。
            var vBuiltInReferences = this.GetBuiltInReferences();
            foreach (string assemblieName in vBuiltInReferences)
            {
                compilerParams.ReferencedAssemblies.Add(assemblieName);
            }

            //添加外部引用
            foreach (string dotNetAssembly in this.m_Struct.References)
            {
                //已引用了就不要添加引用了
                var vFirst = vBuiltInReferences.Where(x => string.Compare(x, dotNetAssembly, true) == 0).FirstOrDefault();
                if (vFirst != null)
                    continue;

                //搜索文件。
                var vFile = this.SeachDotNetAssemblyFile(dotNetAssembly);

                //判定。
                if (File.Exists(vFile))
                    compilerParams.ReferencedAssemblies.Add(vFile);
                else
                    compilerParams.ReferencedAssemblies.Add(dotNetAssembly);
            }

            //actually compile the code
            CompilerResults results = null;

            //生成代码文件。
            if (GenerateCodeFile)
            {
                var vPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
                if (!Directory.Exists(vPath))
                    Directory.CreateDirectory(vPath);
                vPath = Path.Combine(vPath, "C#");
                if (!Directory.Exists(vPath))
                    Directory.CreateDirectory(vPath);

                var xCode = this.m_Struct.SourceCode.GetHashCode().ToString("X8");  // .GetXHashCode().ToString("X8");
                var vFile = Path.Combine(vPath, string.Format("{0}.cs", xCode));
                File.WriteAllText(vFile, this.m_Struct.SourceCode, Encoding.UTF8);
                results = codeProvider.CompileAssemblyFromFile(compilerParams, vFile);
            }
            else
            {
                results = codeProvider.CompileAssemblyFromSource(compilerParams, this.m_Struct.SourceCode);
            }

            //Do we have any compiler errors
            if (results.Errors.Count > 0)
            {
                string message = string.Empty;
                foreach (CompilerError error in results.Errors)
                {
                    if (!string.IsNullOrEmpty(message))
                        message += "\r\n";
                    message += string.Format(@"位置({0},{1}):{2}", error.Line, error.Column, error.ErrorText);
                }
                throw new ScriptCompileException(message, null);
            }

            //get a hold of the actual assembly that was generated
            Assembly generatedAssembly = results.CompiledAssembly;
            //return the assembly
            return generatedAssembly;
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            this.m_Assembly = null;
            this.m_Struct = null;
            this.m_Contexts.Clear();
        }

        #endregion        

        /// <summary>
        /// 搜索程序集。
        /// </summary>
        /// <param name="dotNetAssembly"></param>
        /// <returns></returns>
        string SeachDotNetAssemblyFile(string dotNetAssembly)
        {
            //var vFile = string.Empty;
            //var vList = new List<string>();

            ////固定子目录。
            //vList.Add(AppDomain.CurrentDomain.BaseDirectory);  // AddRange(EAS.Environment.ComponentDirectories);

            ////主目录。
            //var domain = AppDomain.CurrentDomain;
            //vList.Add(domain.BaseDirectory);

            ////PrivateBinPath
            //var vPrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            //if (!string.IsNullOrEmpty(vPrivateBinPath))
            //{
            //    var v2 = vPrivateBinPath.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //    vList.AddRange(v2);
            //}

            //搜索。
            //foreach (var addInDirectory in EAS.Environment.ComponentDirectories)
            //{
            //    vFile = SeachDotNetAssemblyFile(dotNetAssembly, addInDirectory);
            //    if (!string.IsNullOrEmpty(vFile))
            //        return vFile;
            //}

            //return vFile;

            var vFile = SeachDotNetAssemblyFile(dotNetAssembly, AppDomain.CurrentDomain.BaseDirectory);
            if (!string.IsNullOrEmpty(vFile))
                return vFile;

            return String.Empty;
        }

        /// <summary>
        /// 搜索程序集。
        /// </summary>
        /// <param name="dotNetAssembly"></param>
        /// <param name="vDirectory"></param>
        /// <returns></returns>
        string SeachDotNetAssemblyFile(string dotNetAssembly, string vDirectory)
        {
            var vFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string vFile in vFiles)
            {
                if (string.Compare(Path.GetFileName(vFile), dotNetAssembly, true) == 0)
                {
                    return vFile;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 求内置引用程序集 20190916001
        /// </summary>
        /// <returns></returns>
        List<string> GetBuiltInReferences()
        {
            var vList = new string[] { "mscorlib.dll", "System.dll" };  //, "System.Core.dll", "System.Xml.dll", "System.Xml.Linq.dll", "System.Data.dll", "System.Data.Linq.dll", "System.Data.DataSetExtensions.dll" };
            return vList.ToList();
        }
    }
}
