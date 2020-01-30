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
            model.DateTime = DateTime.Now;
            _elasticClient.Index<LogDto>(model, idx => idx.Index(indexName));
        }

        [HttpPost("uploadExcel")]
        public async Task<ExcelResponseDto<List<LogDto>>> Import(IFormFile formFile, CancellationToken cancellationToken)
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

            return ExcelResponseDto<List<LogDto>>.GetResult(0, "OK", list);
        }
    }
}
