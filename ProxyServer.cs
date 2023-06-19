using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Как я люблю мягкотелых, что за хуйня этот ваш iis, когда можно сделать так. 
// Профита по месту и затратам меньше, а безопаснасть меня не мучает. 
// И я вообще скоро линукс поставля, так что пока-пока майкрасофт.
class ProxyServer
{
    public static async Task Start()
    {
        try
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 80);
            listener.Start();
            Program.app.Logger.LogInformation("Proxy-server is running on: port 80 to 1984");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Program.app.Logger.LogDebug("Proxy connected opened");

                _ = HandleClient(client);
            }
        }
        catch (Exception ex)
        {
            Program.app.Logger.LogError("Failed to start proxy server: \n" + ex.Message + "\n" + ex.StackTrace);
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            try
            {
                // Создаем подключение к целевому серверу на порту 1984
                TcpClient targetServer = new TcpClient();
                await targetServer.ConnectAsync("localhost", 1984);

                // Получаем потоки для чтения и записи данных между клиентом и сервером
                using (NetworkStream clientStream = client.GetStream())
                using (NetworkStream targetStream = targetServer.GetStream())
                {
                    // Асинхронно перенаправляем данные от клиента к серверу и наоборот
                    Task clientToServer = clientStream.CopyToAsync(targetStream);
                    Task serverToClient = targetStream.CopyToAsync(clientStream);

                    // Ожидаем завершения обоих задач
                    await Task.WhenAll(clientToServer, serverToClient);
                }
            }
            catch (Exception ex)
            {
                Program.app.Logger.LogError("Failed to send information on proxy server: \n" + ex.Message + "\n" + ex.StackTrace);
            }

            Program.app.Logger.LogDebug("Proxy connected closed");
        }
    }
}