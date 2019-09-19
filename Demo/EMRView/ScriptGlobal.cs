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
using System.Reflection;

namespace EMRView
{
    /// <summary>
    /// 快速反射调用委托。
    /// </summary>
    /// <param name="target">目标对象。</param>
    /// <param name="paramters">调用参数。</param>
    /// <returns>方法返回值。</returns>
    public delegate object FastInvokeHandler(object target, params object[] paramters);

    /// <summary>
    /// 快速翻身调用。
    /// </summary>
    public static class FastInvoker
    {
        /// <summary>
        /// 创建方法调用快速访问委托。
        /// </summary>
        /// <param name="method">目标方法。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateMethodInvoker(MethodInfo method)
        {
            return MethodInvoker.GetMethodInvoker(method);
        }

        /// <summary>
        /// 创建方法调用快速访问委托。
        /// </summary>
        /// <param name="targetType">对象类型。</param>
        /// <param name="methodName">方法名称。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateMethodInvoker(Type targetType, string methodName)
        {
            return MethodInvoker.GetMethodInvoker(targetType, methodName);
        }

        /// <summary>
        /// 创建GetProperty快速访问委托。
        /// </summary>
        /// <param name="targetType">对象类型。</param>
        /// <param name="property">目标属性。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateGetPropertyInvoker(Type targetType, PropertyInfo property)
        {
            return PropertyAccessor.GetPropertyInvoker(targetType, property);
        }

        /// <summary>
        /// 创建GetProperty快速访问委托。
        /// </summary>
        /// <param name="targetType">对象类型。</param>
        /// <param name="PropertyName">属性名称。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateGetPropertyInvoker(Type targetType, string PropertyName)
        {
            return PropertyAccessor.GetPropertyInvoker(targetType, PropertyName);
        }

        /// <summary>
        /// 创建SetProperty快速访问委托。
        /// </summary>
        /// <param name="targetType">对象类型。</param>
        /// <param name="property">目标属性。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateSetPropertyInvoker(Type targetType, PropertyInfo property)
        {
            return PropertyAccessor.SetPropertyInvoker(targetType, property);
        }

        /// <summary>
        /// 创建SetProperty快速访问委托。
        /// </summary>
        /// <param name="targetType">对象类型。</param>
        /// <param name="PropertyName">属性名称。</param>
        /// <returns>快速反射调用委托。</returns>
        public static FastInvokeHandler CreateSetPropertyInvoker(Type targetType, string PropertyName)
        {
            return PropertyAccessor.SetPropertyInvoker(targetType, PropertyName);
        }
    }
}
