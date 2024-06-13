namespace csly_cli_api;

public class CliResult<T>
{
    private bool _isOk;

    public bool IsOK => _isOk;
    
    public bool IsError => !_isOk;
    
    private T _result;

    public T Result => _result;

    private List<string> _errors;

    public List<string> Errors => _errors;

    public new IDictionary<string, long> Timings { get; set; } = new Dictionary<string, long>();

    public CliResult(List<string> errors)
    {
        _isOk = false;
        _errors = errors;
    }

    public CliResult(T result)
    {
        _isOk = true;
        _result = result;
    }

    public static implicit operator CliResult<T>(T value)
    {
        return new CliResult<T>(value);
    }
    
    public static implicit operator CliResult<T>(List<string> errors)
    {
        return new CliResult<T>(errors);
    }

    public static implicit operator T(CliResult<T> result) => result._result;
    
    public static implicit operator List<string>(CliResult<T> result) => result.Errors;
}