using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    public class AccountFactory : IAccountFactory
    {
        private DatabaseManager dbManager = new DatabaseManager();
        private static AccountFactory instance;
        private ILogger _logger;

        private AccountFactory() { }

        public static AccountFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountFactory();
                }
                return instance;
            }
        }
        public void Initialize(ILogger loggerService)
        {
            this._logger = loggerService;
        }

        public BankAccount CreateAccount(
            string accountType,
            string lastName,
            string firstName,
            string middleName,
            DateTime birthDate,
            double initialBalance)
        {
            BankAccount tempAccount = null;

            if (accountType == "CreditAccount")
            {
                tempAccount = new CreditAccount(lastName, firstName, middleName, birthDate, initialBalance);
            }
            else if (accountType == "DebitAccount")
            {
                tempAccount = new DebitAccount(lastName, firstName, middleName, birthDate, initialBalance);
            }
            else if (accountType == "VipCreditAccount")
            {
                tempAccount = new VipCreditAccount(lastName, firstName, middleName, birthDate, initialBalance);
            }
            else
            {
                throw new ArgumentException($"Критическая ошибка: Неизвестный тип счета '{accountType}'");
            }

            DateTime today = DateTime.Today;
            int clientAge = today.Year - tempAccount.BirthDate.Year;

            if (tempAccount.BirthDate.Date > today.AddYears(-clientAge)) clientAge--;
            
            if (accountType == "VipCreditAccount" && clientAge < 21)
            {
                throw new InvalidOperationException($"Операция отклонена службой безопасности! Клиенту {lastName} всего {clientAge} года (лет). Открытие VIP-счетов разрешено строго с 21 года!");
            }

            if (_logger != null)
            {
                _logger.Log($"[СИСТЕМА] Фабрика успешно сгенерировала объект типа {accountType} для клиента {lastName}. Возраст: {clientAge} г.");
            }

            return tempAccount;

            
        }
    }
}
