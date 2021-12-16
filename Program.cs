using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SchematicsProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            Console.CursorVisible = true;

            string Input = CreateInput(args);

            if (args.Length == 0) {
                Console.WriteLine("Input component");
                Input = Console.ReadLine();
            }

            var Engine = new Engine();
            try
            {
                await Engine.InputProcces(Input);
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
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<Engine>();
        }

        public static string CreateInput(string[] args)
        {
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
            return Input;
        }

        
    }

    
}
