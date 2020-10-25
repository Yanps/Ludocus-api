using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class ExperiencePanel
    {
        public Experience experience { get; set; }

        public Metric reference_metric { get; set; }

        public List<PanelSet> panel_sets { get; set; }

        public ExperiencePanel()
        {
            this.panel_sets = new List<PanelSet>();
        }
    }
}
