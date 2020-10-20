using LudocusApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class TemplateController : ControllerBase
    {
        #region Properties
        protected IConfiguration _configuration;

        protected ElasticClient _client;

        protected int _defaultSize;
        #endregion

        #region Constructor
        public TemplateController(IConfiguration configuration, string elastic_index)
        {
            // Sets configuration
            this._configuration = configuration;

            // Connects to ES
            this._client = new ElasticsearchService(this._configuration, elastic_index).Get();

            this._defaultSize = Int32.Parse(this._configuration.GetSection("ElasticsearchSettings").GetSection("defaultSize").Value);
        }
        #endregion
    }
}
