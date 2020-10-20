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
    public class RuleController : TemplateController
    {
        #region Get all Rules
        // Gets all Rules
        // GET: api/<RuleController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Rules
            ISearchResponse<Rule> searchResponse = _client.Search<Rule>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                // If has found Rules, returns 200
                // Maps uid to the Rules
                return new ApiResponse(searchResponse.Hits.Select(h =>
                {
                    h.Source.uid = h.Id;
                    return h.Source;
                }).ToList(), 200);
            }

            return new ApiResponse(null, 204);
        }
        #endregion

        #region Get Rule by uid
        // GET api/<RuleController>/81ba942aaca411e9b143023fad48cc44
        [HttpGet("{rule_uid}")]
        public ApiResponse Get(string rule_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Rules by uid
            IGetResponse<Rule> getResponse = _client.Get<Rule>(rule_uid);

            if (getResponse.IsValid == true)
            {
                // If has found Rule, returns 200
                // Maps uid to the Rule
                getResponse.Source.uid = rule_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Rule
        // POST api/<RuleController>
        [HttpPost]
        public ApiResponse Post([FromBody] Rule rule)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Rule's document
            IndexResponse indexResponse = _client.IndexDocument(rule);

            if (indexResponse.IsValid == true)
            {
                // If has created Rule, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Rule, returns 500
            return new ApiResponse("Internal server error when trying to create Rule", null, 500);
        }
        #endregion

        #region Edit Rule
        // PUT api/<RuleController>/81ba942aaca411e9b143023fad48cc44
        [HttpPut("{rule_uid}")]
        public ApiResponse Put(string rule_uid, [FromBody] Rule rule)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Rule's document
            UpdateResponse<Rule> response = _client.Update<Rule, Rule>(
                new DocumentPath<Rule>(rule_uid),
                u => u
                    .Doc(rule)
            );

            if (response.IsValid == true)
            {
                // If has updated Rule, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Rule, returns 500
            return new ApiResponse("Internal server error when trying to update Rule", null, 500);
        }
        #endregion

        #region Delete Rule by uid
        // DELETE api/<RuleController>/81ba942aaca411e9b143023fad48cc44
        [HttpDelete("{rule_uid}")]
        public ApiResponse Delete(string rule_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Rule's document
            DeleteResponse response = _client.Delete<Rule>(rule_uid);

            if (response.IsValid == true)
            {
                // If has deleted Rule, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Rule, returns 500
            return new ApiResponse("Internal server error when trying to delete Rule", null, 500);
        }
        #endregion

        #region Constructor
        public RuleController(IConfiguration configuration) : base(configuration, "rules")
        {
        }
        #endregion
    }
}
