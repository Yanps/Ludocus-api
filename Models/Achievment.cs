using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class Achievment
    {
        public string code { get; set; }

        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string group_uid { get; set; }

        public string name { get; set; }

        public string affected_metric_code { get; set; }

        public float affected_metric_value { get; set; }

        public DateTime create_date { get; set; }
    }
}
