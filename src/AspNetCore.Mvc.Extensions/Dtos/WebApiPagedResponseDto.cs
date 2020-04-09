using Newtonsoft.Json;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public class WebApiPagedResponseDto<T> : PagingInfoDto
    {
        [JsonIgnore]
        public List<T> Data
        {
            get { return Rows; }
        }

        public List<T> Rows
        { get; set; }
    }
}
