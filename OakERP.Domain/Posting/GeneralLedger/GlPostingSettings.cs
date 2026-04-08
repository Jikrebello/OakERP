namespace OakERP.Domain.Posting.GeneralLedger;

public sealed record GlPostingSettings(
    string BaseCurrencyCode,
    string ArControlAccountNo,
    string ApControlAccountNo,
    string DefaultRevenueAccountNo,
    string DefaultExpenseAccountNo,
    string DefaultInventoryAssetAccountNo,
    string DefaultCogsAccountNo,
    string DefaultTaxOutputAccountNo,
    string DefaultTaxInputAccountNo
);
