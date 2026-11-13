using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    /*public class DatabaseManager
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BankDatabase.mdf;Integrated Security=True;";

        public void CreateTablesIfNotExist()
        {
            string script = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
                BEGIN
                    CREATE TABLE Accounts (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        AccountType NVARCHAR(50) NOT NULL,
                        LastName NVARCHAR(100) NOT NULL,
                        FirstName NVARCHAR(100) NOT NULL,
                        BirthDate DATE NOT NULL,
                        Balance DOUBLE PRECISION NOT NULL
                    )
                END;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка инициализации SQL-таблиц: {ex.Message}", "БД Аврал");
                }
            }
        }

        public void SaveAccount(string accountType, BankAccount account)
        {
            if (account == null) return;

            string query = @"
                INSERT INTO Accounts (AccountType, LastName, FirstName, BirthDate, Balance) 
                VALUES (@AccountType, @LastName, @FirstName, @BirthDate, @Balance);";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountType", accountType);
                        command.Parameters.AddWithValue("@LastName", account.LastNameProperty);
                        command.Parameters.AddWithValue("@FirstName", account.FirstNameProperty);
                        command.Parameters.AddWithValue("@BirthDate", account.BirthDate);
                        command.Parameters.AddWithValue("@Balance", account.Balance);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка записи в SQL-базу: {ex.Message}", "БД Аврал");
                }
            }
        }

        public List<BankAccount> LoadAccounts()
        {
            List<BankAccount> accountList = new List<BankAccount>();
            string query = "SELECT AccountType, LastName, FirstName, BirthDate, Balance FROM Accounts;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string type = reader["AccountType"].ToString();
                                string lastName = reader["LastName"].ToString();
                                string firstName = reader["FirstName"].ToString();
                                DateTime birthDate = Convert.ToDateTime(reader["BirthDate"]);
                                double balance = Convert.ToDouble(reader["Balance"]);

                                BankAccount loadedAcc = AccountFactory.Instance.CreateAccount(
                                    type, lastName, firstName, "", birthDate, balance
                                );

                                accountList.Add(loadedAcc);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка чтения из SQL-базы: {ex.Message}", "БД Чтение Аврал");
                }
            }
            return accountList;
        }
    }*/

    public class DatabaseManager : IAccountRepository
    {
        // Стерильный текстовый файл прямо в папке запуска программы bin\Debug
        private string textFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BankAccountsData.txt");

        public void Initialize()
        {
            try
            {
                // Если файла еще нет на диске — просто создаем его пустым
                if (!File.Exists(textFilePath))
                {
                    using (StreamWriter sw = File.CreateText(textFilePath))
                    {
                        sw.WriteLine("# БОРТОВОЙ СЕЙФ ДАННЫХ БАНКА МЕДИЧИ #");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка инициализации текстового файла: {ex.Message}", "Аврал Диска");
            }
        }

        public void SaveAccount(string accountType, BankAccount account)
        {
            if (account == null) return;

            try
            {
                // Формируем строгую финтех-строку, разделенную вертикальной чертой
                string accountLine = $"{accountType}|{account.LastNameProperty}|{account.FirstNameProperty}|{account.BirthDate:yyyy-MM-dd}|{account.Balance}";

                // Дописываем строку в конец текстового файла на жёсткий диск
                File.AppendAllText(textFilePath, accountLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка записи в файл: {ex.Message}", "Аврал Записи");
            }
        }

        public List<BankAccount> LoadAccounts()
        {
            List<BankAccount> accountList = new List<BankAccount>();

            try
            {
                if (!File.Exists(textFilePath)) return accountList;

                // Считываем все строки из текстового файла лавиной
                string[] lines = File.ReadAllLines(textFilePath);

                foreach (string line in lines)
                {
                    // Пропускаем служебные заголовки или пустые строки
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    // Распиливаем строку по разделителю '|'
                    string[] parts = line.Split('|');
                    if (parts.Length < 5) continue;

                    string type = parts[0];
                    string lastName = parts[1];
                    string firstName = parts[2];
                    DateTime birthDate = Convert.ToDateTime(parts[3]);
                    double balance = Convert.ToDouble(parts[4]);

                    // Через нашу Абстрактную Фабрику возрождаем живой объект в оперативную память RAM
                    BankAccount loadedAcc = AccountFactory.Instance.CreateAccount(
                        type, lastName, firstName, "", birthDate, balance
                    );

                    accountList.Add(loadedAcc);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка чтения из текстового файла: {ex.Message}", "Аврал Чтения");
            }

            return accountList;
        }
    }
}