﻿using Nest;
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

        public MetricValues(string metric_values_uid, string metric_uid, string user_uid, List<string> values, DateTime update_date)
        {
            this.uid = metric_values_uid;
            this.metric_uid = metric_uid;
            this.user_uid = user_uid;
            this.values = values;
            this.update_date = update_date;
        }

        public MetricValues() { }
    }

    public class MetricValuesResponse: MetricValues
    {
        public string user_code { get; set; }

        public string user_full_name { get; set; }

        public MetricValuesResponse(MetricValues metricValues, string user_code, string user_full_name)
        {
            this.uid = metricValues.uid;
            this.metric_uid = metricValues.metric_uid;
            this.user_uid = metricValues.user_uid;
            this.values = metricValues.values;
            this.update_date = metricValues.update_date;

            this.user_code = user_code;
            this.user_full_name = user_full_name;
        }
    }
}
