using System;

namespace AspNetCore.Mvc.Extensions.HostedServices
{
    public class CronJobAttribute : Attribute
    {
        public string[] Schedules { get; }
        public CronJobAttribute(params string[] schedules)
        {
            Schedules = schedules;
        }
    }
}
