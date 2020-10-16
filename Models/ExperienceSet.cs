using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class ExperienceSet
    {
        public string uid { get; set; }

        public string experience_uid { get; set; }

        public string code { get; set; }

        public string metric_code { get; set; }

        public string rule_code { get; set; }

        public string achievment_code { get; set; }
    }
}
