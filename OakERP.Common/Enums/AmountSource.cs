namespace OakERP.Common.Enums;

public enum AmountSource
{
    HeaderDocTotal, // invoice.DocTotal
    HeaderNetTotal, // DocTotal - TaxTotal
    HeaderTaxTotal, // TaxTotal
    LineNet, // line.LineTotal
    LineTax, // line tax portion
    LineCogsValue // qty * unitCost for that line
    ,
}