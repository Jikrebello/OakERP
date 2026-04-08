using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Posting.GeneralLedger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Posting.GeneralLedger;

public sealed class AppSettingGlSettingsProvider(ApplicationDbContext db) : IGlSettingsProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<GlPostingSettings> GetSettingsAsync(
        CancellationToken cancellationToken = default
    )
    {
        AppSetting? setting = await db.AppSettings.SingleOrDefaultAsync(
            x => x.Key == GlPostingSettingsKeys.Posting,
            cancellationToken
        );

        if (setting is null)
        {
            throw new InvalidOperationException(
                $"App setting '{GlPostingSettingsKeys.Posting}' is required for posting."
            );
        }

        var result = JsonSerializer.Deserialize<GlPostingSettings>(
            setting.ValueJson,
            SerializerOptions
        );

        if (result is null)
        {
            throw new InvalidOperationException(
                $"App setting '{GlPostingSettingsKeys.Posting}' could not be deserialized into GL posting settings."
            );
        }

        return result;
    }
}
