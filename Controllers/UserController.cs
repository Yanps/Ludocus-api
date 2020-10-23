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
    public class UserController : TemplateController
    {
        #region Get all Users
        // GET: api/<UserController>
        [HttpGet]
        public ApiResponse GetAll()
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
        public UserController(IConfiguration configuration) : base(configuration, "users")
        {
        }
        #endregion
    }
}
