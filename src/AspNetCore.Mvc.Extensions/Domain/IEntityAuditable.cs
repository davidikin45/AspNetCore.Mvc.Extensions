﻿using System;

namespace AspNetCore.Mvc.Extensions.Domain
{
    public interface IEntityAuditable
    {
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedOn { get; set; }
        string UpdatedBy { get; set; }
    }
}
