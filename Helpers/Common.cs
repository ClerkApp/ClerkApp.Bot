using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClerkBot.Helpers
{
    public static class Common
    {
        public static readonly List<string> BugTypes = new List<string>() { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" };

        public static string BuildDialogId()
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();
            
            if (methodInfo.ReflectedType == null)
            {
                throw new Exception("Cannot get caller class name for building the DialogId.");
            }

            return methodInfo.ReflectedType.Name;
        }

        public static string BuildDialogId(string dialogId)
        {
            var methodInfo = new StackTrace().GetFrame(1).GetMethod();

            if (methodInfo.ReflectedType == null)
            {
                throw new Exception("Cannot get caller class name for building the DialogId.");
            }

            return $"{methodInfo.ReflectedType.Name}.{dialogId}";
        }
    }
}
