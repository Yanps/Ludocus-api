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

        public class RuleResponse : Rule
        {
            public string operator_name { get; set; }

            public RuleResponse(Rule rule)
            {
                this.uid = rule.uid;
                this.code = rule.code;
                this.organization_uid = rule.organization_uid;
                this.owner_user_uid = rule.owner_user_uid;
                this.group_uid = rule.group_uid;
                this.name = rule.name;
                this.metric_data_type_code = rule.metric_data_type_code;
                this.operator_code = rule.operator_code;
                this.rule_value = rule.rule_value;
                this.create_date = rule.create_date;

                if (this.operator_code == "==") this.operator_name = "=";
                else if (this.operator_code == "!=") this.operator_name = "≠";
                else if (this.operator_code == ">=") this.operator_name = "≥";
                else if (this.operator_code == ">") this.operator_name = ">";
                else if (this.operator_code == "<=") this.operator_name = "≤";
                else if (this.operator_code == ">") this.operator_name = "<";
            }
        }
    }
}
