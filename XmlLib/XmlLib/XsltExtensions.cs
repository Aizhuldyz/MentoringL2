using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlLib
{
    public class XsltExtensions
    {
        private readonly Dictionary<string, int> _counters = new Dictionary<string, int>();
        public string GetReportTitle()
        {
            return  $"Report generated at ({DateTime.Now:MM/dd/yyyy})";
        }
        

        public int GetAndInc(string name)
        {
            int result = 0;
            _counters.TryGetValue(name, out result);
            _counters[name] = result + 1;
            return _counters[name];
        }

    }
}
