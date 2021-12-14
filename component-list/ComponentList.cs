using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace SchematicsProject.component_list
{
    public class ComponentList
    {
        public static dynamic AddDynamic(IDictionary<string, Object> model)
        {

            ArrayList filters = new ArrayList();
            var filter = "";
            do
            {
                Console.WriteLine("Which filters does this component need (press enter or empty line to finish)");
                ExpandoObject tip = new ExpandoObject();
                filter = Console.ReadLine();
                if (!string.IsNullOrEmpty(filter))
                {
                    Console.CursorVisible = false;
                    bool first = true;

                    List<string> options = new List<string>();

                    options.Add("input");
                    options.Add("datepicker");
                    options.Add("autocomplete");
                    options.Add("select");
                    options.Add("checkbox");
                    options.Add("textarea");
                    Console.WriteLine("Which type?");
                    Menu menu = new Menu(options.ToArray());
                    int selectedIndex = menu.Run();

                    var filterType = options[selectedIndex];
                    Console.WriteLine("Size?");
                    options.RemoveAll(x => true);
                    options.Add("2");
                    options.Add("3");
                    options.Add("6");
                    options.Add("12");
                    menu = new Menu(options.ToArray());
                    selectedIndex = menu.Run();
                    var size = int.Parse(options[selectedIndex]);
                    tip.TryAdd("filterType", filterType);
                    tip.TryAdd("size", size);
                    //Object filterinjo = new Object({ filter.ToString() = tip; });
                    filters.Add(new Filter { Name = filter, FilterObject = tip });
                }
            }
            while (filter != "");
            model.TryAdd("filters", filters);

            return model;

        }
    }
}
