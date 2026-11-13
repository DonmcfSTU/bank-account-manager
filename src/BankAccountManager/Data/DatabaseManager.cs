/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: DatabaseManager is the only class that talks to the SQLite
 *          database (bank.db). It creates the tables, saves the sample data
 *          the first time the program runs, loads everything back into
 *          Bank/Customer/Account/Transaction objects on startup, and performs
 *          every CRUD operation:
 *            Create - InsertCustomer(), InsertAccount()
 *            Read   - LoadBank(), GetTableCounts(), GetCustomerRows(),
 *                     GetAccountRows(), GetTransactionRows()
 *            Update - UpdateCustomer(), SaveAccountActivity()
 *            Delete - DeleteAccount(), DeleteCustomer()
 *          Final version: added the RetiredIds table. When an account or
 *          customer is deleted, its number/ID is recorded there so it is
 *          never reused (real banks don't reuse account numbers either).
 *          Every SQL command uses parameters (@name) instead of gluing user
 *          input into the SQL string, which prevents SQL injection.
 *
 *          Tables (one table per main class):
 *            Customers    - one row per Customer (the Address is stored in
 *                           the same row because each customer has exactly one)
 *            Accounts     - one row per Account. All three account types share
 *                           this table; AccountType says which class to build,
 *                           and the type-specific columns are NULL when they
 *                           don't apply (e.g. MonthlyFee for a savings account).
 *            Transactions - one row per Transaction, linked to its account.
 *            RetiredIds   - account numbers and customer IDs that were deleted.
 */

using System.Globalization;
using BankAccountManager.Models;
using Microsoft.Data.Sqlite;

namespace BankAccountManager.Data
{
    public class DatabaseManager
    {
        // ACCESS SPECIFIERS - private: the file name and connection string are
        // details of this class. Nothing else in the program needs them.
        private const string DefaultFileName = "bank.db";
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _connectionString;

        // Public read-only so the welcome screen can show which file is used.
        public string FileName { get; }

        // CONSTRUCTOR (parameterized): lets a different database file be used
        // (for example, a separate test database).
        public DatabaseManager(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A database file name is required.");
            }
            FileName = fileName.Trim();
            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = FileName,
                ForeignKeys = true          // turns on the FOREIGN KEY rules below
            }.ToString();
        }

        // CONSTRUCTOR (default): chains to the one above with "bank.db".
        public DatabaseManager() : this(DefaultFileName)
        {
        }

        // ------------------------------------------------------------------
        // SETUP
        // ------------------------------------------------------------------

        // Creates the three tables if they don't exist yet. Returns true when
        // the database has no customers (a brand-new database).
        public bool InitializeDatabase()
        {
            using SqliteConnection connection = OpenConnection();
            Execute(connection, null, @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerId  INTEGER PRIMARY KEY,
                    FirstName   TEXT NOT NULL,
                    LastName    TEXT NOT NULL,
                    Phone       TEXT NOT NULL,
                    Email       TEXT NOT NULL,
                    Street      TEXT NOT NULL,
                    City        TEXT NOT NULL,
                    State       TEXT NOT NULL CHECK (length(State) = 2),
                    ZipCode     TEXT NOT NULL CHECK (length(ZipCode) = 5)
                );
                CREATE TABLE IF NOT EXISTS Accounts (
                    AccountNumber   TEXT PRIMARY KEY,
                    CustomerId      INTEGER NOT NULL
                                    REFERENCES Customers(CustomerId) ON DELETE RESTRICT,
                    AccountType     TEXT NOT NULL
                                    CHECK (AccountType IN ('Checking', 'Savings', 'Money Market')),
                    Balance         NUMERIC NOT NULL,
                    DateOpened      TEXT NOT NULL,
                    MonthlyFee      NUMERIC,
                    OverdraftLimit  NUMERIC,
                    InterestRate    NUMERIC,
                    MinimumBalance  NUMERIC
                );
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId   INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccountNumber   TEXT NOT NULL
                                    REFERENCES Accounts(AccountNumber) ON DELETE CASCADE,
                    TransDate       TEXT NOT NULL,
                    TransType       TEXT NOT NULL,
                    Amount          NUMERIC NOT NULL,
                    BalanceAfter    NUMERIC NOT NULL,
                    Description     TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS RetiredIds (
                    IdType      TEXT NOT NULL CHECK (IdType IN ('Account', 'Customer')),
                    IdValue     TEXT NOT NULL,
                    RetiredOn   TEXT NOT NULL,
                    PRIMARY KEY (IdType, IdValue)
                );");

            using SqliteCommand count = connection.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM Customers;";
            return Convert.ToInt32(count.ExecuteScalar()) == 0;
        }

        // CREATE (bulk): saves a whole Bank (used once, to store the sample
        // data in a new database). One SQL transaction means it's all or nothing.
        public void SaveNewBank(Bank bank)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction tx = connection.BeginTransaction();
            foreach (Customer customer in bank.Customers)
            {
                InsertCustomerRow(connection, tx, customer);
                foreach (Account account in customer.Accounts)
                {
                    InsertAccountRow(connection, tx, customer.CustomerId, account);
                    InsertTransactionRows(connection, tx, account, 0);
                }
            }
            tx.Commit();
        }

        // ------------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------------

        public void InsertCustomer(Customer customer)
        {
            using SqliteConnection connection = OpenConnection();
            InsertCustomerRow(connection, null, customer);
        }

        // Inserts the account row and its opening-deposit transaction together.
        public void InsertAccount(int customerId, Account account)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction tx = connection.BeginTransaction();
            InsertAccountRow(connection, tx, customerId, account);
            InsertTransactionRows(connection, tx, account, 0);
            tx.Commit();
        }

        // ------------------------------------------------------------------
        // READ
        // ------------------------------------------------------------------

        // Rebuilds the whole object model from the database. Each row becomes
        // an object through the classes' CONSTRUCTORS, so all of the same
        // validation runs on data coming out of the database.
        public Bank LoadBank()
        {
            var bank = new Bank();   // CONSTRUCTOR (default)
            var customers = new Dictionary<int, Customer>();

            using SqliteConnection connection = OpenConnection();

            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT CustomerId, FirstName, LastName, Phone, Email,
                                           Street, City, State, ZipCode
                                    FROM Customers ORDER BY CustomerId;";
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    // CONSTRUCTOR overload that builds the Address from its parts.
                    var customer = new Customer(r.GetInt32(0), r.GetString(1), r.GetString(2),
                        r.GetString(3), r.GetString(4), r.GetString(5), r.GetString(6),
                        r.GetString(7), r.GetString(8));
                    customers.Add(customer.CustomerId, customer);
                }
            }

            // Read every transaction first, grouped by account number.
            var history = new Dictionary<string, List<Transaction>>(StringComparer.OrdinalIgnoreCase);
            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT AccountNumber, TransDate, TransType, Amount, BalanceAfter, Description
                                    FROM Transactions ORDER BY TransactionId;";
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string number = r.GetString(0);
                    if (!history.ContainsKey(number))
                    {
                        history[number] = new List<Transaction>();
                    }
                    // Transaction's constructor is internal - DatabaseManager is
                    // in the same project, so it is allowed to call it.
                    history[number].Add(new Transaction(ParseDate(r.GetString(1)),
                        Enum.Parse<TransactionType>(r.GetString(2)), ReadMoney(r, 3),
                        ReadMoney(r, 4), r.GetString(5)));
                }
            }

            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT AccountNumber, CustomerId, AccountType, Balance, DateOpened,
                                           MonthlyFee, OverdraftLimit, InterestRate, MinimumBalance
                                    FROM Accounts ORDER BY CustomerId, rowid;";
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string number = r.GetString(0);
                    decimal balance = ReadMoney(r, 3);
                    DateTime opened = ParseDate(r.GetString(4));
                    history.TryGetValue(number, out List<Transaction>? saved);

                    // The constructor gets the ORIGINAL opening deposit (the
                    // first transaction), because the current balance can be
                    // negative (overdrawn checking) and the constructor rightly
                    // refuses a negative opening balance. RestoreHistory() then
                    // puts back the current balance.
                    decimal opening = saved != null && saved.Count > 0 ? saved[0].Amount : Math.Max(balance, 0m);

                    // The AccountType column decides which concrete class to
                    // build. Each one uses its FULL constructor so the stored
                    // fee, rate, and minimum are kept exactly.
                    Account account = r.GetString(2) switch
                    {
                        "Checking" => new CheckingAccount(number, opening, opened,
                                          ReadMoney(r, 5), ReadMoney(r, 6)),
                        "Savings" => new SavingsAccount(number, opening, opened, ReadRate(r, 7)),
                        _ => new MoneyMarketAccount(number, opening, opened,
                                          ReadRate(r, 7), ReadMoney(r, 8))
                    };

                    if (saved != null)
                    {
                        // internal method on Account: replaces the opening
                        // transaction the constructor made with the real history.
                        account.RestoreHistory(balance, saved);
                    }
                    customers[r.GetInt32(1)].AddAccount(account);
                }
            }

            foreach (Customer customer in customers.Values)
            {
                bank.AddCustomer(customer);
            }

            // Numbers and IDs from deleted records are reserved so they are
            // never handed out again.
            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT IdType, IdValue FROM RetiredIds;";
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    if (r.GetString(0) == "Account")
                    {
                        bank.ReserveAccountNumber(r.GetString(1));
                    }
                    else if (int.TryParse(r.GetString(1), out int id))
                    {
                        bank.ReserveCustomerId(id);
                    }
                }
            }
            return bank;
        }

        // READ: row counts for the "database records" screen.
        public (int Customers, int Accounts, int Transactions) GetTableCounts()
        {
            using SqliteConnection connection = OpenConnection();
            return (CountRows(connection, "Customers"), CountRows(connection, "Accounts"),
                    CountRows(connection, "Transactions"));
        }

        // READ: raw rows straight from each table (SELECT), so the user can see
        // exactly what is stored in the database file.
        public List<string[]> GetCustomerRows()
        {
            return Query(@"SELECT CustomerId, FirstName || ' ' || LastName, Phone, Email,
                                  Street || ', ' || City || ', ' || State || ' ' || ZipCode
                           FROM Customers ORDER BY CustomerId;", null);
        }

        public List<string[]> GetAccountRows()
        {
            return Query(@"SELECT AccountNumber, CustomerId, AccountType, printf('%.2f', Balance),
                                  substr(DateOpened, 1, 10),
                                  CASE WHEN MonthlyFee IS NULL THEN 'NULL' ELSE printf('%.2f', MonthlyFee) END,
                                  CASE WHEN OverdraftLimit IS NULL THEN 'NULL' ELSE printf('%.2f', OverdraftLimit) END,
                                  CASE WHEN InterestRate IS NULL THEN 'NULL' ELSE printf('%.4f', InterestRate) END,
                                  CASE WHEN MinimumBalance IS NULL THEN 'NULL' ELSE printf('%.2f', MinimumBalance) END
                           FROM Accounts ORDER BY CustomerId, rowid;", null);
        }

        public List<string[]> GetRetiredRows()
        {
            return Query(@"SELECT IdType, IdValue, substr(RetiredOn, 1, 16)
                           FROM RetiredIds ORDER BY RetiredOn, IdType, IdValue;", null);
        }

        // READ with a WHERE clause: transactions for one account number.
        public List<string[]> GetTransactionRows(string accountNumber)
        {
            return Query(@"SELECT TransactionId, substr(TransDate, 1, 16), TransType,
                                  printf('%.2f', Amount), printf('%.2f', BalanceAfter), Description
                           FROM Transactions
                           WHERE AccountNumber = @number COLLATE NOCASE
                           ORDER BY TransactionId;",
                         cmd => cmd.Parameters.AddWithValue("@number", accountNumber.Trim()));
        }

        // ------------------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------------------

        public void UpdateCustomer(Customer customer)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Customers
                                SET FirstName = @first, LastName = @last, Phone = @phone, Email = @email,
                                    Street = @street, City = @city, State = @state, ZipCode = @zip
                                WHERE CustomerId = @id;";
            AddCustomerParameters(cmd, customer);
            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException($"Customer {customer.CustomerId} is not in the database.");
            }
        }

        // UPDATE: saves an account's new balance and adds any transactions that
        // aren't in the database yet (deposits, withdrawals, fees, interest).
        public void SaveAccountActivity(IEnumerable<Account> accounts)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction tx = connection.BeginTransaction();
            foreach (Account account in accounts)
            {
                using SqliteCommand cmd = connection.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "UPDATE Accounts SET Balance = @balance WHERE AccountNumber = @number;";
                cmd.Parameters.AddWithValue("@balance", account.Balance);
                cmd.Parameters.AddWithValue("@number", account.AccountNumber);
                cmd.ExecuteNonQuery();

                using SqliteCommand count = connection.CreateCommand();
                count.Transaction = tx;
                count.CommandText = "SELECT COUNT(*) FROM Transactions WHERE AccountNumber = @number;";
                count.Parameters.AddWithValue("@number", account.AccountNumber);
                int alreadySaved = Convert.ToInt32(count.ExecuteScalar());
                InsertTransactionRows(connection, tx, account, alreadySaved);
            }
            tx.Commit();
        }

        // Overload for a single account (deposit or withdrawal).
        public void SaveAccountActivity(Account account)
        {
            SaveAccountActivity(new[] { account });
        }

        // ------------------------------------------------------------------
        // DELETE
        // ------------------------------------------------------------------

        // Deletes the account row. ON DELETE CASCADE removes its transactions too.
        // The account number is also recorded in RetiredIds (same SQL
        // transaction) so it is never reused.
        public void DeleteAccount(string accountNumber)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction tx = connection.BeginTransaction();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM Accounts WHERE AccountNumber = @number;";
            cmd.Parameters.AddWithValue("@number", accountNumber);
            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException($"Account {accountNumber} is not in the database.");
            }
            RetireId(connection, tx, "Account", accountNumber);
            tx.Commit();
        }

        // Deletes a customer. ON DELETE RESTRICT makes SQLite refuse this if the
        // customer still has accounts, as a second safety check behind Bank.
        public void DeleteCustomer(int customerId)
        {
            using SqliteConnection connection = OpenConnection();
            using SqliteTransaction tx = connection.BeginTransaction();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM Customers WHERE CustomerId = @id;";
            cmd.Parameters.AddWithValue("@id", customerId);
            if (cmd.ExecuteNonQuery() != 1)
            {
                throw new InvalidOperationException($"Customer {customerId} is not in the database.");
            }
            RetireId(connection, tx, "Customer", customerId.ToString(CultureInfo.InvariantCulture));
            tx.Commit();
        }

        // ------------------------------------------------------------------
        // PRIVATE HELPERS (ACCESS SPECIFIER - private: only used in this class)
        // ------------------------------------------------------------------

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static void Execute(SqliteConnection connection, SqliteTransaction? tx, string sql)
        {
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static int CountRows(SqliteConnection connection, string table)
        {
            // The table name comes from this class only (never from the user),
            // so it is safe to put in the SQL string.
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {table};";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private List<string[]> Query(string sql, Action<SqliteCommand>? addParameters)
        {
            var rows = new List<string[]>();
            using SqliteConnection connection = OpenConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            addParameters?.Invoke(cmd);
            using SqliteDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                var row = new string[r.FieldCount];
                for (int i = 0; i < r.FieldCount; i++)
                {
                    row[i] = r.IsDBNull(i) ? "NULL" : Convert.ToString(r.GetValue(i), CultureInfo.InvariantCulture) ?? "";
                }
                rows.Add(row);
            }
            return rows;
        }

        private static void InsertCustomerRow(SqliteConnection connection, SqliteTransaction? tx, Customer customer)
        {
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT INTO Customers
                                    (CustomerId, FirstName, LastName, Phone, Email, Street, City, State, ZipCode)
                                VALUES (@id, @first, @last, @phone, @email, @street, @city, @state, @zip);";
            AddCustomerParameters(cmd, customer);
            cmd.ExecuteNonQuery();
        }

        private static void AddCustomerParameters(SqliteCommand cmd, Customer c)
        {
            cmd.Parameters.AddWithValue("@id", c.CustomerId);
            cmd.Parameters.AddWithValue("@first", c.FirstName);
            cmd.Parameters.AddWithValue("@last", c.LastName);
            cmd.Parameters.AddWithValue("@phone", c.Phone);
            cmd.Parameters.AddWithValue("@email", c.Email);
            cmd.Parameters.AddWithValue("@street", c.MailingAddress.Street);
            cmd.Parameters.AddWithValue("@city", c.MailingAddress.City);
            cmd.Parameters.AddWithValue("@state", c.MailingAddress.State);
            cmd.Parameters.AddWithValue("@zip", c.MailingAddress.ZipCode);
        }

        private static void InsertAccountRow(SqliteConnection connection, SqliteTransaction? tx,
                                             int customerId, Account account)
        {
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT INTO Accounts
                                    (AccountNumber, CustomerId, AccountType, Balance, DateOpened,
                                     MonthlyFee, OverdraftLimit, InterestRate, MinimumBalance)
                                VALUES (@number, @customer, @type, @balance, @opened,
                                        @fee, @overdraft, @rate, @minimum);";
            cmd.Parameters.AddWithValue("@number", account.AccountNumber);
            cmd.Parameters.AddWithValue("@customer", customerId);
            cmd.Parameters.AddWithValue("@type", account.AccountType);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@opened", account.DateOpened.ToString(DateFormat, CultureInfo.InvariantCulture));

            // Type-specific columns: NULL when the column doesn't apply.
            object fee = DBNull.Value, overdraft = DBNull.Value, rate = DBNull.Value, minimum = DBNull.Value;
            if (account is CheckingAccount checking)
            {
                fee = checking.MonthlyFee;
                overdraft = checking.OverdraftLimit;
            }
            if (account is SavingsAccount savings)        // also true for Money Market
            {
                rate = savings.InterestRate;
            }
            if (account is MoneyMarketAccount moneyMarket)
            {
                minimum = moneyMarket.MinimumBalance;
            }
            cmd.Parameters.AddWithValue("@fee", fee);
            cmd.Parameters.AddWithValue("@overdraft", overdraft);
            cmd.Parameters.AddWithValue("@rate", rate);
            cmd.Parameters.AddWithValue("@minimum", minimum);
            cmd.ExecuteNonQuery();
        }

        // Inserts the account's transactions starting at position "startIndex"
        // (the ones that are already saved are skipped).
        private static void InsertTransactionRows(SqliteConnection connection, SqliteTransaction tx,
                                                  Account account, int startIndex)
        {
            for (int i = startIndex; i < account.Transactions.Count; i++)
            {
                Transaction t = account.Transactions[i];
                using SqliteCommand cmd = connection.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"INSERT INTO Transactions
                                        (AccountNumber, TransDate, TransType, Amount, BalanceAfter, Description)
                                    VALUES (@number, @date, @type, @amount, @after, @description);";
                cmd.Parameters.AddWithValue("@number", account.AccountNumber);
                cmd.Parameters.AddWithValue("@date", t.Date.ToString(DateFormat, CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@type", t.Type.ToString());
                cmd.Parameters.AddWithValue("@amount", t.Amount);
                cmd.Parameters.AddWithValue("@after", t.BalanceAfter);
                cmd.Parameters.AddWithValue("@description", t.Description);
                cmd.ExecuteNonQuery();
            }
        }

        private static void RetireId(SqliteConnection connection, SqliteTransaction tx, string type, string value)
        {
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT OR IGNORE INTO RetiredIds (IdType, IdValue, RetiredOn)
                                VALUES (@type, @value, @date);";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }

        private static DateTime ParseDate(string text)
        {
            return DateTime.ParseExact(text, DateFormat, CultureInfo.InvariantCulture);
        }

        // SQLite stores NUMERIC values as REAL, so they are rounded back to
        // cents when they are read.
        private static decimal ReadMoney(SqliteDataReader r, int column)
        {
            return Math.Round(Convert.ToDecimal(r.GetDouble(column)), 2);
        }

        private static decimal ReadRate(SqliteDataReader r, int column)
        {
            return Math.Round(Convert.ToDecimal(r.GetDouble(column)), 4);
        }
    }
}
