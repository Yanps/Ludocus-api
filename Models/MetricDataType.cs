using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class MetricDataType
    {
        public string uid { get; set; }

        public string code { get; set; }

        public string name { get; set; }
    }
}
