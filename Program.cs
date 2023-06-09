using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

public class Program
{
    public static string WEB_DIR = @"./pages";

    [NotNull]
    public static WebApplication app;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        app = builder.Build();

        _ = ProxyServer.Start();

        app.Run(async (context) =>
        {
            try
            {
                app.Logger.LogWarning(context.Connection.LocalIpAddress + " " + context.Connection.RemoteIpAddress);

                foreach (var method in typeof(PathHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attribute = method.GetCustomAttribute<PathAttribute>();
                    if (attribute != null && new Regex(attribute.Regex).IsMatch(context.Request.Path))
                    {
                        Task? task = (Task?)(method.Invoke(new PathHandler(), new Object[] { context }));
                        if (task is not null)
                        {
                            await task;
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                app.Logger.LogError("КРИТИЧЕСКАЯ ОШИБКА ОБРАБОТКИ ЗАПРОСА\n" + e.Message + "\n\n" + e.StackTrace);
            }
            return;
        });
        app.Run();
    }
}
