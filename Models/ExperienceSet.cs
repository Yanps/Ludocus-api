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

        public string metric_uid { get; set; }

        public string rule_uid { get; set; }

        public string achievment_uid { get; set; }
    }
}
