using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;

namespace AspNetCore.Mvc.Extensions.Alerts
{
    public static class AlertExtensions
	{
		const string Alerts = "_Alerts";
        public const string AlertAdded = "_AlertAdded";

        public static List<Alert> GetAlertsForOutput(this ITempDataDictionary tempData, HttpContext context)
        {
            //As soon as tempdata is accessed a Cookie ".AspNetCore.Mvc.CookieTempDataProvider" is created which doesn't allow caching!
            if (tempData.Count == 0)
            {
                tempData.SetFieldValue("_loaded", false);
                return new List<Alert>();
            }
            else
            {
                return GetAlerts(tempData);
            }
        }

        private static List<Alert> GetAlerts(this ITempDataDictionary tempData)
		{
			if (!tempData.ContainsKey(Alerts))
			{
				tempData.Put(Alerts, new List<Alert>());
			}

			return tempData.Get<List<Alert>>(Alerts);
		}

        public static void AddAlert(this ITempDataDictionary tempData, Alert alert, HttpContext context)
        {
            var alerts = GetAlerts(tempData);
            alerts.Add(alert);
            tempData.Put(Alerts, alerts);

            if (!context.Items.ContainsKey(AlertExtensions.AlertAdded))
            {
                context.Items.Add(AlertExtensions.AlertAdded, true);
            }
        }

        //Model Errors should be shown in validationsummary not an alert
        //public static ActionResult WithModelErrors(this ActionResult result, ModelStateDictionary modelState)
        //{
        //    foreach (KeyValuePair<string, System.Web.Mvc.ModelState> property in modelState)
        //    {
        //        foreach (System.Web.Mvc.ModelError modelError in property.Value.Errors)
        //        {
        //            var errorMessage = modelError.ErrorMessage;
        //            result = result.WithError(errorMessage);
        //        }
        //    }
        //    return result;
        //}

        public static ActionResult WithSuccess(this ActionResult result, Controller controller, string message)
		{
			return new AlertDecoratorResult(result, "alert-success", message, controller);
		}

		public static ActionResult WithInfo(this ActionResult result, Controller controller, string message)
		{
			return new AlertDecoratorResult(result, "alert-info", message, controller);
		}

		public static ActionResult WithWarning(this ActionResult result, Controller controller, string message)
		{
			return new AlertDecoratorResult(result, "alert-warning", message, controller);
		}

		public static ActionResult WithError(this ActionResult result, Controller controller, string message)
		{
			return new AlertDecoratorResult(result, "alert-danger", message, controller);
		}
	}
}