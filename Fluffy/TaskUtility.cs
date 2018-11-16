using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Fluffy
{
    public class TaskUtility
    {
        private static Dictionary<Type, bool> _hasResultCache = new Dictionary<Type, bool>();

        public static bool HasResult(Task task)
        {
            if (!_hasResultCache.TryGetValue(task.GetType(), out var result))
            {
                var type = task.GetType().GetProperty("Result");
                result = (type != null && type.PropertyType.FullName != "System.Threading.Tasks.VoidTaskResult");
                _hasResultCache[task.GetType()] = result;
            }

            return result;
        }

        public static object GetResultDynamic(Task task)
        {
            if (HasResult(task))
            {
                return ((dynamic)task).Result;
            }

            return default;
        }

        private static Dictionary<Type, FieldInfo> _fiCache = new Dictionary<Type, FieldInfo>();

        public static object GetResult(Task task)
        {
            return task.GetType().GetField("m_result", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(task);
        }

        public static object GetResultCached(Task task)
        {
            return GetResultFieldInfo(task).GetValue(task);
        }

        private static FieldInfo GetResultFieldInfo(Task task)
        {
            if (!_fiCache.TryGetValue(task.GetType(), out var fiValue))
            {
                _fiCache[task.GetType()] = fiValue =
                    task.GetType().GetField("m_result", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return fiValue;
        }

        private static Dictionary<Type, Func<object, object>> _getterCache = new Dictionary<Type, Func<object, object>>();

        public static object GetResultIl(Task task)
        {
            if (!_getterCache.TryGetValue(task.GetType(), out var getter))
            {
                var field = GetResultFieldInfo(task);
                getter = CreateGetter(field);
                _getterCache[task.GetType()] = getter;
            }

            return getter(task);
        }

        private static Func<object, object> CreateGetter(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(object), new Type[1] { typeof(object) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldfld, field);

                if (field.FieldType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, field.FieldType);
                }
            }

            gen.Emit(OpCodes.Ret);
            var deleg = setterMethod.CreateDelegate(typeof(Func<object, object>));
            return (Func<object, object>)deleg;
        }
    }
}