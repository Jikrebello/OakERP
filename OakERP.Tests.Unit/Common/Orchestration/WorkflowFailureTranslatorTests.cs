using OakERP.Application.Common.Orchestration;
using Shouldly;

namespace OakERP.Tests.Unit.Common.Orchestration;

public sealed class WorkflowFailureTranslatorTests
{
    [Fact]
    public void TryTranslate_Should_Return_First_Matching_Translation()
    {
        Exception exception = new InvalidOperationException("bad");

        string? translated = WorkflowFailureTranslator.TryTranslate(
            exception,
            [
                new WorkflowExceptionRule<string>(
                    ex => ex is InvalidOperationException,
                    _ => "matched"
                ),
            ]
        );

        translated.ShouldBe("matched");
    }

    [Fact]
    public void TryTranslate_Should_Return_Null_When_No_Rules_Match()
    {
        Exception exception = new InvalidOperationException("bad");

        string? translated = WorkflowFailureTranslator.TryTranslate(
            exception,
            [new WorkflowExceptionRule<string>(ex => ex is NotSupportedException, _ => "matched")]
        );

        translated.ShouldBeNull();
    }
}
