using AutoMapper;
using System;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Mapping
{
    public static class AutomapperExtensions
    {
        public static MemberInfo GetDestinationMappedProperty<TSource, TDestination>(this IMapper mapper, string sourcePropertyName)
        {
            return GetDestinationMappedProperty(mapper, typeof(TSource), typeof(TDestination), sourcePropertyName);
        }

        public static MemberInfo GetDestinationMappedProperty(this IMapper mapper, Type sourceType, Type destinationType, string sourcePropertyName)
        {
            MemberInfo sourceProperty = sourceType.GetProperty(sourcePropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return GetDestinationMappedProperty(mapper, sourceType, destinationType, sourceProperty);
        }

        public static MemberInfo GetDestinationMappedProperty(this IMapper mapper, Type sourceType, Type destinationType, MemberInfo sourceProperty)
        {
            var map = mapper.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

            PropertyMap propmap = map
            .PropertyMaps
            .SingleOrDefault(m =>
                m.SourceMember != null &&
                m.SourceMember.MetadataToken == sourceProperty.MetadataToken);

            if (propmap == null)
            {
                throw new Exception(
                    string.Format(
                    "Can't map selector. Could not find a PropertyMap for {0}", sourceProperty.Name));
            }

            return propmap.DestinationMember;
        }
    }

}
