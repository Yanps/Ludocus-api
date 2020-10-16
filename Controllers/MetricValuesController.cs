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
    public class MetricValuesController : ControllerBase
    {
        #region Properties
        IConfiguration _configuration;

        ElasticClient _client;

        int _defaultSize;
        #endregion

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
                // If has found MetricsValues, returns 200
                // Maps uid to the MetricsValues
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }), 200);
            }

            // If hasn't found MetricsValues, returns 204
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Metric Values by uid
        // GET api/<MetricValuesController>/5
        [HttpGet("{metric_values_uid}")]
        public ApiResponse Get(string metric_values_uid)
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
        public ApiResponse PostBulk([FromBody] List<MetricValues> metricValuesList)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Instantiates Metrics Values's uid list
            List<string> metricValuesUidList = new List<string>();

            // Indexes Metrics Values' documents
            foreach(MetricValues metricValues in metricValuesList)
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
        // PUT api/<MetricValuesController>/5
        [HttpPut("{metricValues_uid}")]
        public ApiResponse Put(string metricValues_uid, [FromBody] MetricValues metricValues)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

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
        [HttpPost]
        public ApiResponse PutBulk([FromBody] List<MetricValues> metricValuesList)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Metrics Values' documents
            foreach (MetricValues metricValues in metricValuesList)
            {
                // Gets metric values's uid and sets it to null on the object
                string metricValues_uid = metricValues.uid;

                metricValues.uid = null;

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
        // DELETE api/<MetricValuesController>/5
        [HttpDelete("{metricValues_uid}")]
        public ApiResponse Delete(string metricValues_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Metric Values' document
            DeleteResponse response = _client.Delete<Organization>(metricValues_uid);

            if (response.IsValid == true)
            {
                // If has deleted Metric Values, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Metric Values, returns 500
            return new ApiResponse("Internal server error when trying to delete Metric Values", null, 500);
        }
        #endregion

        #region Constructor
        public MetricValuesController(IConfiguration configuration)
        {
            // Sets configuration
            this._configuration = configuration;

            // Connects to ES
            this._client = new ElasticsearchService(this._configuration, "metrics_values").Get();

            this._defaultSize = Int32.Parse(this._configuration.GetSection("ElasticsearchSettings").GetSection("defaultSize").Value);
        }
        #endregion
    }
}
