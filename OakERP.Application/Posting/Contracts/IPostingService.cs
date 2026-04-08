namespace OakERP.Application.Posting.Contracts;

public interface IPostingService
{
    Task<PostResult> PostAsync(PostCommand command, CancellationToken cancellationToken = default);

    Task<UnpostResult> UnpostAsync(
        UnpostCommand command,
        CancellationToken cancellationToken = default
    );
}
