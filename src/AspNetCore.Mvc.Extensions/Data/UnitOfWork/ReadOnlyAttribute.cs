using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data.Attributes
{
    public class ReadOnlyAttribute : TypeFilterAttribute
    {
        public ReadOnlyAttribute() : base(typeof(ReadOnlyAttributeImpl))
        {
        }

        private class ReadOnlyAttributeImpl : ActionFilterAttribute
        {
            private readonly IUnitOfWork[] _unitOfWorks;
            public ReadOnlyAttributeImpl(IUnitOfWork[] unitOfWorks)
            {
                _unitOfWorks = unitOfWorks;
            }

            public override void OnActionExecuting(ActionExecutingContext context)
            {
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = false);
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking);
                base.OnActionExecuting(context);

            }

            public override void OnActionExecuted(ActionExecutedContext context)
            {
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = true);
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll);
                base.OnActionExecuted(context);
            }
        }
    }
}