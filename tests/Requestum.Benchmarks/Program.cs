using BenchmarkDotNet.Running;

namespace Requestum.Benchmarks;

public static class Program
{
    public static void Main(string[] args) 
        => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
