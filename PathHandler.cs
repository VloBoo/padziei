using System.Reflection;
using System.Text;
using System.Text.Json;

public class PathHandler
{
    [Path(@"^\/(css|js).*$")]
    public async Task fun1(HttpContext context)
    {
        await context.Response.SendFileAsync(Program.WEB_DIR + context.Request.Path);
        return;
    }

    [Path(@"^\/api.*$")]
    public async Task fun2(HttpContext context)
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
                    return;
                }
            }
        }
        return;
    }
    [Path(@"^\/.*\..*$")]
    public async Task fun3(HttpContext context)
    {
        try
        {
            await context.Response.SendFileAsync(Program.WEB_DIR + context.Request.Path);
        }
        catch (FileNotFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.SendFileAsync(Program.WEB_DIR + "/404.html");
        }
        return;
    }
    [Path(@"\/?.*$")]
    public async Task fun4(HttpContext context)
    {
        try
        {
            await context.Response.SendFileAsync(Program.WEB_DIR + context.Request.Path + "/index.html");
        }
        catch (FileNotFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.SendFileAsync(Program.WEB_DIR + "/404.html");
        }
        return;
    }

}

[AttributeUsage(AttributeTargets.Method)]
public class PathAttribute : Attribute
{
    private string _regex;
    public PathAttribute(string regex)
    {
        this._regex = regex;
    }

    public string Regex
    {
        get
        {
            return this._regex;
        }
    }
}
