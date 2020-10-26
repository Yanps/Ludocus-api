using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class Experience
    {
        public string uid { get; set; }

        public string code { get; set; }

        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string group_uid { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string type { get; set; }

        public List<string> level_ordering { get; set; }

        public string reference_metric_uid { get; set; }

        public DateTime? create_date { get; set; }
    }

    public class ExperienceResponse : Experience
    {
        public Metric reference_metric { get; set; }

        public ExperienceResponse(Experience experience, Metric reference_metric)
        {
            this.uid = experience.uid;
            this.code = experience.code;
            this.organization_uid = experience.organization_uid;
            this.owner_user_uid = experience.owner_user_uid;
            this.group_uid = experience.group_uid;
            this.name = experience.name;
            this.description = experience.description;
            this.type = experience.type;
            this.level_ordering = experience.level_ordering;
            this.reference_metric_uid = experience.reference_metric_uid;
            this.create_date = experience.create_date;

            this.reference_metric = reference_metric;
        }
    }
}
