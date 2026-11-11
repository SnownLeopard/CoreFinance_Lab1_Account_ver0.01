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
            db.CreateTablesIfNotExist();

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
