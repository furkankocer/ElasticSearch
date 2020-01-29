using ElasticSearch.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Bussiness
{
    public interface IElasticSearchService
    {
        Task<bool> CreateIndexAsync(string indexName);
        Task IndexAsync(string indexName, List<LogDto> users);
        Task<UserSuggestResponseDto> SuggestAsync(string indexName, string keyword);
    }
}
