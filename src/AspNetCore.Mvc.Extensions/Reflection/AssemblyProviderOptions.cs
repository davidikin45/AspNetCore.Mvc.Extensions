using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Reflection
{
    public class AssemblyProviderOptions
    {
        public string BinPath { get; set; } = AppContext.BaseDirectory;
        public Func<string, Boolean> AssemblyFilter { get; set; }
    }
}
