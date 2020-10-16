using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserController : ControllerBase
    {
        #region Properties
        IConfiguration _configuration;

        ElasticClient _client;

        int _defaultSize;
        #endregion

        #region Get all Users
        // GET: api/<UserController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Users
            ISearchResponse<User> searchResponse = _client.Search<User>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Users, returns 200
                // Maps uid to the Users
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }), 200);
            }

            // If hasn't found Users, returns 204
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get User by uid
        // GET api/<UserController>/fd8546c4312d11e985c4641c67730998
        [HttpGet("{user_uid}")]
        public ApiResponse Get(string user_uid)
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
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new User
        // POST api/<UserController>
        [HttpPost]
        public ApiResponse Post([FromBody] User user)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes User's document
            IndexResponse indexResponse = _client.IndexDocument(user);

            if (indexResponse.IsValid == true)
            {
                // If has created User, returns 201
                return new ApiResponse(indexResponse.Id, 201);
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
            DeleteResponse response = _client.Delete<User>(user_uid);

            if (response.IsValid == true)
            {
                // If has deleted User, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted User, returns 500
            return new ApiResponse("Internal server error when trying to delete User", null, 500);
        }
        #endregion

        #region Constructor
        public UserController(IConfiguration configuration)
        {
            // Sets configuration
            this._configuration = configuration;

            // Connects to ES
            this._client = new ElasticsearchService(this._configuration, "users").Get();

            this._defaultSize = Int32.Parse(this._configuration.GetSection("ElasticsearchSettings").GetSection("defaultSize").Value);
        }
        #endregion
    }
}
