using ElasticSearch.Dto;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElasticSearch.Bussiness
{
    public interface IElasticSearchService
    {
        Task IndexAsync(string indexName, List<LogDto> users);
        Task<List<LogDto>> Search(string indexName, string keyword);
        Task<ExcelResponseDto<List<LogDto>>> Import(IFormFile formFile, CancellationToken cancellationToken);
    }
}
