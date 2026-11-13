using System.Collections.Generic;

namespace CoreFinance_Lab1_Account
{
    public interface IAccountRepository
    {
        void Initialize();
        void SaveAccount(string accountType, BankAccount account);
        List<BankAccount> LoadAccounts();
    }
}