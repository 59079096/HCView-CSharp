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
using System;
using System.Collections.Generic;
using System.Xml;

namespace EMRView
{
    /// <summary>
    /// 脚本代码结构。
    /// </summary>
    class CodeStruct
    {
        public CodeStruct()
        {
            this.References = new List<string>();
            this.SourceCode = string.Empty;
        }

        /// <summary>
        /// 初始化CodeStruct对象实例。
        /// </summary>		
        public CodeStruct(string sourceFromFile) : this()
        {
            XmlTextReader xml = null;

            try
            {
                XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None);
                xml = new XmlTextReader(sourceFromFile, XmlNodeType.Element, context);

                while (xml.Read())
                {
                    //引用程序集。
                    if (xml.Name == "reference")
                        References.Add(xml.GetAttribute("assembly"));
                    else if (xml.Name == "scriptCode")  //脚本代码。
                        this.SourceCode = xml.ReadElementString("scriptCode").Trim();
                }
            }
            catch (Exception ex)
            {
                throw new ScriptException("Error:  An exception occurred while parsing the script block", ex);
            }
            finally
            {
                if (xml != null)
                    xml.Close();
            }
        }

        /// <summary>
        /// 脚本源码。
        /// </summary>
        public string SourceCode
        {
            get;
            set;
        }

        /// <summary>
        /// 引用程序集。
        /// </summary>
        public List<string> References
        {
            get;
            set;
        }
    }
}
