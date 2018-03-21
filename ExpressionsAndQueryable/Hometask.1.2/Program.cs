using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hometask._1._2
{
    class Program
    {
        static void Main(string[] args)
        {
            var marker = new Marker
            {
                Name = "Tubo",
                Precision = 0.7,
                Manufacturer = "Toshima",
                Color = "Blue Lagoon",
                LastModelChange = DateTime.Today
            };
            var mapperGenerator = new MappingGenerator();
            var mapper = mapperGenerator.Generate<Marker, MarkerViewModel>();
            var markerViewModel = mapper.Map(marker);
            Console.WriteLine("{0} Name value: {1}, Precision value: {2}, Color value: {3}", 
                markerViewModel, markerViewModel.Name, markerViewModel.Precision, markerViewModel.Color);
            Console.ReadLine();
        }
    }
}
