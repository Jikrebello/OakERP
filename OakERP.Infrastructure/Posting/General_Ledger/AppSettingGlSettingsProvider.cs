using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OakERP.Domain.Entities.Common;
using OakERP.Domain.Posting.General_Ledger;
using OakERP.Infrastructure.Persistence;

namespace OakERP.Infrastructure.Posting.General_Ledger;

public sealed class AppSettingGlSettingsProvider(ApplicationDbContext db) : IGlSettingsProvider
{
    private const string GlPostingSettingsKey = "gl.posting";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<GlPostingSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        AppSetting? setting = await db.AppSettings.SingleOrDefaultAsync(
            x => x.Key == GlPostingSettingsKey,
            cancellationToken
        );

        if (setting is null)
        {
            throw new InvalidOperationException(
                $"App setting '{GlPostingSettingsKey}' is required for posting."
            );
        }

        var result = JsonSerializer.Deserialize<GlPostingSettings>(
            setting.ValueJson,
            SerializerOptions
        );

        if (result is null)
        {
            throw new InvalidOperationException(
                $"App setting '{GlPostingSettingsKey}' could not be deserialized into GL posting settings."
            );
        }

        return result;
    }
}
