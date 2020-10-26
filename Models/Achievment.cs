using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class Achievment
    {
        public string uid { get; set; }

        public string code { get; set; }

        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string group_uid { get; set; }

        public string name { get; set; }

        public string affected_metric_uid { get; set; }

        public string affected_metric_value { get; set; }

        public DateTime? create_date { get; set; }
    }

    public class AchievmentResponse: Achievment
    {
        public string affected_metric_code { get; set; }

        public AchievmentResponse(Achievment achievment, string affected_metric_code)
        {
            this.uid = achievment.uid;
            this.code = achievment.code;
            this.organization_uid = achievment.organization_uid;
            this.owner_user_uid = achievment.owner_user_uid;
            this.group_uid = achievment.group_uid;
            this.name = achievment.name;
            this.affected_metric_uid = achievment.affected_metric_uid;
            this.affected_metric_value = achievment.affected_metric_value;
            this.create_date = achievment.create_date;

            this.affected_metric_code = affected_metric_code;
        }
    }
}
