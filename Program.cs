using System;
using RazorLight;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SchematicsProject
{
    public class Program
    {
        

        public static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            IConfiguration configuration;
            configuration = new ConfigurationBuilder().SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName).AddJsonFile("appsettings.json").Build();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            Console.CursorVisible = true;
            string input = "";
            bool first = true;
            foreach (string argument in args)
            {
                if (first)
                {
                    input += argument;
                    first = false;
                }
                else
                {
                    input += " " + argument;
                }
            }

            if (args.Length == 0) {
                Console.WriteLine("Input command");
                input = Console.ReadLine();
            }

                var engine = new Engine(configuration);
                try
                {
                    await engine.Input(input);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message + " - valid input is \"-t <nameOfSchematics> <nameForGeneratedFile>\" ");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            
        }

        
    }

    
}
