using AutoMapper;

namespace AspNetCore.Mvc.Extensions.Mapping
{
    public interface IHaveCustomMappings
    {
        void CreateMappings(IMapperConfigurationExpression configuration);
    }
}
