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
using System.Reflection.Emit;

namespace EMRView
{
    static class PropertyAccessor
    {
        /// <summary>
        /// This function return a delgate to the target Property Get MethodName.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        /// <param name="PropertyName">The taget property name</param>
        /// <param name="TargetType">The taget property type</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler GetPropertyInvoker(Type TargetType, string PropertyName)
        {
            //return MethodInvoker.GetMethodInvoker(TargetType.GetMethod("get_" + PropertyName));

            PropertyInfo Property = TargetType.GetProperty(PropertyName);
            return GetPropertyInvoker(TargetType, Property);
        }

        /// <summary>
        /// This function return a delgate to the target Property Set MethodName.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        /// <param name="PropertyName">The taget property name</param>
        /// <param name="TargetType">The taget property type</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler SetPropertyInvoker(Type TargetType, string PropertyName)
        {
            //return GetMethodInvoker4Set(TargetType.GetMethod("set_" + TargetType.GetProperty(PropertyName).Name));

            PropertyInfo Property = TargetType.GetProperty(PropertyName);
            return SetPropertyInvoker(TargetType, Property);
        }

        /// <summary>
        /// This function return a delgate to the target Property Get MethodName.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        /// <param name="TargetType">The taget type</param>
        /// <param name="Property">The taget MethodName information</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler GetPropertyInvoker(Type TargetType, PropertyInfo Property)
        {
            MethodInfo methodInfo = TargetType.GetMethod("get_" + Property.Name);
            if (methodInfo == null)
            {
                methodInfo = Property.DeclaringType.GetMethod("get_" + Property.Name);
            }

            if (methodInfo == null)
            {
                //EAS.Loggers.Logger.Warn(string.Format("Shit，Get {0}/{1} Not found!", TargetType.ToString(), Property.Name));
            }

            return MethodInvoker.GetMethodInvoker(methodInfo);
        }

        /// <summary>
        /// This function return a delgate to the target Property Get MethodName.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        /// <param name="TargetType">The taget type</param>
        /// <param name="Property">The taget MethodName information</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler SetPropertyInvoker(Type TargetType, PropertyInfo Property)
        {
            MethodInfo methodInfo = TargetType.GetMethod("set_" + Property.Name);
            if (methodInfo == null)
            {
                methodInfo = Property.DeclaringType.GetMethod("set_" + Property.Name);
            }

            if (methodInfo == null)
            {
                //EAS.Loggers.Logger.Warn(string.Format("Shit，Set {0}/{1} Not found!", TargetType.ToString(), Property.Name));
            }

            return GetMethodInvoker4Set(methodInfo);
        }

        /*//////////////////////////////////////////////////////////////////////////////////////*/

        private static FastInvokeHandler GetMethodInvoker4Set(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);


            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
                locals[i] = il.DeclareLocal(paramTypes[i], true);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            if (!methodInfo.IsStatic)
                il.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }

            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);

            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            FastInvokeHandler invoder = (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
            return invoder;
        }

        /*//////////////////////////////////////////////////////////////////////////////////////*/

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}
