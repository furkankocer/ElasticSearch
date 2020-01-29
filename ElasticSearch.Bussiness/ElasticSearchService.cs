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
        readonly ElasticClient _elasticClient;

        public ElasticSearchService(ConnectionSettings connectionSettings)
        {
            _elasticClient = new ElasticClient(connectionSettings);
        }

        public async Task<bool> CreateIndexAsync(string indexName)
        {
            var createIndexDescriptor = new CreateIndexDescriptor(indexName)
                .Mappings(ms => ms
                          .Map<LogDto>(m => m
                                .AutoMap()
                                .Properties(ps => ps
                                    .Completion(c => c
                                        .Name(p => p.Suggest))))

                         );

            if (_elasticClient.IndexExists(indexName.ToLowerInvariant()).Exists)
            {
                _elasticClient.DeleteIndex(indexName.ToLowerInvariant());
            }

            ICreateIndexResponse createIndexResponse = await _elasticClient.CreateIndexAsync(createIndexDescriptor);

            return createIndexResponse.IsValid;
        }

        public async Task IndexAsync(string indexName, List<LogDto> users)
        {
            await _elasticClient.IndexManyAsync(users, indexName);
        }

        public async Task<UserSuggestResponseDto> SuggestAsync(string indexName, string keyword)
        {
            ISearchResponse<LogDto> searchResponse = await _elasticClient.SearchAsync<LogDto>(s => s
                                     .Index(indexName)
                                     .Suggest(su => su
                                          .Completion("suggestions", c => c
                                               .Field(f => f.Suggest)
                                               .Prefix(keyword)
                                               .Fuzzy(f => f
                                                   .Fuzziness(Fuzziness.Auto)
                                               )
                                               .Size(5))
                                             ));

            var suggests = from suggest in searchResponse.Suggest["suggestions"]
                           from option in suggest.Options
                           select new UserSuggestDto
                           {
                               Name = option.Text,

                           };

            return new UserSuggestResponseDto
            {
                UserSuggests = suggests
            };
        }
    }
}
