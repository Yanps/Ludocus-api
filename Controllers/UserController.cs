using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using LudocusApi.Models;
using LudocusApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LudocusApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : TemplateController
    {
        #region Get all Users
        // GET: api/<UserController>
        [HttpGet]
        public ApiResponse GetAll([FromBody] string response_type = "suppressed")
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            Func<SourceFilterDescriptor<User>, ISourceFilter> selector;
            // Queries Users
            if (response_type == "suppressed")
            {
                selector = new Func<SourceFilterDescriptor<User>, ISourceFilter>(
                    sr => sr
                    .Includes(fi => fi
                        .Field(f => f.uid)
                        .Field(f => f.organization_uid)
                        .Field(f => f.code)
                        .Field(f => f.name)
                        .Field(f => f.surname)
                        .Field(f => f.email)
                    )
                );
            }
            else
            {
                // response_type == "full"
                selector = null;
            }

            ISearchResponse<User> searchResponse = _client.Search<User>(s => s
                .From(0)
                .Size(this._defaultSize)
                .Source(selector)
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Users, returns 200
                    // Maps uid to the Users
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 Users, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Users", null, 500);
        }
        #endregion

        #region Get User by uid
        // GET api/<UserController>/fd8546c4312d11e985c4641c67730998
        [HttpGet("{user_uid}")]
        public ApiResponse GetByUid(string user_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Users by uid
            IGetResponse<User> getResponse = _client.Get<User>(user_uid);

            if (getResponse.IsValid == true)
            {
                // If has found User, returns 200
                // Maps uid to the User
                getResponse.Source.uid = user_uid;
                return new ApiResponse((User)getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Users by Group uid
        // GET api/<UserController>/group/787dc20e354611e98af5641c67730998
        [HttpGet("group/{group_uid}")]
        public ApiResponse GetByGroupUid(string group_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Get Group by uid
            GroupController groupController = new GroupController(this._configuration);
            ApiResponse groupResponse = groupController.GetByUid(group_uid);
            
            if(groupResponse.StatusCode == 200)
            {
                // Gets the user uid list from the Group
                List<string> user_uid_list = ((Group)groupResponse.Result).user_uid_list;

                // Instantiates the User list
                List<User> user_list = new List<User>();

                foreach (string user_uid in user_uid_list)
                {
                    // Queries Users by uid
                    IGetResponse<User> getResponse = _client.Get<User>(user_uid);

                    if (getResponse.IsValid == true)
                    {
                        // Maps uid to the User
                        getResponse.Source.uid = user_uid;
                        // If has found User, adds to the User list
                        user_list.Add(getResponse.Source);
                    } else
                    {
                        // If hasn't found a User, returns 500
                        return new ApiResponse("Internal server error when trying to get Users", null, 500);
                    }
                }

                // If has found all Users
                return new ApiResponse(user_list, 200);
            } 
            // If there's an error with the Group request, returns Status Code
            return new ApiResponse("Error when trying to get Group", groupResponse.StatusCode);
        }
        #endregion

        #region Create new User
        // POST api/<UserController>
        [HttpPost]
        public ApiResponse Create([FromBody] User user)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Sets User's organization_uid and uid
            user.uid = null;
            user.organization_uid = "fdefb6ee312d11e9a3ce641c67730998";

            // Hashes password with md5
            user.password_hash = this.GenerateHash(user.password_hash);

            // Creates a initial session token
            user.session = new Guid().ToString();

            // Sets User's CreateDate
            user.create_date = DateTime.UtcNow;

            // Indexes User's document
            IndexResponse indexResponse = _client.IndexDocument(user);

            if (indexResponse.IsValid == true)
            {
                // If has created User, creates MetricValues for Metrics
                // Search all Metrics first
                MetricController metricController = new MetricController(this._configuration);
                ApiResponse metricResponse = metricController.GetAll();

                if (metricResponse.StatusCode == 200)
                {
                    // If has found Metrics, creates MetricValues for each Metric
                    List<Metric> metricList = (List<Metric>)metricResponse.Result;

                    List<MetricValues> metric_values_list = new List<MetricValues>();
                    foreach (Metric metric in metricList)
                    {
                        // Adds MetricValues to the Metrics Values list
                        metric_values_list.Add(new MetricValues(null, metric.uid, indexResponse.Id, new List<string>(), DateTime.UtcNow));
                    }
                    MetricValuesController metricValuesController = new MetricValuesController(this._configuration);
                    // Bulk adds all Metrics Values
                    ApiResponse metricValuesResponse = metricValuesController.BulkCreate(metric_values_list);
                    if (metricValuesResponse.StatusCode == 201)
                    {
                        // If has created User and MetricValues, returns 201
                        return new ApiResponse(indexResponse.Id, 201);
                    }

                    // If hasn't created Metrics Values, returns 500
                    return new ApiResponse("Internal server error when trying to create Metrics Values", null, 500);
                }

                // If hasn't found Metrics, returns 500
                return new ApiResponse("Internal server error when trying to get Metrics to create Metrics Values", null, 500);
            }

            // If hasn't created User, returns 500
            return new ApiResponse("Internal server error when trying to create User", null, 500);
        }
        #endregion

        #region Edit User
        // PUT api/<UserController>/fd8546c4312d11e985c4641c67730998
        [HttpPut("{user_uid}")]
        public ApiResponse Put(string user_uid, [FromBody] User user)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates User's document
            UpdateResponse<User> response = _client.Update<User, User>(
                new DocumentPath<User>(user_uid),
                u => u
                    .Doc(user)
            );

            if (response.IsValid == true)
            {
                // If has updated User, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated User, returns 500
            return new ApiResponse("Internal server error when trying to update User", null, 500);
        }
        #endregion

        #region Delete User
        // DELETE api/<UserController>/fd8546c4312d11e985c4641c67730998
        [HttpDelete("{user_uid}")]
        public ApiResponse Delete(string user_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes User's document
            DeleteResponse deleteResponse = _client.Delete<User>(user_uid);

            if (deleteResponse.IsValid == true)
            {
                // If has deleted User, deletes MetricValues by User uid
                MetricValuesController metricValuesController = new MetricValuesController(this._configuration);
                ApiResponse metricValuesResponse = metricValuesController.DeleteByUserUid(user_uid);

                if (metricValuesResponse.StatusCode == 200)
                {
                    
                    // If has deleted User and all Metrics Values, returns 200
                    return new ApiResponse("Deleted successfully", null, 200);
                    
                }

                // If hasn't deleted Metrics Values, returns 500
                return new ApiResponse("Internal server error when trying to delete Metrics Values", null, 500);
            }

            // If hasn't deleted User, returns 500
            return new ApiResponse("Internal server error when trying to delete User", null, 500);
        }
        #endregion

        #region Helpers
        private string GenerateHash(string password)
        {
            MD5 md5Hash = MD5.Create();
            // Converter a String para array de bytes, que é como a biblioteca trabalha.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Cria-se um StringBuilder para recompôr a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop para formatar cada byte como uma String em hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
        #endregion

        #region Constructor
        public UserController(IConfiguration configuration) : base(configuration, "users")
        {
        }
        #endregion
    }
}
