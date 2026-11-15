using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreFinance_Lab1_Account
{
    public class BankServerSingleton
    {
        // Потокобезопасный Singleton (Одиночка)
        private static readonly BankServerSingleton _instance = new BankServerSingleton();
        public static BankServerSingleton Instance => _instance;

        // Потокобезопасный кэш счетов в оперативной памяти (ОЗУ). Ключ: Id счёта
        private readonly ConcurrentDictionary<int, BankAccount> _cache = new ConcurrentDictionary<int, BankAccount>();

        private BankServerSingleton() { }

        // ============================================================
        // 1. МЕТОД GET: Получение счёта (Сначала ОЗУ, при промахе — БД)
        // ============================================================
        public BankAccount GetAccount(int accountId, IAccountRepository repository)
        {
            // Проверяем, есть ли счёт уже в оперативной памяти
            if (_cache.TryGetValue(accountId, out BankAccount cachedAccount))
            {
                return cachedAccount; // Найдено в ОЗУ! В базу данных не идём
            }

            // Если в ОЗУ пусто ("холодный старт") — загружаем из хранилища (MS SQL или файл)
            List<BankAccount> allAccounts = repository.LoadAccounts();
            BankAccount dbAccount = allAccounts.FirstOrDefault(a => a.Id == accountId);

            if (dbAccount != null)
            {
                // Сохраняем в кэш ОЗУ, чтобы при следующих запросах отдавать мгновенно
                _cache.TryAdd(accountId, dbAccount);
            }

            return dbAccount;
        }

        // ============================================================
        // 2. СПИСАНИЕ ДЕНЕГ: Атомарная связка (ОЗУ + БД с Оптимистической блокировкой)
        // ============================================================
        public bool DebitMoney(int accountId, double amount, IAccountRepository repository)
        {
            // Шаг А: Получаем счёт из ОЗУ (или подтягиваем из БД)
            BankAccount account = GetAccount(accountId, repository);
            if (account == null)
            {
                throw new ArgumentException($"Счёт с ID {accountId} не найден в системе.");
            }

            // Запоминаем состояние до операции на случай сбоя базы данных
            double balanceBefore = account.Balance;

            try
            {
                // Вызываем ваш родной метод списания из BankAccount (с проверкой всех лимитов!)
                account.Debit(amount);

                // Шаг Б: Фиксируем изменение в MS SQL Server с проверкой версии (Version)
                repository.UpdateAccountWithLock(account);

                return true; // Успешно списано в ОЗУ и запечатано в БД
            }
            catch (Exception)
            {
                // ОТКАТ (Rollback): Если сработала блокировка версий или пропала связь с БД,
                // возвращаем точный исходный баланс в ОЗУ, чтобы память не врала!
                double difference = balanceBefore - account.Balance;
                if (difference > 0)
                {
                    account.Credit(difference);
                }

                // Удаляем устаревший объект из кэша, чтобы при следующем запросе загрузить свежий из БД
                _cache.TryRemove(accountId, out _);

                throw; // Пробрасываем ошибку наверх (в интерфейс или автотест)
            }
        }

        // Вспомогательный метод для очистки кэша (полезно для автотестов)
        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
