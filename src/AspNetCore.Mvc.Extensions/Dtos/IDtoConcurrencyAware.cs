using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Dtos
{
    public interface IDtoConcurrencyAware
    {
        byte[] RowVersion { get; set; }
    }
}
