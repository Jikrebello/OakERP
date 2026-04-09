using System.Linq;

namespace OakERP.Application.Common.Orchestration;

internal sealed class WorkflowExceptionRule<TResult>(
    Func<Exception, bool> matches,
    Func<Exception, TResult> translate
)
{
    public Func<Exception, bool> Matches { get; } = matches;

    public Func<Exception, TResult> Translate { get; } = translate;
}

internal static class WorkflowFailureTranslator
{
    public static TResult? TryTranslate<TResult>(
        Exception exception,
        params WorkflowExceptionRule<TResult>[] rules
    )
    {
        WorkflowExceptionRule<TResult>? matchingRule = rules.FirstOrDefault(rule =>
            rule.Matches(exception)
        );
        return matchingRule is null ? default : matchingRule.Translate(exception);
    }
}
