using csly.cli.model;
using csly.cli.model.lexer;

namespace clsy.cli.builder.checker;



public class ExtensionTokenChecks 
{
    private bool _isOk;

    public bool IsOK => _isOk;
    
    public bool IsError => !_isOk;
    
    private bool _result;

    public bool Result => _result;

    private List<string> _errors;

    public List<string> Errors => _errors;

    public ExtensionTokenChecks(List<string> errors)
    {
        _isOk = false;
        _errors = errors;
    }

    public ExtensionTokenChecks(bool result)
    {
        _isOk = true;
        _errors = new List<string>();
        _result = result;
    }

    public void AddError(string error)
    {
        _result = false;
        _isOk = false;
        
        Errors.Add(error);
    }

    public void AddErrors(List<string> errors)
    {
        _errors.AddRange(errors);
        _isOk = false;
        _result = false;
    }

    public static implicit operator ExtensionTokenChecks(bool value)
    {
        return new ExtensionTokenChecks(value);
    }
    
    public static implicit operator ExtensionTokenChecks(List<string> errors)
    {
        return new ExtensionTokenChecks(errors);
    }

    public static implicit operator bool(ExtensionTokenChecks result) => result._result;
    
    public static implicit operator List<string>(ExtensionTokenChecks result) => result.Errors;
    
}

public class ExtensionTokensVisitor : AbstractModelVisitor<ExtensionTokenChecks>
{

    private string _tokenID = null;
    
    public override ExtensionTokenChecks VisitExtension(ExtensionTokenModel extension, ExtensionTokenChecks results)
    {
        _tokenID = extension.Name;
        return results;
    }
    public override ExtensionTokenChecks VisitChain(TransitionChain chain, ExtensionTokenChecks result)
    {
        if (chain.IsEnded)
        {
            return true && result;
        }
        else
        {
            result.AddError($"extension token {_tokenID} does not have an end.");
            return result;
        }
    }
    
}