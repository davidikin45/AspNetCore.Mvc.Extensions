using AspNetCore.Mvc.Extensions.Data.UnitOfWork;
using System;

namespace AspNetCore.Mvc.Extensions.Data.Attributes
{
    public class DisableChangeTracking : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        public DisableChangeTracking(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _unitOfWork.AutoDetectChangesEnabled = false;
        }

        public void Dispose()
        {
            _unitOfWork.AutoDetectChangesEnabled = true;
        }
    }
}