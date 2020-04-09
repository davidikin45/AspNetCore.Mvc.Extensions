namespace AspNetCore.Mvc.Extensions.Controllers.ApiClient
{
    public interface IApiClient
    {
        IApiControllerEntityClient<TCreateDto, TReadDto, TUpdateDto, TDeleteDto> Repository<TCreateDto, TReadDto, TUpdateDto, TDeleteDto>() 
            where TCreateDto : class
            where TReadDto : class
            where TUpdateDto : class
            where TDeleteDto : class;
    }
}
