using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class Rule
    {
        public string uid { get; set; }

        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string group_uid { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public string metric_data_type_code { get; set; }

        public string operator_code { get; set; }

        public string rule_value { get; set; }

        public DateTime create_date { get; set; }
    }
}
