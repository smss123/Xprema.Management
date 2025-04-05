using System;
using System.Collections.Generic;
using System.Linq;

namespace Xprema.Framework.Bussiness.DataTypes;

public class FullName : ValueObject
{
    public string FirstName { get; }
    public string MiddleName { get; }
    public string LastName { get; }

    private FullName(string firstName, string middleName, string lastName)
    {
        FirstName = firstName ?? string.Empty;
        MiddleName = middleName ?? string.Empty;
        LastName = lastName ?? string.Empty;
    }

    public static FullName Create(string firstName, string lastName)
    {
        return new FullName(firstName, string.Empty, lastName);
    }

    public static FullName Create(string firstName, string middleName, string lastName)
    {
        return new FullName(firstName, middleName, lastName);
    }

    public static FullName Parse(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return new FullName(string.Empty, string.Empty, string.Empty);
        }

        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (nameParts.Length == 1)
        {
            return new FullName(nameParts[0], string.Empty, string.Empty);
        }
        else if (nameParts.Length == 2)
        {
            return new FullName(nameParts[0], string.Empty, nameParts[1]);
        }
        else
        {
            var firstName = nameParts[0];
            var lastName = nameParts[nameParts.Length - 1];
            var middleName = string.Join(" ", nameParts.Skip(1).Take(nameParts.Length - 2));
            return new FullName(firstName, middleName, lastName);
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return MiddleName;
        yield return LastName;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(MiddleName))
        {
            return $"{FirstName} {LastName}".Trim();
        }
        
        return $"{FirstName} {MiddleName} {LastName}".Trim();
    }
}
