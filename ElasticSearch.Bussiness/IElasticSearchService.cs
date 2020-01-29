using ElasticSearch.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Bussiness
{
    public interface IElasticSearchService
    {
        Task IndexAsync(string indexName, List<LogDto> users);
        Task<List<LogDto>> Search(string indexName, string keyword);
    }
}
