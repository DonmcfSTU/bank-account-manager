/*
 * Name:    Donavan D. McFadden
 * Date:    11/29/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 3 - Abstraction, Constructors, & Access Specifiers
 *
 * Purpose: TransactionType is an enum that lists the kinds of activity that
 *          can happen on an account. Using an enum instead of plain strings
 *          keeps the values consistent (no typos like "Depsit").
 */

namespace BankAccountManager.Models
{
    public enum TransactionType
    {
        OpeningDeposit,
        Deposit,
        Withdrawal,
        Fee,
        Interest,
        Closing
    }
}
