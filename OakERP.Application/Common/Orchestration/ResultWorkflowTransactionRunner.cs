using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Dtos.Base;

namespace OakERP.Application.Common.Orchestration;

internal sealed class ResultWorkflowTransactionRunner(IUnitOfWork unitOfWork)
{
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken
    )
        where TResult : BaseResultDto
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            TResult result = await operation(cancellationToken);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
