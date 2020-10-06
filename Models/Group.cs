using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class Group
    {
        public string organization_uid { get; set; }

        public string owner_user_uid { get; set; }

        public string name { get; set; }

        public List<string> user_uid_list { get; set; }

        public DateTime create_date { get; set; }
    }
}
