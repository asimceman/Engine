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
            var ServiceCollection = new ServiceCollection();
            IConfiguration Configuration;
            Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName).AddJsonFile("appsettings.json").Build();
            ServiceCollection.AddSingleton<IConfiguration>(Configuration);
            Console.CursorVisible = true;
            string Input = "";
            bool First = true;
            foreach (string Argument in args)
            {
                if (First)
                {
                    Input += Argument;
                    First = false;
                }
                else
                {
                    Input += " " + Argument;
                }
            }

            if (args.Length == 0) {
                Console.WriteLine("Input command");
                Input = Console.ReadLine();
            }

                var Engine = new Engine(Configuration);
                try
                {
                    await Engine.Input(Input);
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
