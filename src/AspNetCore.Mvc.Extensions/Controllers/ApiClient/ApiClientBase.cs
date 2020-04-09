using AspNetCore.Mvc.Extensions.ApiClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AspNetCore.Mvc.Extensions.Controllers.ApiClient
{
    public abstract class ApiClientBase : ApiClientService, IApiClient
    {
        protected readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public ApiClientBase(HttpClient client, ApiClientSettings apiClientSettings, IMemoryCache memoryCache = null, ILogger logger = null) 
            : base(client, apiClientSettings, memoryCache, logger)
        {
            InitializeRepositories(client);
        }

        public abstract void InitializeRepositories(HttpClient httpClient);

        public void AddRepository<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>(IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto> repository)
            where TCreateDto : class
            where TReadDto : class
            where TUpdateDto : class
            where TDeleteDto : class
        {
            var key = typeof(IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>);
            repositories[key] = repository;
        }

        public IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto> Repository<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>()
            where TCreateDto : class
            where TReadDto : class
            where TUpdateDto : class
            where TDeleteDto : class
        {
            var key = typeof(IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>);
            return (IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>)repositories[key];
        }
    }
}
