using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicFilteringTutorial
{
    public static class TypeExtensions
    {
        public static bool IsAssignableTo(this Type type, object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                if (type.IsValueType)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return type.IsAssignableFrom(obj.GetType());
        }

        public static Func<string, object> TryGetParseMethod(Type type)
        {
            if (typeof(string) == type)
            {
                return (string value) => value;
            }
            if (type.IsEnum)
            {
                return (string value) => Enum.Parse(type, value);
            }
            else if (typeof(Guid) == type)
            {
                return (string value) => Guid.Parse(value);
            }
            else if (typeof(Uri) == type)
            {
                return (string value) => new Uri(value);
            }

            var methodParse = type.GetMethod("Parse", new Type[] { typeof(string) });
            if (null == methodParse)
            {
                return null;
            }
            return (string value) =>
            {
                if (type.IsAssignableTo(value))
                {
                    return Convert.ChangeType(value, type);
                }
                return methodParse.Invoke(null, new object[] { value });
            };
        }

        public static Func<string, object> GetParseMethod(Type type)
        {
            var parseMethod = TryGetParseMethod(type);
            if (null == parseMethod)
            {
                throw new Exception("No Method Parse");
            }
            return parseMethod;
        }


        public static object Parse(this Type type, string value)
        {
            var parseMethod = GetParseMethod(type);
            return parseMethod(value);
        }

        public static bool IsNullable(this Type type)
        {
            return ((type.IsGenericType) &&
                    (typeof(Nullable<>) == type.GetGenericTypeDefinition()));
        }
    }

}
