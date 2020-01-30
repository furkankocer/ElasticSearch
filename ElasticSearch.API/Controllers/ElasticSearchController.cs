using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElasticSearch.Bussiness;
using ElasticSearch.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using OfficeOpenXml;

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
        const string index = "register_logs";

        public ElasticSearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpGet("search")]
        public async Task<List<LogDto>> Search(string keyword, int page, int pageSize)
        {
            return await _elasticSearchService.Search(index, keyword, page, pageSize);
        }

        [HttpPost("createRegister")]
        public void CreateRegister([FromBody] LogDto model)
        {
            if (!_elasticClient.IndexExists(index).Exists)
            {
                var indexSettings = new IndexSettings();
                indexSettings.NumberOfReplicas = 1;
                indexSettings.NumberOfShards = 3;

                var createIndexDescriptor = new CreateIndexDescriptor(index)
                    .Mappings(ms => ms
                              .Map<LogDto>(m => m
                                    .AutoMap()))
                    .InitializeUsing(new IndexState() { Settings = indexSettings })
                    .Aliases(x => x.Alias(index));
            }
            model.DateTime = DateTime.Now;
            _elasticClient.Index<LogDto>(model, idx => idx.Index(index));
        }

        [HttpPost("uploadCreateIndex")]
        public async Task<ExcelResponseDto<List<LogDto>>> Import(IFormFile formFile, CancellationToken cancellationToken)
        {
            string indexName = index;
            return await _elasticSearchService.Import(formFile, cancellationToken, indexName);
        }

        [HttpGet("getListAllUsers")]
        public async Task<List<LogDto>> GetAllErrors(int page, int pageSize)
        {
            var response = await _elasticClient.SearchAsync<LogDto>(p => p
                                 .Index(index)
                                  .Query(q => q
                                   .MatchAll()

                                        ).From((page - 1) * pageSize)
                                        .Size(pageSize)
                                        );

            var result = new List<LogDto>();
            foreach (var document in response.Documents)
            {
                result.Add(document);
            }

            return result.Distinct().ToList();
        }
        [HttpPost("deleteIndex")]
        public void DeleteIndex(string name)
        {
            _elasticClient.DeleteIndex(name);
        }

    }
}
