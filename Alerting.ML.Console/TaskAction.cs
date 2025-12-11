using System.CommandLine;
using System.CommandLine.Invocation;

namespace Alerting.ML.Console;

public class TaskAction(Func<ParseResult, Task<int>> @delegate) : AsynchronousCommandLineAction
{
    public override bool Terminating => false;

    public override async Task<int> InvokeAsync(ParseResult parseResult,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return await @delegate(parseResult);
    }
}