using System;
using System.Collections.Generic;

namespace Xprema.Framework.Bussiness.DataTypes;

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public PhoneNumber PhoneNumber { get; }
    public EmailAddress Email { get; }
    
    private Address(string street, string city, string state, string zipCode, 
                   PhoneNumber phoneNumber, EmailAddress email)
    {
        Street = street ?? string.Empty;
        City = city ?? string.Empty;
        State = state ?? string.Empty;
        ZipCode = zipCode ?? string.Empty;
        PhoneNumber = phoneNumber ?? PhoneNumber.CreateEmpty();
        Email = email ?? EmailAddress.CreateEmpty();
    }
    
    public static Address Create(string street, string city, string state, string zipCode)
    {
        return new Address(street, city, state, zipCode, PhoneNumber.CreateEmpty(), EmailAddress.CreateEmpty());
    }
    
    public static Address Create(string street, string city, string state, string zipCode, 
                              PhoneNumber phoneNumber, EmailAddress email)
    {
        return new Address(street, city, state, zipCode, phoneNumber, email);
    }
    
    public Address WithPhoneNumber(PhoneNumber phoneNumber)
    {
        return new Address(Street, City, State, ZipCode, phoneNumber, Email);
    }
    
    public Address WithEmail(EmailAddress email)
    {
        return new Address(Street, City, State, ZipCode, PhoneNumber, email);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return PhoneNumber;
        yield return Email;
    }
    
    public override string ToString()
    {
        return $"{Street}, {City}, {State} {ZipCode}".Trim().TrimEnd(',').Trim();
    }
} 