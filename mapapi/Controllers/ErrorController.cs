using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace mapapi.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Microsoft.AspNetCore.Mvc.Route("/error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            string detailMsg;
            if (context.Error.InnerException != null)
                detailMsg = context.Error.InnerException.Message;
            else
                detailMsg = "";
            return Problem(
                detail: detailMsg,
                title: context.Error.Message);
        }
        
    }
}
