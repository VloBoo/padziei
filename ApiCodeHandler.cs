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

    [ApiCode(42)]
    public async Task code42(HttpContext context, JsonDocument jd)
    {
        Program.app.Logger.LogWarning(jd.RootElement.ToString());
        Guid? token;
        Guid? id;
        try
        {
            token = new Guid(jd.RootElement.GetProperty("body").GetProperty("token").ToString());
        }
        catch (KeyNotFoundException)
        {
            token = null;
        }

        try
        {
            id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());
        }
        catch (KeyNotFoundException)
        {
            id = null;
        }

        if (id is null)
        {
            if (token is not null)
            {
                id = Database.Hinstance.GetUserByToken(token ?? new Guid());
            }
        }

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";

        if (id is null)
        {
            await response.WriteAsync("{\"code\":\"43\",\"body\":{\"status\":\"NOT\"}}");
            return;
        }
        else
        {
            string? res = Database.Hinstance.GetUserInfo(id ?? new Guid(), (token is null));
            if (res is null)
            {
                await response.WriteAsync("{\"code\":\"43\",\"body\":{\"status\":\"NOT\"}}");
                Program.app.Logger.LogWarning("3");
                return;
            }
            else
            {
                await response.WriteAsync("{\"code\":\"43\",\"body\":{\"status\":\"OK\",\"user\":" + res + "}}");
                Program.app.Logger.LogWarning(res);
                return;
            }
        }
    }

    [ApiCode(52)]
    public async Task code52(HttpContext context, JsonDocument jd)
    {

        int count = Convert.ToInt32(jd.RootElement.GetProperty("body").GetProperty("count").ToString());

        Guid[] guids = Database.Hinstance.SelectTopThreads(count);

        string asnwer = "[";
        for (int i = 0; i < guids.Length - 1; i++)
        {
            asnwer += "\"" + guids[i].ToString() + "\",";
        }
        asnwer += "\"" + guids[guids.Length - 1].ToString() + "\"]";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"53\",\"body\":{\"threads\":" + asnwer + "}}");
    }

    [ApiCode(62)]
    public async Task code62(HttpContext context, JsonDocument jd)
    {

        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());

        string str = Database.Hinstance.GetThredInfo(id) ?? "{}";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"63\",\"body\":" + str + "}");
        Program.app.Logger.LogWarning("{\"code\":\"63\",\"body\":" + str + "}");
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
