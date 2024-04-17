using System.Diagnostics;
using System.Text;

namespace ProcessLateMessagesWithTimeout;

class Program
{
    static void Main(string[] args)
    {
        RunTest(-1);
        RunTest(1000);
        RunTest(-1);
        RunTest(1000);
    }

    private static void RunTest(int timeout)
    {
        Console.WriteLine("Running process with timeout: " + timeout);

        var ps = new ProcessStartInfo();

        ps.UseShellExecute = false;
        ps.CreateNoWindow = true;
        ps.ErrorDialog = false;

        ps.RedirectStandardInput = true;
        ps.RedirectStandardOutput = true;
        ps.RedirectStandardError = true;
        ps.WorkingDirectory = Directory.GetCurrentDirectory();
        ps.FileName = "/opt/homebrew/bin/git";
        ps.Arguments = "rev-parse --abbrev-ref HEAD";

        var log = new StringBuilder();

        var process = new Process();
        process.StartInfo = ps;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (sender, rcv) => AppendLog(log, rcv.Data);
        process.ErrorDataReceived += (sender, rcv) => AppendLog(log, rcv.Data);
        process.Exited += (o, e) => AppendLog(log, "[Exited]");			

        try
        {
            if (!process.Start())
            {
                process.Dispose();
                throw new Exception("Failed to launch");
            }
        }
        catch (Exception)
        {
            process.Dispose();
            throw;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        using (process)
        {
            process.WaitForExit(timeout);
        }

        Console.WriteLine($"Log: {log}");
    }

    private static void AppendLog(StringBuilder log, object? data)
    {
        lock (log)
        {
            log.Append(data?.ToString() ?? "\n");
        }
    }
}