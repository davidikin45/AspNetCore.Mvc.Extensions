using System;

namespace AspNetCore.Mvc.Extensions.Security
{
    public class JwtToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
