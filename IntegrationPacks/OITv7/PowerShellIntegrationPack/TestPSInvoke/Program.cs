using System;
using System.Collections.Generic;
using System.Linq;
using PowerShellIntegrationPack;

namespace TestPSInvoke
{
    class Program
    {
        static void WriteResult(List<Dictionary<string,string>> results)
        {
            Console.WriteLine("RESULTS ({0})", results.Count);

            foreach (var dict in results)
            {
                Console.WriteLine("==== {0}", dict.Count);
                foreach (var kvp in dict.OrderBy(e => e.Key))
                {
                    Console.WriteLine("    {0,20} = {1}", kvp.Key, kvp.Value);
                }
            }
        }

        static void Main(string[] args)
        {
            var runspaceName = "DefaultRunspace";
            var credential = new System.Net.NetworkCredential("", "", "");

            PowerShellClient.OpenRunspace(
                credential,
                false,
                runspaceName,
                "",
                "",
                "",
                "zyv",
                0,
                true,
                "",
                @"C:\temp\out.log",
                @"c:\temp\err.log");

            WriteResult(PowerShellClient.RunScript(
                credential,
                runspaceName,
                new List<string> { "Get-Host; 1+2" }));

            WriteResult(PowerShellClient.RunScript(
                credential,
                runspaceName,
                new List<string> { "hostname | write-host" }));

            WriteResult(PowerShellClient.RunScript(
                credential,
                runspaceName,
                new List<string> { "a=1+2" }));

            PowerShellClient.CloseRunspace(
                credential,
                runspaceName);
        }
    }
}
