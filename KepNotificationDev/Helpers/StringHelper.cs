using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KepNotificationDev.Helpers
{
    public static class StringHelper
    {
        public static List<string> StringBetween(string body, string start, string end)
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
                    indexEnd = indexStart+start.Length + body.Substring(indexStart+start.Length).IndexOf(end);

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
}