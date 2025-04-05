using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xprema.Framework.Bussiness.DataTypes;

public class PhoneNumberValidator : IValidator<string>
{
    private static readonly Regex PhoneRegex = new Regex(
        @"^\+?[0-9\s\-\(\)]+$", 
        RegexOptions.Compiled);
        
    public ValidationResult Validate(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return ValidationResult.Failure("Phone number cannot be empty");
            
        if (!PhoneRegex.IsMatch(phoneNumber))
            return ValidationResult.Failure("Invalid phone number format");
            
        return ValidationResult.Success();
    }
}

public class PhoneNumber : ValueObject
{
    private static readonly PhoneNumberValidator Validator = new PhoneNumberValidator();
    
    public string Value { get; }
    
    private PhoneNumber(string phoneNumber)
    {
        Value = CleanNumber(phoneNumber);
    }
    
    private static string CleanNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;
            
        return phoneNumber.Trim();
    }
    
    public static PhoneNumber Create(string phoneNumber)
    {
        var validationResult = Validator.Validate(phoneNumber);
        
        if (!validationResult.IsValid)
            throw new ArgumentException(validationResult.ErrorMessage, nameof(phoneNumber));
            
        return new PhoneNumber(phoneNumber);
    }
    
    public static PhoneNumber CreateEmpty() => new PhoneNumber(string.Empty);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(PhoneNumber phone) => phone?.Value ?? string.Empty;
    
    public static explicit operator PhoneNumber(string phone) => Create(phone);
} 