using ElasticSearch.Bussiness;
using ElasticSearch.Dto;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.MockData
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<LogDto> users = new List<LogDto>();

            users.Add(new LogDto()
            {
                Name ="Kaley",
                Surname = "Willms",
                MobilNo = "1-666-117-6420",
                BirthDate = "10/7/1996",
                LastLocation = "47.0266, -123.4382",
                DateTime = DateTime.Now,
                Suggest = new CompletionField()
                {
                    Input = new[] { "Kaley", "10", "47" }
                }
            });

            users.Add(new LogDto()
            {
                Name = "Beahan",
                Surname = "Tod",
                MobilNo = "1-666-117-6420",
                BirthDate = "10/7/1996",
                LastLocation = "47.0266, -123.4382",
                DateTime = DateTime.Now,
                Suggest = new CompletionField()
                {
                    Input = new[] { "Beahan", "10", "47" }
                }
            });

            users.Add(new LogDto()
            {
                Name = "deneme",
                Surname = "test",
                MobilNo = "1-666-117-6420",
                BirthDate = "10/7/1996",
                LastLocation = "47.0266, -123.4382",
                DateTime = DateTime.Now,
                Suggest = new CompletionField()
                {
                    Input = new[] { "deneme", "10", "47" }
                }
            });

            var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"));
            IElasticSearchService autocompleteService = new ElasticSearchService(connectionSettings);
            string userSuggestIndex = "user_suggest";

            bool isCreated = autocompleteService.CreateIndexAsync(userSuggestIndex).Result;

            if (isCreated)
            {
                autocompleteService.IndexAsync(userSuggestIndex, users).Wait();
            }
        }
    }
}
