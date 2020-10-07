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
    public class ExperienceController : ControllerBase
    {
        #region Properties
        IConfiguration _configuration;

        ElasticClient _client;

        int _defaultSize;
        #endregion

        #region Get all Experiences
        // GET: api/<ExperienceController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experiences
            ISearchResponse<Experience> searchResponse = _client.Search<Experience>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                return new ApiResponse(searchResponse.Documents, 200);
            }

            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Experience by uid
        // GET api/<ExperienceController>/33ba942aaca411e9b143023fad48cc33
        [HttpGet("{experience_uid}")]
        public ApiResponse Get(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experiences by uid
            IGetResponse<Experience> getResponse = _client.Get<Experience>(experience_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Experience, returns 200
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Experience
        // POST api/<ExperienceController>
        [HttpPost]
        public ApiResponse Post([FromBody] Experience experience)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Experience's document
            IndexResponse indexResponse = _client.IndexDocument(experience);

            if (indexResponse.IsValid == true)
            {
                // If has created Experience, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Experience, returns 500
            return new ApiResponse("Internal server error when trying to create Experience", null, 500);
        }
        #endregion

        #region Edit Experience
        // PUT api/<ExperienceController>/33ba942aaca411e9b143023fad48cc33
        [HttpPut("{experience_uid}")]
        public ApiResponse Put(string experience_uid, [FromBody] Experience experience)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Experience's document
            UpdateResponse<Experience> response = _client.Update<Experience, Experience>(
                new DocumentPath<Experience>(experience_uid),
                u => u
                    .Index("experiences")
                    .Doc(experience)
            );

            if (response.IsValid == true)
            {
                // If has updated Experience, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Experience, returns 500
            return new ApiResponse("Internal server error when trying to update Experience", null, 500);
        }
        #endregion
        
        #region Delete Experience
        // DELETE api/<ExperienceController>/33ba942aaca411e9b143023fad48cc33
        [HttpDelete("{experience_uid}")]
        public ApiResponse Delete(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Experience's document
            DeleteResponse response = _client.Delete<Experience>(experience_uid);

            if (response.IsValid == true)
            {
                // If has deleted Experience, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Experience, returns 500
            return new ApiResponse("Internal server error when trying to delete Experience", null, 500);
        }
        #endregion

        #region Constructor
        public ExperienceController(IConfiguration configuration)
        {
            // Sets configuration
            this._configuration = configuration;

            // Connects to ES
            this._client = new ElasticsearchService(this._configuration, "organizations").Get();

            this._defaultSize = Int32.Parse(this._configuration.GetSection("ElasticsearchSettings").GetSection("defaultSize").Value);
        }
        #endregion
    }
}
