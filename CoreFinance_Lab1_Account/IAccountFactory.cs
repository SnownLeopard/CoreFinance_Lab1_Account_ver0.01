using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    public interface IAccountFactory
    {
        BankAccount CreateAccount(
            string accountType,
            string lastName,
            string firstName,
            string midleName,
            DateTime birthDate,
            double initialBalance
            );
    }
}
