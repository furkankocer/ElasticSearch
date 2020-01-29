using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElasticSearch.Dto;
using Nest;

namespace ElasticSearch.Bussiness
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticClient _elasticClient;

        public ElasticSearchService(ConnectionSettings connectionSettings)
        {
            _elasticClient = new ElasticClient(connectionSettings);
        }

        public async Task IndexAsync(string indexName, List<LogDto> users)
        {
            await _elasticClient.IndexManyAsync(users, indexName);
        }

        public async Task<List<LogDto>> Search(string indexName, string keyword)
        {
            var searchResponse = await _elasticClient.SearchAsync<LogDto>(s => s
                                     .Index(indexName)
                                        .Query(q => q
                                        .Prefix(p => p.Name, keyword) && +q
                                        .Range(r => r
                                        .Field(f => f.Name))));

            var result = new List<LogDto>();

            foreach (var item in searchResponse.Documents)
            {
                result.Add(item);
            }

            return result.ToList();
        }
    }
}
