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
        ps.FileName = GetGitExecutablePath();
        ps.Arguments = "rev-parse --abbrev-ref HEAD";

        var log = new StringBuilder();

        var process = new Process();
        process.StartInfo = ps;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (sender, rcv) => AppendLog(log, $"[stdout] {rcv.Data}");
        process.ErrorDataReceived += (sender, rcv) => AppendLog(log, $"[stderr] {rcv.Data}");
        process.Exited += (o, e) => AppendLog(log, "[exited]");			

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

        var result = log.ToString().Trim();
        Console.WriteLine(result);
        Console.WriteLine("-----------");

        if (!result.Contains("main"))
        {
            Console.WriteLine("[X] Failed");
        }
        else
        {
            Console.WriteLine("[\u2713] Success");
        }
        Console.WriteLine("-----------");
    }

    private static string GetGitExecutablePath()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.MacOSX:
                return "/opt/homebrew/bin/git";
            
            case PlatformID.Win32NT:
                return @"C:\Program Files\Git\cmd\git.exe";
            
            default:
                throw new Exception("Unsupported platform");
        }
    }

    private static void AppendLog(StringBuilder log, object? data)
    {
        lock (log)
        {
            log.Append(data?.ToString() ?? "");
            log.Append("\n");
        }
    }
}