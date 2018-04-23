using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace DataAccessLibrary
{
    public static class DataAccess
    {
        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();

                string tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS ItemList (ID VARCHAR(50) PRIMARY KEY, " +
                    "TITLE VARCHAR(20) NOT NULL, " +
                    "DES VARCHAR(50) NOT NULL, " +
                    "DATE VARCHAR(15) NOT NULL," +
                    "CHECKED INT NOT NULL," +
                    "PIC TEXT)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();
                db.Close();
            }
        }
        public static void AddData(string id, string title, string des, string date, int check, string pic)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                insertCommand.CommandText = "INSERT INTO ItemList VALUES (@id, @title, @des, @date, @check, @pic);";
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@title", title);
                insertCommand.Parameters.AddWithValue("@des", des);
                insertCommand.Parameters.AddWithValue("@date", date);
                insertCommand.Parameters.AddWithValue("@check", check);
                insertCommand.Parameters.AddWithValue("@pic", pic);
                insertCommand.ExecuteReader();
                db.Close();
            }
        }
        public static void UpdateData(string id, string title, string des, string date, int check, string pic)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;
                updateCommand.CommandText = "UPDATE ItemList SET TITLE = @title," +
                    "DES = @des, DATE = @date, CHECKED = @check, PIC = @pic WHERE ID = @id";
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@title", title);
                updateCommand.Parameters.AddWithValue("@des", des);
                updateCommand.Parameters.AddWithValue("@date", date);
                updateCommand.Parameters.AddWithValue("@check", check);
                updateCommand.Parameters.AddWithValue("@pic", pic);
                updateCommand.ExecuteReader();
                db.Close();
            }
        }
        public static void UpdateData(string id, string title, string des, string date, string pic)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;
                updateCommand.CommandText = "UPDATE ItemList SET TITLE = @title," +
                    "DES = @des, DATE = @date, PIC = @pic WHERE ID = @id";
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@title", title);
                updateCommand.Parameters.AddWithValue("@des", des);
                updateCommand.Parameters.AddWithValue("@date", date);
                updateCommand.Parameters.AddWithValue("@pic", pic);
                updateCommand.ExecuteReader();
                db.Close();
            }
        }
        public static void UpdateData(string id, int check)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;
                updateCommand.CommandText = "UPDATE ItemList SET CHECKED = @check WHERE ID = @id";
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@check", check);
                updateCommand.ExecuteReader();
                db.Close();
            }
        }
        public static List<string> search(string message)
        {
            List<string> entries = new List<string>();
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                string mess = "%" + message + "%";
                SqliteCommand searchCommand = new SqliteCommand("SELECT * FROM ItemList " +
                    "WHERE TITLE LIKE @mess OR DES LIKE @mess OR DATE LIKE @mess", db);
                searchCommand.Parameters.AddWithValue("@mess", mess);
                SqliteDataReader query = searchCommand.ExecuteReader();
                while (query.Read())
                {
                    entries.Add(query.GetString(1));
                    entries.Add(query.GetString(2));
                    entries.Add(query.GetString(3));
                    entries.Add(query.GetString(4));
                }
                db.Close();
            }
            return entries;
        }
        public static void DeleteData(string id)
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();
                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;
                deleteCommand.CommandText = "DELETE FROM ItemList WHERE ID = @id";
                deleteCommand.Parameters.AddWithValue("@id", id);
                deleteCommand.ExecuteReader();
                db.Close();
            }
        }
        public static List<string> GetData()
        {
            List<string> entries = new List<string>();

            using (SqliteConnection db =
                new SqliteConnection("Filename=MyListItem.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * from ItemList", db);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                    entries.Add(query.GetString(1));
                    entries.Add(query.GetString(2));
                    entries.Add(query.GetString(3));
                    entries.Add(query.GetString(4));
                    entries.Add(query.GetString(5));
                }
                db.Close();
            }
            return entries;
        }
    }
}
