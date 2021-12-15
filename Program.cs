using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SchematicsProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            Console.CursorVisible = true;

            string Input = CreateInput(args);

            if (args.Length == 0) {
                Console.WriteLine("Input command");
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
