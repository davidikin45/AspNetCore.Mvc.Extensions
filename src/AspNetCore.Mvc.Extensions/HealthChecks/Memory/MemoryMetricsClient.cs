﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AspNetCore.Mvc.Extensions.HealthChecks.Memory
{
    public class MemoryMetrics
    {
        public double Total;
        public double Used;
        public double Free;
        public long Duration;
    }

    //https://gunnarpeipman.com/aspnet/aspnet-core-memory-health-check/?fbclid=IwAR33rFwdbXj1p7GeAMgg_7-InK252y7LHtzS1tanSEEq57XHZ8vx1vrGyy0#.XVb9Sv_htEg.facebook
    public class MemoryMetricsClient
    {
        public MemoryMetrics GetMetrics()
        {
            MemoryMetrics metrics;
            var watch = new Stopwatch();

            watch.Start();
            if (IsUnix())
            {
                metrics = GetUnixMetrics();
            }
            else
            {
                metrics = GetWindowsMetrics();
            }

            watch.Stop();

            metrics.Duration = watch.ElapsedMilliseconds;

            return metrics;
        }

        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private MemoryMetrics GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;


            return metrics;
        }

        private MemoryMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = double.Parse(memory[1]);
            metrics.Used = double.Parse(memory[2]);
            metrics.Free = double.Parse(memory[3]);

            return metrics;
        }
    }
}
