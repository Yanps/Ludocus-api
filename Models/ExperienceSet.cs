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

    public class AnalyzableExperienceSet : ExperienceSet
    {
        public Metric metric { get; set; }

        public MetricValues metric_values { get; set; }

        public Rule rule { get; set; }

        public Achievment achievment { get; set; }

        public AnalyzableExperienceSet(ExperienceSet experience_set, Metric metric, Rule rule, Achievment achievment)
        {
            this.uid = experience_set.uid;
            this.experience_uid = experience_set.experience_uid;
            this.code = experience_set.code;
            this.metric_uid = experience_set.metric_uid;
            this.rule_uid = experience_set.rule_uid;
            this.achievment_uid = experience_set.achievment_uid;

            this.metric = metric;
            this.rule = rule;
            this.achievment = achievment;
        }
    }
}
