using AspNetCore.Mvc.Extensions.Attributes.Display;
using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public class FolderMetadataDto : DtoBase<string>, IHaveCustomMappings
    {
        [Render(ShowForDisplay = false, ShowForEdit = false, ShowForGrid = false)]
        public DirectoryInfo Folder { get; set; }

        [Render(AllowSortForGrid = true)]
        [Required]
        public DateTime CreationTime { get; set; }

        [Render(ShowForGrid = false), Required]
        public string Name { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<DirectoryInfo, FolderMetadataDto>()
            .ForMember(dto => dto.Id, bo => bo.MapFrom(s => s.FullName))
            .ForMember(dto => dto.Name, bo => bo.MapFrom(s => s.Name))
            .ForMember(dto => dto.CreationTime, bo => bo.MapFrom(s => s.LastWriteTime))
            .ForMember(dto => dto.Folder, bo => bo.MapFrom(s => s));
        }

    }
}
