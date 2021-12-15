using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace SchematicsProject
{
    public class Filter
    {
        public string Name { get; set; }
        public IDictionary<string, Object> FilterObject { get; set; }
    }
}
