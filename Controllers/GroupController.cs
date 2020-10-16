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
    public class GroupController : ControllerBase
    {
        #region Properties
        IConfiguration _configuration;

        ElasticClient _client;

        int _defaultSize;
        #endregion

        #region Get all Groups
        // GET: api/<GroupController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Groups
            ISearchResponse<Group> searchResponse = _client.Search<Group>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Groups, returns 200
                // Maps uid to the Groups
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList(), 200);
            }

            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Group by uid
        // GET api/<GroupController>/787dc20e354611e98af5641c67730998
        [HttpGet("{group_uid}")]
        public ApiResponse GetByUid(string group_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Groups by uid
            IGetResponse<Group> getResponse = _client.Get<Group>(group_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Group, returns 200
                // Maps uid to the Group
                getResponse.Source.uid = group_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Group
        // POST api/<GroupController>
        [HttpPost]
        public ApiResponse Post([FromBody] Group group)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Group's document
            IndexResponse indexResponse = _client.IndexDocument(group);

            if (indexResponse.IsValid == true)
            {
                // If has created Group, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Group, returns 500
            return new ApiResponse("Internal server error when trying to create Group", null, 500);
        }
        #endregion

        #region Edit Group
        // PUT api/<GroupController>/787dc20e354611e98af5641c67730998
        [HttpPut("{group_uid}")]
        public ApiResponse Put(string group_uid, [FromBody] Group group)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Group's document
            UpdateResponse<Group> response = _client.Update<Group, Group>(
                new DocumentPath<Group>(group_uid),
                u => u
                    .Doc(group)
            );

            if (response.IsValid == true)
            {
                // If has updated Group, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Group, returns 500
            return new ApiResponse("Internal server error when trying to update Group", null, 500);
        }
        #endregion

        #region Delete Group
        // DELETE api/<GroupController>/787dc20e354611e98af5641c67730998
        [HttpDelete("{group_uid}")]
        public ApiResponse Delete(string group_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Group's document
            DeleteResponse response = _client.Delete<Group>(group_uid);

            if (response.IsValid == true)
            {
                // If has deleted Group, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Group, returns 500
            return new ApiResponse("Internal server error when trying to delete Group", null, 500);
        }
        #endregion

        #region Constructor
        public GroupController(IConfiguration configuration)
        {
            // Sets configuration
            this._configuration = configuration;

            // Connects to ES
            this._client = new ElasticsearchService(this._configuration, "groups").Get();

            this._defaultSize = Int32.Parse(this._configuration.GetSection("ElasticsearchSettings").GetSection("defaultSize").Value);
        }
        #endregion
    }
}
