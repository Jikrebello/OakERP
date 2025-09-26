namespace OakERP.Domain.Shared.Enums;

public enum AmountKey
{
    HeaderDocTotal,
    HeaderNetTotal,
    HeaderTaxTotal, // header-level amounts
    LineExtPrice, // qty * unit price (ex tax)
    LineCogsValue // qty * moving-average cost
    ,
}
