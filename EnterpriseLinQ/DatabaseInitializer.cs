using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace EnterpriseLinQ
{
    public static class DatabaseInitializer
    {
        public static string DbPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "enterprise.db"); }
        }

        private static SqliteConnection Open()
        {
            var connection = new SqliteConnection("Data Source=" + DbPath);
            connection.Open();
            return connection;
        }

        public static void Initialize()
        {
            using (var connection = Open())
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Categories(
 Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, DiscountRate REAL NOT NULL);
CREATE TABLE IF NOT EXISTS Products(
 Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, CategoryId INTEGER NOT NULL,
 Price REAL NOT NULL, Stock INTEGER NOT NULL,
 FOREIGN KEY(CategoryId) REFERENCES Categories(Id));
CREATE TABLE IF NOT EXISTS Sales(
 Id INTEGER PRIMARY KEY, ProductId INTEGER NOT NULL, CustomerName TEXT NOT NULL,
 Quantity INTEGER NOT NULL, SaleDate TEXT NOT NULL, Channel TEXT NOT NULL,
 FOREIGN KEY(ProductId) REFERENCES Products(Id));";
                cmd.ExecuteNonQuery();

                cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Categories;";
                long count = (long)cmd.ExecuteScalar();
                if (count > 0) return;

                using (var tx = connection.BeginTransaction())
                {
                    Insert(connection, tx, "INSERT INTO Categories VALUES (1,'Электроника',0.05),(2,'Книги',0.10),(3,'Дом',0.07),(4,'Спорт',0.03);");
                    Insert(connection, tx, @"INSERT INTO Products VALUES
(1,'Наушники',1,12000,30),(2,'Мышь',1,6500,45),(3,'Клавиатура',1,15000,20),
(4,'Монитор',1,78000,8),(5,'C# для начинающих',2,5500,40),
(6,'История Казахстана',2,4200,35),(7,'Лампа',3,3500,50),
(8,'Чайник',3,12500,15),(9,'Футбольный мяч',4,8000,25),
(10,'Гантели',4,11000,18);");
                    Insert(connection, tx, @"INSERT INTO Sales VALUES
(1,1,'Иванов И.И.',2,'2026-10-01','Online'),
(2,2,'Петров П.П.',1,'2026-10-01','Retail'),
(3,3,'Сидоров С.С.',1,'2026-10-02','Online'),
(4,4,'Ким А.А.',1,'2026-10-02','Retail'),
(5,5,'Иванов И.И.',2,'2026-10-03','Online'),
(6,6,'Алиева А.Б.',1,'2026-10-03','Retail'),
(7,7,'Петров П.П.',3,'2026-10-04','Online'),
(8,8,'Сергеев С.С.',1,'2026-10-04','Retail'),
(9,9,'Ким А.А.',2,'2026-10-05','Online'),
(10,10,'Алиева А.Б.',1,'2026-10-05','Retail'),
(11,1,'Сергеев С.С.',1,'2026-10-06','Retail'),
(12,2,'Иванов И.И.',2,'2026-10-06','Online'),
(13,3,'Петров П.П.',1,'2026-10-07','Retail'),
(14,4,'Алиева А.Б.',1,'2026-10-07','Online'),
(15,5,'Ким А.А.',1,'2026-10-08','Retail'),
(16,6,'Сергеев С.С.',2,'2026-10-08','Online'),
(17,7,'Иванов И.И.',1,'2026-10-09','Retail'),
(18,8,'Петров П.П.',2,'2026-10-09','Online'),
(19,9,'Сидоров С.С.',1,'2026-10-10','Retail'),
(20,10,'Ким А.А.',2,'2026-10-10','Online');");
                    tx.Commit();
                }
            }
        }

        private static void Insert(SqliteConnection c, SqliteTransaction tx, string sql)
        {
            var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        public static List<Category> LoadCategories()
        {
            var result = new List<Category>();
            using (var c = Open())
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Id,Name,DiscountRate FROM Categories ORDER BY Id";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(new Category(r.GetInt32(0), r.GetString(1), r.GetDouble(2)));
            }
            return result;
        }

        public static List<Product> LoadProducts()
        {
            var result = new List<Product>();
            using (var c = Open())
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Id,Name,CategoryId,Price,Stock FROM Products ORDER BY Id";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(new Product(r.GetInt32(0), r.GetString(1), r.GetInt32(2), r.GetDouble(3), r.GetInt32(4)));
            }
            return result;
        }

        public static List<Sale> LoadSales()
        {
            var result = new List<Sale>();
            using (var c = Open())
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT Id,ProductId,CustomerName,Quantity,SaleDate,Channel FROM Sales ORDER BY Id";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) result.Add(new Sale(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetInt32(3), r.GetString(4), r.GetString(5)));
            }
            return result;
        }
    }
}
