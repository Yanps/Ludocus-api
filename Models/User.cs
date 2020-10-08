using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class User
    {
        public string organization_uid { get; set; }

        public string name { get; set; }

        public string surname { get; set; }

        public string email { get; set; }

        public string password_hash { get; set; }

        public string role { get; set; }

        public string session { get; set; }

        public UserMetric metrics { get; set; }

        public DateTime create_date { get; set; }
    }
}
