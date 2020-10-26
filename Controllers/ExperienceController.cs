using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using LudocusApi.Helpers;
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
        public ApiResponse GetAll()
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
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Experiences, maps uid to the Experiences
                    List<Experience> experiences_list = searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList();

                    // Instantiates Metric Controller
                    MetricController metricController = new MetricController(this._configuration);
                    // Instantiates Experience Response list
                    List<ExperienceResponse> experiences_response_list = new List<ExperienceResponse>();

                    // Searchs Metric data for each Experience
                    foreach (Experience experience in experiences_list)
                    {
                        ApiResponse metricApiResponse = metricController.GetByUid(experience.reference_metric_uid);

                        if (metricApiResponse.StatusCode == 200)
                        {
                            // If has found Metric, adds to the response object
                            experiences_response_list.Add(new ExperienceResponse(experience, (Metric)metricApiResponse.Result));
                        } else
                        {
                            // If has happened and error, returns 500
                            return new ApiResponse("Internal server error when trying to get Metric data", null, 500);
                        }
                    }

                    // If has found all Experiences and Metrics data, returns 200
                    return new ApiResponse(experiences_response_list, 200);
                }

                // If has found 0 Experiences, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Experiences", null, 500);
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
            ApiResponse experienceApiResponse = this.GetByUid(experience_uid);

            if (experienceApiResponse.StatusCode == 200)
            {
                // Maps uid to the Experience
                Experience main_experience = (Experience)experienceApiResponse.Result;
                main_experience.uid = experience_uid;

               // Gets Experience's reference Metric's Data
                MetricController metricController = new MetricController(this._configuration);
                ApiResponse metricApiResponse = metricController.GetByUid(main_experience.reference_metric_uid);

                if (metricApiResponse.StatusCode == 200)
                {
                    // Maps uid to the Metric
                    Metric reference_metric = (Metric)metricApiResponse.Result;
                    reference_metric.uid = main_experience.reference_metric_uid;

                    // Gets All Achievments where affected Metric uid is the same as reference Metric's uid
                    AchievmentController achievmentController = new AchievmentController(this._configuration);
                    ApiResponse achievmentApiResponse = achievmentController.GetAll(reference_metric.uid);

                    if (achievmentApiResponse.StatusCode == 200)
                    {
                        // Maps achievments uid list
                        List<string> achievments_uid_list = ((List<Achievment>)achievmentApiResponse.Result).Select(h =>
                        {
                            return h.uid;
                        }).ToList();

                        // If has found Achievments, searchs for Experiences Sets 
                        // where Experience Set's achievment_uid is in Achievment's list searched before
                        ExperienceSetController experienceSetController = new ExperienceSetController(this._configuration);
                        ApiResponse experienceSetApiResponse = experienceSetController.GetByAchievmentsUidList(achievments_uid_list);

                        if (experienceSetApiResponse.StatusCode == 200)
                        {
                            // If has found verification Experiences Sets,
                            // creates verification Experiences Sets list
                            List<ExperienceSet> verification_experiences_sets_list = (List<ExperienceSet>)experienceSetApiResponse.Result;

                            // Then, searches for every User inside Experience's group
                            UserController userController = new UserController(this._configuration);
                            ApiResponse userApiResponse = userController.GetAll();

                            if (userApiResponse.StatusCode == 200)
                            {
                                // If has found all Users,
                                // creates Users list
                                List<UserResponse> users_list = (List<UserResponse>)userApiResponse.Result;

                                // Then, instantiates the Calculator helper with every data found
                                Calculator helper = new Calculator(
                                    main_experience,
                                    reference_metric,
                                    verification_experiences_sets_list,
                                    users_list,
                                    metricController,
                                    new MetricValuesController(this._configuration),
                                    new RuleController(this._configuration),
                                    achievmentController);

                                // Calculates reference Metric's Metric Values for each User,
                                // and returns Experience Panel
                                ExperiencePanel experiencePanel = helper.CalculateExperiencePanel();

                                // If has found all Experience Panel data, returns 200
                                return new ApiResponse(experiencePanel, 200);
                            }

                            // If hasn't found Users, returns 500
                            return new ApiResponse("Internal server error when trying to get Users data", null, 500);
                        }

                        // If hasn't found Experiences Sets, returns 500
                        return new ApiResponse("Internal server error when trying to get Experiences Sets data", null, 500);
                    }

                    // If hasn't found Achievments, returns 500
                    return new ApiResponse("Internal server error when trying to get reference Achievments data", null, 500);
                }

                // If hasn't found Metric, returns 500
                return new ApiResponse("Internal server error when trying to get reference Metric data", null, 500);
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

            // Sets Experience's initial values
            experience.uid = null;
            experience.organization_uid = "fdefb6ee312d11e9a3ce641c67730998";
            experience.owner_user_uid = "5b48d49a8fd10a0901212430";
            experience.create_date = DateTime.UtcNow;

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

            // Deletes non editable fields
            experience.uid = null;
            experience.organization_uid = null;
            experience.group_uid = null;
            experience.create_date = null;
            experience.owner_user_uid = null;

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
        public ApiResponse DeleteByUid(string experience_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Experience Sets first
            ExperienceSetController experienceSetController = new ExperienceSetController(this._configuration);
            ApiResponse experienceSetResponse = experienceSetController.DeleteByExperienceUid(experience_uid);

            if (experienceSetResponse.StatusCode == 200)
            {
                // If has deleted Experience Sets, deletes Experience's document
                DeleteResponse deleteResponse = _client.Delete<Experience>(experience_uid);

                if (deleteResponse.IsValid == true)
                {
                    // If has deleted Experience, returns 200
                    return new ApiResponse("Deleted successfully", null, 200);
                }

                // If hasn't deleted Experience, returns 500
                return new ApiResponse("Internal server error when trying to delete Experience", null, 500);
            }

            // If hasn't deleted Experience Sets, returns 500
            return new ApiResponse("Internal server error when trying to delete Experience Sets", null, 500);
        }
        #endregion

        #region Delete Experiences by reference Metric uid
        // DELETE api/<ExperienceController>/metric/33ba942aaca411e9b143023fad48cc33
        [HttpDelete("metric/{metric_uid}")]
        public ApiResponse DeleteByMetricUid(string metric_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Experiences with reference Metric uid
            ISearchResponse<Experience> searchResponse = _client.Search<Experience>(s => s
                .From(0)
                .Size(this._defaultSize)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.reference_metric_uid)
                        .Query(metric_uid)
                    )
                )
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Experiences
                // Maps Experiences uids
                List<Experience> experiences_list = searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList();

                foreach (Experience experience in experiences_list)
                {
                    // Deletes Experience by uid (and it's Experience Sets)
                    ApiResponse apiResponse = this.DeleteByUid(experience.uid);

                    if (apiResponse.StatusCode == 500)
                    {
                        // If hasn't deleted Experience, returns 500
                        return new ApiResponse("Internal server error when trying to delete Experience", null, 500);
                    }
                }
                
                // If has deleted Experiences, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Experiences, returns 500
            return new ApiResponse("Internal server error when trying to delete Experiences", null, 500);
        }
        #endregion

        #region Constructor
        public ExperienceController(IConfiguration configuration) : base(configuration, "experiences")
        {
        }
        #endregion
    }
}
