using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
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
        public void UpdateAccountWithLock(BankAccount account)
        {

        }
    }
}