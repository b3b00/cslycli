

using System.Diagnostics.CodeAnalysis;

namespace clsy.cli.builder;

public class Result<T> : Result<T, List<string>>
{

    public Result(T value) : base(value)
    {
        error = new List<string>();
    }

    public Result(List<string> error) : base (error)
    {
    }

    public static implicit operator T(Result<T> r) {
        return r.result;
    } 
    
    public static implicit operator List<string>(Result<T> r) {
        return r.error;
    } 
    
    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator  Result<T>(List<string> error)
    {
        return new Result<T>(error);
    }

    public void AddError(string errorMessage)
    {
        SetIsOk(false);
        if (error == null)
        {
            error = new List<string>();
        }
        error.Add(errorMessage);
    }
    
    public void AddErrors(IEnumerable<string> errors)
    {
        if (errors.Any())
        {
            error.AddRange(errors);
            SetIsOk(false);
        }
    }
    
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        if (IsError)
        {
            return "ERROR\n"+string.Join("\n", Error as List<string>);
        }
        return "OK";
    }
}

public class Result<T,E> 
{
    internal T result;
    internal E error;

    private bool _isOk;
    public bool IsOk => _isOk;
    public bool IsError => !IsOk;

    protected void SetIsOk(bool isOk)
    {
        _isOk = isOk;
    }

    public E Error => error;

    public T Value => result;
    
    public Result()
    {
        _isOk = false;
    }
    
    public Result(T value)
    {
        result = value;
        _isOk = true;
    }

    public Result(E error)
    {
        this.error = error;
        SetIsOk(false);
    }

    public static implicit operator T(Result<T,E> r) {
        return r.result;
    } 
    
    public static implicit operator E(Result<T,E> r) {
        return r.error;
    } 
    
    public static implicit operator Result<T,E>(T value)
    {
        return new Result<T, E>(value);
    }

    public static implicit operator  Result<T,E>(E error)
    {
        return new Result<T, E>(error);
    }

    public override string ToString()
    {
        if (IsError)
        {
            return "ERROR\n"+string.Join("\n", Error);
        }

        return "OK";
    }
    
}