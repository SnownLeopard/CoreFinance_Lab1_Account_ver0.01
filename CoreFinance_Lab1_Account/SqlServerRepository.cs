using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CoreFinance_Lab1_Account
{
    public class SqlServerRepository : IAccountRepository
    {
        private string masterConnStr = @"Server=.\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
        private string dbConnStr = @"Server=.\SQLEXPRESS;Database=CoreFinanceBankDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public void Initialize()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(masterConnStr))
                {
                    conn.Open();
                    string createDb = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CoreFinanceBankDB') CREATE DATABASE CoreFinanceBankDB;";
                    using (SqlCommand cmd = new SqlCommand(createDb, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection conn = new SqlConnection(dbConnStr))
                {
                    conn.Open();
                    string createTable = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
                        CREATE TABLE Accounts (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            AccountType NVARCHAR(50) NOT NULL,
                            LastName NVARCHAR(100) NOT NULL,
                            FirstName NVARCHAR(100) NOT NULL,
                            BirthDate DATE NOT NULL,
                            Balance FLOAT NOT NULL
                        );";
                    using (SqlCommand cmd = new SqlCommand(createTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка подключения к службе MS SQL Server: {ex.Message}", "Ошибка СУБД");
            }
        }

        public void SaveAccount(string accountType, BankAccount account)
        {
            if (account == null) return;
            string query = "INSERT INTO Accounts (AccountType, LastName, FirstName, BirthDate, Balance) VALUES (@Type, @Last, @First, @Birth, @Bal);";

            using (SqlConnection conn = new SqlConnection(dbConnStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Type", accountType);
                    cmd.Parameters.AddWithValue("@Last", account.LastNameProperty);
                    cmd.Parameters.AddWithValue("@First", account.FirstNameProperty);
                    cmd.Parameters.AddWithValue("@Birth", account.BirthDate);
                    cmd.Parameters.AddWithValue("@Bal", account.Balance);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<BankAccount> LoadAccounts()
        {
            List<BankAccount> list = new List<BankAccount>();
            string query = "SELECT AccountType, LastName, FirstName, BirthDate, Balance FROM Accounts;";

            using (SqlConnection conn = new SqlConnection(dbConnStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BankAccount acc = AccountFactory.Instance.CreateAccount(
                            reader["AccountType"].ToString(),
                            reader["LastName"].ToString(),
                            reader["FirstName"].ToString(),
                            "",
                            Convert.ToDateTime(reader["BirthDate"]),
                            Convert.ToDouble(reader["Balance"])
                        );
                        list.Add(acc);
                    }
                }
            }
            return list;
        }
    }
}