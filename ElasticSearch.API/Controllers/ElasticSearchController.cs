using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticSearch.Bussiness;
using ElasticSearch.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace ElasticSearch.API.Controllers
{
    [Route("api/elasticSearch")]
    [ApiController]
    public class ElasticSearchController : Controller
    {
        public static IElasticSearchService _elasticSearchService;
        public static readonly ConnectionSettings connectionSettings =
            new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("register_log");
        private static readonly ElasticClient _elasticClient = new ElasticClient(connectionSettings);
        const string indexName = "register_log";

        public ElasticSearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpGet("search")]
        public async Task<List<LogDto>> Search(string keyword)
        {
            return await _elasticSearchService.Search(indexName, keyword);
        }

        [HttpPost("createRegister")]
        public void CreateRegister([FromBody] LogDto model)
        {
            if (!_elasticClient.IndexExists(indexName).Exists)
            {
                var indexSettings = new IndexSettings();
                indexSettings.NumberOfReplicas = 1;
                indexSettings.NumberOfShards = 3;

                var createIndexDescriptor = new CreateIndexDescriptor(indexName)
                    .Mappings(ms => ms
                              .Map<LogDto>(m => m
                                    .AutoMap()) )
                    .InitializeUsing(new IndexState() { Settings = indexSettings })
                    .Aliases(x => x.Alias(indexName));
            }

            _elasticClient.Index<LogDto>(model, idx => idx.Index(indexName));
        }
    }
}
