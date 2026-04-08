using OakERP.Application.Interfaces.Persistence;
using OakERP.Common.Dtos.Auth;

namespace OakERP.Auth.Internal.Support;

internal sealed class AuthTransactionRunner(IUnitOfWork unitOfWork)
{
    public async Task<AuthResultDto> ExecuteAsync(Func<Task<AuthResultDto>> operation)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            AuthResultDto result = await operation();
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.SaveChangesAsync();
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
