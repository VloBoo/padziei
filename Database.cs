using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

class Database
{
    private static Database? HINSTANCE;
    private NpgsqlConnection _connection;
    private const string CONNECTION_STRING = "Server=localhost;Port=5432;Database=padziei;User Id=postgres;Password=postgres1;";
    private Database()
    {
        this._connection = new NpgsqlConnection(CONNECTION_STRING);
        this._connection.Open();
    }
    ~Database()
    {
        this._connection.Close();
    }

    public static Database Hinstance
    {
        get
        {
            if (HINSTANCE is null)
            {
                HINSTANCE = new Database();
            }
            return HINSTANCE;
        }
    }

    public Guid? FindUserByUsername(string username)
    {
        string sqlstr = $"SELECT id FROM Users WHERE Users.username = '{username}';";
        Guid? ret;
        using (var con = new NpgsqlConnection(CONNECTION_STRING))
        {
            con.Open();
            using (var command = new NpgsqlCommand(sqlstr, con))
            {
                ret = (Guid?)command.ExecuteScalar();
            }
        }
        return ret;
    }

    // normal
    // banned
    // admin

    public int CreateUser(string username, string password, string email)
    {
        string hashPassword = password;
        using (var sha256 = SHA256.Create())
        {
            hashPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        try
        {
            string sqlstr = $"INSERT INTO Users VALUES ('{Guid.NewGuid().ToString()}', '{username}', '{hashPassword}', '{DateTime.Now.ToUniversalTime().ToString()}', '{email}', 'normal');";
            Program.app.Logger.LogDebug(sqlstr);
            int ret = 0;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return -1;
        }
    }

    public Guid? CreateToken(string username, string password)
    {
        string hashPassword = password;
        using (var sha256 = SHA256.Create())
        {
            hashPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        try
        {
            string sqlstr = $"SELECT id FROM Users WHERE username = '{username}' AND password = '{hashPassword}';";
            Program.app.Logger.LogDebug(sqlstr);
            Guid? guid;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    guid = (Guid?)command.ExecuteScalar();
                }
            }

            if (guid is null)
            {
                return null;
            }
            Guid token_guid = Guid.NewGuid();
            sqlstr = $"INSERT INTO tokens VALUES ('{token_guid.ToString()}', '{guid.ToString()}', '{DateTime.Now.ToUniversalTime().ToString()}', '30 day');";
            int i = 0;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    i = command.ExecuteNonQuery();
                }
            }
            if (i > 0)
            {
                return token_guid;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return null;
        }
    }

    public Guid? GetUserByToken(Guid userToken)
    {
        Guid? ret;
        try
        {
            string sqlstr = $"SELECT user_id FROM tokens WHERE id = '{userToken.ToString()}';";
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    ret = (Guid?)command.ExecuteScalar();
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return null;
        }
    }

    public string? GetUserInfo(Guid userGuid, bool email)
    {
        try
        {
            string sqlstr = $"SELECT * FROM Users WHERE id = '{userGuid.ToString()}';";
            string outt = "";
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        outt = "{" +
                        "\"id\":\"" + reader.GetGuid(0) + "\"," +
                        "\"username\":\"" + reader.GetString(1) + "\"," +
                        (email ? ("\"email\":\"" + reader.GetString(4) + "\",") : "") +
                        "\"date\":\"" + reader.GetDateTime(3).ToString("dd.MM.yyyy hh:mm") + "\"," +
                        "\"role\":\"" + reader.GetString(5) + "\"" +
                        "}";
                    }
                }
            }
            return outt;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public string? GetThredInfo(Guid threadGuid)
    {
        try
        {
            string sqlstr = $"SELECT * FROM Threads WHERE id = '{threadGuid.ToString()}';";
            string outt = "";
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        outt = "{" +
                        "\"id\":\"" + reader.GetGuid(0) + "\"," +
                        "\"author\":\"" + reader.GetGuid(1) + "\"," +
                        "\"date\":\"" + reader.GetDateTime(2).ToString("dd.MM.yyyy hh:mm") + "\"," +
                        "\"title\":\"" + reader.GetString(3) + "\"," +
                        "\"body\":\"" + reader.GetString(4) + "\"," +
                        "\"karma_count\":\"" + ((Guid[])reader["karma"]).Length + "\"" +
                        "}";
                    }
                }
            }
            return outt;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public Guid[] SelectTopThreads(int count)
    {
        try
        {
            string sqlstr = $"SELECT id FROM Threads LIMIT {count};";
            List<Guid> guids = new List<Guid>();
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guids.Add((Guid)reader["id"]);
                        }
                    }
                }
            }
            return guids.ToArray();
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return new Guid[0];
        }
    }

    public Guid? CreateThread(Guid token, string title, string content)
    {
        Guid? user = GetUserByToken(token);
        if (user is null)
        {
            return null;
        }
        Guid? ret;
        try
        {
            string sqlstr = $"INSERT INTO Threads (id, author, data_create, title, body, karma) VALUES ('{Guid.NewGuid().ToString()}', '{user.ToString()}', '{DateTime.Now.ToUniversalTime().ToString()}', '{title}', '{content}', ARRAY[uuid('{user.ToString()}')]) RETURNING id;";
            Program.app.Logger.LogWarning(sqlstr);
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        ret = (Guid)reader["id"];
                    }
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public Guid[] GetCommentsFromThread(Guid thread)
    {
        try
        {
            string sqlstr = $"SELECT id FROM Comments WHERE thread_id = '{thread.ToString()}';";
            List<Guid> guids = new List<Guid>();
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guids.Add((Guid)reader["id"]);
                        }
                    }
                }
            }
            return guids.ToArray();
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return new Guid[0];
        }
    }

    public string? GetCommentInfo(Guid id)
    {
        try
        {
            string sqlstr = $"SELECT * FROM Comments WHERE id = '{id.ToString()}';";
            string ret = "";
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        ret = "{" +
                       "\"id\":\"" + reader.GetGuid(0) + "\"," +
                       "\"thread\":\"" + reader.GetGuid(1) + "\"," +
                       "\"author\":\"" + reader.GetGuid(2) + "\"," +
                       "\"date\":\"" + reader.GetDateTime(3).ToString("dd.MM.yyyy hh:mm") + "\"," +
                       "\"content\":\"" + reader.GetString(4) + "\"" +
                       "}";
                    }
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public Guid? CreateComment(Guid token, Guid thread, string content)
    {
        Guid? user = GetUserByToken(token);
        if (user is null)
        {
            return null;
        }
        Guid? ret;
        try
        {
            string sqlstr = $"INSERT INTO Comments (id, thread_id, author, data_create, body) VALUES ('{Guid.NewGuid().ToString()}', '{thread.ToString()}', '{user.ToString()}', '{DateTime.Now.ToUniversalTime().ToString()}', '{content}') RETURNING id;";
            Program.app.Logger.LogWarning(sqlstr);
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        ret = (Guid)reader["id"];
                    }
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public int SetUserRole(Guid user, string role)
    {
        try
        {
            string sqlstr = $"UPDATE Users SET role = '{role}' WHERE id = '{user.ToString()}';";
            Program.app.Logger.LogDebug(sqlstr);
            int ret = 0;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return -1;
        }
    }

    public int RemoveComment(Guid comment)
    {
        try
        {
            string sqlstr = $"DELETE FROM Comments WHERE id = '{comment.ToString()}';";
            Program.app.Logger.LogDebug(sqlstr);
            int ret = 0;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return -1;
        }
    }

    public int RemoveThread(Guid thread)
    {
        try
        {
            string sqlstr = $"DELETE FROM Threads WHERE id = '{thread.ToString()}';";
            Program.app.Logger.LogDebug(sqlstr);
            int ret = 0;
            using (var con = new NpgsqlConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var command = new NpgsqlCommand(sqlstr, con))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message);
            return -1;
        }
    }
}
