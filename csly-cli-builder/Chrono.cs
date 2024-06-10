using System.Diagnostics;
using System.Text;

namespace clsy.cli.builder;

public  class Chrono
{
    public List<long> ElapsedMilliseconds { get; set; }

    private bool _isStarted = false;
    
    public bool IsStarted => _isStarted;
    
    public IDictionary<string,long> LabeledElapsedMilliseconds { get; set; }
        
    private Stopwatch chrono;
    public Chrono()
    {
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
        ElapsedMilliseconds.Add(chrono.ElapsedMilliseconds);
        if (!string.IsNullOrEmpty(label))
        {
            LabeledElapsedMilliseconds[label] = chrono.ElapsedMilliseconds;
        }
    }

    public void Tick(string label = null)
    {
        chrono.Stop();
        Console.WriteLine($"{label ?? "no label" } : {chrono.ElapsedMilliseconds}");
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
}