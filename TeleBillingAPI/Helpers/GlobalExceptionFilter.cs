using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace TeleBillingAPI.Helpers
{
	public class GlobalExceptionFilter : IExceptionFilter
	{
		private readonly Logger logger = LogManager.GetLogger("logger");      

        public GlobalExceptionFilter()
		{

		}

		public void OnException(ExceptionContext context)
		{
			//peachlogger.Trace("GlobalExceptionFilter: " + context.Exception.Message);
			logger.Error("GlobalExceptionFilter: " + context.Exception.Message);
			logger.Trace("GlobalExceptionFilter Trace File: " + context.Exception.StackTrace);
           
        }
	}
}
