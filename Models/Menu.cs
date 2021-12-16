using System;
using System.Collections.Generic;
using System.Text;

namespace SchematicsProject
{
    public class Menu
    {
        private int selectedIndex;
        private string[] Options;
        private bool First = true;
        public Menu(string[] options)
        {
            this.Options = options;
            selectedIndex = 0;
        }

        public void DisplayOptions()
        {
            Console.CursorVisible = false;

            if (!First)
            {
                Console.SetCursorPosition(0, Console.CursorTop - Options.Length);
            }

            for (int i = 0; i < Options.Length; i++)
            {
                if (!First)
                {
                    ClearCurrentConsoleLine();
                }
                string currentOption = Options[i];
                string prefix;

                if (i == selectedIndex)
                {
                    prefix = ">";
                }
                else
                {
                    prefix = "";
                }
                Console.WriteLine($"{prefix}{currentOption}");
            }
            First = false;
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
            First = true;
            do
            {
                DisplayOptions();
                First = false;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;
                if (keyPressed == ConsoleKey.UpArrow)
                {
                    if(selectedIndex!=0)
                        selectedIndex--;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    if(selectedIndex!=Options.Length-1)
                        selectedIndex++;
                }
            }
            while (keyPressed != ConsoleKey.Enter);
            return selectedIndex;
        }
    }
}
