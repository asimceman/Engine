using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace SchematicsProject.component_list
{
    public class ComponentList
    {
        public static dynamic AddDynamic(IDictionary<string, Object> Model)
        {

            ArrayList Filters = new ArrayList();
            var Filter = "";
            do
            {
                Console.WriteLine("Which filters does this component need (press enter or empty line to finish)");
                ExpandoObject Tip = new ExpandoObject();
                Filter = Console.ReadLine();
                if (!string.IsNullOrEmpty(Filter))
                {
                    Console.CursorVisible = false;

                    List<string> Options = new List<string>();

                    Options.Add("input");
                    Options.Add("datepicker");
                    Options.Add("autocomplete");
                    Options.Add("select");
                    Options.Add("checkbox");
                    Options.Add("textarea");
                    Console.WriteLine("Which type?");
                    Menu Menu = new Menu(Options.ToArray());
                    int SelectedIndex = Menu.Run();

                    var FilterType = Options[SelectedIndex];
                    Console.WriteLine("Size?");
                    Options.RemoveAll(x => true);
                    Options.Add("2");
                    Options.Add("3");
                    Options.Add("6");
                    Options.Add("12");
                    Menu = new Menu(Options.ToArray());
                    SelectedIndex = Menu.Run();
                    var Size = int.Parse(Options[SelectedIndex]);
                    Tip.TryAdd("filterType", FilterType);
                    Tip.TryAdd("size", Size);
                    //Object filterinjo = new Object({ filter.ToString() = tip; });
                    Filters.Add(new Filter { Name = Filter, FilterObject = Tip });
                }
            }
            while (Filter != "");
            Model.TryAdd("filters", Filters);

            return Model;

        }
    }
}
