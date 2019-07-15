using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.NdjsonStream
{
    internal interface INdjsonWriterFactory
    {
        INdjsonWriter CreateWriter(ActionContext context, NdjsonStreamResult result);
    }
}
