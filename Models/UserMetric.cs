using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class UserMetric
    {
        public string name { get; set; }

        public List<float> float_value { get; set; }

        public List<bool> bool_value { get; set; }

        public List<string> string_value { get; set; }
    }
}
