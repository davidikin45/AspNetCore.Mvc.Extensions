using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data.Attributes
{
    //AutoDetectChangesEnabled and QueryTrackingBehavior doesn't really help as can still Add to DbContext
    //The best thing in query mode is disable tracking
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
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking); //performance
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = false);  //safety net for update
                _unitOfWorks.ToList().ForEach(uow => uow.CommitingChanges = true); //safety net for add/update
                base.OnActionExecuting(context);

            }

            public override void OnActionExecuted(ActionExecutedContext context)
            {
                _unitOfWorks.ToList().ForEach(uow => uow.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll);
                _unitOfWorks.ToList().ForEach(uow => uow.AutoDetectChangesEnabled = true);
                _unitOfWorks.ToList().ForEach(uow => uow.CommitingChanges = false);
                base.OnActionExecuted(context);
            }
        }
    }
}