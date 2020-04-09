using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class AssemblyHelper
    {
        public static Assembly EntryAssembly { get; set; } = Assembly.GetEntryAssembly();

        public static Assembly GetEntryAssembly() => EntryAssembly;
    }
}
