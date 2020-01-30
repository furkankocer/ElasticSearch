using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Dto
{
    public class ExcelResponseDto<T>
    {
        public int Code { get; set; }

        public string Msg { get; set; }

        public T Data { get; set; }

        public static ExcelResponseDto<T> GetResult(int code, string msg, T data = default(T))
        {
            return new ExcelResponseDto<T>
            {
                Code = code,
                Msg = msg,
                Data = data
            };
        }
    }
}
