/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: The Address class stores a customer's mailing address. Customer
 *          "has an" Address (COMPOSITION).
 *          Week 3: properties are now read-only (an address is replaced as a
 *          whole when a customer moves), the constructor validates input,
 *          and a COPY CONSTRUCTOR was added.
 */

namespace BankAccountManager.Models
{
    public class Address
    {
        // ACCESS SPECIFIERS: public get-only. Once an Address is created it
        // can't be partly changed - that prevents half-updated addresses.
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string ZipCode { get; }

        // CONSTRUCTOR (main): validates and cleans up the values.
        public Address(string street, string city, string state, string zipCode)
        {
            Street = Require(street, "Street");
            City = Require(city, "City");
            State = Require(state, "State").ToUpper();
            ZipCode = Require(zipCode, "ZIP code");

            if (State.Length != 2)
            {
                throw new ArgumentException("State must be a 2-letter abbreviation (example: VA).");
            }
            if (ZipCode.Length != 5 || !ZipCode.All(char.IsDigit))
            {
                throw new ArgumentException("ZIP code must be 5 digits.");
            }
        }

        // COPY CONSTRUCTOR: makes a new Address with the same values as another
        // one. Customer uses this so it keeps its OWN copy of the address
        // (composition - the customer owns its parts).
        public Address(Address other)
            : this(other.Street, other.City, other.State, other.ZipCode)
        {
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State} {ZipCode}";
        }

        // ACCESS SPECIFIER - private static: input check used only here.
        private static string Require(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{fieldName} is required.");
            }
            return value.Trim();
        }
    }
}
