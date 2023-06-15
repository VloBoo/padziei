using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;
using System.Diagnostics.CodeAnalysis;

public class Program
{
    public static string WEB_DIR = @"C:/Users/VloBo/Documents/code/padziei-webpages";

    [NotNull]
    public static WebApplication app;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        app = builder.Build();

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

                        foreach (var method in typeof(ApiCodeHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var attribute = method.GetCustomAttribute<ApiCodeAttribute>();
                            if (attribute != null && attribute.Code == code)
                            {
                                Task? task = (Task?)(method.Invoke(new ApiCodeHandler(), new Object[] { context, jd }));
                                if (task is not null)
                                {
                                    await task;
                                }
                            }
                        }

                        return;
                    }
                case var c when new Regex(@"^\/.*\..*$").IsMatch(c):
                    {
                        try
                        {
                            await context.Response.SendFileAsync(WEB_DIR + context.Request.Path);
                        }
                        catch (FileNotFoundException)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.SendFileAsync(WEB_DIR + "/404.html");
                        }
                        return;
                    }
                case var d when new Regex(@"\/?.*$").IsMatch(d):
                    {
                        try
                        {
                            await context.Response.SendFileAsync(WEB_DIR + context.Request.Path + "/index.html");
                        }
                        catch (FileNotFoundException)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.SendFileAsync(WEB_DIR + "/404.html");
                        }
                        return;
                    }

            }
        });
        app.Run();
    }
}