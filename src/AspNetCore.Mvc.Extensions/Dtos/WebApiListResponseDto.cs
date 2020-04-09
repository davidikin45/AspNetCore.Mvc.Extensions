using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public class WebApiListResponseDto<TDto>
    {
        public List<TDto> Value { get; set; }
        public List<LinkDto> Links { get; set; }
    }
}
