using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mapapi.Services
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyName = "api-key";
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                context.Response.Headers.Add("api-key", "Api Key was not provided");
                //var description = new
                //{
                //    title = "Api Key was not provided. Define the key in ApiKey header"
                //};
                //var json = JsonConvert.SerializeObject(description);
                //await context.Response.WriteAsync(json);
                return;
            }

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();

            //var apiKey = appSettings.GetValue<string>(ApiKeyName);
            //var apiKey = appSettings.GetValue<List<string>>("AppSettings:api-keys");
            var apiKey = appSettings.GetSection("AppSettings:api-keys").Get<List<string>>();

            if (!apiKey.Contains(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                context.Response.Headers.Add("api-key", "Api key is not valid");
                //var description = new
                //{
                //    title = "Api key is not valid"
                //};
                //var json = JsonConvert.SerializeObject(description);
                //await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }
    }
}

