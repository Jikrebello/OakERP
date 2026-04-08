using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OakERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:doc_status", "draft,posted,voided,closed")
                .Annotation("Npgsql:Enum:gl_account_type", "asset,liability,equity,revenue,expense")
                .Annotation(
                    "Npgsql:Enum:inventory_transaction_type",
                    "receipt,issue,adjustment,transfer_in,transfer_out,sales_cogs"
                )
                .Annotation("Npgsql:Enum:item_type", "stock,nonstock,service");

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    key = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: false
                    ),
                    value_json = table.Column<string>(type: "jsonb", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamptz",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.key);
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    normalized_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    numeric_code = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(80)",
                        maxLength: 80,
                        nullable: false
                    ),
                    symbol = table.Column<string>(
                        type: "character varying(8)",
                        maxLength: 8,
                        nullable: true
                    ),
                    decimals = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currencies", x => x.code);
                    table.CheckConstraint("ck_currency_code_len3", "char_length(\"code\") = 3");
                    table.CheckConstraint("ck_currency_code_upper", "\"code\" = upper(\"code\")");
                    table.CheckConstraint(
                        "ck_currency_decimals_range",
                        "\"decimals\" BETWEEN 0 AND 4"
                    );
                    table.CheckConstraint(
                        "ck_currency_numeric_range",
                        "\"numeric_code\" BETWEEN 1 AND 999"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_code = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    address = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    tax_number = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    terms_days = table.Column<int>(type: "integer", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_on_hold = table.Column<bool>(type: "boolean", nullable: false),
                    credit_hold_reason = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    credit_hold_until = table.Column<DateOnly>(type: "date", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.CheckConstraint(
                        "ck_customer_code_not_blank",
                        "btrim(\"customer_code\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_customer_creditlimit_nonneg",
                        "\"credit_limit\" IS NULL OR \"credit_limit\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_customer_email_basic_shape",
                        "\"email\" IS NULL OR (position('@' in \"email\") > 1 AND position('.' in \"email\") > 3)"
                    );
                    table.CheckConstraint(
                        "ck_customer_hold_until_future",
                        "\"credit_hold_until\" IS NULL OR \"credit_hold_until\" >= CURRENT_DATE"
                    );
                    table.CheckConstraint("ck_customer_name_not_blank", "btrim(\"name\") <> ''");
                    table.CheckConstraint(
                        "ck_customer_termsdays_range",
                        "\"terms_days\" BETWEEN 0 AND 180"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "fiscal_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: false),
                    period_no = table.Column<int>(type: "integer", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fiscal_periods", x => x.id);
                    table.CheckConstraint(
                        "ck_fiscper_periodno_range",
                        "\"period_no\" BETWEEN 1 AND 12"
                    );
                    table.CheckConstraint(
                        "ck_fiscper_start_le_end",
                        "\"period_start\" <= \"period_end\""
                    );
                    table.CheckConstraint(
                        "ck_fiscper_status_allowed",
                        "\"status\" IN ('open','closed')"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_accounts",
                columns: table => new
                {
                    account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    type = table.Column<int>(type: "gl_account_type", nullable: false),
                    parent_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    is_control = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("ak_gl_accounts_account_no", x => x.account_no);
                    table.CheckConstraint("ck_glacct_name_not_blank", "btrim(\"name\") <> ''");
                    table.CheckConstraint(
                        "ck_glacct_no_chars",
                        "\"account_no\" ~ '^[A-Z0-9][A-Z0-9\\.-]{0,19}$'"
                    );
                    table.CheckConstraint("ck_glacct_no_not_blank", "btrim(\"account_no\") <> ''");
                    table.CheckConstraint(
                        "ck_glacct_no_upper",
                        "\"account_no\" = upper(\"account_no\")"
                    );
                    table.CheckConstraint(
                        "ck_glacct_parent_not_self",
                        "\"parent_account\" IS NULL OR \"parent_account\" <> \"account_no\""
                    );
                    table.ForeignKey(
                        name: "fk_gl_accounts_gl_accounts_parent_account",
                        column: x => x.parent_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                    table.CheckConstraint("ck_location_code_not_blank", "btrim(\"code\") <> ''");
                    table.CheckConstraint("ck_location_code_upper", "\"code\" = upper(\"code\")");
                    table.CheckConstraint("ck_location_name_not_blank", "btrim(\"name\") <> ''");
                }
            );

            migrationBuilder.CreateTable(
                name: "tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: false
                    ),
                    rate_percent = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    is_input = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    rate_fraction = table.Column<decimal>(
                        type: "numeric(9,6)",
                        nullable: false,
                        computedColumnSql: "(\"rate_percent\" / 100.0)",
                        stored: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_rates", x => x.id);
                    table.CheckConstraint(
                        "ck_taxrate_dates",
                        "\"effective_to\" IS NULL OR \"effective_to\" >= \"effective_from\""
                    );
                    table.CheckConstraint("ck_taxrate_name_not_blank", "btrim(\"name\") <> ''");
                    table.CheckConstraint(
                        "ck_taxrate_pct_range",
                        "\"rate_percent\" >= 0 AND \"rate_percent\" <= 100"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "uuid_generate_v4()"
                    ),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "timezone('utc', now())"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vendors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_code = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    phone = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    address = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    tax_number = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    terms_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendors", x => x.id);
                    table.CheckConstraint(
                        "ck_vendor_code_not_blank",
                        "btrim(\"vendor_code\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_vendor_email_basic_shape",
                        "\"email\" IS NULL OR (position('@' in \"email\") > 1 AND position('.' in \"email\") > 3)"
                    );
                    table.CheckConstraint("ck_vendor_name_not_blank", "btrim(\"name\") <> ''");
                    table.CheckConstraint(
                        "ck_vendor_termsdays_range",
                        "\"terms_days\" BETWEEN 0 AND 180"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: true),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    currency_code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    ship_to = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    tax_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    doc_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ar_invoices", x => x.id);
                    table.CheckConstraint(
                        "ck_arinvoice_currency_len3",
                        "char_length(\"currency_code\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_due_after_invoice",
                        "\"due_date\" >= \"invoice_date\""
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_posted_requires_postingdate",
                        "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
                    );
                    table.CheckConstraint(
                        "ck_arinvoice_totals_nonnegative",
                        "(\"tax_total\" >= 0) AND (\"doc_total\" >= 0)"
                    );
                    table.ForeignKey(
                        name: "fk_ar_invoices_currencies_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currencies",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ar_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_journals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    journal_date = table.Column<DateOnly>(type: "date", nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: true),
                    period_id = table.Column<Guid>(type: "uuid", nullable: true),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gl_journals", x => x.id);
                    table.CheckConstraint(
                        "ck_gljournal_no_not_blank",
                        "btrim(\"journal_no\") <> ''"
                    );
                    table.CheckConstraint(
                        "ck_gljournal_posted_requires_postingdate",
                        "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
                    );
                    table.CheckConstraint(
                        "ck_gljournal_posting_ge_journal",
                        "\"posting_date\" IS NULL OR \"posting_date\" >= \"journal_date\""
                    );
                    table.ForeignKey(
                        name: "fk_gl_journals_fiscal_periods_period_id",
                        column: x => x.period_id,
                        principalTable: "fiscal_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    bank_name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: true
                    ),
                    account_number = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    gl_account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    opening_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency_code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_accounts", x => x.id);
                    table.CheckConstraint(
                        "ck_bankacct_currency_len3",
                        "char_length(\"currency_code\") = 3"
                    );
                    table.CheckConstraint("ck_bankacct_name_not_blank", "btrim(\"name\") <> ''");
                    table.ForeignKey(
                        name: "fk_bank_accounts_currencies_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currencies",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_bank_accounts_gl_accounts_gl_account_no",
                        column: x => x.gl_account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    source_type = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: false
                    ),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_no = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gl_entries", x => x.id);
                    table.CheckConstraint(
                        "ck_glentry_one_sided_amount",
                        "(\"debit\" >= 0) AND (\"credit\" >= 0) AND ((\"debit\" = 0 AND \"credit\" > 0) OR (\"credit\" = 0 AND \"debit\" > 0))"
                    );
                    table.CheckConstraint(
                        "ck_glentry_source_pairing",
                        "(\"source_type\" IS NULL) OR (\"source_id\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "fk_gl_entries_fiscal_periods_period_id",
                        column: x => x.period_id,
                        principalTable: "fiscal_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_gl_entries_gl_accounts_account_no",
                        column: x => x.account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "item_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    inventory_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    cogs_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    adjust_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    revenue_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_categories_gl_accounts_adjust_account",
                        column: x => x.adjust_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_item_categories_gl_accounts_cogs_account",
                        column: x => x.cogs_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_item_categories_gl_accounts_inventory_account",
                        column: x => x.inventory_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_item_categories_gl_accounts_revenue_account",
                        column: x => x.revenue_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "stock_counts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    count_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    scheduled_on = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_counts", x => x.id);
                    table.CheckConstraint("ck_sc_countno_not_blank", "btrim(\"count_no\") <> ''");
                    table.ForeignKey(
                        name: "fk_stock_counts_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    user_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    normalized_user_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    normalized_email = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "licenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    expiry_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_licenses_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    currency_code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    tax_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    doc_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ap_invoices", x => x.id);
                    table.CheckConstraint(
                        "ck_apinvoice_currency_len3",
                        "char_length(\"currency_code\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_apinvoice_due_after_invoice",
                        "\"due_date\" >= \"invoice_date\""
                    );
                    table.CheckConstraint(
                        "ck_apinvoice_totals_nonnegative",
                        "(\"tax_total\" >= 0) AND (\"doc_total\" >= 0)"
                    );
                    table.ForeignKey(
                        name: "fk_ap_invoices_currencies_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currencies",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ap_invoices_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "gl_journal_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    signed_amount = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        computedColumnSql: "(\"debit\" - \"credit\")",
                        stored: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gl_journal_lines", x => x.id);
                    table.CheckConstraint("ck_gjl_lineno_positive", "\"line_no\" > 0");
                    table.CheckConstraint(
                        "ck_gjl_one_sided_amount",
                        "(\"debit\" >= 0) AND (\"credit\" >= 0) AND ((\"debit\" = 0 AND \"credit\" > 0) OR (\"credit\" = 0 AND \"debit\" > 0))"
                    );
                    table.ForeignKey(
                        name: "fk_gl_journal_lines_gl_accounts_account_no",
                        column: x => x.account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_gl_journal_lines_gl_journals_journal_id",
                        column: x => x.journal_id,
                        principalTable: "gl_journals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ap_payments", x => x.id);
                    table.CheckConstraint("ck_appayment_amount_positive", "\"amount\" > 0");
                    table.CheckConstraint(
                        "ck_appayment_cleared_not_before_posting",
                        "(\"cleared_date\" IS NULL) OR (\"posting_date\" IS NULL) OR (\"cleared_date\" >= \"posting_date\")"
                    );
                    table.CheckConstraint(
                        "ck_appayment_posted_requires_postingdate",
                        "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "fk_ap_payments_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ap_payments_vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doc_no = table.Column<string>(
                        type: "character varying(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_date = table.Column<DateOnly>(type: "date", nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency_code = table.Column<string>(
                        type: "character varying(3)",
                        maxLength: 3,
                        nullable: false
                    ),
                    amount_foreign = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    exchange_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    doc_status = table.Column<int>(type: "doc_status", nullable: false),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ar_receipts", x => x.id);
                    table.CheckConstraint("ck_arreceipt_amount_positive", "\"amount\" > 0");
                    table.CheckConstraint(
                        "ck_arreceipt_cleared_not_before_posting",
                        "(\"cleared_date\" IS NULL) OR (\"posting_date\" IS NULL) OR (\"cleared_date\" >= \"posting_date\")"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_currency_len3",
                        "char_length(\"currency_code\") = 3"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_consistency",
                        "(\"amount_foreign\" IS NULL OR \"exchange_rate\" IS NULL) OR abs((\"amount_foreign\" * \"exchange_rate\") - \"amount\") <= 0.01"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_pair_nullness",
                        "(\"amount_foreign\" IS NULL) = (\"exchange_rate\" IS NULL)"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_fx_positive",
                        "(\"amount_foreign\" IS NULL OR \"amount_foreign\" > 0) AND (\"exchange_rate\" IS NULL OR \"exchange_rate\" > 0)"
                    );
                    table.CheckConstraint(
                        "ck_arreceipt_posted_requires_postingdate",
                        "(\"doc_status\" <> 'posted'::doc_status) OR (\"posting_date\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "fk_ar_receipts_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ar_receipts_currencies_currency_code",
                        column: x => x.currency_code,
                        principalTable: "currencies",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ar_receipts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_reconciliations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    statement_from = table.Column<DateOnly>(type: "date", nullable: false),
                    statement_to = table.Column<DateOnly>(type: "date", nullable: false),
                    opening_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    closing_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true
                    ),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_reconciliations", x => x.id);
                    table.CheckConstraint(
                        "ck_bankrec_range_valid",
                        "\"statement_to\" >= \"statement_from\""
                    );
                    table.ForeignKey(
                        name: "fk_bank_reconciliations_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_statements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    statement_from = table.Column<DateOnly>(type: "date", nullable: false),
                    statement_to = table.Column<DateOnly>(type: "date", nullable: false),
                    source = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    external_id = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    file_name = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    notes = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true
                    ),
                    opening_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    closing_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    imported_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_statements", x => x.id);
                    table.CheckConstraint(
                        "ck_bankstmt_range_valid",
                        "\"statement_to\" >= \"statement_from\""
                    );
                    table.ForeignKey(
                        name: "fk_bank_statements_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dr_account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    cr_account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    source_type = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_ref = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    is_reconciled = table.Column<bool>(type: "boolean", nullable: false),
                    reconciled_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_transactions", x => x.id);
                    table.CheckConstraint("ck_banktxn_amount_nonzero", "\"amount\" <> 0");
                    table.CheckConstraint(
                        "ck_banktxn_dr_neq_cr",
                        "\"dr_account_no\" <> \"cr_account_no\""
                    );
                    table.CheckConstraint(
                        "ck_banktxn_reconciled_requires_date",
                        "(NOT \"is_reconciled\") OR (\"reconciled_date\" IS NOT NULL)"
                    );
                    table.ForeignKey(
                        name: "fk_bank_transactions_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_bank_transactions_gl_accounts_cr_account_no",
                        column: x => x.cr_account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_bank_transactions_gl_accounts_dr_account_no",
                        column: x => x.dr_account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    type = table.Column<int>(type: "item_type", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    uom = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    default_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now() at time zone 'utc'"
                    ),
                    default_revenue_account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    default_expense_account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.id);
                    table.CheckConstraint("ck_item_defaultprice_nonneg", "\"default_price\" >= 0");
                    table.CheckConstraint("ck_item_name_not_blank", "btrim(\"name\") <> ''");
                    table.CheckConstraint("ck_item_sku_not_blank", "btrim(\"sku\") <> ''");
                    table.CheckConstraint("ck_item_uom_upper", "\"uom\" = upper(\"uom\")");
                    table.ForeignKey(
                        name: "fk_items_gl_accounts_default_expense_account_no",
                        column: x => x.default_expense_account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_items_gl_accounts_default_revenue_account_no",
                        column: x => x.default_revenue_account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_items_item_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "item_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_asp_net_user_logins",
                        x => new { x.login_provider, x.provider_key }
                    );
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_asp_net_user_tokens",
                        x => new
                        {
                            x.user_id,
                            x.login_provider,
                            x.name,
                        }
                    );
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_payment_allocations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ap_payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ap_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount_applied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_taken = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    write_off_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ap_payment_allocations", x => x.id);
                    table.CheckConstraint("ck_apalloc_amount_positive", "\"amount_applied\" > 0");
                    table.CheckConstraint(
                        "ck_apalloc_discount_nonneg",
                        "\"discount_taken\" IS NULL OR \"discount_taken\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_apalloc_has_value",
                        "(\"amount_applied\" > 0) OR (COALESCE(\"discount_taken\",0) > 0) OR (COALESCE(\"write_off_amount\",0) > 0)"
                    );
                    table.CheckConstraint(
                        "ck_apalloc_writeoff_nonneg",
                        "\"write_off_amount\" IS NULL OR \"write_off_amount\" >= 0"
                    );
                    table.ForeignKey(
                        name: "fk_ap_payment_allocations_ap_invoices_ap_invoice_id",
                        column: x => x.ap_invoice_id,
                        principalTable: "ap_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ap_payment_allocations_ap_payments_ap_payment_id",
                        column: x => x.ap_payment_id,
                        principalTable: "ap_payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_receipt_allocations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount_applied = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_given = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    write_off_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    memo = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ar_receipt_allocations", x => x.id);
                    table.CheckConstraint("ck_aralloc_amount_positive", "\"amount_applied\" > 0");
                    table.CheckConstraint(
                        "ck_aralloc_discount_nonneg",
                        "\"discount_given\" IS NULL OR \"discount_given\" >= 0"
                    );
                    table.CheckConstraint(
                        "ck_aralloc_has_value",
                        "(\"amount_applied\" > 0) OR (COALESCE(\"discount_given\", 0) > 0) OR (COALESCE(\"write_off_amount\", 0) > 0)"
                    );
                    table.CheckConstraint(
                        "ck_aralloc_writeoff_nonneg",
                        "\"write_off_amount\" IS NULL OR \"write_off_amount\" >= 0"
                    );
                    table.ForeignKey(
                        name: "fk_ar_receipt_allocations_ar_invoices_ar_invoice_id",
                        column: x => x.ar_invoice_id,
                        principalTable: "ar_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_ar_receipt_allocations_ar_receipts_ar_receipt_id",
                        column: x => x.ar_receipt_id,
                        principalTable: "ar_receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "bank_statement_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    reference = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    counterparty = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true
                    ),
                    external_line_id = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: true
                    ),
                    raw_code = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: true
                    ),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    match_status = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_statement_lines", x => x.id);
                    table.CheckConstraint("ck_bankstmtline_amount_nonzero", "\"amount\" <> 0");
                    table.CheckConstraint(
                        "ck_bankstmtline_matchstatus_allowed",
                        "\"match_status\" IN ('unmatched','proposed','matched','ignored')"
                    );
                    table.ForeignKey(
                        name: "fk_bank_statement_lines_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_bank_statement_lines_bank_statements_bank_statement_id",
                        column: x => x.bank_statement_id,
                        principalTable: "bank_statements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_bank_statement_lines_bank_transactions_bank_transaction_id",
                        column: x => x.bank_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ap_invoice_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ap_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    account_no = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tax_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ap_invoice_lines", x => x.id);
                    table.CheckConstraint(
                        "ck_apline_has_account_or_item",
                        "(\"account_no\" IS NOT NULL) OR (\"item_id\" IS NOT NULL)"
                    );
                    table.CheckConstraint("ck_apline_lineno_positive", "\"line_no\" > 0");
                    table.CheckConstraint("ck_apline_price_nonnegative", "\"unit_price\" >= 0");
                    table.CheckConstraint("ck_apline_qty_nonnegative", "\"qty\" >= 0");
                    table.CheckConstraint("ck_apline_total_nonnegative", "\"line_total\" >= 0");
                    table.ForeignKey(
                        name: "fk_ap_invoice_lines_ap_invoices_ap_invoice_id",
                        column: x => x.ap_invoice_id,
                        principalTable: "ap_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_ap_invoice_lines_gl_accounts_account_no",
                        column: x => x.account_no,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_ap_invoice_lines_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_ap_invoice_lines_tax_rates_tax_rate_id",
                        column: x => x.tax_rate_id,
                        principalTable: "tax_rates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "ar_invoice_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ar_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    revenue_account = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    tax_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ar_invoice_lines", x => x.id);
                    table.CheckConstraint(
                        "ck_arline_has_item_or_revenue",
                        "(\"item_id\" IS NOT NULL) OR (\"revenue_account\" IS NOT NULL)"
                    );
                    table.CheckConstraint("ck_arline_lineno_positive", "\"line_no\" > 0");
                    table.CheckConstraint("ck_arline_price_nonnegative", "\"unit_price\" >= 0");
                    table.CheckConstraint("ck_arline_qty_nonnegative", "\"qty\" >= 0");
                    table.CheckConstraint("ck_arline_total_nonnegative", "\"line_total\" >= 0");
                    table.ForeignKey(
                        name: "fk_ar_invoice_lines_ar_invoices_ar_invoice_id",
                        column: x => x.ar_invoice_id,
                        principalTable: "ar_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_ar_invoice_lines_gl_accounts_revenue_account",
                        column: x => x.revenue_account,
                        principalTable: "gl_accounts",
                        principalColumn: "account_no",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_ar_invoice_lines_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_ar_invoice_lines_tax_rates_tax_rate_id",
                        column: x => x.tax_rate_id,
                        principalTable: "tax_rates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "inventory_ledgers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trx_date = table.Column<DateOnly>(type: "date", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_type = table.Column<int>(
                        type: "inventory_transaction_type",
                        nullable: false
                    ),
                    qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    value_change = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_ledgers", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_ledgers_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_inventory_ledgers_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "stock_count_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_count_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_no = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    counted_qty = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    variance_qty = table.Column<decimal>(
                        type: "numeric(18,4)",
                        nullable: false,
                        computedColumnSql: "\"counted_qty\" - \"expected_qty\"",
                        stored: true
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_count_lines", x => x.id);
                    table.CheckConstraint("ck_scl_counted_nonneg", "\"counted_qty\" >= 0");
                    table.CheckConstraint("ck_scl_expected_nonneg", "\"expected_qty\" >= 0");
                    table.CheckConstraint("ck_scl_lineno_positive", "\"line_no\" > 0");
                    table.ForeignKey(
                        name: "fk_stock_count_lines_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_stock_count_lines_stock_counts_stock_count_id",
                        column: x => x.stock_count_id,
                        principalTable: "stock_counts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoice_lines_account_no",
                table: "ap_invoice_lines",
                column: "account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoice_lines_ap_invoice_id_line_no",
                table: "ap_invoice_lines",
                columns: new[] { "ap_invoice_id", "line_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoice_lines_item_id",
                table: "ap_invoice_lines",
                column: "item_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoice_lines_item_id_account_no",
                table: "ap_invoice_lines",
                columns: new[] { "item_id", "account_no" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoice_lines_tax_rate_id",
                table: "ap_invoice_lines",
                column: "tax_rate_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_currency_code",
                table: "ap_invoices",
                column: "currency_code"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_doc_no",
                table: "ap_invoices",
                column: "doc_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_doc_status",
                table: "ap_invoices",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_invoice_date",
                table: "ap_invoices",
                column: "invoice_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_vendor_id_due_date",
                table: "ap_invoices",
                columns: new[] { "vendor_id", "due_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_invoices_vendor_id_invoice_no",
                table: "ap_invoices",
                columns: new[] { "vendor_id", "invoice_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payment_allocations_ap_invoice_id",
                table: "ap_payment_allocations",
                column: "ap_invoice_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payment_allocations_ap_invoice_id_allocation_date",
                table: "ap_payment_allocations",
                columns: new[] { "ap_invoice_id", "allocation_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payment_allocations_ap_payment_id",
                table: "ap_payment_allocations",
                column: "ap_payment_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payment_allocations_ap_payment_id_allocation_date",
                table: "ap_payment_allocations",
                columns: new[] { "ap_payment_id", "allocation_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_bank_account_id_payment_date",
                table: "ap_payments",
                columns: new[] { "bank_account_id", "payment_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_bank_account_id_posting_date",
                table: "ap_payments",
                columns: new[] { "bank_account_id", "posting_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_cleared_date",
                table: "ap_payments",
                column: "cleared_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_doc_no",
                table: "ap_payments",
                column: "doc_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_doc_status",
                table: "ap_payments",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_payment_date",
                table: "ap_payments",
                column: "payment_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_posting_date",
                table: "ap_payments",
                column: "posting_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_vendor_id_payment_date",
                table: "ap_payments",
                columns: new[] { "vendor_id", "payment_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ap_payments_vendor_id_posting_date",
                table: "ap_payments",
                columns: new[] { "vendor_id", "posting_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoice_lines_ar_invoice_id_line_no",
                table: "ar_invoice_lines",
                columns: new[] { "ar_invoice_id", "line_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoice_lines_item_id",
                table: "ar_invoice_lines",
                column: "item_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoice_lines_revenue_account",
                table: "ar_invoice_lines",
                column: "revenue_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoice_lines_tax_rate_id",
                table: "ar_invoice_lines",
                column: "tax_rate_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_currency_code",
                table: "ar_invoices",
                column: "currency_code"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_customer_id_due_date",
                table: "ar_invoices",
                columns: new[] { "customer_id", "due_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_doc_no",
                table: "ar_invoices",
                column: "doc_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_doc_status",
                table: "ar_invoices",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_invoice_date",
                table: "ar_invoices",
                column: "invoice_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoices_posting_date",
                table: "ar_invoices",
                column: "posting_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipt_allocations_ar_invoice_id",
                table: "ar_receipt_allocations",
                column: "ar_invoice_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipt_allocations_ar_invoice_id_allocation_date",
                table: "ar_receipt_allocations",
                columns: new[] { "ar_invoice_id", "allocation_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipt_allocations_ar_receipt_id",
                table: "ar_receipt_allocations",
                column: "ar_receipt_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipt_allocations_ar_receipt_id_allocation_date",
                table: "ar_receipt_allocations",
                columns: new[] { "ar_receipt_id", "allocation_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_bank_account_id_posting_date",
                table: "ar_receipts",
                columns: new[] { "bank_account_id", "posting_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_bank_account_id_receipt_date",
                table: "ar_receipts",
                columns: new[] { "bank_account_id", "receipt_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_cleared_date",
                table: "ar_receipts",
                column: "cleared_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_currency_code",
                table: "ar_receipts",
                column: "currency_code"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_customer_id_posting_date",
                table: "ar_receipts",
                columns: new[] { "customer_id", "posting_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_customer_id_receipt_date",
                table: "ar_receipts",
                columns: new[] { "customer_id", "receipt_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_doc_no",
                table: "ar_receipts",
                column: "doc_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_doc_status",
                table: "ar_receipts",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_posting_date",
                table: "ar_receipts",
                column: "posting_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_receipts_receipt_date",
                table: "ar_receipts",
                column: "receipt_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id"
            );

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email"
            );

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_tenant_id",
                table: "AspNetUsers",
                column: "tenant_id"
            );

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_account_number",
                table: "bank_accounts",
                column: "account_number",
                unique: true,
                filter: "\"account_number\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_currency_code",
                table: "bank_accounts",
                column: "currency_code"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_gl_account_no",
                table: "bank_accounts",
                column: "gl_account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_is_active",
                table: "bank_accounts",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_name",
                table: "bank_accounts",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_reconciliations_bank_account_id_statement_from_stateme",
                table: "bank_reconciliations",
                columns: new[] { "bank_account_id", "statement_from", "statement_to" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statement_lines_bank_account_id_txn_date",
                table: "bank_statement_lines",
                columns: new[] { "bank_account_id", "txn_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statement_lines_bank_statement_id_external_line_id",
                table: "bank_statement_lines",
                columns: new[] { "bank_statement_id", "external_line_id" },
                unique: true,
                filter: "\"external_line_id\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statement_lines_bank_transaction_id",
                table: "bank_statement_lines",
                column: "bank_transaction_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statement_lines_match_status",
                table: "bank_statement_lines",
                column: "match_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statements_bank_account_id_statement_from_statement_to",
                table: "bank_statements",
                columns: new[] { "bank_account_id", "statement_from", "statement_to" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_statements_source_external_id",
                table: "bank_statements",
                columns: new[] { "source", "external_id" },
                unique: true,
                filter: "\"external_id\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_bank_account_id_txn_date",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "txn_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_cr_account_no",
                table: "bank_transactions",
                column: "cr_account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_dr_account_no",
                table: "bank_transactions",
                column: "dr_account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_is_reconciled",
                table: "bank_transactions",
                column: "is_reconciled"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_reconciled_date",
                table: "bank_transactions",
                column: "reconciled_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_source_type_source_id",
                table: "bank_transactions",
                columns: new[] { "source_type", "source_id" },
                unique: true,
                filter: "\"source_type\" IS NOT NULL AND \"source_id\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_currencies_is_active",
                table: "currencies",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_currencies_numeric_code",
                table: "currencies",
                column: "numeric_code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_customer_code",
                table: "customers",
                column: "customer_code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_email",
                table: "customers",
                column: "email",
                unique: true,
                filter: "\"email\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_is_active",
                table: "customers",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_is_on_hold",
                table: "customers",
                column: "is_on_hold"
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_name",
                table: "customers",
                column: "name"
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_tax_number",
                table: "customers",
                column: "tax_number",
                unique: true,
                filter: "\"tax_number\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_periods_fiscal_year",
                table: "fiscal_periods",
                column: "fiscal_year"
            );

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_periods_fiscal_year_period_no",
                table: "fiscal_periods",
                columns: new[] { "fiscal_year", "period_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_periods_period_end",
                table: "fiscal_periods",
                column: "period_end"
            );

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_periods_period_start",
                table: "fiscal_periods",
                column: "period_start"
            );

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_periods_status",
                table: "fiscal_periods",
                column: "status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_accounts_is_active",
                table: "gl_accounts",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_accounts_name",
                table: "gl_accounts",
                column: "name"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_accounts_parent_account",
                table: "gl_accounts",
                column: "parent_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_accounts_type",
                table: "gl_accounts",
                column: "type"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_entries_account_no_entry_date",
                table: "gl_entries",
                columns: new[] { "account_no", "entry_date" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_entries_period_id",
                table: "gl_entries",
                column: "period_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_entries_period_id_account_no",
                table: "gl_entries",
                columns: new[] { "period_id", "account_no" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_entries_source_type_source_id",
                table: "gl_entries",
                columns: new[] { "source_type", "source_id" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journal_lines_account_no",
                table: "gl_journal_lines",
                column: "account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journal_lines_journal_id_line_no",
                table: "gl_journal_lines",
                columns: new[] { "journal_id", "line_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journal_lines_signed_amount",
                table: "gl_journal_lines",
                column: "signed_amount"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journals_doc_status",
                table: "gl_journals",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journals_journal_date",
                table: "gl_journals",
                column: "journal_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journals_journal_no",
                table: "gl_journals",
                column: "journal_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journals_period_id",
                table: "gl_journals",
                column: "period_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_gl_journals_posting_date",
                table: "gl_journals",
                column: "posting_date"
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_ledgers_item_id",
                table: "inventory_ledgers",
                column: "item_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_ledgers_location_id",
                table: "inventory_ledgers",
                column: "location_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_adjust_account",
                table: "item_categories",
                column: "adjust_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_code",
                table: "item_categories",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_cogs_account",
                table: "item_categories",
                column: "cogs_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_inventory_account",
                table: "item_categories",
                column: "inventory_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_name",
                table: "item_categories",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_item_categories_revenue_account",
                table: "item_categories",
                column: "revenue_account"
            );

            migrationBuilder.CreateIndex(
                name: "ix_items_category_id",
                table: "items",
                column: "category_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_items_default_expense_account_no",
                table: "items",
                column: "default_expense_account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_items_default_revenue_account_no",
                table: "items",
                column: "default_revenue_account_no"
            );

            migrationBuilder.CreateIndex(
                name: "ix_items_is_active",
                table: "items",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(name: "ix_items_name", table: "items", column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_items_sku",
                table: "items",
                column: "sku",
                unique: true
            );

            migrationBuilder.CreateIndex(name: "ix_items_type", table: "items", column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_licenses_tenant_id",
                table: "licenses",
                column: "tenant_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_locations_code",
                table: "locations",
                column: "code",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_locations_is_active",
                table: "locations",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_locations_name",
                table: "locations",
                column: "name"
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_item_id",
                table: "stock_count_lines",
                column: "item_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_stock_count_id_item_id",
                table: "stock_count_lines",
                columns: new[] { "stock_count_id", "item_id" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_count_lines_stock_count_id_line_no",
                table: "stock_count_lines",
                columns: new[] { "stock_count_id", "line_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_count_no",
                table: "stock_counts",
                column: "count_no",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_doc_status",
                table: "stock_counts",
                column: "doc_status"
            );

            migrationBuilder.CreateIndex(
                name: "ix_stock_counts_location_id_scheduled_on",
                table: "stock_counts",
                columns: new[] { "location_id", "scheduled_on" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_effective_from",
                table: "tax_rates",
                column: "effective_from"
            );

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_is_active",
                table: "tax_rates",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_tax_rates_name_effective_from",
                table: "tax_rates",
                columns: new[] { "name", "effective_from" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vendors_email",
                table: "vendors",
                column: "email",
                unique: true,
                filter: "\"email\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vendors_is_active",
                table: "vendors",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(name: "ix_vendors_name", table: "vendors", column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_tax_number",
                table: "vendors",
                column: "tax_number",
                unique: true,
                filter: "\"tax_number\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vendors_vendor_code",
                table: "vendors",
                column: "vendor_code",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ap_invoice_lines");

            migrationBuilder.DropTable(name: "ap_payment_allocations");

            migrationBuilder.DropTable(name: "app_settings");

            migrationBuilder.DropTable(name: "ar_invoice_lines");

            migrationBuilder.DropTable(name: "ar_receipt_allocations");

            migrationBuilder.DropTable(name: "AspNetRoleClaims");

            migrationBuilder.DropTable(name: "AspNetUserClaims");

            migrationBuilder.DropTable(name: "AspNetUserLogins");

            migrationBuilder.DropTable(name: "AspNetUserRoles");

            migrationBuilder.DropTable(name: "AspNetUserTokens");

            migrationBuilder.DropTable(name: "bank_reconciliations");

            migrationBuilder.DropTable(name: "bank_statement_lines");

            migrationBuilder.DropTable(name: "gl_entries");

            migrationBuilder.DropTable(name: "gl_journal_lines");

            migrationBuilder.DropTable(name: "inventory_ledgers");

            migrationBuilder.DropTable(name: "licenses");

            migrationBuilder.DropTable(name: "stock_count_lines");

            migrationBuilder.DropTable(name: "ap_invoices");

            migrationBuilder.DropTable(name: "ap_payments");

            migrationBuilder.DropTable(name: "tax_rates");

            migrationBuilder.DropTable(name: "ar_invoices");

            migrationBuilder.DropTable(name: "ar_receipts");

            migrationBuilder.DropTable(name: "AspNetRoles");

            migrationBuilder.DropTable(name: "AspNetUsers");

            migrationBuilder.DropTable(name: "bank_statements");

            migrationBuilder.DropTable(name: "bank_transactions");

            migrationBuilder.DropTable(name: "gl_journals");

            migrationBuilder.DropTable(name: "items");

            migrationBuilder.DropTable(name: "stock_counts");

            migrationBuilder.DropTable(name: "vendors");

            migrationBuilder.DropTable(name: "customers");

            migrationBuilder.DropTable(name: "tenants");

            migrationBuilder.DropTable(name: "bank_accounts");

            migrationBuilder.DropTable(name: "fiscal_periods");

            migrationBuilder.DropTable(name: "item_categories");

            migrationBuilder.DropTable(name: "locations");

            migrationBuilder.DropTable(name: "currencies");

            migrationBuilder.DropTable(name: "gl_accounts");
        }
    }
}
