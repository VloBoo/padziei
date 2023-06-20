using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

class Database
{
    private static Database? HINSTANCE;
    private NpgsqlConnection _connection;
    private Database()
    {
        const string CONNECTION_STRING = "Server=localhost;Port=5432;Database=padziei;User Id=postgres;Password=postgres1;";
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
        string sqlstr = $"SELECT id FROM Users WHERE Users.username = '{username}'";
        Guid? ret;
        using (var command = new NpgsqlCommand(sqlstr, this._connection))
        {
            ret = (Guid?)command.ExecuteScalar();
        }
        return ret;
    }

    public int CreateUser(string username, string password, string email)
    {
        string hashPassword = password;
        using (var sha256 = SHA256.Create())
        {
            hashPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        try
        {
            string sqlstr = $"INSERT INTO Users VALUES ('{Guid.NewGuid().ToString()}', '{username}', '{hashPassword}', '{DateTime.Now.ToUniversalTime().ToString()}', '{email}', 'normal')";
            Program.app.Logger.LogDebug(sqlstr);
            int ret = 0;
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
            {
                ret = command.ExecuteNonQuery();
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
            string sqlstr = $"SELECT id FROM Users WHERE username = '{username}' AND password = '{hashPassword}'";
            Program.app.Logger.LogDebug(sqlstr);
            Guid? guid;
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
            {
                guid = (Guid?)command.ExecuteScalar();
            }

            if (guid is null)
            {
                return null;
            }
            Guid token_guid = Guid.NewGuid();
            sqlstr = $"INSERT INTO tokens VALUES ('{token_guid.ToString()}', '{guid.ToString()}', '{DateTime.Now.ToUniversalTime().ToString()}', '30 day')";
            int i = 0;
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
            {
                i = command.ExecuteNonQuery();
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
            string sqlstr = $"SELECT user_id FROM tokens WHERE id = '{userToken.ToString()}'";
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
            {
                ret = (Guid?)command.ExecuteScalar();
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
            string sqlstr = $"SELECT * FROM Users WHERE id = '{userGuid.ToString()}'";
            string outt = "";
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
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
            return outt;
        }
        catch (Exception e)
        {
            Program.app.Logger.LogError(e.Message + "\n" + e.StackTrace);
            return null;
        }
    }
}
