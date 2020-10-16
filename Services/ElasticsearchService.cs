using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace LudocusApi.Services
{
    public class ElasticsearchService
    {
        public ElasticClient _client;

        private IConfiguration configuration;

        private Uri elasticsearchUri;

        private string defaultIndex;

        private string username;

        private string password;

        public ElasticClient Get()
        {
            return _client;
        }

        public ElasticsearchService(IConfiguration configuration, string defaultIndex = null)
        {
            this.configuration = configuration;

            _client = new ElasticClient();

            this.elasticsearchUri = new Uri(this.configuration.GetSection("ElasticsearchSettings").GetSection("uri").Value);

            if(defaultIndex != null)
            {
                this.defaultIndex = defaultIndex;
            } else
            {
                this.defaultIndex = this.configuration.GetSection("ElasticsearchSettings").GetSection("defaultIndex").Value;
            }

            this.username = this.configuration.GetSection("ElasticsearchSettings").GetSection("username").Value;

            this.password= this.configuration.GetSection("ElasticsearchSettings").GetSection("password").Value;

            //var x = ConfigurationManager.AppSettings;

            var settings = new ConnectionSettings(this.elasticsearchUri)
                .DefaultIndex(this.defaultIndex)
                .BasicAuthentication(this.username, this.password);

            _client = new ElasticClient(settings);
        }
    }
}
