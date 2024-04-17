# Process Late Messages With Timeout

This code shows some strange behaviour in the C# `System.Diagnositics.Process` class,
specifically how the behaviour changes between a call to `WaitForExit` with a timeout of `-1` (unlimited)
and `1000` (milliseconds).

In both cases the process exits correctly, but only if you use an unlimited timeout
do you get the correct message (via standard error stream).
