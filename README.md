# SDC320L Course Project – Bank Account Management Application

**Course:** SDC320L – Advanced Object-Oriented Programming Using C#  
**School:** ECPI University, College of Technology  
**Student:** Donavan D. McFadden (Donmcf8357)  
**Instructor:** Dr. Wissam Hammoud  

## About This Project

For my SDC320L course project, I chose the **Bank Account Management Application** option. It's a C# console application that lets a bank employee view customers and their accounts, make deposits and withdrawals, and (in later phases) add, remove, and update customers and accounts. I'm building it in phases over the five-week term, and each phase gets its own GitHub tag.

| Phase | Tag | Focus |
|-------|-----|-------|
| Week 1 | `Phase-1` | User interaction design, inheritance, composition |
| Week 2 | `Phase-2` | Polymorphism and an interface |
| Week 3 | `Phase-3` | Constructors, access specifiers, abstract classes |
| Week 4 | `Phase-4` | SQLite database |
| Week 5 | `Phase-5` | Final project |

## Week 1 Features

- Header showing the project week, assignment title, and my name
- Welcome message with instructions and a short quote
- Menu-driven console interface that keeps running until the user types `Q`, `Quit`, or `Exit`
- List all customers and all accounts in aligned tables
- View one customer's profile, address, and accounts
- Deposit and withdraw, with input validation and account-specific rules
- Sample customers and accounts loaded at startup (a database comes in Week 4)

## OOP Concepts Demonstrated

**Inheritance**

```
Account                    (base class)
├── CheckingAccount        (derived – monthly fee, overdraft limit)
└── SavingsAccount         (derived – interest rate)
    └── MoneyMarketAccount (derived from SavingsAccount – minimum balance)
```

**Composition**

- `Customer` **has an** `Address` (street, city, state, ZIP)
- `Customer` **has a** list of `Account` objects

## Folder Structure

```
sdc320l-course-project/
├── src/
│   └── BankAccountManager/
│       ├── BankAccountManager.csproj
│       ├── Program.cs              # Entry point and main menu loop
│       ├── Models/
│       │   ├── Account.cs          # Base class
│       │   ├── CheckingAccount.cs  # Derived class
│       │   ├── SavingsAccount.cs   # Derived class
│       │   ├── MoneyMarketAccount.cs # Derived from SavingsAccount
│       │   ├── Address.cs          # Used by Customer (composition)
│       │   └── Customer.cs         # Composition: has Address + Accounts
│       ├── UI/
│       │   └── ConsoleUI.cs        # All console input/output
│       └── Data/
│           └── SampleData.cs       # Creates the starting customers
├── docs/
├── .gitignore
└── README.md
```

## Requirements

- .NET SDK 10.0 (if your machine has an older SDK, such as 8.0, change `<TargetFramework>` in `BankAccountManager.csproj` to match, e.g. `net8.0`)
- Visual Studio Code with the C# Dev Kit extension, or Visual Studio

## Build and Run

```bash
cd src/BankAccountManager
dotnet build
dotnet run
```

## How to Use

1. Read the welcome screen and type a menu number, then press Enter.
2. For option 3, type a Customer ID from the list (example: `1003`).
3. For options 4 and 5, type an account number (example: `CHK-100101`) and a dollar amount.
4. Type `Q` at the main menu to exit.
