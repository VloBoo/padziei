using System.Text.Json;

public class ApiCodeHandler
{

    [ApiCode(12)]
    public async Task code12(HttpContext context, JsonDocument jd)
    {
        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"13\",\"body\":{\"content\":\"hello padziei\"}}");
    }

    [ApiCode(20)]
    public async Task code20(HttpContext context, JsonDocument jd)
    {

        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        string email = jd.RootElement.GetProperty("body").GetProperty("email").ToString();
        string password = jd.RootElement.GetProperty("body").GetProperty("password").ToString();

        string status = -1 != Database.Hinstance.CreateUser(username, password, email) ? "OK" : "NOT";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"status\":\"" + status + "\"}}");
    }

    [ApiCode(30)]
    public async Task code30(HttpContext context, JsonDocument jd)
    {

        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        string password = jd.RootElement.GetProperty("body").GetProperty("password").ToString();

        Guid? g = Database.Hinstance.CreateToken(username, password);
        string status = g is null ? "NOT" : "OK";

        var response = context.Response;
        if (g is not null)
        {
            response.Cookies.Append("_t", g.ToString() ?? "_");
        }
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"status\":\"" + status + "\"}}");
    }
}


[AttributeUsage(AttributeTargets.Method)]
public class ApiCodeAttribute : Attribute
{
    private int _code;
    public ApiCodeAttribute(int code)
    {
        this._code = code;
    }

    public int Code
    {
        get
        {
            return this._code;
        }
    }
}
