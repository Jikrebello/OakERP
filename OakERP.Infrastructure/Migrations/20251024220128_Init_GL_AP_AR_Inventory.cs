using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OakERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_GL_AP_AR_Inventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                    DO $$
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'gl_account_type') THEN
                            CREATE TYPE gl_account_type AS ENUM ('asset','liability','equity','revenue','expense');
                        END IF;

                        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'doc_status') THEN
                            CREATE TYPE doc_status AS ENUM ('draft','posted','voided','closed');
                        END IF;

                        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'item_type') THEN
                            CREATE TYPE item_type AS ENUM ('stock','nonstock','service');
                        END IF;

                        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'inventory_transaction_type') THEN
                            CREATE TYPE inventory_transaction_type AS ENUM (
                                'receipt','issue','adjustment','transfer_in','transfer_out','sales_cogs'
                            );
                        END IF;
                    END $$;
                """
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Licenses_Tenants_TenantId",
                table: "Licenses"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Tenants", table: "Tenants");

            migrationBuilder.DropPrimaryKey(name: "PK_Licenses", table: "Licenses");

            migrationBuilder.RenameTable(name: "Tenants", newName: "tenants");

            migrationBuilder.RenameTable(name: "Licenses", newName: "licenses");

            migrationBuilder.RenameIndex(
                name: "IX_Licenses_TenantId",
                table: "licenses",
                newName: "IX_licenses_TenantId"
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "tenants",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "licenses",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetUserTokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetUsers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetUserRoles",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetUserLogins",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetUserClaims",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetRoles",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AspNetRoleClaims",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AddPrimaryKey(name: "PK_tenants", table: "tenants", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_licenses", table: "licenses", column: "Id");

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    Key = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: false
                    ),
                    ValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamptz",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.Key);
                }
            );

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    Code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    NumericCode = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(80)",
                        maxLength: 80,
                        nullable: false
                    ),
                    Symbol = table.Column<string>(
                        type: "character varying(8)",
                        maxLength: 8,
                        nullable: true
                    ),
                    Decimals = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.Code);
                    table.CheckConstraint("ck_currency_code_len3", "char_length(\"Code\") = 3");
                    table.CheckConstraint("ck_currency_code_upper", "\"Code\" = upper(\"Code\")");
                    table.CheckConstraint(
                        "ck_currency_decimals_range",
                        "\"Decimals\" BETWEEN 0 AND 4"
                    );
                    table.CheckConstraint(
                        "ck_currency_numeric_range",
                        "\"NumericCode\" BETWEEN 1 AND 999"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    Email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    Address = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    TaxNumber = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    TermsDays = table.Column<int>(type: "integer", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsOnHold = table.Column<bool>(type: "boolean", nullable: false),
                    CreditHoldReason = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    CreditHoldUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                    table.CheckConstraint(
                        "ck_customer_code_not_blank",
                        "btrim(\"CustomerCode\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_customer_creditlimit_nonneg",
                        "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_customer_email_basic_shape",
                        "\"Email\" IS NULL OR (position('@' in \"Email\") > 1 AND position('.' in \"Email\") > 3)"
                    );
                    table.CheckConstraint(
                        "ck_customer_hold_until_future",
                        "\"CreditHoldUntil\" IS NULL OR \"CreditHoldUntil\" >= CURRENT_DATE"
                    );
                    table.CheckConstraint("ck_customer_name_not_blank", "btrim(\"Name\") <> ''");
                    table.CheckConstraint(
                        "ck_customer_termsdays_range",
                        "\"TermsDays\" BETWEEN 0 AND 180"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "fiscal_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodNo = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_periods", x => x.Id);
                    table.CheckConstraint(
                        "ck_fiscper_periodno_range",
                        "\"PeriodNo\" BETWEEN 1 AND 12"
                    );
                    table.CheckConstraint(
                        "ck_fiscper_start_le_end",
                        "\"PeriodStart\" <= \"PeriodEnd\""
                    );
                    table.CheckConstraint(
                        "ck_fiscper_status_allowed",
                        "\"Status\" IN ('open','closed')"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_accounts",
                columns: table => new
                {
                    AccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Type = table.Column<int>(type: "gl_account_type", nullable: false),
                    ParentAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    IsControl = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_accounts", x => x.AccountNo);
                    table.CheckConstraint("ck_glacct_name_not_blank", "btrim(\"Name\") <> ''");
                    table.CheckConstraint(
                        "ck_glacct_no_chars",
                        "\"AccountNo\" ~ '^[A-Z0-9][A-Z0-9\\.-]{0,19}$'"
                    );
                    table.CheckConstraint("ck_glacct_no_not_blank", "btrim(\"AccountNo\") <> ''");
                    table.CheckConstraint(
                        "ck_glacct_no_upper",
                        "\"AccountNo\" = upper(\"AccountNo\")"
                    );
                    table.CheckConstraint(
                        "ck_glacct_parent_not_self",
                        "\"ParentAccount\" IS NULL OR \"ParentAccount\" <> \"AccountNo\""
                    );
                    table.ForeignKey(
                        name: "FK_gl_accounts_gl_accounts_ParentAccount",
                        column: x => x.ParentAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.Id);
                    table.CheckConstraint("ck_location_code_not_blank", "btrim(\"Code\") <> ''");
                    table.CheckConstraint("ck_location_code_upper", "\"Code\" = upper(\"Code\")");
                    table.CheckConstraint("ck_location_name_not_blank", "btrim(\"Name\") <> ''");
                }
            );

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RatePercent = table.Column<decimal>(type: "numeric", nullable: false),
                    IsInput = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorCode = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    Email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    Address = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    TaxNumber = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    TermsDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendors", x => x.Id);
                    table.CheckConstraint(
                        "ck_vendor_code_not_blank",
                        "btrim(\"VendorCode\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_vendor_email_basic_shape",
                        "\"Email\" IS NULL OR (position('@' in \"Email\") > 1 AND position('.' in \"Email\") > 3)"
                    );
                    table.CheckConstraint("ck_vendor_name_not_blank", "btrim(\"Name\") <> ''");
                    table.CheckConstraint(
                        "ck_vendor_termsdays_range",
                        "\"TermsDays\" BETWEEN 0 AND 180"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    CurrencyCode = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    ShipTo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    TaxTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DocTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_invoices", x => x.Id);
                    table.CheckConstraint(
                        "ck_arinvoice_currency_len3",
                        "char_length(\"CurrencyCode\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_due_after_invoice",
                        "\"DueDate\" >= \"InvoiceDate\""
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_posted_requires_postingdate",
                        "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_totals_nonnegative",
                        "(\"TaxTotal\" >= 0) AND (\"DocTotal\" >= 0)"
                    );
                    table.ForeignKey(
                        name: "FK_ar_invoices_currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ar_invoices_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_journals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    JournalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_journals", x => x.Id);
                    table.CheckConstraint(
                        "ck_gljournal_no_not_blank",
                        "btrim(\"JournalNo\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_gljournal_posted_requires_postingdate",
                        "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
                    );
                    table.CheckConstraint(
                        "ck_gljournal_posting_ge_journal",
                        "\"PostingDate\" IS NULL OR \"PostingDate\" >= \"JournalDate\""
                    );
                    table.ForeignKey(
                        name: "FK_gl_journals_fiscal_periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "fiscal_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    BankName = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: true
                    ),
                    AccountNumber = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    GlAccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => x.Id);
                    table.CheckConstraint(
                        "ck_bankacct_currency_len3",
                        "char_length(\"CurrencyCode\") = 3"
                    );
                    table.CheckConstraint("ck_bankacct_name_not_blank", "btrim(\"Name\") <> ''");
                    table.ForeignKey(
                        name: "FK_bank_accounts_currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_bank_accounts_gl_accounts_GlAccountNo",
                        column: x => x.GlAccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    SourceType = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: false
                    ),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNo = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_entries", x => x.Id);
                    table.CheckConstraint(
                        "ck_glentry_one_sided_amount",
                        "(\"Debit\" >= 0) AND (\"Credit\" >= 0) AND ((\"Debit\" = 0 AND \"Credit\" > 0) OR (\"Credit\" = 0 AND \"Debit\" > 0))"
                    );
                    table.CheckConstraint(
                        "ck_glentry_source_pairing",
                        "(\"SourceType\" IS NULL) OR (\"SourceId\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "FK_gl_entries_fiscal_periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "fiscal_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_gl_entries_gl_accounts_AccountNo",
                        column: x => x.AccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "item_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    InventoryAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    CogsAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    AdjustAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    RevenueAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_categories_gl_accounts_AdjustAccount",
                        column: x => x.AdjustAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_item_categories_gl_accounts_CogsAccount",
                        column: x => x.CogsAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_item_categories_gl_accounts_InventoryAccount",
                        column: x => x.InventoryAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_item_categories_gl_accounts_RevenueAccount",
                        column: x => x.RevenueAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "stock_counts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    ScheduledOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_counts", x => x.Id);
                    table.CheckConstraint("ck_sc_countno_not_blank", "btrim(\"CountNo\") <> ''");
                    table.ForeignKey(
                        name: "FK_stock_counts_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    CurrencyCode = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    TaxTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DocTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_invoices", x => x.Id);
                    table.CheckConstraint(
                        "ck_apinvoice_currency_len3",
                        "char_length(\"CurrencyCode\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_apinvoice_due_after_invoice",
                        "\"DueDate\" >= \"InvoiceDate\""
                    );
                    table.CheckConstraint(
                        "ck_apinvoice_totals_nonnegative",
                        "(\"TaxTotal\" >= 0) AND (\"DocTotal\" >= 0)"
                    );
                    table.ForeignKey(
                        name: "FK_ap_invoices_currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ap_invoices_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_journal_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    AccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    SignedAmount = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        computedColumnSql: "(\"Debit\" - \"Credit\")",
                        stored: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_journal_lines", x => x.Id);
                    table.CheckConstraint("ck_gjl_lineno_positive", "\"LineNo\" > 0");
                    table.CheckConstraint(
                        "ck_gjl_one_sided_amount",
                        "(\"Debit\" >= 0) AND (\"Credit\" >= 0) AND ((\"Debit\" = 0 AND \"Credit\" > 0) OR (\"Credit\" = 0 AND \"Debit\" > 0))"
                    );
                    table.ForeignKey(
                        name: "FK_gl_journal_lines_gl_accounts_AccountNo",
                        column: x => x.AccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_gl_journal_lines_gl_journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "gl_journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClearedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_payments", x => x.Id);
                    table.CheckConstraint("ck_appayment_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint(
                        "ck_appayment_cleared_not_before_posting",
                        "(\"ClearedDate\" IS NULL) OR (\"PostingDate\" IS NULL) OR (\"ClearedDate\" >= \"PostingDate\")"
                    );
                    table.CheckConstraint(
                        "ck_appayment_posted_requires_postingdate",
                        "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "FK_ap_payments_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ap_payments_vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocNo = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClearedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    AmountForeign = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: true),
                    DocStatus = table.Column<int>(type: "doc_status", nullable: false),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_receipts", x => x.Id);
                    table.CheckConstraint("ck_arreceipt_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint(
                        "ck_arreceipt_cleared_not_before_posting",
                        "(\"ClearedDate\" IS NULL) OR (\"PostingDate\" IS NULL) OR (\"ClearedDate\" >= \"PostingDate\")"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_currency_len3",
                        "char_length(\"CurrencyCode\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_consistency",
                        "(\"AmountForeign\" IS NULL OR \"ExchangeRate\" IS NULL) OR abs((\"AmountForeign\" * \"ExchangeRate\") - \"Amount\") <= 0.01"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_pair_nullness",
                        "(\"AmountForeign\" IS NULL) = (\"ExchangeRate\" IS NULL)"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_positive",
                        "(\"AmountForeign\" IS NULL OR \"AmountForeign\" > 0) AND (\"ExchangeRate\" IS NULL OR \"ExchangeRate\" > 0)"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_posted_requires_postingdate",
                        "(\"DocStatus\" <> 'posted'::doc_status) OR (\"PostingDate\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "FK_ar_receipts_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ar_receipts_currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ar_receipts_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_reconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    StatementTo = table.Column<DateOnly>(type: "date", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_reconciliations", x => x.Id);
                    table.CheckConstraint(
                        "ck_bankrec_range_valid",
                        "\"StatementTo\" >= \"StatementFrom\""
                    );
                    table.ForeignKey(
                        name: "FK_bank_reconciliations_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_statements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatementFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    StatementTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Source = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    ExternalId = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    FileName = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    Notes = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true
                    ),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ImportedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ImportedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_statements", x => x.Id);
                    table.CheckConstraint(
                        "ck_bankstmt_range_valid",
                        "\"StatementTo\" >= \"StatementFrom\""
                    );
                    table.ForeignKey(
                        name: "FK_bank_statements_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TxnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DrAccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    CrAccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    SourceType = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalRef = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    ReconciledDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_transactions", x => x.Id);
                    table.CheckConstraint("ck_banktxn_amount_nonzero", "\"Amount\" <> 0");
                    table.CheckConstraint(
                        "ck_banktxn_dr_neq_cr",
                        "\"DrAccountNo\" <> \"CrAccountNo\""
                    );
                    table.CheckConstraint(
                        "ck_banktxn_reconciled_requires_date",
                        "(NOT \"IsReconciled\") OR (\"ReconciledDate\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "FK_bank_transactions_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_bank_transactions_gl_accounts_CrAccountNo",
                        column: x => x.CrAccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_bank_transactions_gl_accounts_DrAccountNo",
                        column: x => x.DrAccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    Name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Type = table.Column<int>(type: "item_type", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Uom = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    DefaultPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    DefaultRevenueAccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    DefaultExpenseAccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                    table.CheckConstraint("ck_item_defaultprice_nonneg", "\"DefaultPrice\" >= 0");
                    table.CheckConstraint("ck_item_name_not_blank", "btrim(\"Name\") <> ''");
                    table.CheckConstraint("ck_item_sku_not_blank", "btrim(\"Sku\") <> ''");
                    table.CheckConstraint("ck_item_uom_upper", "\"Uom\" = upper(\"Uom\")");
                    table.ForeignKey(
                        name: "FK_items_gl_accounts_DefaultExpenseAccountNo",
                        column: x => x.DefaultExpenseAccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_items_gl_accounts_DefaultRevenueAccountNo",
                        column: x => x.DefaultRevenueAccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_items_item_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "item_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_payment_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApPaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountTaken = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    WriteOffAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_payment_allocations", x => x.Id);
                    table.CheckConstraint("ck_apalloc_amount_positive", "\"AmountApplied\" > 0");
                    table.CheckConstraint(
                        "ck_apalloc_discount_nonneg",
                        "\"DiscountTaken\" IS NULL OR \"DiscountTaken\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_apalloc_has_value",
                        "(\"AmountApplied\" > 0) OR (COALESCE(\"DiscountTaken\",0) > 0) OR (COALESCE(\"WriteOffAmount\",0) > 0)"
                    );
                    table.CheckConstraint(
                        "ck_apalloc_writeoff_nonneg",
                        "\"WriteOffAmount\" IS NULL OR \"WriteOffAmount\" >= 0"
                    );
                    table.ForeignKey(
                        name: "FK_ap_payment_allocations_ap_invoices_ApInvoiceId",
                        column: x => x.ApInvoiceId,
                        principalTable: "ap_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ap_payment_allocations_ap_payments_ApPaymentId",
                        column: x => x.ApPaymentId,
                        principalTable: "ap_payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_receipt_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountGiven = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    WriteOffAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_receipt_allocations", x => x.Id);
                    table.CheckConstraint("ck_aralloc_amount_positive", "\"AmountApplied\" > 0");
                    table.CheckConstraint(
                        "ck_aralloc_discount_nonneg",
                        "\"DiscountGiven\" IS NULL OR \"DiscountGiven\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_aralloc_has_value",
                        "(\"AmountApplied\" > 0) OR (COALESCE(\"DiscountGiven\", 0) > 0) OR (COALESCE(\"WriteOffAmount\", 0) > 0)"
                    );
                    table.CheckConstraint(
                        "ck_aralloc_writeoff_nonneg",
                        "\"WriteOffAmount\" IS NULL OR \"WriteOffAmount\" >= 0"
                    );
                    table.ForeignKey(
                        name: "FK_ar_receipt_allocations_ar_invoices_ArInvoiceId",
                        column: x => x.ArInvoiceId,
                        principalTable: "ar_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ar_receipt_allocations_ar_receipts_ArReceiptId",
                        column: x => x.ArReceiptId,
                        principalTable: "ar_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_statement_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankStatementId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TxnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    Reference = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    Counterparty = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    ExternalLineId = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    RawCode = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    BankTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatchStatus = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_statement_lines", x => x.Id);
                    table.CheckConstraint("ck_bankstmtline_amount_nonzero", "\"Amount\" <> 0");
                    table.CheckConstraint(
                        "ck_bankstmtline_matchstatus_allowed",
                        "\"MatchStatus\" IN ('unmatched','proposed','matched','ignored')"
                    );
                    table.ForeignKey(
                        name: "FK_bank_statement_lines_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_bank_statement_lines_bank_statements_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "bank_statements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_bank_statement_lines_bank_transactions_BankTransactionId",
                        column: x => x.BankTransactionId,
                        principalTable: "bank_transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_invoice_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    AccountNo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ap_invoice_lines", x => x.Id);
                    table.CheckConstraint(
                        "ck_apline_has_account_or_item",
                        "(\"AccountNo\" IS NOT NULL) OR (\"ItemId\" IS NOT NULL)"
                    );
                    table.CheckConstraint("ck_apline_lineno_positive", "\"LineNo\" > 0");
                    table.CheckConstraint("ck_apline_price_nonnegative", "\"UnitPrice\" >= 0");
                    table.CheckConstraint("ck_apline_qty_nonnegative", "\"Qty\" >= 0");
                    table.CheckConstraint("ck_apline_total_nonnegative", "\"LineTotal\" >= 0");
                    table.ForeignKey(
                        name: "FK_ap_invoice_lines_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_ap_invoice_lines_ap_invoices_ApInvoiceId",
                        column: x => x.ApInvoiceId,
                        principalTable: "ap_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ap_invoice_lines_gl_accounts_AccountNo",
                        column: x => x.AccountNo,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_ap_invoice_lines_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_invoice_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    Qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    RevenueAccount = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ar_invoice_lines", x => x.Id);
                    table.CheckConstraint(
                        "ck_arline_has_item_or_revenue",
                        "(\"ItemId\" IS NOT NULL) OR (\"RevenueAccount\" IS NOT NULL)"
                    );
                    table.CheckConstraint("ck_arline_lineno_positive", "\"LineNo\" > 0");
                    table.CheckConstraint("ck_arline_price_nonnegative", "\"UnitPrice\" >= 0");
                    table.CheckConstraint("ck_arline_qty_nonnegative", "\"Qty\" >= 0");
                    table.CheckConstraint("ck_arline_total_nonnegative", "\"LineTotal\" >= 0");
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_ar_invoices_ArInvoiceId",
                        column: x => x.ArInvoiceId,
                        principalTable: "ar_invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_gl_accounts_RevenueAccount",
                        column: x => x.RevenueAccount,
                        principalTable: "gl_accounts",
                        principalColumn: "AccountNo",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_ar_invoice_lines_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "inventory_ledgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrxDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(
                        type: "inventory_transaction_type",
                        nullable: false
                    ),
                    Qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_ledgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_ledgers_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_inventory_ledgers_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "stock_count_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedQty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CountedQty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    VarianceQty = table.Column<decimal>(
                        type: "numeric(18,4)",
                        nullable: false,
                        computedColumnSql: "\"CountedQty\" - \"ExpectedQty\"",
                        stored: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_count_lines", x => x.Id);
                    table.CheckConstraint("ck_scl_counted_nonneg", "\"CountedQty\" >= 0");
                    table.CheckConstraint("ck_scl_expected_nonneg", "\"ExpectedQty\" >= 0");
                    table.CheckConstraint("ck_scl_lineno_positive", "\"LineNo\" > 0");
                    table.ForeignKey(
                        name: "FK_stock_count_lines_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_stock_count_lines_stock_counts_StockCountId",
                        column: x => x.StockCountId,
                        principalTable: "stock_counts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_AccountNo",
                table: "ap_invoice_lines",
                column: "AccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_ApInvoiceId_LineNo",
                table: "ap_invoice_lines",
                columns: ["ApInvoiceId", "LineNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_ItemId",
                table: "ap_invoice_lines",
                column: "ItemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_ItemId_AccountNo",
                table: "ap_invoice_lines",
                columns: ["ItemId", "AccountNo"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoice_lines_TaxRateId",
                table: "ap_invoice_lines",
                column: "TaxRateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_CurrencyCode",
                table: "ap_invoices",
                column: "CurrencyCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_DocNo",
                table: "ap_invoices",
                column: "DocNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_DocStatus",
                table: "ap_invoices",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_InvoiceDate",
                table: "ap_invoices",
                column: "InvoiceDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_VendorId_DueDate",
                table: "ap_invoices",
                columns: ["VendorId", "DueDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_invoices_VendorId_InvoiceNo",
                table: "ap_invoices",
                columns: ["VendorId", "InvoiceNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payment_allocations_ApInvoiceId",
                table: "ap_payment_allocations",
                column: "ApInvoiceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payment_allocations_ApInvoiceId_AllocationDate",
                table: "ap_payment_allocations",
                columns: ["ApInvoiceId", "AllocationDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payment_allocations_ApPaymentId",
                table: "ap_payment_allocations",
                column: "ApPaymentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payment_allocations_ApPaymentId_AllocationDate",
                table: "ap_payment_allocations",
                columns: ["ApPaymentId", "AllocationDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_BankAccountId_PaymentDate",
                table: "ap_payments",
                columns: ["BankAccountId", "PaymentDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_BankAccountId_PostingDate",
                table: "ap_payments",
                columns: ["BankAccountId", "PostingDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_ClearedDate",
                table: "ap_payments",
                column: "ClearedDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_DocNo",
                table: "ap_payments",
                column: "DocNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_DocStatus",
                table: "ap_payments",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_PaymentDate",
                table: "ap_payments",
                column: "PaymentDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_PostingDate",
                table: "ap_payments",
                column: "PostingDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_VendorId_PaymentDate",
                table: "ap_payments",
                columns: ["VendorId", "PaymentDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ap_payments_VendorId_PostingDate",
                table: "ap_payments",
                columns: ["VendorId", "PostingDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_ArInvoiceId_LineNo",
                table: "ar_invoice_lines",
                columns: ["ArInvoiceId", "LineNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_ItemId",
                table: "ar_invoice_lines",
                column: "ItemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_RevenueAccount",
                table: "ar_invoice_lines",
                column: "RevenueAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoice_lines_TaxRateId",
                table: "ar_invoice_lines",
                column: "TaxRateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_CurrencyCode",
                table: "ar_invoices",
                column: "CurrencyCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_CustomerId_DueDate",
                table: "ar_invoices",
                columns: ["CustomerId", "DueDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_DocNo",
                table: "ar_invoices",
                column: "DocNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_DocStatus",
                table: "ar_invoices",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_InvoiceDate",
                table: "ar_invoices",
                column: "InvoiceDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_invoices_PostingDate",
                table: "ar_invoices",
                column: "PostingDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipt_allocations_ArInvoiceId",
                table: "ar_receipt_allocations",
                column: "ArInvoiceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipt_allocations_ArInvoiceId_AllocationDate",
                table: "ar_receipt_allocations",
                columns: ["ArInvoiceId", "AllocationDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipt_allocations_ArReceiptId",
                table: "ar_receipt_allocations",
                column: "ArReceiptId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipt_allocations_ArReceiptId_AllocationDate",
                table: "ar_receipt_allocations",
                columns: ["ArReceiptId", "AllocationDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_BankAccountId_PostingDate",
                table: "ar_receipts",
                columns: ["BankAccountId", "PostingDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_BankAccountId_ReceiptDate",
                table: "ar_receipts",
                columns: ["BankAccountId", "ReceiptDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_ClearedDate",
                table: "ar_receipts",
                column: "ClearedDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_CurrencyCode",
                table: "ar_receipts",
                column: "CurrencyCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_CustomerId_PostingDate",
                table: "ar_receipts",
                columns: ["CustomerId", "PostingDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_CustomerId_ReceiptDate",
                table: "ar_receipts",
                columns: ["CustomerId", "ReceiptDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_DocNo",
                table: "ar_receipts",
                column: "DocNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_DocStatus",
                table: "ar_receipts",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_PostingDate",
                table: "ar_receipts",
                column: "PostingDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ar_receipts_ReceiptDate",
                table: "ar_receipts",
                column: "ReceiptDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_AccountNumber",
                table: "bank_accounts",
                column: "AccountNumber",
                unique: true,
                filter: "\"AccountNumber\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_CurrencyCode",
                table: "bank_accounts",
                column: "CurrencyCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_GlAccountNo",
                table: "bank_accounts",
                column: "GlAccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_IsActive",
                table: "bank_accounts",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_Name",
                table: "bank_accounts",
                column: "Name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_reconciliations_BankAccountId_StatementFrom_StatementTo",
                table: "bank_reconciliations",
                columns: ["BankAccountId", "StatementFrom", "StatementTo"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statement_lines_BankAccountId_TxnDate",
                table: "bank_statement_lines",
                columns: ["BankAccountId", "TxnDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statement_lines_BankStatementId_ExternalLineId",
                table: "bank_statement_lines",
                columns: ["BankStatementId", "ExternalLineId"],
                unique: true,
                filter: "\"ExternalLineId\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statement_lines_BankTransactionId",
                table: "bank_statement_lines",
                column: "BankTransactionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statement_lines_MatchStatus",
                table: "bank_statement_lines",
                column: "MatchStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statements_BankAccountId_StatementFrom_StatementTo",
                table: "bank_statements",
                columns: ["BankAccountId", "StatementFrom", "StatementTo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_statements_Source_ExternalId",
                table: "bank_statements",
                columns: ["Source", "ExternalId"],
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_BankAccountId_TxnDate",
                table: "bank_transactions",
                columns: ["BankAccountId", "TxnDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_CrAccountNo",
                table: "bank_transactions",
                column: "CrAccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_DrAccountNo",
                table: "bank_transactions",
                column: "DrAccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_IsReconciled",
                table: "bank_transactions",
                column: "IsReconciled"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_ReconciledDate",
                table: "bank_transactions",
                column: "ReconciledDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_bank_transactions_SourceType_SourceId",
                table: "bank_transactions",
                columns: ["SourceType", "SourceId"],
                unique: true,
                filter: "\"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_currencies_IsActive",
                table: "currencies",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_currencies_NumericCode",
                table: "currencies",
                column: "NumericCode",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_CustomerCode",
                table: "customers",
                column: "CustomerCode",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_Email",
                table: "customers",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_IsActive",
                table: "customers",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_IsOnHold",
                table: "customers",
                column: "IsOnHold"
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_Name",
                table: "customers",
                column: "Name"
            );

            migrationBuilder.CreateIndex(
                name: "IX_customers_TaxNumber",
                table: "customers",
                column: "TaxNumber",
                unique: true,
                filter: "\"TaxNumber\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_periods_FiscalYear",
                table: "fiscal_periods",
                column: "FiscalYear"
            );

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_periods_FiscalYear_PeriodNo",
                table: "fiscal_periods",
                columns: ["FiscalYear", "PeriodNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_periods_PeriodEnd",
                table: "fiscal_periods",
                column: "PeriodEnd"
            );

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_periods_PeriodStart",
                table: "fiscal_periods",
                column: "PeriodStart"
            );

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_periods_Status",
                table: "fiscal_periods",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_accounts_IsActive",
                table: "gl_accounts",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_accounts_Name",
                table: "gl_accounts",
                column: "Name"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_accounts_ParentAccount",
                table: "gl_accounts",
                column: "ParentAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_accounts_Type",
                table: "gl_accounts",
                column: "Type"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_entries_AccountNo_EntryDate",
                table: "gl_entries",
                columns: ["AccountNo", "EntryDate"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_entries_PeriodId",
                table: "gl_entries",
                column: "PeriodId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_entries_PeriodId_AccountNo",
                table: "gl_entries",
                columns: ["PeriodId", "AccountNo"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_entries_SourceType_SourceId",
                table: "gl_entries",
                columns: ["SourceType", "SourceId"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journal_lines_AccountNo",
                table: "gl_journal_lines",
                column: "AccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journal_lines_JournalId_LineNo",
                table: "gl_journal_lines",
                columns: ["JournalId", "LineNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journal_lines_SignedAmount",
                table: "gl_journal_lines",
                column: "SignedAmount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journals_DocStatus",
                table: "gl_journals",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journals_JournalDate",
                table: "gl_journals",
                column: "JournalDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journals_JournalNo",
                table: "gl_journals",
                column: "JournalNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journals_PeriodId",
                table: "gl_journals",
                column: "PeriodId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_gl_journals_PostingDate",
                table: "gl_journals",
                column: "PostingDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_inventory_ledgers_ItemId",
                table: "inventory_ledgers",
                column: "ItemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_inventory_ledgers_LocationId",
                table: "inventory_ledgers",
                column: "LocationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_AdjustAccount",
                table: "item_categories",
                column: "AdjustAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_Code",
                table: "item_categories",
                column: "Code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_CogsAccount",
                table: "item_categories",
                column: "CogsAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_InventoryAccount",
                table: "item_categories",
                column: "InventoryAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_Name",
                table: "item_categories",
                column: "Name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_item_categories_RevenueAccount",
                table: "item_categories",
                column: "RevenueAccount"
            );

            migrationBuilder.CreateIndex(
                name: "IX_items_CategoryId",
                table: "items",
                column: "CategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_items_DefaultExpenseAccountNo",
                table: "items",
                column: "DefaultExpenseAccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_items_DefaultRevenueAccountNo",
                table: "items",
                column: "DefaultRevenueAccountNo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_items_IsActive",
                table: "items",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(name: "IX_items_Name", table: "items", column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_items_Sku",
                table: "items",
                column: "Sku",
                unique: true
            );

            migrationBuilder.CreateIndex(name: "IX_items_Type", table: "items", column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_locations_Code",
                table: "locations",
                column: "Code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_locations_IsActive",
                table: "locations",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_locations_Name",
                table: "locations",
                column: "Name"
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_count_lines_ItemId",
                table: "stock_count_lines",
                column: "ItemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_count_lines_StockCountId_ItemId",
                table: "stock_count_lines",
                columns: ["StockCountId", "ItemId"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_count_lines_StockCountId_LineNo",
                table: "stock_count_lines",
                columns: ["StockCountId", "LineNo"],
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_counts_CountNo",
                table: "stock_counts",
                column: "CountNo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_counts_DocStatus",
                table: "stock_counts",
                column: "DocStatus"
            );

            migrationBuilder.CreateIndex(
                name: "IX_stock_counts_LocationId_ScheduledOn",
                table: "stock_counts",
                columns: ["LocationId", "ScheduledOn"]
            );

            migrationBuilder.CreateIndex(
                name: "IX_vendors_Email",
                table: "vendors",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_vendors_IsActive",
                table: "vendors",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(name: "IX_vendors_Name", table: "vendors", column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_vendors_TaxNumber",
                table: "vendors",
                column: "TaxNumber",
                unique: true,
                filter: "\"TaxNumber\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_vendors_VendorCode",
                table: "vendors",
                column: "VendorCode",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_licenses_tenants_TenantId",
                table: "licenses",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                    DO $$ BEGIN
                        IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'inventory_transaction_type') THEN
                            DROP TYPE inventory_transaction_type;
                        END IF;
                        IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'item_type') THEN
                            DROP TYPE item_type;
                        END IF;
                        IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'doc_status') THEN
                            DROP TYPE doc_status;
                        END IF;
                        IF EXISTS (SELECT 1 FROM pg_type WHERE typname = 'gl_account_type') THEN
                            DROP TYPE gl_account_type;
                        END IF;
                    END $$;
                """
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_tenants_TenantId",
                table: "AspNetUsers"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_licenses_tenants_TenantId",
                table: "licenses"
            );

            migrationBuilder.DropTable(name: "ap_invoice_lines");

            migrationBuilder.DropTable(name: "ap_payment_allocations");

            migrationBuilder.DropTable(name: "app_settings");

            migrationBuilder.DropTable(name: "ar_invoice_lines");

            migrationBuilder.DropTable(name: "ar_receipt_allocations");

            migrationBuilder.DropTable(name: "bank_reconciliations");

            migrationBuilder.DropTable(name: "bank_statement_lines");

            migrationBuilder.DropTable(name: "gl_entries");

            migrationBuilder.DropTable(name: "gl_journal_lines");

            migrationBuilder.DropTable(name: "inventory_ledgers");

            migrationBuilder.DropTable(name: "stock_count_lines");

            migrationBuilder.DropTable(name: "ap_invoices");

            migrationBuilder.DropTable(name: "ap_payments");

            migrationBuilder.DropTable(name: "TaxRates");

            migrationBuilder.DropTable(name: "ar_invoices");

            migrationBuilder.DropTable(name: "ar_receipts");

            migrationBuilder.DropTable(name: "bank_statements");

            migrationBuilder.DropTable(name: "bank_transactions");

            migrationBuilder.DropTable(name: "gl_journals");

            migrationBuilder.DropTable(name: "items");

            migrationBuilder.DropTable(name: "stock_counts");

            migrationBuilder.DropTable(name: "vendors");

            migrationBuilder.DropTable(name: "customers");

            migrationBuilder.DropTable(name: "bank_accounts");

            migrationBuilder.DropTable(name: "fiscal_periods");

            migrationBuilder.DropTable(name: "item_categories");

            migrationBuilder.DropTable(name: "locations");

            migrationBuilder.DropTable(name: "currencies");

            migrationBuilder.DropTable(name: "gl_accounts");

            migrationBuilder.DropPrimaryKey(name: "PK_tenants", table: "tenants");

            migrationBuilder.DropPrimaryKey(name: "PK_licenses", table: "licenses");

            migrationBuilder.DropColumn(name: "xmin", table: "tenants");

            migrationBuilder.DropColumn(name: "xmin", table: "licenses");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetUserTokens");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetUserRoles");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetUserLogins");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetUserClaims");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetRoles");

            migrationBuilder.DropColumn(name: "xmin", table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(name: "tenants", newName: "Tenants");

            migrationBuilder.RenameTable(name: "licenses", newName: "Licenses");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_TenantId",
                table: "Licenses",
                newName: "IX_Licenses_TenantId"
            );

            migrationBuilder
                .AlterDatabase()
                .OldAnnotation("Npgsql:Enum:doc_status.doc_status", "draft,posted,voided,closed")
                .OldAnnotation(
                    "Npgsql:Enum:gl_account_type.gl_account_type",
                    "asset,liability,equity,revenue,expense"
                )
                .OldAnnotation(
                    "Npgsql:Enum:inventory_transaction_type.inventory_transaction_type",
                    "receipt,issue,adjustment,transfer_in,transfer_out,sales_cogs"
                )
                .OldAnnotation("Npgsql:Enum:item_type.item_type", "stock,nonstock,service");

            migrationBuilder.AddPrimaryKey(name: "PK_Tenants", table: "Tenants", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Licenses", table: "Licenses", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Licenses_Tenants_TenantId",
                table: "Licenses",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}