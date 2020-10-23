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
    public class MetricValuesController : TemplateController
    {
        #region Get all Metrics Values
        // GET: api/<MetricValuesController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries MetricsValues
            ISearchResponse<MetricValues> searchResponse = _client.Search<MetricValues>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found MetricsValues, returns 200
                    // Maps uid to the MetricsValues
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 MetricsValues, returns 204
                return new ApiResponse(null, 204);
            }
                        
            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Metric Values", null, 500);
        }
        #endregion

        #region Get Metric Values by uid
        // GET api/<MetricValuesController>/rrGcMXUBqdy07-Lf1681
        [HttpGet("{metric_values_uid}")]
        public ApiResponse GetByUid(string metric_values_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Metrics Values by uid
            IGetResponse<MetricValues> getResponse = _client.Get<MetricValues>(metric_values_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Metric Values, returns 200
                // Maps uid to the Metric Values
                getResponse.Source.uid = metric_values_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Metrics Values by Metric uid
        // GET api/<MetricValuesController>/metric/99117dd0354611e9b766641c67730998
        [HttpGet("metric/{metric_uid}")]
        public ApiResponse GetByMetricUid(string metric_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Metrics Values by Metric uid
            ISearchResponse<MetricValues> searchResponse = _client.Search<MetricValues>(s => s
                .From(0)
                .Size(this._defaultSize)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.metric_uid)
                        .Query(metric_uid)
                    )
                )
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Metric Values, maps uid to the MetricValue
                List<MetricValues> metricValuesList = searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList();

                // Instantiates the User Controller
                UserController userController = new UserController(this._configuration);

                // Instantiates the Metric Values Response list
                List<MetricValuesResponse> metricValuesResponseList = new List<MetricValuesResponse>();

                foreach (MetricValues metricValues in metricValuesList)
                {
                    // Gets User data
                    ApiResponse userResponse = userController.GetByUid(metricValues.user_uid);
                    if (userResponse.StatusCode == 200)
                    {
                        // If has found User, maps to the MetricValuesResponse
                        User user = (User)userResponse.Result;

                        MetricValuesResponse metricValuesResponse = new MetricValuesResponse(metricValues, user.code, user.name + " " + user.surname);

                        metricValuesResponseList.Add(metricValuesResponse);
                    } else
                    {
                        // If hasn't found User data, returns 500
                        return new ApiResponse("Internal server error when trying to get User data", null, 500);
                    }
                }

                // If has found all MetricsValues data, returns 200
                return new ApiResponse(metricValuesResponseList, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Metric Values
        // POST api/<MetricValuesController>
        [HttpPost]
        public ApiResponse Post([FromBody] MetricValues metricValues)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Metric Values' document
            IndexResponse indexResponse = _client.IndexDocument(metricValues);

            if (indexResponse.IsValid == true)
            {
                // If has created Metric Values, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to create Metric Values", null, 500);
        }
        #endregion

        #region Create new Metrics Values by Bulk
        // POST api/<MetricValuesController>
        [HttpPost]
        public ApiResponse BulkCreate([FromBody] List<MetricValues> metric_values_list)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Instantiates Metrics Values's uid list
            List<string> metricValuesUidList = new List<string>();

            // Indexes Metrics Values' documents
            foreach(MetricValues metricValues in metric_values_list)
            {
                IndexResponse indexResponse = _client.IndexDocument(metricValues);

                if (indexResponse.IsValid != true)
                {
                    // If hasn't created any Metric Values, returns 500
                    return new ApiResponse("Internal server error when trying to create Metric Values", null, 500);
                }

                metricValuesUidList.Add(indexResponse.Id);
            }

            // If has created Metrics Values, returns 201
            return new ApiResponse(metricValuesUidList, 201);
        }
        #endregion

        #region Edit Metric Values
        // PUT api/<MetricValuesController>/rrGcMXUBqdy07-Lf1681
        [HttpPut("{metricValues_uid}")]
        public ApiResponse Edit(string metricValues_uid, [FromBody] MetricValues metricValues)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates update_date value's 
            metricValues.update_date = DateTime.UtcNow;

            // Updates Metric Values' document
            UpdateResponse<MetricValues> updateResponse = _client.Update<MetricValues, MetricValues>(
                new DocumentPath<MetricValues>(metricValues_uid),
                u => u
                    .Doc(metricValues)
            );

            if (updateResponse.IsValid == true)
            {
                // If has updated Metric Values, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to update Metric Values", null, 500);
        }
        #endregion

        #region Edit Metrics Values by Bulk
        // PUT api/<MetricValuesController>
        [HttpPut]
        public ApiResponse BulkEdit([FromBody] List<MetricValues> metric_values_list)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Metrics Values' documents
            foreach (MetricValues metricValues in metric_values_list)
            {
                // Gets metric values's uid and sets it to null on the object
                string metricValues_uid = metricValues.uid;

                metricValues.uid = null;

                // Updates update_date value's 
                metricValues.update_date = DateTime.UtcNow;

                // Updates Metric Values' document
                UpdateResponse<MetricValues> updateResponse = _client.Update<MetricValues, MetricValues>(
                    new DocumentPath<MetricValues>(metricValues_uid),
                    u => u
                        .Doc(metricValues)
                );

                if (updateResponse.IsValid != true)
                {
                    // If hasn't updated any Metric Values, returns 500
                    return new ApiResponse("Internal server error when trying to update Metric Values", null, 500);
                }
            }

            // If has updated Metrics Values, returns 201
            return new ApiResponse("Updated successfully", 200);
        }
        #endregion

        #region Delete Metric Values
        // DELETE api/<MetricValuesController>/4LEVMHUBqdy07-Lfda6U
        [HttpDelete("{metricValues_uid}")]
        public ApiResponse Delete(string metricValues_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Metric Values' document
            DeleteResponse response = _client.Delete<MetricValues>(metricValues_uid);

            if (response.IsValid == true)
            {
                // If has deleted Metric Values, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to delete Metric Values", null, 500);
        }
        #endregion

        #region Delete Metrics Values by Bulk
        // DELETE api/<MetricValuesController>
        [HttpDelete]
        public ApiResponse DeleteByBulk([FromBody] List<string> metrics_values_uid_list)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            foreach (string metric_values_uid in metrics_values_uid_list)
            {
                // Deletes Metric Values' document
                DeleteResponse response = _client.Delete<MetricValues>(metric_values_uid);

                if (response.IsValid != true)
                {
                    // If hasn't deleted Metric Values, returns 500
                    return new ApiResponse("Internal server error when trying to delete Metric Values", null, 500);
                }
            }

            // If has deleted all Metrics Values, returns 200
            return new ApiResponse("Deleted successfully", null, 200);
        }
        #endregion

        #region Delete Metrics Values by User uid
        // DELETE api/<MetricValuesController>/user/fd8546c4312d11e985c4641c67730998
        [HttpDelete("user/{user_uid}")]
        public ApiResponse DeleteByUserUid(string user_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            var deleteResponse = _client.DeleteByQuery<MetricValues>(q => q
                .Query(rq => rq
                    .Match(m => m
                        .Field(f => f.user_uid)
                        .Query(user_uid)
                    )
                )
            );

            if (deleteResponse.IsValid == true)
            {
                // If has deleted all Metrics Values, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to delete Metric Values", null, 500);
        }
        #endregion

        #region Delete Metrics Values by Metric uid
        // DELETE api/<MetricValuesController>/user/fd8546c4312d11e985c4641c67730998
        [HttpDelete("metric/{metric_uid}")]
        public ApiResponse DeleteByMetricUid(string metric_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Metrics Values by Metric uid
            DeleteByQueryResponse deleteResponse = _client.DeleteByQuery<MetricValues>(q => q
                .Query(rq => rq
                    .Match(m => m
                        .Field(f => f.metric_uid)
                        .Query(metric_uid)
                    )
                )
            );

            if (deleteResponse.IsValid == true)
            {
                // If has deleted all Metrics Values, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to delete Metric Values", null, 500);
        }
        #endregion

        #region Constructor
        public MetricValuesController(IConfiguration configuration) : base(configuration, "metrics_values")
        {
        }
        #endregion
    }
}
