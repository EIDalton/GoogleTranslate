using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

using GoogleTranslateAPI.JSON;

namespace GoogleTranslateAPI.Controllers
{
    public class TranslateQuery
    {
        public string q { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public string format { get; set; }
    }hdfghdfgh

dfghdfgh    public class AddCORSHeaderFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            actionExecutedContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            actionExecutedContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET,OPTIONS");
            actionExecutedContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
        }
    }

    [Route("[controller]")]
    public class TranslateController : Controller
    {
        private readonly ILogger _logger;

        public TranslateController(ILogger<TranslateController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [AddCORSHeaderFilter]
        public async Task<string> TranslateQuery(string q, string source, string target)
        {
            TranslateQuery query = new TranslateQuery();
            query.q = q;
            query.source = source;
            query.target = target;
            query.format = "text";

            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(query));

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage httpResponse;

                try
                {
                    _logger.LogTrace("Requesting Translation");
                    httpResponse = await httpClient.PostAsync("https://translation.googleapis.com/language/translate/v2?key=AIzaSyAdnf2VlylP4gobukQHAvDKQmoVh8iT9hE", httpContent);

                    if (httpResponse.Content != null)
                    {
                        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                        return jsonResponse;
                    }
                    else
                        throw new Exception("Response content is null!");
                }
                catch (Exception e)
                {
                    Error errorMessage = new Error();
                    errorMessage.code = 1;
                    errorMessage.message = e.Message;

                    ErrorRoot root = new ErrorRoot
                    {
                        error = errorMessage
                    };

                    string errorJSON = await Task.Run(() =>JsonConvert.SerializeObject(root));
                    return errorJSON;
                }
            }
        }
    }
}
