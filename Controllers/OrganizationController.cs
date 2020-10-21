using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LudocusApi.Models;
using LudocusApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using AutoWrapper.Wrappers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LudocusApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : TemplateController
    {
        #region Get all Organizations
        // Gets all Organizations
        // GET: api/<OrganizationController>
        [HttpGet]
        public ApiResponse Get()
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Organizations
            ISearchResponse<Organization> searchResponse = _client.Search<Organization>(s => s
                .From(0)
                .Size(this._defaultSize)
            );

            if (searchResponse.IsValid == true)
            {
                if (searchResponse.Hits.Count > 0)
                {
                    // If has found Organizations, returns 200
                    // Maps uid to the Organizations
                    return new ApiResponse(searchResponse.Hits.Select(h =>
                    {
                        h.Source.uid = h.Id;
                        return h.Source;
                    }).ToList(), 200);
                }

                // If has found 0 Organizations, returns 204
                return new ApiResponse(null, 204);
            }

            // If has happened and error, returns 500
            return new ApiResponse("Internal server error when trying to get Organizations", null, 500);
        }
        #endregion

        #region Get Organization by uid
        // Gets Organization by uid
        // GET api/<OrganizationController>/5
        [HttpGet("{organization_uid}")]
        public ApiResponse Get(string organization_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Queries Organizations by uid
            IGetResponse<Organization> getResponse = _client.Get<Organization>(organization_uid);

            if(getResponse.IsValid == true)
            {
                // If has found Organization, returns 200
                // Maps uid to the Organization
                getResponse.Source.uid = organization_uid;
                return new ApiResponse(getResponse.Source, 200);
            }

            // Returns not found
            return new ApiResponse(null, 204);
        }
        #endregion

        #region Create new Organization
        // Creates new Organization
        // POST api/<OrganizationController>
        [HttpPost]
        public ApiResponse Post([FromBody] Organization organization)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Indexes Organization's document
            IndexResponse indexResponse = _client.IndexDocument(organization);

            if(indexResponse.IsValid == true)
            {
                // If has created Organization, returns 201
                return new ApiResponse(indexResponse.Id, 201);
            }

            // If hasn't created Organization, returns 500
            return new ApiResponse("Internal server error when trying to create Organization", null, 500);
        }
        #endregion

        #region Edit Organization
        // Edits Organization
        // PUT api/<OrganizationController>/5
        [HttpPut("{organization_uid}")]
        public ApiResponse Put(string organization_uid, [FromBody] Organization organization)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Updates Organization's document
            UpdateResponse<Organization> response = _client.Update<Organization, Organization>(
                new DocumentPath<Organization>(organization_uid),
                u => u
                    .Doc(organization)
            );

            if (response.IsValid == true)
            {
                // If has updated Organization, returns 200
                return new ApiResponse("Updated successfully", null, 200);
            }

            // If hasn't updated Organization, returns 500
            return new ApiResponse("Internal server error when trying to update Organization", null, 500);
        }
        #endregion

        #region Delete Organization
        // DELETE api/<OrganizationController>/5
        [HttpDelete("{organization_uid}")]
        public ApiResponse Delete(string organization_uid)
        {
            // Verifies if user has authorization
            // TODO
            // return new ApiResponse(null, 401);

            // Deletes Organization's document
            DeleteResponse response = _client.Delete<Organization>(organization_uid);

            if (response.IsValid == true)
            {
                // If has deleted Organization, returns 200
                return new ApiResponse("Deleted successfully", null, 200);
            }

            // If hasn't deleted Organization, returns 500
            return new ApiResponse("Internal server error when trying to delete Organization", null, 500);
        }
        #endregion

        #region Constructor
        public OrganizationController(IConfiguration configuration) : base(configuration, "organizations")
        {
        }
        #endregion
    }
}
