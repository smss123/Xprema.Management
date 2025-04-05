using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xprema.Framework.Bussiness.DataTypes;

public class EmailValidator : IValidator<string>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", 
        RegexOptions.Compiled);
        
    public ValidationResult Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationResult.Failure("Email address cannot be empty");
            
        if (!EmailRegex.IsMatch(email))
            return ValidationResult.Failure("Invalid email address format");
            
        return ValidationResult.Success();
    }
}

public class EmailAddress : ValueObject
{
    private static readonly EmailValidator Validator = new EmailValidator();
    
    public string Value { get; }
    
    private EmailAddress(string email)
    {
        Value = email?.Trim() ?? string.Empty;
    }
    
    public static EmailAddress Create(string email)
    {
        var validationResult = Validator.Validate(email);
        
        if (!validationResult.IsValid)
            throw new ArgumentException(validationResult.ErrorMessage, nameof(email));
            
        return new EmailAddress(email);
    }
    
    public static EmailAddress CreateEmpty() => new EmailAddress(string.Empty);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    
    public static implicit operator string(EmailAddress email) => email?.Value ?? string.Empty;
    
    public static explicit operator EmailAddress(string email) => Create(email);
} 