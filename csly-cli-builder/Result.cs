namespace clsy.cli.builder.parser;

public class Result<T,E> 
{
    internal readonly T result;
    internal readonly E error;

    public bool IsOk { get; private set; }
    public bool IsError{ get; private set; }


    public E Error => error;

    public T Value => result;
    
    public Result()
    {
        IsOk = false;
        IsError = false;
    }
    
    public Result(T value)
    {
        result = value;
        IsOk = true;
        IsError = false;
    }

    public Result(E error)
    {
        this.error = error;
        IsOk = false;
        IsError = true;
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
    
}