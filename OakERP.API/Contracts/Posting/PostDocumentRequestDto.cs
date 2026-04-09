namespace OakERP.API.Contracts.Posting;

public sealed class PostDocumentRequestDto
{
    public DateOnly? PostingDate { get; init; }

    public bool Force { get; init; }
}
