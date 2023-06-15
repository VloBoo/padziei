using Npgsql;
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
        var connString = "Server=localhost;Port=5432;Database=padziei;User Id=postgres;Password=postgres1;";

        string strbuf = "";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            using (var command = new NpgsqlCommand("SELECT column_name FROM information_schema.columns WHERE table_name = 'users'", conn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        strbuf += $"{reader.GetString(0)} ";
                    }
                }
            }
        }
        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"23\",\"body\":{\"content\":\"" + strbuf + "\"}}");
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