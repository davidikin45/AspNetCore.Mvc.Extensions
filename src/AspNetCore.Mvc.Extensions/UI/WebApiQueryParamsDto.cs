using AspNetCore.Mvc.Extensions.Specification;
using AspNetCore.Mvc.Extensions.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json.Serialization;

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

    public class WebApiQueryParamsDto<T> : WebApiParamsDto<T>
    {
        const int maxPageSize = 100;

        // no. of records to fetch
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        // the page index
        public int Page { get; set; } = 1;

        [ModelBinder(BinderType = typeof(UserFilterModelBinder), Name = "Filter")]
        public FilterSpecification<T> FilterSpecification { get; set; }

        //public FilterSpecification<T> FilterSpecification(string queryString) => UserFilterSpecification.Create<T>(queryString);
        //public FilterSpecification<T> FilterSpecification(IQueryCollection queryCollection) => UserFilterSpecification.Create<T>(queryCollection);
        //public FilterSpecification<T> FilterSpecification(object queryParams) => UserFilterSpecification.Create<T>(queryParams);

        public string OrderBy { get; set; }

        [ModelBinder(BinderType = typeof(UserOrderByModelBinder), Name = nameof(OrderBy))]
        public OrderBySpecification<T> OrderBySpecification { get; set; }
        //public OrderBySpecification<T> OrderBySpecification => new Lazy<OrderBySpecification<T>>(() => UserOrderBySpecification.Create<T>(OrderBy)).Value;
    }

    public class WebApiSearchQueryParamsDto : WebApiQueryParamsDto
    {
        public string Search { get; set; }
    }

    public class WebApiQueryParamsDto : WebApiParamsDto
    {
        const int maxPageSize = 100;

        // no. of records to fetch
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        // the page index
        public int Page { get; set; } = 1;

        public FilterSpecification FilterSpecification(Type type, string queryString) => UserFilterSpecification.Create(type, queryString);
        public FilterSpecification FilterSpecification(Type type, IQueryCollection queryCollection) => UserFilterSpecification.Create(type, queryCollection);
        public FilterSpecification FilterSpecification(Type type, object queryParams) => UserFilterSpecification.Create(type, queryParams);

        public string OrderBy { get; set; }

        public OrderBySpecification OrderBySpecification(Type type) => UserOrderBySpecification.Create(type, OrderBy);
    }
}
