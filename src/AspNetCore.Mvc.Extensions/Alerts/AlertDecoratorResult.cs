using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Mvc.Extensions.Alerts
{
	public class AlertDecoratorResult : ActionResult
	{
		public ActionResult InnerResult { get; set; }
		public string AlertClass { get; set; }
		public string Message { get; set; }
        Controller Controller { get; set; }


        public AlertDecoratorResult(ActionResult innerResult,
				string alertClass,
				string message,
                Controller controller)
		{
			InnerResult = innerResult;
			AlertClass = alertClass;
			Message = message;
            Controller = controller;
		}

        public async override Task ExecuteResultAsync(ActionContext context)
        {
            Controller.TempData.AddAlert(new Alert(AlertClass, Message), context.HttpContext);
            await InnerResult.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
		{
            Controller.TempData.AddAlert(new Alert(AlertClass, Message), context.HttpContext);
			InnerResult.ExecuteResult(context);
		}
	}
}