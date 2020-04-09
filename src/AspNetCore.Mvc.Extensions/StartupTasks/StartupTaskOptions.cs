using System;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public class StartupTaskOptions
    {
        public bool LoadApplicationDependencies { get; set; } = true;

        public string LoadPathDependencies { get; set; }
        public Func<Assembly, bool> Predicate { get; set; } = a => true;
    }
}
