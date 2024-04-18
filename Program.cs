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

        var psi = new ProcessStartInfo
        {
            FileName = GetGitExecutablePath(),
            Arguments = "rev-parse --abbrev-ref HEAD",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            
            UseShellExecute = false,
            CreateNoWindow = true,
            ErrorDialog = false,
            
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;

        var log = new StringBuilder();
        process.OutputDataReceived += (_, rcv) => AppendLog(log, $"[stdout] {rcv.Data}");
        process.ErrorDataReceived += (_, rcv) => AppendLog(log, $"[stderr] {rcv.Data}");
        process.Exited += (_, _) => AppendLog(log, "[exited]");			

        try
        {
            if (!process.Start())
            {
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
            if (!process.WaitForExit(timeout))
            {
                throw new Exception("Failed to finish in time");
            }
        }
        
        //  After the Dispose at this point. All buffers should have been flushed.

        var result = log.ToString().Trim();
        
        Console.WriteLine(result);
        Console.WriteLine("-----------");
        Console.WriteLine(!result.Contains("main") ? "[X] Failed" : "[\u2713] Success");
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
 
            case PlatformID.Unix:
                return "/usr/bin/git";
            
            default:
                throw new Exception("Unsupported platform");
        }
    }

    private static void AppendLog(StringBuilder log, object? data)
    {
        lock (log)
        {
            log.Append(data?.ToString() ?? "");
            log.Append('\n');
        }
    }
}