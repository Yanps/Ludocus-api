using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class OrganizationController : ControllerBase
    {
        IConfiguration configuration;

        // GET: api/<OrganizationController>
        [HttpGet]
        public IReadOnlyCollection<Organization> Get()
        {
            List<Organization> organizationList = new List<Organization>();

            // Connects to ES
            ElasticClient client = new ElasticsearchService(this.configuration).Get();

            // Queries Organizations
            ISearchResponse<Organization> searchResponse = client.Search<Organization>(s => s
                .From(0)
                .Size(100)
            );

            IReadOnlyCollection<Organization> organization = searchResponse.Documents;

            return organization;
        }

        // GET api/<OrganizationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<OrganizationController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<OrganizationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrganizationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public OrganizationController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
