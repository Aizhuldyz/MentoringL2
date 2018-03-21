using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hometask._1._2
{
    public class Marker
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public double Precision { get; set; }
        public string Manufacturer { get; set; }
        public DateTime LastModelChange { get; set; }
    }

    public class MarkerViewModel
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public double Precision { get; set; }
    }
}
