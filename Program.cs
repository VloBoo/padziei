using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;

const string WEB_DIR = @"C:/Users/VloBo/Documents/code/padieja";



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



app.Run(async (context) =>
{
    app.Logger.LogDebug(context.Request.Path);
    switch (context.Request.Path)
    {
        case var a when new Regex(@"^\/(css|js).*$").IsMatch(a):
            {
                await context.Response.SendFileAsync(WEB_DIR + context.Request.Path);
                return;
            }
        case var b when new Regex(@"^\/api.*$").IsMatch(b):
            {
                string requestBody;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                JsonDocument jd = JsonDocument.Parse(requestBody);
                int code = Convert.ToInt32(jd.RootElement.GetProperty("code").ToString());

                // TODO: Раскинуть эти if в методы потом с атрибутами и все будет чики пуки

                if (code == 12)
                {
                    var response = context.Response;
                    response.Headers.ContentType = "application/json; charset=utf-8";
                    await response.WriteAsync("{\"code\":\"13\",\"body\":{\"content\":\"hello padziei\"}}");
                }

                return;
            }
        case var b when new Regex(@"^\/.*$").IsMatch(b):
            {
                try
                {
                    await context.Response.SendFileAsync(WEB_DIR + context.Request.Path + "/index.html");
                }
                catch (FileNotFoundException ee)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.SendFileAsync(WEB_DIR + "/404.html");
                }
                return;
            }
    }
    // context.Response.ContentType = "text/html; charset=utf-8";
    // string str = "1";
    // foreach (var param in context.Request.Query)
    // {
    //     if (param.Key == "key")
    //     {
    //         str = param.Value.ToString();
    //     }
    // }
    // await context.Response.WriteAsync(str);
});
app.Run();

