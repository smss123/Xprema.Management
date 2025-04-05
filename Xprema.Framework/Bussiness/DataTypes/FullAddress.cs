using System;
using System.Collections.Generic;

namespace Xprema.Framework.Bussiness.DataTypes;

public class FullAddress : ValueObject
{
    public string Street { get; }
    public string BuildingNumber { get; }
    public string ApartmentNumber { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }
    public string AdditionalInfo { get; }
    public PhoneNumber PhoneNumber { get; }
    public EmailAddress Email { get; }
    
    private FullAddress(
        string street, 
        string buildingNumber,
        string apartmentNumber,
        string city, 
        string state, 
        string zipCode, 
        string country,
        string additionalInfo,
        PhoneNumber phoneNumber,
        EmailAddress email)
    {
        Street = street ?? string.Empty;
        BuildingNumber = buildingNumber ?? string.Empty;
        ApartmentNumber = apartmentNumber ?? string.Empty;
        City = city ?? string.Empty;
        State = state ?? string.Empty;
        ZipCode = zipCode ?? string.Empty;
        Country = country ?? string.Empty;
        AdditionalInfo = additionalInfo ?? string.Empty;
        PhoneNumber = phoneNumber ?? PhoneNumber.CreateEmpty();
        Email = email ?? EmailAddress.CreateEmpty();
    }
    
    public Address ToAddress()
    {
        return Address.Create(Street, City, State, ZipCode, PhoneNumber, Email);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return BuildingNumber;
        yield return ApartmentNumber;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
        yield return AdditionalInfo;
        yield return PhoneNumber;
        yield return Email;
    }
    
    public override string ToString()
    {
        string result = Street;
        
        if (!string.IsNullOrWhiteSpace(BuildingNumber))
            result += $" {BuildingNumber}";
            
        if (!string.IsNullOrWhiteSpace(ApartmentNumber))
            result += $", Apt {ApartmentNumber}";
            
        result += $", {City}, {State} {ZipCode}";
        
        if (!string.IsNullOrWhiteSpace(Country))
            result += $", {Country}";
            
        if (!string.IsNullOrWhiteSpace(AdditionalInfo))
            result += $" ({AdditionalInfo})";
            
        return result.Trim().TrimEnd(',').Trim();
    }
    
    public class Builder
    {
        private string _street = string.Empty;
        private string _buildingNumber = string.Empty;
        private string _apartmentNumber = string.Empty;
        private string _city = string.Empty;
        private string _state = string.Empty;
        private string _zipCode = string.Empty;
        private string _country = string.Empty;
        private string _additionalInfo = string.Empty;
        private PhoneNumber _phoneNumber = PhoneNumber.CreateEmpty();
        private EmailAddress _email = EmailAddress.CreateEmpty();
        
        public Builder WithStreet(string street)
        {
            _street = street;
            return this;
        }
        
        public Builder WithBuildingNumber(string buildingNumber)
        {
            _buildingNumber = buildingNumber;
            return this;
        }
        
        public Builder WithApartmentNumber(string apartmentNumber)
        {
            _apartmentNumber = apartmentNumber;
            return this;
        }
        
        public Builder WithCity(string city)
        {
            _city = city;
            return this;
        }
        
        public Builder WithState(string state)
        {
            _state = state;
            return this;
        }
        
        public Builder WithZipCode(string zipCode)
        {
            _zipCode = zipCode;
            return this;
        }
        
        public Builder WithCountry(string country)
        {
            _country = country;
            return this;
        }
        
        public Builder WithAdditionalInfo(string additionalInfo)
        {
            _additionalInfo = additionalInfo;
            return this;
        }
        
        public Builder WithPhoneNumber(PhoneNumber phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }
        
        public Builder WithEmail(EmailAddress email)
        {
            _email = email;
            return this;
        }
        
        public Builder FromAddress(Address address)
        {
            if (address == null) return this;
            
            _street = address.Street;
            _city = address.City;
            _state = address.State;
            _zipCode = address.ZipCode;
            _phoneNumber = address.PhoneNumber;
            _email = address.Email;
            
            return this;
        }
        
        public FullAddress Build()
        {
            return new FullAddress(
                _street,
                _buildingNumber,
                _apartmentNumber,
                _city,
                _state,
                _zipCode,
                _country,
                _additionalInfo,
                _phoneNumber,
                _email);
        }
    }
    
    public static Builder CreateBuilder() => new Builder();
} 