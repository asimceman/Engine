using System;
using System.Collections.Generic;
using System.Text;

namespace SchematicsProject
{
    public class Menu
    {
        private int SelectedIndex;
        private string[] Options;
        private bool First = true;
        public Menu(string[] Options)
        {
            this.Options = Options;
            SelectedIndex = 0;
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
                string CurrentOption = Options[i];
                string Prefix;

                if (i == SelectedIndex)
                {
                    Prefix = ">";
                }
                else
                {
                    Prefix = "";
                }
                Console.WriteLine($"{Prefix}{CurrentOption}");

            }
            First = false;

        }

        public static void ClearCurrentConsoleLine()
        {

            int CurrentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, CurrentLineCursor);

        }


        public int Run()
        {
            ConsoleKey KeyPressed;
            First = true;
            do
            {
                DisplayOptions();
                First = false;
                ConsoleKeyInfo KeyInfo = Console.ReadKey(true);
                KeyPressed = KeyInfo.Key;
                if (KeyPressed == ConsoleKey.UpArrow)
                {
                    if(SelectedIndex!=0)
                        SelectedIndex--;
                }
                else if (KeyPressed == ConsoleKey.DownArrow)
                {
                    if(SelectedIndex!=Options.Length-1)
                        SelectedIndex++;
                }
            }
            while (KeyPressed != ConsoleKey.Enter);
            return SelectedIndex;
        }

    }
}
