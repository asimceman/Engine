using System;
using System.Collections.Generic;
using System.Text;

namespace SchematicsProject
{
    public class Menu
    {
        private int SelectedIndex;
        private string[] Options;
        private bool first = true;
        public Menu(string[] options)
        {
            Options = options;
            SelectedIndex = 0;
        }

        public void DisplayOptions()
        {
            Console.CursorVisible = false;

            if (!first)
            {
                Console.SetCursorPosition(0, Console.CursorTop - Options.Length);

            }


            for (int i = 0; i < Options.Length; i++)
            {
                if (!first)
                {
                    ClearCurrentConsoleLine();
                }
                string currentOption = Options[i];
                string prefix;

                if (i == SelectedIndex)
                {
                    prefix = ">";
                }
                else
                {
                    prefix = "";
                }
                Console.WriteLine($"{prefix}{currentOption}");

            }
            first = false;

        }

        public static void ClearCurrentConsoleLine()
        {

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);

        }


        public int Run()
        {
            ConsoleKey keyPressed;
            first = true;
            do
            {
                DisplayOptions();
                first = false;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;
                if (keyPressed == ConsoleKey.UpArrow)
                {
                    if(SelectedIndex!=0)
                        SelectedIndex--;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    if(SelectedIndex!=Options.Length-1)
                        SelectedIndex++;
                }
            }
            while (keyPressed != ConsoleKey.Enter);
            return SelectedIndex;
        }

    }
}
