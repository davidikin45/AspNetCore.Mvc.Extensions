using AspNetCore.Specification.UI;

namespace AspNetCore.Mvc.Extensions.UI
{
    //CustomModelingBinding and return ModelBindingResult.Failed()
    //Should return 400.
    //[FromQuery]

    public class WebApiSearchQueryParamsDto<T> :  WebApiQueryParamsDto<T>
    {
        public string Search { get; set; }
        public string UserId { get; set; }
    }


    public class WebApiSearchQueryParamsDto : WebApiQueryParamsDto
    {
        public string Search { get; set; }
    }

}
