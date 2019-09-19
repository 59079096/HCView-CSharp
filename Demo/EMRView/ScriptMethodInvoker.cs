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
    static class MethodInvoker
    {
        #region 代码1

        ///// <summary>
        ///// This function return a delgate to the target procedures.
        ///// Using the returning deletege you can call the target procedures so fast.
        ///// </summary>
        ///// <param name="Method">The taget MethodName information</param>
        ///// <returns>The Fast Invoke Handler delegate</returns>
        //public static FastInvokeHandler GetMethodInvoker(MethodInfo Method)
        //{
        //    ParameterInfo[] ps = Method.GetParameters();
        //    Type[] paramTypes = new Type[ps.Length];

        //    //Type[] paramTypes2 = new Type[ps.Length]; //james
        //    for (int i = 0; i < paramTypes.Length; i++)
        //    {
        //        //paramTypes2[i] = typeof(object); //james
        //        if (ps[i].ParameterType.IsByRef)
        //            paramTypes[i] = ps[i].ParameterType.GetElementType();
        //        else
        //            paramTypes[i] = ps[i].ParameterType;
        //    }

        //    DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
        //        typeof(object), new Type[] { typeof(object), typeof(object) }, Method.DeclaringType.Module);

        //    ILGenerator il = dynamicMethod.GetILGenerator();

        //    LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

        //    for (int i = 0; i < paramTypes.Length; i++)
        //        locals[i] = il.DeclareLocal(paramTypes[i], true);

        //    for (int i = 0; i < paramTypes.Length; i++)
        //    {
        //        il.Emit(OpCodes.Ldarg_1);
        //        EmitFastInt(il, i);
        //        il.Emit(OpCodes.Ldelem_Ref);
        //        EmitCastToReference(il, paramTypes[i]);
        //        il.Emit(OpCodes.Stloc, locals[i]);
        //    }

        //    if (!Method.IsStatic)
        //        il.Emit(OpCodes.Ldarg_0);

        //    for (int i = 0; i < paramTypes.Length; i++)
        //    {
        //        if (ps[i].ParameterType.IsByRef)
        //            il.Emit(OpCodes.Ldloca_S, locals[i]);
        //        else
        //            il.Emit(OpCodes.Ldloc, locals[i]);
        //    }

        //    if (Method.IsStatic)
        //        il.EmitCall(OpCodes.Call, Method, null);
        //    else
        //        il.EmitCall(OpCodes.Callvirt, Method, null);

        //    if (Method.ReturnType == typeof(void))
        //        il.Emit(OpCodes.Ldnull);
        //    else
        //        EmitBoxIfNeeded(il, Method.ReturnType);

        //    for (int i = 0; i < paramTypes.Length; i++)
        //    {
        //        if (ps[i].ParameterType.IsByRef)
        //        {
        //            il.Emit(OpCodes.Ldarg_1);
        //            EmitFastInt(il, i);
        //            il.Emit(OpCodes.Ldloc, locals[i]);
        //            if (locals[i].LocalType.IsValueType)
        //                il.Emit(OpCodes.Box, locals[i].LocalType);
        //            il.Emit(OpCodes.Stelem_Ref);
        //        }
        //    }

        //    il.Emit(OpCodes.Ret);

        //    return (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
        //}

        #endregion

        #region 代码2

        /// <summary>
        /// This function return a delgate to the target procedures.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        /// <param name="Method">The taget MethodName information</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler GetMethodInvoker(MethodInfo Method)
        {
            //动态方法。
            var dynamicMethod = new DynamicMethod(string.Empty,typeof(object), new Type[] { typeof(object), typeof(object[]) }, Method.DeclaringType.Module);

            //参数。
            ParameterInfo[] ps = Method.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }

            //方法返回。
            ILGenerator il = dynamicMethod.GetILGenerator();

            //参数、定义变量。
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }

            //参数。
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            //静态方法。
            if (!Method.IsStatic)
                il.Emit(OpCodes.Ldarg_0);

            //参数？？
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }

            //生成方法调用。
            if (Method.IsStatic)
                il.EmitCall(OpCodes.Call, Method, null);
            else
                il.EmitCall(OpCodes.Callvirt, Method, null);

            //生成返回值。
            if (Method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, Method.ReturnType);

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

            //返回。
            il.Emit(OpCodes.Ret);

            return (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
        }

        #endregion

        /// <summary>
        /// This function return a delgate to the target procedures.
        /// Using the returning deletege you can call the target procedures so fast.
        /// </summary>
        ///<param name="TargetType">Target object type.</param>
        /// <param name="MethodName">Method name.</param>
        /// <returns>The Fast Invoke Handler delegate</returns>
        public static FastInvokeHandler GetMethodInvoker(Type TargetType, string MethodName)
        {
            MethodInfo methodInfo = TargetType.GetMethod(MethodName);
            return GetMethodInvoker(methodInfo);
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
