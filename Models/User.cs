using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class UserResponse
    {
        public string uid { get; set; }

        public string organization_uid { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public string surname { get; set; }

        public string email { get; set; }

        public UserResponse(string uid, string organization_uid, string code, string name, string surname, string email)
        {
            this.uid = uid;
            this.organization_uid = organization_uid;
            this.code = code;
            this.name = name;
            this.surname = surname;
            this.email = email;
        }

        public UserResponse() { }

        public UserResponse(User user)
        {
            this.uid = user.uid;
            this.organization_uid = user.organization_uid;
            this.code = user.code;
            this.name = user.name;
            this.surname = user.surname;
            this.email = user.email;
        }
    }

    [ElasticsearchType(IdProperty = nameof(uid))]
    public class User: UserResponse
    {

        public string password_hash { get; set; }

        public string role { get; set; }

        public string session { get; set; }

        public DateTime? create_date { get; set; }

        public User() { }

        public User(string uid, string organization_uid, string code, string name, string surname, string email, string password_hash, string role, string session, DateTime? create_date)
        {
            this.uid = uid;
            this.organization_uid = organization_uid;
            this.code = code;
            this.name = name;
            this.surname = surname;
            this.email = email;

            this.password_hash = password_hash;
            this.role = role;
            this.session = session;
            this.create_date = create_date;
        }

        public User(UserResponse userResponse, string password_hash, string role, string session, DateTime? create_date)
        {
            this.uid = userResponse.uid;
            this.organization_uid = userResponse.organization_uid;
            this.code = userResponse.code;
            this.name = userResponse.name;
            this.surname = userResponse.surname;
            this.email = userResponse.email;

            this.password_hash = password_hash;
            this.role = role;
            this.session = session;
            this.create_date = create_date;
        }
    }
}
