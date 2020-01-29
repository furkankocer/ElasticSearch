using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Dto
{
    public class LogDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string MobilNo { get; set; }
        public string LastLocation { get; set; }
        public string BirthDate { get; set; }
        public DateTime DateTime { get; set; }
    }
}
