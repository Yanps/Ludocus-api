using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class Metric
    {
        public string uid { get; set; }

        public string code { get; set; }

        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string group_uid { get; set; }

        public string name { get; set; }

        public string data_type { get; set; }

        public string classification { get; set; }

        public string model { get; set; }

        public DateTime create_date { get; set; }
    }
}
