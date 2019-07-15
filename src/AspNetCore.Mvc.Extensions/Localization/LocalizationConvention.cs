using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Base.Localization
{
    //https://andrewlock.net/url-culture-provider-using-middleware-as-mvc-filter-in-asp-net-core-1-1-0/
    //https://andrewlock.net/applying-the-routedatarequest-cultureprovider-globally-with-middleware-as-filters/
    //https://andrewlock.net/using-a-culture-constraint-and-catching-404s-with-the-url-culture-provider/
    //https://andrewlock.net/redirecting-unknown-cultures-to-the-default-culture-when-using-the-url-culture-provider/
    public class LocalizationConvention : IApplicationModelConvention
    {
        private readonly bool _optional;
        private readonly string _cultureConstraintKey;

        public LocalizationConvention(bool optional, string cultureConstraintKey)
        {
            _optional = optional;
            _cultureConstraintKey = cultureConstraintKey;
        }

        public void Apply(ApplicationModel application)
        {
            var culturePrefix = new AttributeRouteModel(new RouteAttribute("{culture" + (!string.IsNullOrEmpty(_cultureConstraintKey) ? $":{_cultureConstraintKey}" : "") + "}"));

            foreach (var controller in application.Controllers)
            {
                var newSelectors = new List<SelectorModel>();

                var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                if (matchedSelectors.Any())
                {
                    foreach (var selectorModel in matchedSelectors)
                    {
                        var routeModel = AttributeRouteModel.CombineAttributeRouteModel(culturePrefix, selectorModel.AttributeRouteModel);

                        if (_optional)
                        {
                            var newSelector = new SelectorModel();
                            newSelector.AttributeRouteModel = routeModel;
                            newSelector.AttributeRouteModel.Order = -1;
                            newSelectors.Add(newSelector);
                        }
                        else
                        {
                            selectorModel.AttributeRouteModel = routeModel;
                        }
                    }
                }

                var unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();
                if (unmatchedSelectors.Any())
                {
                    foreach (var selectorModel in unmatchedSelectors)
                    {
                        var routeModel = culturePrefix;
                        if (_optional)
                        {
                            var newSelector = new SelectorModel();
                            newSelector.AttributeRouteModel = routeModel;
                            newSelector.AttributeRouteModel.Order = -1;
                            newSelectors.Add(newSelector);
                        }
                        else
                        {
                            selectorModel.AttributeRouteModel = routeModel;
                        }

                    }
                }

                foreach (var newSelector in newSelectors)
                {
                    controller.Selectors.Insert(0, newSelector);
                }
            }
        }
    }
}
