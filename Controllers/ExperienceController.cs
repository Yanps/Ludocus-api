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
    public class ExperienceController : TemplateController
    {
        #region Get all Experiences
        // GET: api/<ExperienceController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // If User has admin role, queries all experiences within Organization
            // If User has user role, queries all experiences within User's Groups

            // Queries Experiences
            ISearchResponse<Experience> searchResponse = _client.Search<Experience>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Experiences, returns 200
                // Maps uid to the Experiences
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList(), 200);
            }

            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Experience by uid
        // GET api/<ExperienceController>/33ba942aaca411e9b143023fad48cc33
        [HttpGet("{experience_uid}")]
        public ApiResponse GetByUid(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experiences by uid
            IGetResponse<Experience> getResponse = _client.Get<Experience>(experience_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Experience, returns 200
                // Maps uid to the Experience
                getResponse.Source.uid = experience_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Experience's Panel by uid
        // GET api/<ExperienceController>/33ba942aaca411e9b143023fad48cc33/panel
        [HttpGet("{experience_uid}/panel")]
        public ApiResponse GetPanelByUid(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experience by uid
            ApiResponse experienceResponse = this.GetByUid(experience_uid);

            // Gets Experience's reference Metric Values

            if (experienceResponse.StatusCode == 200)
            {
                // Maps uid to the Experience
                Experience experience = (Experience)experienceResponse.Result;
                experience.uid = experience_uid;

                // If has found Experience, creates Experience Panel object
                ExperiencePanel experiencePanel = new ExperiencePanel();
                experiencePanel.experience = experience;

                // If is Level type Experience, gets Metric Values for user

                // If is Rank type Experience, gets Metric Values, User and Metric data
                // Gets Experience's reference Metric Values by reference Metric uid
                MetricValuesController metricValuesController = new MetricValuesController(this._configuration);
                ApiResponse metricValuesResponse = metricValuesController.GetByMetricUid(experiencePanel.experience.reference_metric_uid);

                //{ , LudocusApi.Models.MetricValues>}

                //var a = (System.Linq.Enumerable.SelectArrayIterator<Nest.IHit<LudocusApi.Models.MetricValues>)metricValuesResponse.Result;

                //var b = a.ToList();

                List<MetricValues> metricsValuesList = ((List<MetricValues>)metricValuesResponse.Result);

                foreach (MetricValues metricValues in metricsValuesList)
                {
                    // Gets User data by User uid
                    UserController userController = new UserController(this._configuration);
                    ApiResponse userResponse = userController.GetByUid(metricValues.user_uid);

                    if (userResponse.StatusCode == 200)
                    {
                        // Gets Metric data by Metric uid
                        MetricController metricController = new MetricController(this._configuration);
                        ApiResponse metricResponse = metricController.GetByUid(metricValues.metric_uid);

                        if (metricResponse.StatusCode == 200)
                        {
                            // Adds Panel Set to the Experience Panel
                            experiencePanel.panel_sets.Add(new PanelSet(metricValues, (User)userResponse.Result, (Metric)metricResponse.Result));
                        }
                        else
                        {
                            // If hasn't found Metric, returns 500
                            return new ApiResponse("Internal server error when trying to get Metric", null, 500);
                        }
                    }
                    else
                    {
                        // If hasn't found User, returns 500
                        return new ApiResponse("Internal server error when trying to get User", null, 500);
                    }
                }

                // If has found all Experience, Metrics Values, Users and Metrics data, returns 200
                return new ApiResponse(experiencePanel, 200);
            }

            // Returns not found
            return new ApiResponse("Experience not found", 204);
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
        public ExperienceController(IConfiguration configuration) : base(configuration, "experiences")
        {
        }
        #endregion
    }
}
