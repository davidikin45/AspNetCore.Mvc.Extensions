using AspNetCore.Mvc.Extensions.Dtos;
using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.Dtos
{
    public class AuthorDto : DtoAggregateRootBase<int>, IHaveCustomMappings
    {
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        public string UrlSlug { get; set; }

        public AuthorDto()
        {

        }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<AuthorDto, Author>()
             .ForMember(bo => bo.UpdatedOn, dto => dto.Ignore())
            .ForMember(bo => bo.CreatedOn, dto => dto.Ignore());

            configuration.CreateMap<Author, AuthorDto>();
        }
    }
}
