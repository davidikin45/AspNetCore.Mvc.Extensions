using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Mapping;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Dtos
{
    public class AuthorDeleteDto : DtoAggregateRootBase<int>, IMapFrom<Author>, IMapTo<Author>
    {
        public AuthorDeleteDto()
        {

        }
    }
}
