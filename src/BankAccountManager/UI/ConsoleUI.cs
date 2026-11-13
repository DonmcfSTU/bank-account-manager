/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: ConsoleUI holds all of the methods that write to the screen or
 *          read from the keyboard - the header, welcome message, menu,
 *          tables, and input prompts. Keeping this in one class means the
 *          Models classes don't have to know anything about the console,
 *          which should make the project easier to grow in later weeks.
 */

using BankAccountManager.Interfaces;
using BankAccountManager.Models;

namespace BankAccountManager.UI
{
    public static class ConsoleUI
    {
        private const int Width = 92;

        // Week indicator, assignment title, and my name.
        public static void ShowHeader()
        {
            Console.WriteLine(new string('=', Width));
            Console.WriteLine(Center("SDC320L Course Project - WEEK 2 (Phase 2)"));
            Console.WriteLine(Center("Software Design; Interfaces & Polymorphism"));
            Console.WriteLine(Center("Bank Account Management Application"));
            Console.WriteLine(Center("Created by Donavan D. McFadden"));
            Console.WriteLine(new string('=', Width));
        }

        // Welcome message with basic instructions.
        public static void ShowWelcome()
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to the McFadden Community Bank account manager!");
            Console.WriteLine("\"A penny saved is a penny earned.\" - Benjamin Franklin (probably)");
            Console.WriteLine();
            Console.WriteLine("How to use this program:");
            Console.WriteLine("  - Type the number of a menu option and press Enter.");
            Console.WriteLine("  - When asked for a customer, type the Customer ID shown in the customer list.");
            Console.WriteLine("  - When asked for an account, type the account number (example: CHK-100101).");
            Console.WriteLine("  - Option 6 shows only accounts that earn interest; option 7 runs");
            Console.WriteLine("    month-end fees and interest for every account.");
            Console.WriteLine("  - Type Q (or Quit) at the main menu to exit.");
        }

        public static void ShowMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------- MAIN MENU ----------------------------");
            Console.WriteLine("  1. List all customers");
            Console.WriteLine("  2. List all accounts for all customers");
            Console.WriteLine("  3. View one customer and their accounts");
            Console.WriteLine("  4. Make a deposit");
            Console.WriteLine("  5. Make a withdrawal");
            Console.WriteLine("  6. Interest report (interest-earning accounts)");
            Console.WriteLine("  7. Run month-end processing (fees and interest)");
            Console.WriteLine("  Q. Quit");
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("  (Add, remove, and update features are coming in later phases.)");
        }

        // Table of customers. Each row uses the Customer's composed
        // Address object to show the city and state.
        public static void ShowCustomerTable(List<Customer> customers)
        {
            Console.WriteLine();
            Console.WriteLine($"{"ID",-6}{"Name",-18}{"Phone",-16}{"City, State",-22}{"Accounts",9}{"Total Balance",16}");
            Console.WriteLine(new string('-', 87));
            foreach (Customer c in customers)
            {
                string cityState = $"{c.MailingAddress.City}, {c.MailingAddress.State}";
                Console.WriteLine($"{c.CustomerId,-6}{c.FullName,-18}{c.Phone,-16}{cityState,-22}{c.Accounts.Count,9}{c.GetTotalBalance(),16:C}");
            }
        }

        // Table of accounts. Works for any mix of Checking, Savings, and
        // Money Market accounts because they all inherit from Account.
        public static void ShowAccountTable(IEnumerable<Account> accounts)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Owner",-17}{"Opened",-12}{"Balance",13}  {"Details"}");
            Console.WriteLine(new string('-', Width));
            foreach (Account a in accounts)
            {
                Console.WriteLine($"{a.AccountNumber,-12}{a.AccountType,-14}{a.OwnerName,-17}{a.DateOpened,-12:MM/dd/yyyy}{a.Balance,13:C}  {a.GetAccountDetails()}");
            }
        }

        // POLYMORPHISM through the INTERFACE: this method only knows about
        // IInterestBearing, not about SavingsAccount or MoneyMarketAccount.
        // Each object runs its own CalculateMonthlyInterest() version.
        public static void ShowInterestReport(List<IInterestBearing> accounts)
        {
            Console.WriteLine();
            Console.WriteLine($"{"Account #",-12}{"Type",-14}{"Owner",-17}{"Balance",13}{"Rate",9}{"Est. Monthly Interest",24}");
            Console.WriteLine(new string('-', 89));
            decimal total = 0m;
            foreach (IInterestBearing item in accounts)
            {
                // The interface reference is also an Account, so it can be
                // cast back to read the shared account information.
                Account account = (Account)item;
                decimal interest = item.CalculateMonthlyInterest();
                total += interest;
                Console.WriteLine($"{account.AccountNumber,-12}{account.AccountType,-14}{account.OwnerName,-17}{account.Balance,13:C}{item.InterestRate,9:P2}{interest,24:C}");
            }
            Console.WriteLine(new string('-', 89));
            Console.WriteLine($"{"Total estimated interest this month:",-65}{total,24:C}");
            Console.WriteLine("(Money market accounts below their minimum balance earn $0.00.)");
        }

        // Shows the results of month-end processing for each account.
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

        // Full profile for one customer.
        public static void ShowCustomerDetails(Customer c)
        {
            Console.WriteLine();
            Console.WriteLine($"Customer #{c.CustomerId}: {c.FullName}");
            Console.WriteLine($"  Phone:   {c.Phone}");
            Console.WriteLine($"  Email:   {c.Email}");
            Console.WriteLine($"  Address: {c.MailingAddress}");
            ShowAccountTable(c.Accounts);
            Console.WriteLine($"{"Total balance:",-67}{c.GetTotalBalance(),13:C}");
        }

        public static string Prompt(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? string.Empty).Trim();
        }

        // Keeps asking until the user types a positive dollar amount.
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

        // Clears the console window. Console.Clear() can throw an error when
        // the output is redirected (for example, to a file), so in that case
        // the program just keeps going without clearing.
        public static void ClearScreen()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // Nothing to clear - output is not going to a real console window.
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
    }
}
