﻿using System;
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
    public class MetricController : TemplateController
    {
        #region Get all Metrics with optional parameters
        // Gets all metrics
        // GET: api/<MetricController>
        [HttpGet]
        public ApiResponse GetAll([FromQuery] string metric_data_type = null, string metric_classification = null, string metric_model = null)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Metrics
            ISearchResponse<Metric> searchResponse = _client.Search<Metric>(s => s
                .From(0)
                .Size(this._defaultSize)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.data_type)
                        .Query(metric_data_type)
                    ) && q
                    .Match(m => m
                        .Field(f => f.classification)
                        .Query(metric_classification)
                    ) && q
                    .Match(m => m
                        .Field(f => f.model)
                        .Query(metric_model)
                    )
                )
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Metrics, returns 200
                    // Maps uid to the Metrics
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 Metrics, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Metrics", null, 500);
        }
        #endregion

        #region Get Metric by code
        // Gets Metric by code
        // GET api/<MetricController>/code/matematica_2020.2
        [HttpGet("/code/{code}")]
        public ApiResponse GetByCode(string code)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Metrics by code
            ISearchResponse<Metric> searchResponse = _client.Search<Metric>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.code)
                        .Query(code)
                    )
                )
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Metric, returns 200
                // Maps uid to the Metric
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).FirstOrDefault(), 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Metric by uid
        // Gets Metric by uid
        // GET api/<MetricController>/99117dd0354611e9b766641c67730998
        [HttpGet("{metric_uid}")]
        public ApiResponse GetByUid(string metric_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Metrics by uid
            IGetResponse<Metric> getResponse = _client.Get<Metric>(metric_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Metric, returns 200
                // Maps uid to the Metric
                getResponse.Source.uid = metric_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Metric
        // Creates new Metric
        // POST api/<MetricController>
        [HttpPost]
        public ApiResponse Create([FromBody] Metric metric)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Sets Metric's initial values
            metric.uid = null;
            metric.organization_uid = "fdefb6ee312d11e9a3ce641c67730998";
            metric.owner_user_uid = "5b48d49a8fd10a0901212430";
            metric.group_uid = "787dc20e354611e98af5641c67730998";

            // Sets Metric's CreateDate
            metric.create_date = DateTime.UtcNow;

            // Indexes Metric's document
            IndexResponse indexResponse = _client.IndexDocument(metric);

            if (indexResponse.IsValid == true)
            {
                // If has created Metric, creates MetricValues for users
                // Search all Users first
                UserController userController = new UserController(this._configuration);
                ApiResponse userApiResponse = userController.GetAll();

                if (userApiResponse.StatusCode == 200)
                {
                    // If has found Users, creates MetricValues for each User
                    List<UserResponse> userList = (List<UserResponse>)userApiResponse.Result;

                    List<MetricValues> metric_values_list = new List<MetricValues>();
                    foreach (UserResponse user in userList)
                    {
                        // Adds MetricValues to the Metrics Values list
                        metric_values_list.Add(new MetricValues(null, indexResponse.Id, user.uid, new List<string>(), DateTime.UtcNow));
                    }
                    MetricValuesController metricValuesController = new MetricValuesController(this._configuration);
                    // Bulk adds all Metrics Values
                    ApiResponse metricValuesResponse = metricValuesController.BulkCreate(metric_values_list);
                    if (metricValuesResponse.StatusCode == 201)
                    {
                        // If has created Metric and MetricValues, returns 201
                        return new ApiResponse(indexResponse.Id, 201);
                    }

                    // If hasn't created Metrics Values, returns 500
                    return new ApiResponse("Internal server error when trying to create Metrics Values", null, 500);
                }

                // If hasn't found User, returns 500
                return new ApiResponse("Internal server error when trying to get Users to create Metrics Values", null, 500);
            }

            // If hasn't created Metric, returns 500
            return new ApiResponse("Internal server error when trying to create Metric", null, 500);
        }
        #endregion

        #region Edit Metric
        // PUT api/<MetricController>/99117dd0354611e9b766641c67730998
        [HttpPut("{metric_uid}")]
        public ApiResponse Put(string metric_uid, [FromBody] Metric metric)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes non editable fields
            metric.uid = null;
            metric.organization_uid = null;
            metric.group_uid = null;
            metric.create_date = null;
            metric.owner_user_uid = null;

            // Updates Metric's document
            UpdateResponse<Metric> response = _client.Update<Metric, Metric>(
                new DocumentPath<Metric>(metric_uid),
                u => u
                    .Doc(metric)
            );

            if (response.IsValid == true)
            {
                // If has updated Metric, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Metric, returns 500
            return new ApiResponse("Internal server error when trying to update Metric", null, 500);
        }
        #endregion

        #region Edit Metric by code
        // Edits Metric by code
        // PUT api/<MetricController>/matematica_2020.2
        [HttpPut("code/{code}")]
        public ApiResponse PutByCode(string code, [FromBody] Metric metric)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Gets organization_uid
            string organization_uid = "fdefb6ee312d11e9a3ce641c67730998";
            
            // Gets owner_user_uid
            string owner_user_uid = "5b48d49a8fd10a0901212430";

            // Gets Metric's uid
            string metric_uid = GetMetricUidByCode(code, organization_uid, owner_user_uid);

            // Updates Metric's document
            UpdateResponse<Metric> updateResponse = _client.Update<Metric, Metric>(
                new DocumentPath<Metric>(metric_uid),
                u => u
                    .Doc(metric)
            );

            if (updateResponse.IsValid == true)
            {
                // If has updated Metric, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Metric, returns 500
            return new ApiResponse("Internal server error when trying to update Metric", null, 500);
        }
        #endregion

        #region Delete Metric by uid
        // Deletes Metric by uid
        // DELETE api/<MetricController>/VaywPnUBoU9gXwWefHpk
        [HttpDelete("{metric_uid}")]
        public ApiResponse DeleteByUid(string metric_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // First, deletes Experiences (and it's Experience Sets)
            // with same reference Metric uid as Metric's uid
            ExperienceController experienceController = new ExperienceController(this._configuration);
            ApiResponse experienceApiResponse = experienceController.DeleteByMetricUid(metric_uid);

            if (experienceApiResponse.StatusCode == 200)
            {
                // If has deleted Experiences and Experience Sets,
                // then deletes Experience Sets with same Metric uid
                ExperienceSetController experienceSetController = new ExperienceSetController(this._configuration);
                ApiResponse experienceSetApiResponse = experienceSetController.DeleteByMetricUid(metric_uid);

                if (experienceSetApiResponse.StatusCode == 200)
                {
                    // If has deleted Experiences Sets with same Metric uid,
                    // then deletes Achievments with same affected Metric uid
                    AchievmentController achievmentController = new AchievmentController(this._configuration);
                    ApiResponse achievmentApiResponse = achievmentController.DeleteByAffectedMetricUid(metric_uid);

                    if (achievmentApiResponse.StatusCode == 200)
                    {
                        // If has deleted all Achievments, then deletes Metrics Values
                        MetricValuesController metricValuesController = new MetricValuesController(this._configuration);
                        ApiResponse metricValuesResponse = metricValuesController.DeleteByMetricUid(metric_uid);

                        if (metricValuesResponse.StatusCode == 200)
                        {
                            // If has deleted all Metrics Values, then deletes Metric's document
                            DeleteResponse deleteResponse = _client.Delete<Metric>(metric_uid);

                            if (deleteResponse.IsValid == true)
                            {
                                // If has deleted Metric, deletes MetricValues by Metric uid
                                return new ApiResponse("Deleted successfully", null, 200);
                            }

                            // If hasn't deleted Metric, returns 500
                            return new ApiResponse("Internal server error when trying to delete Metric", null, 500);
                        }

                        // If hasn't deleted Metrics Values, returns 500
                        return new ApiResponse("Internal server error when trying to delete Metrics Values", null, 500);
                    }

                    // If hasn't deleted Achievments, returns 500
                    return new ApiResponse("Internal server error when trying to delete Achievments", null, 500);
                }

                // If hasn't deleted Experiences Sets, returns 500
                return new ApiResponse("Internal server error when trying to delete Experiences Sets", null, 500);
            }

            // If hasn't deleted Experiences, returns 500
            return new ApiResponse("Internal server error when trying to delete Experiences", null, 500);
        }
        #endregion

        #region Helpers
        private string GetMetricUidByCode(string code, string organization_uid, string owner_user_uid)
        {
            // Queries Metrics by code
            ISearchResponse<Metric> searchResponse = _client.Search<Metric>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.code)
                        .Query(code)
                    ) && q
                    .Match(m => m
                        .Field(f => f.organization_uid)
                        .Query(organization_uid)
                    ) && q
                    .Match(m => m
                        .Field(f => f.owner_user_uid)
                        .Query(owner_user_uid)
                    )
                )
            );

            return searchResponse.Hits.FirstOrDefault().Id;
        }
        
        private int DeleteAllMetricsValuesByMetricUid(string metric_uid)
        {
            // If has deleted all Metrics Values, returns 200
            return 200;

            // If hasn't deleted all Metrics Values, returns 500
            return 500;
        }

        private int DeleteAllExperiencesSetsByMetricUid(string metric_uid)
        {
            // If has deleted all Experiences Sets, returns 200
            return 200;

            // If hasn't deleted all Experiences Sets, returns 500
            return 500;
        }

        private int DeleteAllExperiencesByMetricUid(string metric_uid)
        {
            // If has deleted all Experiences, returns 200
            return 200;

            // If hasn't deleted all Experiences, returns 500
            return 500;
        }
        #endregion

        #region Constructor
        public MetricController(IConfiguration configuration) : base(configuration, "metrics")
        {
        }
        #endregion
    }
}
