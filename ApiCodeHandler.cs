using System.Text.Json;

public class ApiCodeHandler
{

    [ApiCode(12)]
    public async void code12(HttpContext context, JsonDocument jd)
    {
        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"13\",\"body\":{\"content\":\"hello padziei\"}}");
    }

    [ApiCode(20)]
    public async void code20(HttpContext context, JsonDocument jd)
    {

        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        string email = jd.RootElement.GetProperty("body").GetProperty("email").ToString();
        string password = jd.RootElement.GetProperty("body").GetProperty("password").ToString();

        string status = -1 != Database.Hinstance.CreateUser(username, password, email) ? "OK" : "NOT";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"content\":\"" + status + "\"}}");
    }

    [ApiCode(999)]
    public async void code999(HttpContext context, JsonDocument jd)
    {
        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        var uuid = Database.Hinstance.FindUserByUsername(username);
        string uuidstr = "Not Found UUID";
        if (uuid is not null)
        {
            uuidstr = uuid.ToString() ?? "Not Found UUID";
        }
        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1000\",\"body\":{\"uuid\":\"" + uuidstr + "\"}}");
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
