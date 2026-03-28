namespace OakERP.Domain.Posting.General_Ledger;

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