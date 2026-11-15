using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    [TestClass]
    public class AccountTests
    {
        [TestMethod]
        public void Test_VipCreditAccount_Limit_Border()
        {
            IAccountFactory factory = AccountFactory.Instance;
            BankAccount vipAcc = factory.CreateAccount(
                "VipCreditAccount", "Петров", "Максим", "Игоревич",
                new DateTime(1995, 5, 12), 100000
            );

            vipAcc.Debit(550000);
            Assert.AreEqual(-450000, vipAcc.Balance, "Критическая ошибка: Баланс VIP-карты рассчитан неверно!");
        }

        [TestMethod]
        public void Test_DebitAccount_Underage_Banned()
        {
            IAccountFactory factory = AccountFactory.Instance;

            try
            {
                factory.CreateAccount("DebitAccount", "Иванов", "Михаил", "Петрович", new DateTime(2010, 8, 15), 0);

                Assert.Fail("Критическая ошибка: Система пропустила несовершеннолетнего клиента без исключения!");
            }
            catch (InvalidOperationException)
            {

            }
        }


        [TestMethod]
        public void Test_VipAccount_Under21_Banned()
        {
            IAccountFactory factory = AccountFactory.Instance;

            try
            {
                factory.CreateAccount("VipCreditAccount", "Сидоров", "Алексей", "Сергеевич", new DateTime(2007, 4, 10), 50000);

                Assert.Fail("Критическая ошибка: Фабрика выдала VIP-лимит лицу моложе 21 года!");
            }
            catch (InvalidOperationException)
            {

            }
        }

        [TestMethod]
        public void Test_AccountFactory_Logger_Output()
        {
            IAccountFactory factory = AccountFactory.Instance;
            FakeLogger fakeLogger = new FakeLogger();

            AccountFactory.Instance.Initialize(fakeLogger);

            factory.CreateAccount("DebitAccount", "Иванов", "Иван", "Иванович", new DateTime(1990, 1, 1), 5000);

            Assert.IsNotNull(fakeLogger.LastMessage, "Критическая ошибка: Фабрика не отправила сообщение в логгер!");

            bool containsKeyword = fakeLogger.LastMessage.Contains("[СИСТЕМА] Фабрика успешно сгенерировала объект");
            Assert.IsTrue(containsKeyword, $"Критическая ошибка: Текст лога не соответствует ГОСТу! Было записано: {fakeLogger.LastMessage}");
        }

        [TestMethod]
        public void Test_DatabaseManager_SaveAndLoad_TextFile_Success()
        {
            DatabaseManager db = new DatabaseManager();
            db.Initialize();

            string testLastName = "Тестов_" + Guid.NewGuid().ToString().Substring(0, 5);
            BankAccount originalAccount = AccountFactory.Instance.CreateAccount(
                "DebitAccount", testLastName, "Владимир", "", new DateTime(1984, 11, 11), 5000
            );

            db.SaveAccount("DebitAccount", originalAccount);
            List<BankAccount> loadedAccounts = db.LoadAccounts();

            BankAccount foundAccount = loadedAccounts.Find(acc => acc.LastNameProperty == testLastName);

            Assert.IsNotNull(foundAccount, "Критическая ошибка: Сохраненный счет не найден в текстовом файле!");
            Assert.AreEqual(originalAccount.Balance, foundAccount.Balance, "Ошибка: Баланс считанного счета не совпадает с оригиналом!");
            Assert.AreEqual(originalAccount.FirstNameProperty, foundAccount.FirstNameProperty, "Ошибка: Имя клиента искажено при чтении с диска!");
        }

        [TestMethod]
        public void Test_MS_SQL_Server_SaveAndLoad()
        {
            IAccountRepository sqlRepo = new SqlServerRepository();
            sqlRepo.Initialize();

            string uniqueLastName = "ТестовSQL_" + Guid.NewGuid().ToString().Substring(0, 5);
            BankAccount testAccount = AccountFactory.Instance.CreateAccount(
                "DebitAccount", uniqueLastName, "Иван", "Иванович", new DateTime(1995, 5, 15), 25000
            );

            sqlRepo.SaveAccount("DebitAccount", testAccount);
            List<BankAccount> loadedAccounts = sqlRepo.LoadAccounts();

            BankAccount found = loadedAccounts.Find(a => a.LastNameProperty == uniqueLastName);

            Assert.IsNotNull(found, "Счёт не был найден в базе данных MS SQL Server после сохранения!");
            Assert.AreEqual(25000, found.Balance, "Баланс счёта в базе данных не совпадает с исходным!");
        }

        [TestMethod]
        public void Test_OptimisticLock_RaceConditionBlocked()
        {
            SqlServerRepository repo = new SqlServerRepository();
            repo.Initialize();

            string testName = "Иван_Race_" + Guid.NewGuid().ToString().Substring(0, 4);
            BankAccount initialAcc = AccountFactory.Instance.CreateAccount(
                "DebitAccount", testName, "Иван", "Иванович", new DateTime(1990, 1, 1), 100000);
            repo.SaveAccount("DebitAccount", initialAcc);

            BankAccount device1_Phone = repo.LoadAccounts().Find(a => a.LastNameProperty == testName);
            BankAccount device2_Web = repo.LoadAccounts().Find(a => a.LastNameProperty == testName);

            device1_Phone.Debit(100000);
            repo.UpdateAccountWithLock(device1_Phone);

            try
            {
                device2_Web.Credit(500);
                repo.UpdateAccountWithLock(device2_Web);

                Assert.Fail("Критическая ошибка: Система пропустила перезапись устаревшими данными без исключения!");
            }
            catch (InvalidOperationException)
            {

            }
        }

        [TestMethod]
        public void Test_BankServerSingleton_CacheAndDebit_Success()
        {
            // 1. Arrange: Очищаем кэш и создаём нового клиента в MS SQL Server
            BankServerSingleton.Instance.ClearCache();
            IAccountRepository repo = new SqlServerRepository();
            repo.Initialize();

            string uniqueName = "КэшТест_" + Guid.NewGuid().ToString().Substring(0, 4);
            BankAccount newAcc = AccountFactory.Instance.CreateAccount(
                "DebitAccount", uniqueName, "Анна", "Сергеевна", new DateTime(1992, 3, 10), 50000);
            repo.SaveAccount("DebitAccount", newAcc);

            // Получаем присвоенный базой ID этого счёта
            BankAccount savedInDb = repo.LoadAccounts().Find(a => a.LastNameProperty == uniqueName);
            int targetId = savedInDb.Id;

            // 2. Act:
            // Первый вызов GetAccount подтянет счёт из SQL в ОЗУ (ConcurrentDictionary)
            BankAccount fromDbToCache = BankServerSingleton.Instance.GetAccount(targetId, repo);

            // Списываем 15 000 руб. через наш сервер (обновится и кэш, и SQL-таблица)
            bool result = BankServerSingleton.Instance.DebitMoney(targetId, 15000, repo);

            // Второй вызов GetAccount возьмёт счёт уже мгновенно из кэша ОЗУ
            BankAccount fromCacheFast = BankServerSingleton.Instance.GetAccount(targetId, repo);

            // 3. Assert: Проверяем синхронизацию ОЗУ и базы данных
            Assert.IsTrue(result);
            Assert.AreEqual(35000, fromCacheFast.Balance, "Баланс в кэше ОЗУ должен стать 35 000 руб.!");

            BankAccount verifyInDb = repo.LoadAccounts().Find(a => a.Id == targetId);
            Assert.AreEqual(35000, verifyInDb.Balance, "Баланс в MS SQL Server должен синхронно стать 35 000 руб.!");
        }
    }

    public class FakeLogger : ILogger
    {
        public string LastMessage { get; private set; }

        public void Log(string message)
        {
            this.LastMessage = message;
        }
    }
}
