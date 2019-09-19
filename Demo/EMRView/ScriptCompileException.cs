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
using System.Runtime.Serialization;

namespace EMRView
{
    /// <summary>
    /// 表示脚本构造、执行过程这中的错误。
    /// </summary>
    [Serializable]
    public class ScriptCompileException:System.Exception
    {
        /// <summary>
        /// 初始化 ScriptCompileException 类的新实例。
        /// </summary>
        public ScriptCompileException()
            : base()
        {
        }

        /// <summary>
         /// 使用指定的错误消息初始化 ScriptCompileException 类的新实例。
        /// </summary>
         /// <param name="message">描述错误的消息。</param>
        public ScriptCompileException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 用序列化数据初始化 ScriptCompileException 类的新实例。
        /// </summary>
        /// <param name="info">System.Runtime.Serialization.SerializationInfo，它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context">System.Runtime.Serialization.StreamingContext，它包含有关源或目标的上下文信息。</param>
        protected ScriptCompileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// 使用指定错误消息和对作为此异常原因的内部异常的引用来初始化 ScriptException 类的新实例。
        /// </summary>
        /// <param name="message">解释异常原因的错误消息。</param>
        /// <param name="innerException">导致当前异常的异常；如果未指定内部异常，则是一个 null 引用（在 Visual Basic 中为 Nothing）。</param>
        public ScriptCompileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
