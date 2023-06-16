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
        using (var command = new NpgsqlCommand(sqlstr, this._connection))
        {
            return (Guid?)command.ExecuteScalar();
        }
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
            string sqlstr = $"INSERT INTO Users Values ('{Guid.NewGuid().ToString()}', '{username}', '{hashPassword}', '{DateTime.Now.ToUniversalTime().ToString()}', '{email}', 'normal')";
            Program.app.Logger.LogWarning(sqlstr);
            using (var command = new NpgsqlCommand(sqlstr, this._connection))
            {
                return command.ExecuteNonQuery();
            }
        }
        catch (Exception)
        {
            return -1;
        }
    }
}
