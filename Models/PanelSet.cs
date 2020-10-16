using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class PanelSet
    {
        public MetricValues metric_values { get; set; }

        public User user { get; set; }

        public Metric metric { get; set; }

        public PanelSet(MetricValues metric_values, User user, Metric metric)
        {
            this.metric_values = metric_values;
            this.user = user;
            this.metric = metric;
        }
    }
}
