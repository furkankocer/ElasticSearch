using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Dto
{
    public class UserSuggestResponseDto
    {
        public IEnumerable<UserSuggestDto> UserSuggests { get; set; }
    }
}
