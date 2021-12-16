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

            string input = CreateInput(args);

            if (args.Length == 0) {
                Console.WriteLine("Input component");
                input = Console.ReadLine();
            }

            var engine = new Engine();
            try
            {
                await engine.InputProcces(input);
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
            return input;
        }

        
    }

    
}
