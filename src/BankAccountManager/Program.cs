/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: Program is the starting point of the Bank Account Management
 *          application. It shows the header and welcome message, creates the
 *          Bank with its sample customers, and runs the main menu loop until
 *          the user chooses to quit. Each menu choice calls a small helper
 *          method. Week 2 adds the interest report (interface polymorphism)
 *          and month-end processing (method-override polymorphism).
 */

using System.Globalization;
using BankAccountManager.Data;
using BankAccountManager.Interfaces;
using BankAccountManager.Models;
using BankAccountManager.UI;

namespace BankAccountManager
{
    public class Program
    {
        private static Bank _bank = new Bank("McFadden Community Bank");

        public static void Main(string[] args)
        {
            // Use U.S. formatting so currency always shows as $1,234.56
            // no matter what region the computer is set to.
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            ConsoleUI.ShowHeader();
            ConsoleUI.ShowWelcome();
            bool firstTime = true;

            // Instantiate the classes with realistic (sample) information.
            _bank = SampleData.CreateBank();

            bool running = true;
            while (running)
            {
                // After the first pass, clear the screen so each menu
                // action starts on a clean screen with the header at the top.
                if (!firstTime)
                {
                    ConsoleUI.ClearScreen();
                    ConsoleUI.ShowHeader();
                }
                firstTime = false;

                ConsoleUI.ShowMainMenu();
                string choice = ConsoleUI.Prompt("Enter your choice: ").ToUpper();

                switch (choice)
                {
                    case "1":
                        ConsoleUI.ShowCustomerTable(_bank.Customers.ToList());
                        ConsoleUI.Pause();
                        break;
                    case "2":
                        ShowAllAccounts();
                        ConsoleUI.Pause();
                        break;
                    case "3":
                        ViewOneCustomer();
                        ConsoleUI.Pause();
                        break;
                    case "4":
                        MakeDeposit();
                        ConsoleUI.Pause();
                        break;
                    case "5":
                        MakeWithdrawal();
                        ConsoleUI.Pause();
                        break;
                    case "6":
                        ShowInterestReport();
                        ConsoleUI.Pause();
                        break;
                    case "7":
                        RunMonthEnd();
                        ConsoleUI.Pause();
                        break;
                    case "Q":
                    case "QUIT":
                    case "EXIT":
                        running = false;
                        break;
                    default:
                        ConsoleUI.ShowError($"\"{choice}\" is not a menu option. Please choose 1-7 or Q.");
                        ConsoleUI.Pause();
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Thank you for using the {_bank.Name} Account Manager. Goodbye!");
        }

        // POLYMORPHISM: every account is stored as the base type Account,
        // but each row's "Details" column comes from that object's own
        // GetAccountDetails() override.
        private static void ShowAllAccounts()
        {
            ConsoleUI.ShowAccountTable(_bank.GetAllAccounts());
        }

        // POLYMORPHISM through the INTERFACE: only IInterestBearing accounts.
        private static void ShowInterestReport()
        {
            List<IInterestBearing> interestAccounts = _bank.GetInterestBearingAccounts();
            ConsoleUI.ShowInterestReport(interestAccounts);
        }

        // POLYMORPHISM: one loop calls ProcessMonthEnd() on every account.
        // Checking charges a fee, savings adds interest, and money market
        // either adds interest or charges a low-balance fee - all decided by
        // the object's actual type at run time, with no if/else on the type.
        private static void RunMonthEnd()
        {
            string confirm = ConsoleUI.Prompt("\nThis will apply fees and interest to every account. Continue? (Y/N): ").ToUpper();
            if (confirm != "Y" && confirm != "YES")
            {
                ConsoleUI.ShowError("Month-end processing was canceled. No balances were changed.");
                return;
            }

            var results = new List<(Account Account, decimal Before, string Result)>();
            foreach (Account account in _bank.GetAllAccounts())
            {
                decimal before = account.Balance;
                string result = account.ProcessMonthEnd();
                results.Add((account, before, result));
            }
            ConsoleUI.ShowMonthEndResults(results);
            ConsoleUI.ShowSuccess("Month-end processing complete.");
        }

        private static void ViewOneCustomer()
        {
            ConsoleUI.ShowCustomerTable(_bank.Customers.ToList());
            Customer? customer = FindCustomer(ConsoleUI.Prompt("\nEnter a Customer ID: "));
            if (customer == null)
            {
                ConsoleUI.ShowError("No customer was found with that ID.");
                return;
            }
            ConsoleUI.ShowCustomerDetails(customer);
        }

        private static void MakeDeposit()
        {
            Account? account = AskForAccount();
            if (account == null) return;

            decimal amount = ConsoleUI.PromptForAmount("Deposit amount: $");
            try
            {
                account.Deposit(amount);
                ConsoleUI.ShowSuccess($"Deposited {amount:C}. New balance for {account.AccountNumber}: {account.Balance:C}");
            }
            catch (ArgumentException ex)
            {
                ConsoleUI.ShowError(ex.Message);
            }
        }

        private static void MakeWithdrawal()
        {
            Account? account = AskForAccount();
            if (account == null) return;

            decimal amount = ConsoleUI.PromptForAmount("Withdrawal amount: $");
            try
            {
                // Each account type applies its own withdrawal rule here
                // (overdraft for checking, minimum balance for money market).
                account.Withdraw(amount);
                ConsoleUI.ShowSuccess($"Withdrew {amount:C}. New balance for {account.AccountNumber}: {account.Balance:C}");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ConsoleUI.ShowError($"Withdrawal declined: {ex.Message}");
            }
        }

        // Shows the account list and asks the user to pick one by number.
        private static Account? AskForAccount()
        {
            ShowAllAccounts();
            string accountNumber = ConsoleUI.Prompt("\nEnter the account number: ");
            Account? account = _bank.FindAccount(accountNumber);
            if (account == null)
            {
                ConsoleUI.ShowError("No account was found with that number.");
            }
            return account;
        }

        private static Customer? FindCustomer(string input)
        {
            if (!int.TryParse(input, out int id)) return null;
            return _bank.FindCustomer(id);
        }
    }
}
