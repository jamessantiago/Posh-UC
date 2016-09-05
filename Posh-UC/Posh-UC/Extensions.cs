using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Runtime.InteropServices;
using System.ComponentModel;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Posh_UC
{
    public static partial class Extensions
    {
        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool HasValue(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static string IsNullOrEmptyReturn(this string s, params string[] otherPossibleResults)
        {
            if (s.HasValue())
                return s;

            if (otherPossibleResults == null)
                return "";

            foreach (var t in otherPossibleResults)
            {
                if (t.HasValue())
                    return t;
            }
            return "";
        }

        public static void ThrowIfNotNull(this Exception e)
        {
            if (e != null)
                throw e;
        }

        public static List<XmlNodeList> ToNodeList(this object[] data)
        {
            List<XmlNodeList> result = new List<XmlNodeList>();
            foreach (var node in data)
                result.Add(((XmlNode[])node).First().SelectNodes("/"));
            return result;
        }

        public static string EscapeSql(this string data)
        {
            if (!data.HasValue())
                return data;
            else
                return data.Replace("'", @"""");
        }

        public static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum) throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return enumerationValue.ToString();
        }

        public static void SetLoggingConfig(bool enabled)
        {
            var config = new LoggingConfiguration();
            if (enabled)
            {
                var consoleTarget = new ColoredConsoleTarget();
                config.AddTarget("console", consoleTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
            }

            LogManager.Configuration = config;

        }
    }
}
