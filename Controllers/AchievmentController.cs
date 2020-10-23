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
    public class AchievmentController : TemplateController
    {

        #region Get all Achievments
        // GET: api/<AchievmentController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Achievments
            ISearchResponse<Achievment> searchResponse = _client.Search<Achievment>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Achievments, returns 200
                    // Maps uid to the Achievments
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 Achievments, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Achievments", null, 500);
        }
        #endregion

        #region Get Achievment by uid
        // GET api/<AchievmentController>/23ba942aaca411e9b143023fad48cc44
        [HttpGet("{achievment_uid}")]
        public ApiResponse Get(string achievment_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Achievments by uid
            IGetResponse<Achievment> getResponse = _client.Get<Achievment>(achievment_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Achievment, returns 200
                // Maps uid to the Achievment
                getResponse.Source.uid = achievment_uid;

                // Queries Metric to get it's code
                MetricController metricController = new MetricController(this._configuration);
                ApiResponse metricResponse = metricController.GetByUid(getResponse.Source.affected_metric_uid);

                // If status code is 200, maps affected_metric_code and returns AchievmentResponse with code 200
                if(metricResponse.StatusCode == 200)
                {
                    return new ApiResponse(new AchievmentResponse(getResponse.Source, ((Metric)metricResponse.Result).code), 200);
                }
                return new ApiResponse("Internal server error when trying to get Metric code", 500);

            }

            // Returns not found
            return new ApiResponse("Failed to find Achievment", 204);
        }
        #endregion

        #region Create new Achievment
        // POST api/<AchievmentController>
        [HttpPost]
        public ApiResponse Post([FromBody] Achievment achievment)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Achievment's document
            IndexResponse indexResponse = _client.IndexDocument(achievment);

            if (indexResponse.IsValid == true)
            {
                // If has created Achievment, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Achievment, returns 500
            return new ApiResponse("Internal server error when trying to create Achievment", null, 500);
        }
        #endregion

        #region Edit Achievment
        // PUT api/<AchievmentController>/23ba942aaca411e9b143023fad48cc44
        [HttpPut("{achievment_uid}")]
        public ApiResponse Put(string achievment_uid, [FromBody] Achievment achievment)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Achievment's document
            UpdateResponse<Achievment> response = _client.Update<Achievment, Achievment>(
                new DocumentPath<Achievment>(achievment_uid),
                u => u
                    .Doc(achievment)
            );

            if (response.IsValid == true)
            {
                // If has updated Achievment, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Achievment, returns 500
            return new ApiResponse("Internal server error when trying to update Achievment", null, 500);
        }
        #endregion

        #region Delete Achievment
        // DELETE api/<AchievmentController>/23ba942aaca411e9b143023fad48cc44
        [HttpDelete("{achievment_uid}")]
        public ApiResponse Delete(string achievment_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Experience Sets first
            ExperienceSetController experienceSetController = new ExperienceSetController(this._configuration);
            ApiResponse experienceSetResponse = experienceSetController.DeleteByAchievmentUid(achievment_uid);

            if (experienceSetResponse.StatusCode == 200)
            {
                // If has deleted Experience Sets, deletes Achievment's document
                DeleteResponse deleteResponse = _client.Delete<Achievment>(achievment_uid);

                if (deleteResponse.IsValid == true)
                {
                    // If has deleted Achievment, returns 200
                    return new ApiResponse("Deleted successfully", null, 200);
                }

                // If hasn't deleted Achievment, returns 500
                return new ApiResponse("Internal server error when trying to delete Achievment", null, 500);
            }

            // If hasn't deleted Experience Sets, returns 500
            return new ApiResponse("Internal server error when trying to delete Experience Sets", null, 500);
        }
        #endregion

        #region Constructor
        public AchievmentController(IConfiguration configuration) : base(configuration, "achievments")
        {
        }
        #endregion
    }
}
