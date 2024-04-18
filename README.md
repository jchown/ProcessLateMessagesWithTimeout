# Process Late Messages With Timeout

This code shows some strange behaviour in the C# `System.Diagnositics.Process` class on Mac OS,
specifically how the behaviour changes between a call to `WaitForExit` with a timeout of `-1` (unlimited)
and `1000` (milliseconds).

In both cases the process exits correctly, but only if you use an unlimited timeout
do you get the correct message (via standard error stream). The process being run is `git rev-parse --abbrev-ref HEAD`
which should just print the current branch.

# Reproduction

1. Clone this repository
3. Run `which git` and make sure the code matches.
2. Run the code in Rider or Visual Studio.
4. Observe the output:

```
Running process with timeout: -1
Log: main

[Exited]
-----------
Running process with timeout: 1000
Log: [Exited]
-----------
Running process with timeout: -1
Log: main

[Exited]
-----------
Running process with timeout: 1000
Log: main

[Exited]
-----------
```

As you can see above on one of the `Running process with timeout: -1` runs, the `main` branch is not printed. The reproduction rate is about 75%.

# Other Platforms

On Windows, the output is as expected:

```
Running process with timeout: -1
[stdout] main
[stdout] 
[stderr] 
[exited]
-----------
[√] Success
-----------
Running process with timeout: 1000
[stdout] main
[stdout] 
[stderr] 
[exited]
-----------
[√] Success
-----------
Running process with timeout: -1
[stdout] main
[stdout]
[stderr]
[exited]
-----------
[√] Success
-----------
Running process with timeout: 1000
[stdout] main
[stderr]
[stdout]
[exited]
-----------
[√] Success
-----------
```

# Dependencies

This was first observed inside Unity's runtime. This example uses .NET 6. I am using an M3 Macbook Air with git 2.44.0 and Rider 2024.1.
