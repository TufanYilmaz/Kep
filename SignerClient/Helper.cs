using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SignerClient
{
    public enum MyJobStatus
    {
        [Display(Name = "Taslak")]
        Draft = 0,
        [Display(Name = "İmzalı")]
        Signed = 1,
        [Display(Name = "Gönderildi")]
        Sent = 2,
        [Display(Name = "Hata oluştu")]
        Error = 3,
    }
    public static class Helper
    {
        public static string AppPath => System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private const string UriScheme = "kepsigner://";
        internal static (bool, string, IEnumerable<int>) ParseArguments(string[] args)
        {
            var temp = string.Join("~", args);
            if (!temp.ToLower().StartsWith(UriScheme))
            {
                MessageBox.Show("İmza Uygulaması sadece ABYSIS KEP Arayüzünden çalıştırılabilir.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (false, null, null);
            }

            temp = temp.Substring(UriScheme.Length);
            if (temp.EndsWith("/"))
            {
                temp = temp.Substring(0, temp.Length - 1);
            }

            var parts = temp.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
            var session = parts.First();
            var items = parts.Skip(1).Select(p => Convert.ToInt32(p));
            return (true, session, items);

        }
        public static void PerformKepSend()
        {

        }
        public static List<string> ExtractMessageIdFromEML(string body, string start, string end)
        {
            List<string> matched = new List<string>();

            int indexStart = 0;
            int indexEnd = 0;

            bool exit = false;
            while (!exit)
            {
                indexStart = body.IndexOf(start);

                if (indexStart != -1)
                {
                    indexEnd = indexStart + body.Substring(indexStart).IndexOf(end);

                    matched.Add(body.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length));

                    body = body.Substring(indexEnd + end.Length);
                }
                else
                {
                    exit = true;
                }
            }

            return matched;
        }

    }
    public static class EnumHelper<T>
     where T : struct, Enum // This constraint requires C# 7.3 or later.
    {
        public static IList<T> GetValues(Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        private static string lookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (PropertyInfo staticProperty in resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
                {
                    System.Resources.ResourceManager resourceManager = (System.Resources.ResourceManager)staticProperty.GetValue(null, null);
                    return resourceManager.GetString(resourceKey);
                }
            }

            return resourceKey; // Fallback with the key name
        }

        public static string GetDisplayValue(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes[0].ResourceType != null)
                return lookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }

       
    }
}
