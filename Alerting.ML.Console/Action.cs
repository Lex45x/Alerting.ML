using System.CommandLine;
using System.CommandLine.Invocation;

namespace Alerting.ML.Console;

public class Action(Func<ParseResult, int> @delegate) : SynchronousCommandLineAction
{
    public override bool Terminating => false;
    public override int Invoke(ParseResult parseResult)
    {
        return @delegate(parseResult);
    }
}