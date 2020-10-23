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
    public class ExperienceSetController : TemplateController
    {
        #region Get all Experiences Sets
        // GET: api/<ExperienceSetController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experiences Sets
            ISearchResponse<ExperienceSet> searchResponse = _client.Search<ExperienceSet>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Experiences Sets, returns 200
                    // Maps uid to the Experiences Sets
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 Experiences Sets, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Experiences Sets", null, 500);
        }
        #endregion

        #region Get Experience Set by uid
        // GET api/<ExperienceSetController>/33ba942aaca411e9b143023fad48cc33
        [HttpGet("{experience_set_uid}")]
        public ApiResponse GetByUid(string experience_set_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experience Set by uid
            IGetResponse<ExperienceSet> getResponse = _client.Get<ExperienceSet>(experience_set_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Experience Set, returns 200
                // Maps uid to the Experience Set
                getResponse.Source.uid = experience_set_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Experience Set by Experience uid and Experience Set code
        // GET api/<ExperienceSetController>/rc_1/experience/33ba942aaca411e9b143023fad48cc33
        [HttpGet("{experience_set_code}/experience/{experience_uid}")]
        public ApiResponse GetByCodeAndUid(string experience_set_code, string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experience Set by Experience uid and Experience Set code
            ISearchResponse<ExperienceSet> searchResponse = _client.Search<ExperienceSet>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.experience_uid)
                        .Query(experience_uid)
                    ) && q
                    .Match(m => m
                        .Field(f => f.code)
                        .Query(experience_set_code)
                    )
                )
            );

            if(searchResponse.IsValid == true)
            {
                // If has found Experience Set, returns 200
                // Maps uid to the Experience Set
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).FirstOrDefault(), 200);
            }

            // If doesn't find any, returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Experience Sets by Experience uid
        // GET api/<ExperienceSetController>/33ba942aaca411e9b143023fad48cc33
        [HttpGet("experience/{experience_uid}")]
        public ApiResponse GetByExperienceUid(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experience Sets by Experience uid
            ISearchResponse<ExperienceSet> searchResponse = _client.Search<ExperienceSet>(s => s
                .From(0)
                .Size(this._defaultSize)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.experience_uid)
                        .Query(experience_uid)
                    )
                )
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Experience Sets, returns 200
                // Maps uid to the Experience Sets
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList(), 200);
            }

            // If doesn't find any, returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Experience Set
        // POST api/<ExperienceSetController>
        [HttpPost]
        public ApiResponse Post([FromBody] ExperienceSet experience_set)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Experience Set's document
            IndexResponse indexResponse = _client.IndexDocument(experience_set);

            if (indexResponse.IsValid == true)
            {
                // If has created Experience Set, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Experience Set, returns 500
            return new ApiResponse("Internal server error when trying to create Experience Set", null, 500);
        }
        #endregion

        #region Edit Experience Set
        // PUT api/<ExperienceSetController>/33ba942aaca411e9b143023fad48cc33
        [HttpPut("{experience_set_uid}")]
        public ApiResponse Put(string experience_set_uid, [FromBody] ExperienceSet experience_set)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Experience Set's document
            UpdateResponse<ExperienceSet> response = _client.Update<ExperienceSet, ExperienceSet>(
                new DocumentPath<ExperienceSet>(experience_set_uid),
                u => u
                    .Doc(experience_set)
            );

            if (response.IsValid == true)
            {
                // If has updated Experience Set, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Experience Set, returns 500
            return new ApiResponse("Internal server error when trying to update Experience Set", null, 500);
        }
        #endregion

        #region Delete Experience Set
        // DELETE api/<ExperienceSetController>/33ba942aaca411e9b143023fad48cc33
        [HttpDelete("{experience_set_uid}")]
        public ApiResponse Delete(string experience_set_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Experience Set's document
            DeleteResponse response = _client.Delete<ExperienceSet>(experience_set_uid);

            if (response.IsValid == true)
            {
                // If has deleted Experience Set, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Experience Set, returns 500
            return new ApiResponse("Internal server error when trying to delete Experience Set", null, 500);
        }
        #endregion

        #region Delete Experience Sets by Experience Uid
        // DELETE api/<ExperienceSetController>/experience/33ba942aaca411e9b143023fad48cc33
        [HttpDelete("experience/{experience_uid}")]
        public ApiResponse DeleteByExperienceUid(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Experience Sets documents by Experience uid
            var deleteResponse = _client.DeleteByQuery<ExperienceSet>(q => q
                .Query(rq => rq
                    .Match(m => m
                        .Field(f => f.experience_uid)
                        .Query(experience_uid)
                    )
                )
            );

            if (deleteResponse.IsValid == true)
            {
                // If has deleted Experience Sets, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Experience Sets, returns 500
            return new ApiResponse("Internal server error when trying to delete Experience Sets", null, 500);
        }
        #endregion

        #region Constructor
        public ExperienceSetController(IConfiguration configuration) : base(configuration, "experiencesets")
        {
        }
        #endregion
    }
}
