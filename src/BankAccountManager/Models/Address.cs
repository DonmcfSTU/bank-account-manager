/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: The Address class stores a customer's mailing address
 *          (street, city, state, and ZIP code). It doesn't do much on its
 *          own - it exists so the Customer class can be built out of
 *          smaller pieces. Customer "has an" Address, which is how I
 *          demonstrate composition in this project.
 */

namespace BankAccountManager.Models
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        public Address(string street, string city, string state, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
        }

        // Returns the address on one line, for example:
        // "4100 Monument Ave, Richmond, VA 23230"
        public override string ToString()
        {
            return $"{Street}, {City}, {State} {ZipCode}";
        }
    }
}
