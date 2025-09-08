using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHONE_STORE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartsAndOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HEHE");

            migrationBuilder.CreateTable(
                name: "BRANDS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    IS_ACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BRANDS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CARTS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    SESSION_ID = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CATEGORIES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PARENT_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    NAME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SORT_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IS_ACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORIES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMERS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    USER_ACCOUNT_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    PHONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    FULL_NAME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CUSTOMERS_USER_ACCOUNTS_USER_ACCOUNT_ID",
                        column: x => x.USER_ACCOUNT_ID,
                        principalTable: "USER_ACCOUNTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODE = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    SHIPPING_ADDRESS_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    SUBTOTAL = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    VAT_AMOUNT = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    SHIPPING_FEE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GRAND_TOTAL = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    PAID_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true),
                    SHIPPED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_ATTRIBUTES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODE = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    DATA_TYPE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_ATTRIBUTES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WAREHOUSES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODE = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    ADDRESS_LINE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    DISTRICT = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    PROVINCE = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    IS_ACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WAREHOUSES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CART_ITEMS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CART_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UNIT_PRICE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    CURRENCY = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false),
                    ADDED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CART_ITEMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CART_ITEMS_CARTS_CART_ID",
                        column: x => x.CART_ID,
                        principalSchema: "HEHE",
                        principalTable: "CARTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCTS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    BRAND_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    DEFAULT_CATEGORY_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SLUG = table.Column<string>(type: "NVARCHAR2(220)", maxLength: 220, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "CLOB", nullable: true),
                    SPEC_JSON = table.Column<string>(type: "CLOB", nullable: true),
                    IS_ACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_BRANDS_BRAND_ID",
                        column: x => x.BRAND_ID,
                        principalSchema: "HEHE",
                        principalTable: "BRANDS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRODUCTS_CATEGORIES_DEFAULT_CATEGORY_ID",
                        column: x => x.DEFAULT_CATEGORY_ID,
                        principalSchema: "HEHE",
                        principalTable: "CATEGORIES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ADDRESSES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    LABEL = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: true),
                    RECIPIENT = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PHONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    LINE1 = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    WARD = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    DISTRICT = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    PROVINCE = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    POSTAL_CODE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    ADDRESS_TYPE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    IS_DEFAULT = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADDRESSES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ADDRESSES_CUSTOMERS_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalSchema: "HEHE",
                        principalTable: "CUSTOMERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_ITEMS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ORDER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    COLOR = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    STORAGE_GB = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    UNIT_PRICE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LINE_TOTAL = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_ITEMS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ORDER_ITEMS_ORDERS_ORDER_ID",
                        column: x => x.ORDER_ID,
                        principalSchema: "HEHE",
                        principalTable: "ORDERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_ATTRIBUTE_VALUES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PRODUCT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ATTRIBUTE_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    INT_VALUE = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    DEC_VALUE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    BOOL_VALUE = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TEXT_VALUE = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_ATTRIBUTE_VALUES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_ATTRIBUTE_VALUES_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRODUCT_ATTRIBUTE_VALUES_PRODUCT_ATTRIBUTES_ATTRIBUTE_ID",
                        column: x => x.ATTRIBUTE_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_ATTRIBUTES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_VARIANTS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PRODUCT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    SKU = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    COLOR = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    STORAGE_GB = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    BARCODE = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    WEIGHT_GRAM = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    IS_ACTIVE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_VARIANTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_VARIANTS_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DEVICE_UNITS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    IMEI = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false),
                    SERIAL_NO = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    WAREHOUSE_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    RECEIVED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    SOLD_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true),
                    RETURNED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DEVICE_UNITS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DEVICE_UNITS_PRODUCT_VARIANTS_VARIANT_ID",
                        column: x => x.VARIANT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_VARIANTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DEVICE_UNITS_WAREHOUSES_WAREHOUSE_ID",
                        column: x => x.WAREHOUSE_ID,
                        principalSchema: "HEHE",
                        principalTable: "WAREHOUSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "INVENTORY",
                schema: "HEHE",
                columns: table => new
                {
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    WAREHOUSE_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    QTY_ON_HAND = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QTY_RESERVED = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORY", x => new { x.VARIANT_ID, x.WAREHOUSE_ID });
                    table.ForeignKey(
                        name: "FK_INVENTORY_PRODUCT_VARIANTS_VARIANT_ID",
                        column: x => x.VARIANT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_VARIANTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_INVENTORY_WAREHOUSES_WAREHOUSE_ID",
                        column: x => x.WAREHOUSE_ID,
                        principalSchema: "HEHE",
                        principalTable: "WAREHOUSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_IMAGES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PRODUCT_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    IMAGE_URL = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ALT_TEXT = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    IS_PRIMARY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SORT_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_IMAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_IMAGES_PRODUCTS_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRODUCT_IMAGES_PRODUCT_VARIANTS_VARIANT_ID",
                        column: x => x.VARIANT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_VARIANTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_PRICES",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    LIST_PRICE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    SALE_PRICE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: true),
                    CURRENCY = table.Column<string>(type: "NCHAR(3)", fixedLength: true, maxLength: 3, nullable: false),
                    STARTS_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false),
                    ENDS_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_PRICES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_PRICES_PRODUCT_VARIANTS_VARIANT_ID",
                        column: x => x.VARIANT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_VARIANTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STOCK_MOVEMENTS",
                schema: "HEHE",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    VARIANT_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    WAREHOUSE_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    MOVEMENT_TYPE = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    QTY_DELTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REF_TYPE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    REF_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    REF_CODE = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    NOTE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP WITH LOCAL TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    CREATED_BY = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STOCK_MOVEMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_STOCK_MOVEMENTS_PRODUCT_VARIANTS_VARIANT_ID",
                        column: x => x.VARIANT_ID,
                        principalSchema: "HEHE",
                        principalTable: "PRODUCT_VARIANTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_STOCK_MOVEMENTS_WAREHOUSES_WAREHOUSE_ID",
                        column: x => x.WAREHOUSE_ID,
                        principalSchema: "HEHE",
                        principalTable: "WAREHOUSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADDR_DEFAULT",
                schema: "HEHE",
                table: "ADDRESSES",
                columns: new[] { "CUSTOMER_ID", "ADDRESS_TYPE", "IS_DEFAULT" });

            migrationBuilder.CreateIndex(
                name: "UX_BRANDS_SLUG",
                schema: "HEHE",
                table: "BRANDS",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CARTITEM_CART_SKU",
                schema: "HEHE",
                table: "CART_ITEMS",
                columns: new[] { "CART_ID", "VARIANT_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CARTS_CUS",
                schema: "HEHE",
                table: "CARTS",
                column: "CUSTOMER_ID",
                unique: true,
                filter: "\"CUSTOMER_ID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_CARTS_SESSION",
                schema: "HEHE",
                table: "CARTS",
                column: "SESSION_ID",
                unique: true,
                filter: "\"SESSION_ID\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORIES_PARENT",
                schema: "HEHE",
                table: "CATEGORIES",
                column: "PARENT_ID");

            migrationBuilder.CreateIndex(
                name: "UX_CATEGORIES_SLUG",
                schema: "HEHE",
                table: "CATEGORIES",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CUS_EMAIL",
                schema: "HEHE",
                table: "CUSTOMERS",
                column: "EMAIL");

            migrationBuilder.CreateIndex(
                name: "IX_CUS_PHONE",
                schema: "HEHE",
                table: "CUSTOMERS",
                column: "PHONE");

            migrationBuilder.CreateIndex(
                name: "IX_CUSTOMERS_USER_ACCOUNT_ID",
                schema: "HEHE",
                table: "CUSTOMERS",
                column: "USER_ACCOUNT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DEVICE_UNITS_VARIANT_ID",
                schema: "HEHE",
                table: "DEVICE_UNITS",
                column: "VARIANT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DEVICE_UNITS_WAREHOUSE_ID",
                schema: "HEHE",
                table: "DEVICE_UNITS",
                column: "WAREHOUSE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_DU_IMEI",
                schema: "HEHE",
                table: "DEVICE_UNITS",
                column: "IMEI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_WAREHOUSE_ID",
                schema: "HEHE",
                table: "INVENTORY",
                column: "WAREHOUSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_ITEMS_ORDER_ID",
                schema: "HEHE",
                table: "ORDER_ITEMS",
                column: "ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ORDERS_CODE",
                schema: "HEHE",
                table: "ORDERS",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ATTRVALS_ATTR",
                schema: "HEHE",
                table: "PRODUCT_ATTRIBUTE_VALUES",
                column: "ATTRIBUTE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ATTRVALS_PRODUCT",
                schema: "HEHE",
                table: "PRODUCT_ATTRIBUTE_VALUES",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ATTRVALS",
                schema: "HEHE",
                table: "PRODUCT_ATTRIBUTE_VALUES",
                columns: new[] { "PRODUCT_ID", "ATTRIBUTE_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_ATTRIBUTES_CODE",
                schema: "HEHE",
                table: "PRODUCT_ATTRIBUTES",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IMAGES_PRODUCT",
                schema: "HEHE",
                table: "PRODUCT_IMAGES",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGES_VARIANT",
                schema: "HEHE",
                table: "PRODUCT_IMAGES",
                column: "VARIANT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRICES_ACTIVE",
                schema: "HEHE",
                table: "PRODUCT_PRICES",
                columns: new[] { "VARIANT_ID", "STARTS_AT", "ENDS_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCT_VARIANTS_SKU",
                schema: "HEHE",
                table: "PRODUCT_VARIANTS",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VARIANTS_PRODUCT",
                schema: "HEHE",
                table: "PRODUCT_VARIANTS",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_BRAND",
                schema: "HEHE",
                table: "PRODUCTS",
                column: "BRAND_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CATEGORY",
                schema: "HEHE",
                table: "PRODUCTS",
                column: "DEFAULT_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "UX_PRODUCTS_SLUG",
                schema: "HEHE",
                table: "PRODUCTS",
                column: "SLUG",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SM_VAR",
                schema: "HEHE",
                table: "STOCK_MOVEMENTS",
                column: "VARIANT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SM_WH",
                schema: "HEHE",
                table: "STOCK_MOVEMENTS",
                column: "WAREHOUSE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_WAREHOUSES_CODE",
                schema: "HEHE",
                table: "WAREHOUSES",
                column: "CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADDRESSES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "CART_ITEMS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "DEVICE_UNITS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "INVENTORY",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "ORDER_ITEMS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCT_ATTRIBUTE_VALUES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCT_IMAGES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCT_PRICES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "STOCK_MOVEMENTS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "CUSTOMERS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "CARTS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "ORDERS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCT_ATTRIBUTES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCT_VARIANTS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "WAREHOUSES",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "PRODUCTS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "BRANDS",
                schema: "HEHE");

            migrationBuilder.DropTable(
                name: "CATEGORIES",
                schema: "HEHE");
        }
    }
}
