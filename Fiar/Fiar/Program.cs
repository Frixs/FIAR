// Task: https://github.com/osvetlik/pia2020/tree/master/semester-project

using Ixs.DNA;
using Ixs.DNA.AspNet;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Fiar
{
    public class Program
    {
        /// <summary>
        /// Version string
        /// </summary>
        public static string Version = "1.0.0.0";

        public static void Main(string[] args)
        {
            Console.WriteLine("*******************************************************************************************");
            Console.WriteLine("**********************-----------------------------------------------**********************");
            Console.WriteLine("*******************************------ STARTING - FIAR ------*******************************");
            Console.WriteLine("***********************_____________________________________________***********************");
            Console.WriteLine("*******************************************************************************************\n");

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder()
                // Add DNA Framework
                .UseDnaFramework(construction =>
                {
                    // Configure framework

                    // Add file logger
                    construction.AddFileLogger(
                        logPath: Framework.Construction.Environment.IsDevelopment ? "logs/debug.log" : "logs/Fiar.log",
                        logLevel: (LogLevel)Enum.Parse(typeof(LogLevel), Framework.Construction.Configuration.GetSection("Logging:LogLevel:Default").Value, true),
                        trimSize: 50000000 // 50MB limit
                        );

                    // Rewrite the default logger from DNA Framework to our own
                    construction.Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger(typeof(Program).Namespace));

                    // Bind config box
                    construction.Services.AddSingleton<IConfigBox>(new ConfigBox(Framework.Construction.Configuration));
                })
                .UseStartup<Startup>();
        }
    }
}
