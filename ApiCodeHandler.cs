using System.Text.Json;

public class ApiCodeHandler
{
    /// <summary>
    /// Обработчик кода 12.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(12)]
    public async Task code12(HttpContext context, JsonDocument jd)
    {
        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"13\",\"body\":{\"content\":\"hello padziei\"}}");
    }

    /// <summary>
    /// Обработчик кода 20.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(20)]
    public async Task code20(HttpContext context, JsonDocument jd)
    {
        // Извлечение данных из JSON-документа
        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        string email = jd.RootElement.GetProperty("body").GetProperty("email").ToString();
        string password = jd.RootElement.GetProperty("body").GetProperty("password").ToString();

        // Создание пользователя в базе данных
        string status = -1 != Database.Hinstance.CreateUser(username, password, email) ? "OK" : "NOT";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"status\":\"" + status + "\"}}");
    }

    /// <summary>
    /// Обработчик кода 30.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(30)]
    public async Task code30(HttpContext context, JsonDocument jd)
    {
        // Извлечение данных из JSON-документа
        string username = jd.RootElement.GetProperty("body").GetProperty("username").ToString();
        string password = jd.RootElement.GetProperty("body").GetProperty("password").ToString();

        // Создание токена и проверка наличия пользователя в базе данных
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

    /// <summary>
    /// Обработчик кода 42.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
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

    /// <summary>
    /// Обработчик кода 52.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(52)]
    public async Task code52(HttpContext context, JsonDocument jd)
    {
        int count = Convert.ToInt32(jd.RootElement.GetProperty("body").GetProperty("count").ToString());

        // Выборка верхних потоков из базы данных
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

    /// <summary>
    /// Обработчик кода 62.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(62)]
    public async Task code62(HttpContext context, JsonDocument jd)
    {
        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());

        // Получение информации о потоке из базы данных
        string str = Database.Hinstance.GetThredInfo(id) ?? "{}";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"63\",\"body\":" + str + "}");
        Program.app.Logger.LogWarning("{\"code\":\"63\",\"body\":" + str + "}");
    }

    /// <summary>
    /// Обработчик кода 70.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(70)]
    public async Task code70(HttpContext context, JsonDocument jd)
    {
        Guid token = new Guid(jd.RootElement.GetProperty("body").GetProperty("token").ToString());
        string title = jd.RootElement.GetProperty("body").GetProperty("title").ToString();
        string content = jd.RootElement.GetProperty("body").GetProperty("content").ToString();

        // Создание нового потока в базе данных
        Guid? id = Database.Hinstance.CreateThread(token, title, content);

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        if (id is null)
        {
            await response.WriteAsync("{\"code\":\"71\",\"body\":{\"id\":null}}");
            return;
        }
        await response.WriteAsync("{\"code\":\"71\",\"body\":{\"id\":\"" + id + "\"}}");
        Program.app.Logger.LogWarning("{\"code\":\"71\",\"body\":{\"id\":\"" + id + "\"}}");
    }

    /// <summary>
    /// Обработчик кода 82.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(82)]
    public async Task code82(HttpContext context, JsonDocument jd)
    {
        Guid thread = new Guid(jd.RootElement.GetProperty("body").GetProperty("thread").ToString());

        // Получение комментариев из базы данных для указанного потока
        Guid[] guids = Database.Hinstance.GetCommentsFromThread(thread);

        string asnwer = "[";
        for (int i = 0; i < guids.Length - 1; i++)
        {
            asnwer += "\"" + guids[i].ToString() + "\",";
        }
        if (guids.Length == 0)
        {
            asnwer += "]";
        }
        else
        {
            asnwer += "\"" + guids[guids.Length - 1].ToString() + "\"]";
        }

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"83\",\"body\":{\"comments\":" + asnwer + "}}");
    }

    /// <summary>
    /// Обработчик кода 92.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(92)]
    public async Task code92(HttpContext context, JsonDocument jd)
    {
        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());

        // Получение информации о комментарии из базы данных
        string str = Database.Hinstance.GetCommentInfo(id) ?? "{}";

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"93\",\"body\":" + str + "}");
        Program.app.Logger.LogWarning("{\"code\":\"93\",\"body\":" + str + "}");
    }

    /// <summary>
    /// Обработчик кода 100.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(100)]
    public async Task code100(HttpContext context, JsonDocument jd)
    {
        Guid token = new Guid(jd.RootElement.GetProperty("body").GetProperty("token").ToString());
        Guid thread = new Guid(jd.RootElement.GetProperty("body").GetProperty("thread").ToString());
        string content = jd.RootElement.GetProperty("body").GetProperty("content").ToString();

        // Создание нового комментария в базе данных
        Guid? id = Database.Hinstance.CreateComment(token, thread, content);

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        if (id is null)
        {
            await response.WriteAsync("{\"code\":\"101\",\"body\":{\"id\":null}}");
            return;
        }
        await response.WriteAsync("{\"code\":\"101\",\"body\":{\"id\":\"" + id + "\"}}");
        Program.app.Logger.LogWarning("{\"code\":\"101\",\"body\":{\"id\":\"" + id + "\"}}");
    }

    /// <summary>
    /// Обработчик кода 110.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(110)]
    public async Task code110(HttpContext context, JsonDocument jd)
    {
        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());

        string status = -1 != Database.Hinstance.RemoveThread(id) ? "OK" : "NOT";

        Program.app.Logger.LogWarning(status + "!");

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"status\":\"" + status + "\"}}");
    }

    /// <summary>
    /// Обработчик кода 120.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(120)]
    public async Task code120(HttpContext context, JsonDocument jd)
    {
        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("id").ToString());

        string status = -1 != Database.Hinstance.RemoveComment(id) ? "OK" : "NOT";

        Program.app.Logger.LogWarning(status + "!");

        var response = context.Response;
        response.Headers.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"code\":\"1\",\"body\":{\"status\":\"" + status + "\"}}");
    }

    /// <summary>
    /// Обработчик кода 130.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <param name="jd">JSON-документ.</param>
    [ApiCode(130)]
    public async Task code130(HttpContext context, JsonDocument jd)
    {
        Guid id = new Guid(jd.RootElement.GetProperty("body").GetProperty("user").ToString());
        string role = jd.RootElement.GetProperty("body").GetProperty("role").ToString();

        string status = -1 != Database.Hinstance.SetUserRole(id, role) ? "OK" : "NOT";

        Program.app.Logger.LogWarning(status + "!");

        var response = context.Response;
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
