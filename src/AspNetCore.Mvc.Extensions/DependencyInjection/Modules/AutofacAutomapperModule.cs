using AspNetCore.Mvc.Extensions.Mapping;
using Autofac;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.QueryableExtensions;
using System;

namespace AspNetCore.Mvc.Extensions.DependencyInjection.Modules
{
    public class AutofacAutomapperModule : Module
    {      
        public Func<System.Reflection.Assembly, Boolean> Filter;

        protected override void Load(ContainerBuilder builder)
        {
            var config = new MapperConfiguration(cfg => {
                //https://github.com/AutoMapper/AutoMapper.Extensions.ExpressionMapping
                //AutoMapper.Extensions.ExpressionMapping
                cfg.AddExpressionMapping();
                //AutoMapper.Collection
                cfg.AddCollectionMappers();
                new AutoMapperConfiguration(cfg, Filter);
            });

            builder.RegisterInstance(config).As<MapperConfiguration>();
            builder.Register(ctx => config).As<IConfigurationProvider>();
            builder.Register(ctx => new ExpressionBuilder(config)).As<IExpressionBuilder>();
            builder.Register(c => { return config.CreateMapper(); }).As<IMapper>().SingleInstance();      
        }
    }
}
