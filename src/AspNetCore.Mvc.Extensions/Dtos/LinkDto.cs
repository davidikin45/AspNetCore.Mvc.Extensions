using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public class LinkDto
    {
        public string Href { get; private set; }
        public string Rel { get; private set; }
        public string Method { get; private set; }

        public LinkDto()
        {

        }

        public LinkDto(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
    }
}
