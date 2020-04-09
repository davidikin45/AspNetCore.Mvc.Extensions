using AspNetCore.Mvc.Extensions.Domain;

namespace TemplateAspNetCore3.Models
{
    public class Author : EntityAggregateRootBase<int>
    {
        //[Required]
        public string Name { get; set; }

        //[StringLength(50)]
        public string UrlSlug { get; set; }

        public Author()
        {

        }
    }
}
