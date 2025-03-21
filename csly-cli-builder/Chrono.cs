using System.Diagnostics;
using System.Text;

namespace clsy.cli.builder;

public  class Chrono
{
    public List<long> ElapsedMilliseconds { get; set; }

    private bool _isStarted = false;
    
    public bool IsStarted => _isStarted;

    private readonly Action<string> _callback = null;
    
    public IDictionary<string,long> LabeledElapsedMilliseconds { get; set; }
        
    private Stopwatch chrono;
    public Chrono(Action<string> callback = null)
    {
        _callback = callback;
        chrono = new Stopwatch();
        ElapsedMilliseconds = new List<long>();
        LabeledElapsedMilliseconds = new Dictionary<string, long>();
    }

    public void Start()
    {
        _isStarted = true;
        chrono.Start();
    }

    public void Stop(string label = null)
    {
        chrono.Stop();
        var message = $"{label ?? "no label"} : {chrono.ElapsedMilliseconds}";
        Log(message);
        ElapsedMilliseconds.Add(chrono.ElapsedMilliseconds);
        if (!string.IsNullOrEmpty(label))
        {
            LabeledElapsedMilliseconds[label] = chrono.ElapsedMilliseconds;
        }
    }

    public void Tick(string label = null)
    {
        chrono.Stop();
        var message = $"{label ?? "no label"} : {chrono.ElapsedMilliseconds}";
        Log(message);
        ElapsedMilliseconds.Add(chrono.ElapsedMilliseconds);
        if (!string.IsNullOrEmpty(label))
        {
            LabeledElapsedMilliseconds[label] = chrono.ElapsedMilliseconds;
        }
        chrono.Reset();
        chrono.Start();
    }
        
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var step in LabeledElapsedMilliseconds)
        {
            builder.AppendLine($"{step.Key} : {step.Value} ms");
        }
        return builder.ToString();
    }
    
    private void Log(string message)
    {
        if (_callback != null)
        {
            _callback(message);
        }
    }
}