using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class MetricValues
    {
        public string uid { get; set; }

        public string metric_uid { get; set; }

        public string user_uid { get; set; }

        public List<string> values { get; set; }

        public DateTime update_date { get; set; }
    }
}
