using System;
using RazorLight;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace SchematicsProject
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
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

                var engine = new Engine();
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
