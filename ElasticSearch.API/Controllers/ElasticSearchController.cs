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
        public readonly IElasticSearchService _elasticSearchService;
        const string USER_SUGGEST_INDEX = "user_suggest";
        public ElasticSearchController(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpGet("search")]
        public async Task<UserSuggestResponseDto> Get(string keyword)
        {
            return await _elasticSearchService.SuggestAsync(USER_SUGGEST_INDEX, keyword);
        }

        [HttpPost("insertLog")]
        public void InsertLog([FromBody] LogListDto model)
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"));
            IElasticSearchService autocompleteService = new ElasticSearchService(connectionSettings);
            string userSuggestIndex = "user_suggest";

            bool isCreated = autocompleteService.CreateIndexAsync(userSuggestIndex).Result;

            if (isCreated)
            {
                autocompleteService.IndexAsync(userSuggestIndex, model.Logs).Wait();
            }
        }
    }
}
