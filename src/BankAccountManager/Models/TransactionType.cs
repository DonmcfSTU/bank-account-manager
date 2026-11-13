/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
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
