/*
 * Name:    Donavan D. McFadden
 * Date:    12/06/2026 (updated for Week 4)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 4 - Database Interactions
 *
 * Purpose: ConsoleUI holds all of the methods that write to the screen or
 *          read from the keyboard - the header, welcome message, menu,
 *          tables, and input prompts. Keeping this in one class means the
 *          Models classes don't need to know anything about the console.
 *          Week 3: Week 3 header, menu options 8-12, transaction history
 *          table, and new prompt helpers for adding/updating information.
 *          Week 4: Week 4 header, database instructions in the welcome
 *          message, menu options 13-14, and ShowRows() for displaying rows
 *          read straight from the SQLite tables.
 */

using BankAccountManager.Interfaces;
using BankAccountManager.Models;

namespace BankAccountManager.UI
{
    // ACCESS SPECIFIERS: the class is public static. Methods that Program
    // needs are public; formatting helpers used only here are private.
    public static class ConsoleUI
    {
        private const int Width = 92;

        public static void ShowHeader()
        {
            Console.WriteLine(new string('=', Width));
            Console.WriteLine(Center("SDC320L Course Project - WEEK 4 (Phase 4)"));
            Console.WriteLine(Center("Database Interactions"));
            Console.WriteLine(Center("Bank Account Management Application"));
            Console.WriteLine(Center("Created by Donavan D. McFadden"));
            Console.WriteLine(new string('=', Width));
        }

        public static void ShowWelcome()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to the McFadden Community Bank account manager!");
            Console.WriteLine("\"A penny saved is a penny earned.\" - Benjamin Franklin (probably)");
            Console.WriteLine();
            Console.WriteLine("How to use this program:");
            Console.WriteLine("  - Type the number of a menu option and press Enter.");
            Console.WriteLine("  - Pick customers by Customer ID (example: 1003) and accounts by account");
            Console.WriteLine("    number (example: CHK-100101). The list is shown first so you can look it up.");
            Console.WriteLine("  - Everything is stored in an SQLite database file (bank.db). Every add,");
            Console.WriteLine("    deposit, withdrawal, update, and delete is saved right away, so your");
            Console.WriteLine("    data is still there the next time you start the program.");
            Console.WriteLine("  - Options 8-11 add customers, open and close accounts, and update customers.");
            Console.WriteLine("    When updating, press Enter on any field to keep its current value.");
            Console.WriteLine("  - Option 12 shows one account's history. Option 13 deletes a customer");
            Console.WriteLine("    (close their accounts first). Option 14 shows the raw database tables.");
            Console.WriteLine("  - Type Q (or Quit) at the main menu to exit.");
        }

        public static void ShowMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------- MAIN MENU ----------------------------");
            Console.WriteLine("  1. List all customers              7. Run month-end processing");
            Console.WriteLine("  2. List all accounts               8. Add a new customer");
            Console.WriteLine("  3. View one customer               9. Open a new account");
            Console.WriteLine("  4. Make a deposit                 10. Close (delete) an account");
            Console.WriteLine("  5. Make a withdrawal              11. Update customer information");
            Console.WriteLine("  6. Interest report                12. View account transaction history");
            Console.WriteLine("  Q. Quit                           13. Delete a customer");
            Console.WriteLine("                                    14. View database records (SQLite)");
            Console.WriteLine("-------------------------------------------------------------------");
        }

        public static void ShowCustomerTable(IEnumerable<Customer> customers)
        {
            Console.WriteLine();
            Console.WriteLine($"{"ID",-6}{"Name",-18}{"Phone",-16}{"City, State",-22}{"Accounts",9}{"Total Balance",16}");
            Console.WriteLine(new string('-', 87));
            foreach (Customer c in customers)
            {
                string cityState = $"{c.MailingAddress.City}, {c.MailingAddress.State}";
                Console.WriteLine($"{c.CustomerId,-6}{Fit(c.FullName, 17),-18}{c.Phone,-16}{Fit(cityState, 21),-22}{c.Accounts.Count,9}{c.GetTotalBalance(),16:C}");
            }
        }

        // Works for any mix of account types because they all inherit from
        // the abstract Account class.
        public static void ShowAccountTable(IEnumerable<Account> accounts)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Owner",-17}{"Opened",-12}{"Balance",13}  {"Details"}");
            Console.WriteLine(new string('-', Width));
            foreach (Account a in accounts)
            {
                Console.WriteLine($"{a.AccountNumber,-12}{a.AccountType,-14}{Fit(a.OwnerName, 16),-17}{a.DateOpened,-12:MM/dd/yyyy}{a.Balance,13:C}  {a.GetAccountDetails()}");
            }
        }

        public static void ShowCustomerDetails(Customer c)
        {
            Console.WriteLine();
            Console.WriteLine($"Customer #{c.CustomerId}: {c.FullName}");
            Console.WriteLine($"  Phone:   {c.Phone}");
            Console.WriteLine($"  Email:   {c.Email}");
            Console.WriteLine($"  Address: {c.MailingAddress}");
            if (c.Accounts.Count == 0)
            {
                Console.WriteLine("\n  (No open accounts)");
                return;
            }
            ShowAccountTable(c.Accounts);
            Console.WriteLine($"{"Total balance:",-67}{c.GetTotalBalance(),13:C}");
        }

        public static void ShowInterestReport(List<IInterestBearing> accounts)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Owner",-17}{"Balance",13}{"Rate",9}{"Est. Monthly Interest",24}");
            Console.WriteLine(new string('-', 89));
            decimal total = 0m;
            foreach (IInterestBearing item in accounts)
            {
                Account account = (Account)item;
                decimal interest = item.CalculateMonthlyInterest();
                total += interest;
                Console.WriteLine($"{account.AccountNumber,-12}{account.AccountType,-14}{Fit(account.OwnerName, 16),-17}{account.Balance,13:C}{item.InterestRate,9:P2}{interest,24:C}");
            }
            Console.WriteLine(new string('-', 89));
            Console.WriteLine($"{"Total estimated interest this month:",-65}{total,24:C}");
        }

        public static void ShowMonthEndResults(List<(Account Account, decimal Before, string Result)> results)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Before",13}{"After",13}  {"What happened"}");
            Console.WriteLine(new string('-', Width));
            foreach (var r in results)
            {
                Console.WriteLine($"{r.Account.AccountNumber,-12}{r.Account.AccountType,-14}{r.Before,13:C}{r.Account.Balance,13:C}  {r.Result}");
            }
        }

        public static void ShowTransactionHistory(Account account)
        {
            Console.WriteLine();
            Console.WriteLine($"Transaction history for {account.AccountNumber} ({account.AccountType}) - {account.OwnerName}");
            Console.WriteLine();
            Console.WriteLine($"{"Date",-12}{"Type",-17}{"Description",-33}{"Amount",13}{"Balance",14}");
            Console.WriteLine(new string('-', 89));
            foreach (Transaction t in account.Transactions)
            {
                Console.WriteLine($"{t.Date,-12:MM/dd/yyyy}{FormatType(t.Type),-17}{Fit(t.Description, 32),-33}{t.Amount,13:C}{t.BalanceAfter,14:C}");
            }
            Console.WriteLine(new string('-', 89));
            Console.WriteLine($"{"Current balance:",-75}{account.Balance,14:C}");
        }

        // Week 4: prints rows that came straight from a SELECT query. Each
        // column has a header and a width; long values are shortened to fit.
        public static void ShowRows(string title, (string Header, int Width)[] columns, List<string[]> rows)
        {
            Console.WriteLine();
            Console.WriteLine($"{title}   ({rows.Count} row{(rows.Count == 1 ? "" : "s")})");
            Console.WriteLine();
            int total = 0;
            foreach (var col in columns)
            {
                Console.Write(col.Header.PadRight(col.Width));
                total += col.Width;
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', total));
            foreach (string[] row in rows)
            {
                for (int i = 0; i < columns.Length && i < row.Length; i++)
                {
                    Console.Write(Fit(row[i], columns[i].Width - 1).PadRight(columns[i].Width));
                }
                Console.WriteLine();
            }
            if (rows.Count == 0)
            {
                Console.WriteLine("(no rows found)");
            }
        }

        public static string Prompt(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? string.Empty).Trim();
        }

        // Asks for a value and keeps asking until something is typed.
        public static string PromptRequired(string label)
        {
            while (true)
            {
                string value = Prompt($"  {label}: ");
                if (value.Length > 0)
                {
                    return value;
                }
                ShowError($"  {label} is required.");
            }
        }

        // Shows the current value in brackets; pressing Enter keeps it.
        public static string PromptWithDefault(string label, string currentValue)
        {
            string value = Prompt($"  {label} [{currentValue}]: ");
            return value.Length == 0 ? currentValue : value;
        }

        public static bool PromptYesNo(string message)
        {
            string answer = Prompt(message + " (Y/N): ").ToUpper();
            return answer == "Y" || answer == "YES";
        }

        public static decimal PromptForAmount(string message)
        {
            while (true)
            {
                string input = Prompt(message).Replace("$", "").Replace(",", "");
                if (decimal.TryParse(input, out decimal amount) && amount > 0)
                {
                    return Math.Round(amount, 2);
                }
                ShowError("Please enter a dollar amount greater than zero (example: 125.50).");
            }
        }

        public static void ClearScreen()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // Output is redirected (not a real console window) - nothing to clear.
            }
        }

        public static void ShowSuccess(string message) => WriteColor(message, ConsoleColor.Green);
        public static void ShowError(string message) => WriteColor(message, ConsoleColor.Red);

        public static void Pause()
        {
            Console.WriteLine();
            Console.Write("Press Enter to return to the main menu...");
            Console.ReadLine();
        }

        // ACCESS SPECIFIER - private: helpers only used inside ConsoleUI.
        private static void WriteColor(string message, ConsoleColor color)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }

        private static string Center(string text)
        {
            int padding = Math.Max(0, (Width - text.Length) / 2);
            return new string(' ', padding) + text;
        }

        // Turns an enum value like OpeningDeposit into "Opening Deposit".
        private static string FormatType(TransactionType type)
        {
            return type == TransactionType.OpeningDeposit ? "Opening Deposit" : type.ToString();
        }

        // Shortens long text so table columns stay lined up.
        private static string Fit(string text, int maxLength)
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 1) + "~";
        }
    }
}
