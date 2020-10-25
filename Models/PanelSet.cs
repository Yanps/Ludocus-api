using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class PanelSet
    {
        public MetricValues metric_values { get; set; }

        public UserResponse user { get; set; }

        public PanelSet(MetricValues metric_values, UserResponse user)
        {
            this.metric_values = metric_values;
            this.user = user;
        }
    }
}
