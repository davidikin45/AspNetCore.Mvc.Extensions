using AspNetCore.Mvc.Extensions.Attributes.Display;
using AspNetCore.Mvc.Extensions.Data.RepositoryFileSystem.File;
using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using NetTopologySuite.Geometries;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public class ImageInfoDto : DtoBase<string>, IHaveCustomMappings
    {
        //[Render(ShowForDisplay =false, ShowForEdit = false, ShowForGrid = false)]
        public FileInfo Image { get; set; }

        [Required]
        public DateTime DateTaken { get; set; }

        [Required, Render(ShowForGrid = false)]
        public DateTime DateCreated { get; set; }

        [Render(ShowForGrid = false), Required]
        public string Caption { get; set; }

        public string PlaceId { get; set; }
        public Point GPSLocation { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<ImageInfo, ImageInfoDto>()
            .ForMember(dto => dto.Id, bo => bo.MapFrom(s => s.Id))
            .ForMember(dto => dto.PlaceId, bo => bo.MapFrom(s => s.Comments))
            .ForMember(dto => dto.Caption, bo => bo.MapFrom(s => Path.GetFileNameWithoutExtension(s.FileInfo.Name)))
            .ForMember(dto => dto.DateTaken, bo => bo.MapFrom(s => s.DateTimeCreated))
            .ForMember(dto => dto.DateCreated, bo => bo.MapFrom(s => s.FileInfo.LastWriteTime))
            .ForMember(dto => dto.Image, bo => bo.MapFrom(s => s.FileInfo))
            .ForMember(dto => dto.GPSLocation, bo => bo.MapFrom(s => s.GPSLatitudeDegrees.HasValue && s.GPSLongitudeDegrees.HasValue ?
            GeographyExtensions.CreatePoint(s.GPSLatitudeDegrees.Value, s.GPSLongitudeDegrees.Value) : default(Point)));

            configuration.CreateMap<ImageInfoDto, ImageInfo>()
           .ForMember(bo => bo.Comments, dto => dto.MapFrom(s => s.PlaceId))
           .ForMember(bo => bo.DateTimeCreated, dto => dto.MapFrom(s => s.DateTaken))
           .ForMember(bo => bo.GPSLatitudeDegrees, dto => dto.MapFrom(s => s.GPSLocation != null ? s.GPSLocation.Y : (double?)null))
           .ForMember(bo => bo.GPSLongitudeDegrees, dto => dto.MapFrom(s => s.GPSLocation != null ? s.GPSLocation.X : (double?)null));
        }
    }
}
