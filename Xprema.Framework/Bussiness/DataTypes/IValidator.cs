namespace Xprema.Framework.Bussiness.DataTypes;

public interface IValidator<T>
{
    ValidationResult Validate(T value);
} 