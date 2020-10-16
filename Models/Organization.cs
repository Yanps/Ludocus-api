using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    [ElasticsearchType(IdProperty = nameof(uid))]
    public class Organization
    {
        public string uid { get; set; }

        public string admin_user_uid { get; set; }

        public string name { get; set; }

        public string fancy_name { get; set; }

        public string slug { get; set; }

        public DateTime create_date { get; set; }
    }
}
