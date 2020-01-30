using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ElasticSearch.Dto;
using Microsoft.AspNetCore.Http;
using Nest;
using OfficeOpenXml;

namespace ElasticSearch.Bussiness
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly ElasticClient _elasticClient;

        public ElasticSearchService(ConnectionSettings connectionSettings)
        {
            _elasticClient = new ElasticClient(connectionSettings);
        }
        public void CreateIndex(string indexName)
        {
            if (!_elasticClient.IndexExists(indexName).Exists)
            {
                var indexSettings = new IndexSettings();
                indexSettings.NumberOfReplicas = 1;
                indexSettings.NumberOfShards = 3;

                var createIndexDescriptor = new CreateIndexDescriptor(indexName)
                    .Mappings(ms => ms
                              .Map<LogDto>(m => m
                                    .AutoMap()))
                    .InitializeUsing(new IndexState() { Settings = indexSettings })
                    .Aliases(x => x.Alias(indexName));

                ICreateIndexResponse createIndexResponse = _elasticClient.CreateIndex(createIndexDescriptor);
            }
        }

        public async Task<ExcelResponseDto<List<LogDto>>> Import(IFormFile formFile, CancellationToken cancellationToken, string indexName)
        {
            if (formFile == null || formFile.Length <= 0)
            {
                return ExcelResponseDto<List<LogDto>>.GetResult(-1, "formfile is empty");
            }

            if (!Path.GetExtension(formFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ExcelResponseDto<List<LogDto>>.GetResult(-1, "Not Support file extension");
            }

            var list = new List<LogDto>();

            using (var stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream, cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        list.Add(new LogDto
                        {
                            Name = worksheet.Cells[row, 1].Value.ToString().Trim(),
                            Surname = worksheet.Cells[row, 2].Value.ToString().Trim(),
                            MobilNo = worksheet.Cells[row, 3].Value.ToString().Trim(),
                            BirthDate = worksheet.Cells[row, 4].Value.ToString().Trim(),
                            LastLocation = worksheet.Cells[row, 5].Value.ToString().Trim(),
                        });
                    }
                }
            }
            CreateIndex(indexName);
            IndexAsync(indexName, list).Wait();

            return ExcelResponseDto<List<LogDto>>.GetResult(0, "OK", list);
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
