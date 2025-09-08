using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Data;

public class PhoneDbContext : DbContext
{
    public PhoneDbContext(DbContextOptions<PhoneDbContext> options) : base(options) { }
    //PhoneDbContext kế thừa từ DbContext(EF Core).
    //Constructor nhận DbContextOptions(được cấu hình ở Program.cs với UseOracle).
    //👉 Đây là nơi EF Core biết cách kết nối tới DB và ánh xạ entity ↔ bảng.

    // DbSet cho 3 bảng user/role
    //Mỗi DbSet<T> đại diện cho 1 bảng.
    //Cho phép bạn query bằng LINQ:
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories { get; set; } = default!;  // <— thêm dòng này

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();

    // DbSet
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<DeviceUnit> DeviceUnits => Set<DeviceUnit>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();


    //EF Core mặc định tự ánh xạ class → bảng, nhưng bạn override để chính xác hơn với Oracle.
    protected override void OnModelCreating(ModelBuilder m)
    {

        // USER_ACCOUNTS
        m.Entity<UserAccount>(e =>
        {
            e.ToTable("USER_ACCOUNTS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.Email).HasColumnName("EMAIL");
            e.Property(x => x.Phone).HasColumnName("PHONE");
            e.Property(x => x.PasswordHash).HasColumnName("PASSWORD_HASH");
            e.Property(x => x.Status).HasColumnName("STATUS");
            e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");

            // ✅ Unique index (vì bạn đã lưu email = lowercase ở Register)
            e.HasIndex(x => x.Email)
             .IsUnique()
             .HasDatabaseName("UX_USER_ACCOUNTS_EMAIL");

            // ✅ Unique index cho SĐT (cho phép null, Oracle coi "" là NULL)
            e.HasIndex(x => x.Phone)
             .IsUnique()
             .HasDatabaseName("UX_USER_ACCOUNTS_PHONE");
        });

        // ROLES
        m.Entity<Role>(e =>
        {
            e.ToTable("ROLES");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.Code).HasColumnName("CODE");
            e.Property(x => x.Name).HasColumnName("NAME");
            e.Property(x => x.Description).HasColumnName("Description");
        });

        // USER_ROLES
        m.Entity<UserRole>(e =>
        {
            e.ToTable("USER_ROLES");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.Property(x => x.UserId).HasColumnName("USER_ID");
            e.Property(x => x.RoleId).HasColumnName("ROLE_ID");

            // ℹ️ PK (USER_ID, ROLE_ID) đã tự có unique index → không cần thêm.
            // Nhưng thêm index phụ theo ROLE_ID để truy vấn "ai có role X" nhanh hơn:
            e.HasIndex(x => x.RoleId)
             .HasDatabaseName("IX_USER_ROLES_ROLE_ID");
        });

        //BRANDS
        m.Entity<Brand>(e =>
        {
            e.ToTable("BRANDS", "HEHE");        // hoặc chỉ "BRANDS" nếu đã HasDefaultSchema
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(100).IsRequired();
            e.Property(x => x.Slug).HasColumnName("SLUG").HasMaxLength(150).IsRequired();

            // NUMBER(1,0) ↔ bool: map 0/1 rõ ràng
            e.Property(x => x.IsActive)
             .HasColumnName("IS_ACTIVE")
             .HasConversion(v => v ? 1 : 0, v => v == 1);

            // TIMESTAMP(6) WITH LOCAL TIME ZONE
            e.Property(x => x.CreatedAt)
             .HasColumnName("CREATED_AT")
             .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
             .HasDefaultValueSql("SYSTIMESTAMP");

            // Phản chiếu unique SLUG (cho EF biết, dù DB đã có constraint)
            e.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_BRANDS_SLUG");
        });

        m.Entity<Category>(e =>
        {
            e.ToTable("CATEGORIES", "HEHE"); // hoặc HasDefaultSchema("HEHE")
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.ParentId).HasColumnName("PARENT_ID");
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            e.Property(x => x.Slug).HasColumnName("SLUG").HasMaxLength(150).IsRequired();
            e.Property(x => x.SortOrder).HasColumnName("SORT_ORDER");
            e.Property(x => x.IsActive)
                .HasColumnName("IS_ACTIVE")
                .HasConversion(v => v ? 1 : 0, v => v == 1);
            e.Property(x => x.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            e.HasIndex(x => x.ParentId).HasDatabaseName("IX_CATEGORIES_PARENT");
            e.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_CATEGORIES_SLUG");
        });
        // ================== PRODUCTS ==================
        m.Entity<Product>(e =>
        {
            e.ToTable("PRODUCTS", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.BrandId).HasColumnName("BRAND_ID").IsRequired();
            e.Property(x => x.DefaultCategoryId).HasColumnName("DEFAULT_CATEGORY_ID");
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasColumnName("SLUG").HasMaxLength(220).IsRequired();
            e.Property(x => x.Description).HasColumnName("DESCRIPTION").HasColumnType("CLOB");
            e.Property(x => x.SpecJson).HasColumnName("SPEC_JSON").HasColumnType("CLOB");
            e.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasConversion(v => v ? 1 : 0, v => v == 1);
            e.Property(x => x.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            e.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("UX_PRODUCTS_SLUG");
            e.HasIndex(x => x.BrandId).HasDatabaseName("IX_PRODUCTS_BRAND");
            e.HasIndex(x => x.DefaultCategoryId).HasDatabaseName("IX_PRODUCTS_CATEGORY");

            // FK: Brand (RESTRICT), Category mặc định (RESTRICT)
            e.HasOne(x => x.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.DefaultCategory)
                .WithMany() // hoặc .WithMany(c => c.Products) nếu bạn có navigation trong Category
                .HasForeignKey(x => x.DefaultCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ================== PRODUCT_VARIANTS ==================
        m.Entity<ProductVariant>(e =>
        {
            e.ToTable("PRODUCT_VARIANTS", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.ProductId).HasColumnName("PRODUCT_ID").IsRequired();
            e.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(64).IsRequired();
            e.Property(x => x.Color).HasColumnName("COLOR").HasMaxLength(50);
            e.Property(x => x.StorageGb).HasColumnName("STORAGE_GB");
            e.Property(x => x.Barcode).HasColumnName("BARCODE").HasMaxLength(64);
            e.Property(x => x.WeightGram).HasColumnName("WEIGHT_GRAM"); // decimal(10,2) tự map
            e.Property(x => x.IsActive).HasColumnName("IS_ACTIVE").HasConversion(v => v ? 1 : 0, v => v == 1);

            e.HasIndex(x => x.ProductId).HasDatabaseName("IX_VARIANTS_PRODUCT");
            e.HasIndex(x => x.Sku).IsUnique();

            e.HasOne(x => x.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // DDL không cascade
        });

        // ================== PRODUCT_PRICES ==================
        m.Entity<ProductPrice>(e =>
        {
            e.ToTable("PRODUCT_PRICES", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID").IsRequired();
            e.Property(x => x.ListPrice).HasColumnName("LIST_PRICE");   // decimal(12,2)
            e.Property(x => x.SalePrice).HasColumnName("SALE_PRICE");   // decimal(12,2)?
            e.Property(x => x.Currency).HasColumnName("CURRENCY").HasMaxLength(3).IsFixedLength().IsRequired();
            e.Property(x => x.StartsAt)
                .HasColumnName("STARTS_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .IsRequired();
            e.Property(x => x.EndsAt)
                .HasColumnName("ENDS_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");

            e.HasIndex(x => new { x.VariantId, x.StartsAt, x.EndsAt }).HasDatabaseName("IX_PRICES_ACTIVE");

            e.HasOne(x => x.Variant)
                .WithMany(v => v.Prices)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade); // DDL: ON DELETE CASCADE
        });

        // ================== PRODUCT_IMAGES ==================
        m.Entity<ProductImage>(e =>
        {
            e.ToTable("PRODUCT_IMAGES", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.ProductId).HasColumnName("PRODUCT_ID");
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.ImageUrl).HasColumnName("IMAGE_URL").HasMaxLength(500).IsRequired();
            e.Property(x => x.AltText).HasColumnName("ALT_TEXT").HasMaxLength(200);
            e.Property(x => x.IsPrimary).HasColumnName("IS_PRIMARY").HasConversion(v => v ? 1 : 0, v => v == 1);
            e.Property(x => x.SortOrder).HasColumnName("SORT_ORDER");

            e.HasIndex(x => x.ProductId).HasDatabaseName("IX_IMAGES_PRODUCT");
            e.HasIndex(x => x.VariantId).HasDatabaseName("IX_IMAGES_VARIANT");

            e.HasOne(x => x.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Variant)
                .WithMany(v => v.Images)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // CK_IMAGES_TARGET (chỉ 1 trong 2 có giá trị) là CHECK ở DB; EF không map, bỏ qua.
        });

        // ================== PRODUCT_ATTRIBUTES ==================
        m.Entity<ProductAttribute>(e =>
        {
            e.ToTable("PRODUCT_ATTRIBUTES", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            e.Property(x => x.DataType).HasColumnName("DATA_TYPE").HasMaxLength(20).IsRequired();

            e.HasIndex(x => x.Code).IsUnique();
        });

        // ================== PRODUCT_ATTRIBUTE_VALUES ==================
        m.Entity<ProductAttributeValue>(e =>
        {
            e.ToTable("PRODUCT_ATTRIBUTE_VALUES", "HEHE");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.ProductId).HasColumnName("PRODUCT_ID").IsRequired();
            e.Property(x => x.AttributeId).HasColumnName("ATTRIBUTE_ID").IsRequired();
            e.Property(x => x.IntValue).HasColumnName("INT_VALUE");
            e.Property(x => x.DecValue).HasColumnName("DEC_VALUE");     // decimal(12,3)
            e.Property(x => x.BoolValue).HasColumnName("BOOL_VALUE").HasConversion<int?>();
            e.Property(x => x.TextValue).HasColumnName("TEXT_VALUE").HasMaxLength(400);

            e.HasIndex(x => x.ProductId).HasDatabaseName("IX_ATTRVALS_PRODUCT");
            e.HasIndex(x => x.AttributeId).HasDatabaseName("IX_ATTRVALS_ATTR");
            e.HasIndex(x => new { x.ProductId, x.AttributeId }).IsUnique().HasDatabaseName("UQ_ATTRVALS");

            e.HasOne(x => x.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Attribute)
                .WithMany(a => a.Values)
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // CHECK (chỉ 1 trong 4 cột có giá trị) là constraint ở DB; EF không model hoá được, để DB kiểm tra.
        });

        // ===== WAREHOUSES =====
        m.Entity<Warehouse>(e =>
        {
            e.ToTable("WAREHOUSES", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(120).IsRequired();
            e.Property(x => x.AddressLine).HasColumnName("ADDRESS_LINE").HasMaxLength(200);
            e.Property(x => x.District).HasColumnName("DISTRICT").HasMaxLength(120);
            e.Property(x => x.Province).HasColumnName("PROVINCE").HasMaxLength(120);
            e.Property(x => x.IsActive)
                .HasColumnName("IS_ACTIVE")
                .HasConversion(v => v ? 1 : 0, v => v == 1);
            e.Property(x => x.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_WAREHOUSES_CODE");
        });

        // ===== INVENTORY =====
        m.Entity<Inventory>(e =>
        {
            e.ToTable("INVENTORY", "HEHE");
            e.HasKey(x => new { x.VariantId, x.WarehouseId });
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.WarehouseId).HasColumnName("WAREHOUSE_ID");
            e.Property(x => x.QtyOnHand).HasColumnName("QTY_ON_HAND");
            e.Property(x => x.QtyReserved).HasColumnName("QTY_RESERVED");
            e.Property(x => x.UpdatedAt)
                .HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");

            e.HasOne(x => x.Variant)
                .WithMany(v => v.Inventories)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse)
                .WithMany(w => w.Inventories)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== STOCK_MOVEMENTS =====
        m.Entity<StockMovement>(e =>
        {
            e.ToTable("STOCK_MOVEMENTS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.WarehouseId).HasColumnName("WAREHOUSE_ID");
            e.Property(x => x.MovementType).HasColumnName("MOVEMENT_TYPE").HasMaxLength(10).IsRequired();
            e.Property(x => x.QtyDelta).HasColumnName("QTY_DELTA");
            e.Property(x => x.RefType).HasColumnName("REF_TYPE").HasMaxLength(30);
            e.Property(x => x.RefId).HasColumnName("REF_ID");
            e.Property(x => x.RefCode).HasColumnName("REF_CODE").HasMaxLength(64);
            e.Property(x => x.Note).HasColumnName("NOTE").HasMaxLength(200);
            e.Property(x => x.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.CreatedBy).HasColumnName("CREATED_BY");

            e.HasIndex(x => x.WarehouseId).HasDatabaseName("IX_SM_WH");
            e.HasIndex(x => x.VariantId).HasDatabaseName("IX_SM_VAR");

            e.HasOne(x => x.Variant)
                .WithMany(v => v.StockMovements)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse)
                .WithMany(w => w.StockMovements)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== DEVICE_UNITS =====
        m.Entity<DeviceUnit>(e =>
        {
            e.ToTable("DEVICE_UNITS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.Imei).HasColumnName("IMEI").HasMaxLength(32).IsRequired();
            e.Property(x => x.SerialNo).HasColumnName("SERIAL_NO").HasMaxLength(64);
            e.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired();
            e.Property(x => x.WarehouseId).HasColumnName("WAREHOUSE_ID");
            e.Property(x => x.ReceivedAt)
                .HasColumnName("RECEIVED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE")
                .HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.SoldAt).HasColumnName("SOLD_AT").HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");
            e.Property(x => x.ReturnedAt).HasColumnName("RETURNED_AT").HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");

            e.HasIndex(x => x.Imei).IsUnique().HasDatabaseName("UQ_DU_IMEI");

            e.HasOne(x => x.Variant)
                .WithMany(v => v.DeviceUnits)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Warehouse)
                .WithMany(w => w.DeviceUnits)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== CUSTOMERS =====
        m.Entity<Customer>(e =>
        {
            e.ToTable("CUSTOMERS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();

            e.Property(x => x.UserAccountId).HasColumnName("USER_ACCOUNT_ID");
            e.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(150);
            e.Property(x => x.Phone).HasColumnName("PHONE").HasMaxLength(20);
            e.Property(x => x.FullName).HasColumnName("FULL_NAME").HasMaxLength(150);
            e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");

            e.HasOne(x => x.UserAccount)
                .WithMany() // hoặc tạo navigation nếu muốn
                .HasForeignKey(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.Email).HasDatabaseName("IX_CUS_EMAIL");
            e.HasIndex(x => x.Phone).HasDatabaseName("IX_CUS_PHONE");
        });

        // ===== ADDRESSES =====
        m.Entity<Address>(e =>
        {
            e.ToTable("ADDRESSES", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();

            e.Property(x => x.CustomerId).HasColumnName("CUSTOMER_ID").IsRequired();
            e.Property(x => x.Label).HasColumnName("LABEL").HasMaxLength(60);
            e.Property(x => x.Recipient).HasColumnName("RECIPIENT").HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasColumnName("PHONE").HasMaxLength(20).IsRequired();
            e.Property(x => x.Line1).HasColumnName("LINE1").HasMaxLength(200).IsRequired();
            e.Property(x => x.Ward).HasColumnName("WARD").HasMaxLength(120);
            e.Property(x => x.District).HasColumnName("DISTRICT").HasMaxLength(120);
            e.Property(x => x.Province).HasColumnName("PROVINCE").HasMaxLength(120);
            e.Property(x => x.PostalCode).HasColumnName("POSTAL_CODE").HasMaxLength(20);
            e.Property(x => x.AddressType).HasColumnName("ADDRESS_TYPE").HasMaxLength(20).IsRequired();
            e.Property(x => x.IsDefault).HasColumnName("IS_DEFAULT")
                .HasConversion(v => v ? 1 : 0, v => v == 1).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");

            e.HasOne(x => x.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.CustomerId, x.AddressType, x.IsDefault })
                .HasDatabaseName("IX_ADDR_DEFAULT");
        });

        // ===== CARTS =====
        // CARTS
        m.Entity<Cart>(e =>
        {
            e.ToTable("CARTS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.CustomerId).HasColumnName("CUSTOMER_ID");
            e.Property(x => x.SessionId).HasColumnName("SESSION_TOKEN").HasMaxLength(64);
            e.Property(x => x.Currency).HasColumnName("CURRENCY");
            e.Property(x => x.Status).HasColumnName("STATUS");
            e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");
        });

        // CART_ITEMS (PK kép, không có ID)
        m.Entity<CartItem>(e =>
        {
            e.ToTable("CART_ITEMS", "HEHE");
            e.HasKey(x => new { x.CartId, x.VariantId });
            e.Property(x => x.CartId).HasColumnName("CART_ID");
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.Quantity).HasColumnName("QUANTITY");
            e.Property(x => x.UnitPrice).HasColumnName("UNIT_PRICE"); // NUMBER(12,2) nullable
            e.Property(x => x.Currency).HasColumnName("CURRENCY");
            e.Property(x => x.AddedAt).HasColumnName("ADDED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.HasOne(x => x.Cart).WithMany(c => c.Items).HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // ORDERS
        m.Entity<Order>(e =>
        {
            e.ToTable("ORDERS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.Code).HasColumnName("CODE").HasMaxLength(32).IsRequired();
            e.Property(x => x.CustomerId).HasColumnName("CUSTOMER_ID");
            e.Property(x => x.ShippingAddressId).HasColumnName("SHIPPING_ADDRESS_ID");
            e.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(20);
            e.Property(x => x.Currency).HasColumnName("CURRENCY");
            e.Property(x => x.Subtotal).HasColumnName("SUBTOTAL");
            e.Property(x => x.DiscountTotal).HasColumnName("DISCOUNT_TOTAL");
            e.Property(x => x.TaxTotal).HasColumnName("TAX_TOTAL");
            e.Property(x => x.ShippingFee).HasColumnName("SHIPPING_FEE");
            e.Property(x => x.GrandTotal).HasColumnName("GRAND_TOTAL");
            e.Property(x => x.Note).HasColumnName("NOTE");
            e.Property(x => x.PlacedAt).HasColumnName("PLACED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE").HasDefaultValueSql("SYSTIMESTAMP");
            e.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP WITH LOCAL TIME ZONE");
        });

        // ORDER_ITEMS
        m.Entity<OrderItem>(e =>
        {
            e.ToTable("ORDER_ITEMS", "HEHE");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.OrderId).HasColumnName("ORDER_ID");
            e.Property(x => x.VariantId).HasColumnName("VARIANT_ID");
            e.Property(x => x.ProductName).HasColumnName("PRODUCT_NAME");
            e.Property(x => x.Sku).HasColumnName("SKU");
            e.Property(x => x.Quantity).HasColumnName("QUANTITY");
            e.Property(x => x.UnitPrice).HasColumnName("UNIT_PRICE");
            e.Property(x => x.Currency).HasColumnName("CURRENCY");
            e.Property(x => x.TaxAmount).HasColumnName("TAX_AMOUNT");
            e.Property(x => x.DiscountAmount).HasColumnName("DISCOUNT_AMOUNT");
            e.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });



        // Boolean -> NUMBER(1,0)
        m.Entity<Product>().Property(x => x.IsActive).HasConversion<int>().HasColumnName("IS_ACTIVE");
        m.Entity<ProductVariant>().Property(x => x.IsActive).HasConversion<int>().HasColumnName("IS_ACTIVE");
        m.Entity<ProductImage>().Property(x => x.IsPrimary).HasConversion<int>().HasColumnName("IS_PRIMARY");
        m.Entity<ProductAttributeValue>().Property(x => x.BoolValue).HasConversion<int?>().HasColumnName("BOOL_VALUE");

        // Tên bảng/cột nếu cần (bạn đã trùng sẵn)
        m.Entity<Product>().ToTable("PRODUCTS", "HEHE");
        m.Entity<ProductVariant>().ToTable("PRODUCT_VARIANTS", "HEHE");
        m.Entity<ProductPrice>().ToTable("PRODUCT_PRICES", "HEHE");
        m.Entity<ProductImage>().ToTable("PRODUCT_IMAGES", "HEHE");
        m.Entity<ProductAttribute>().ToTable("PRODUCT_ATTRIBUTES", "HEHE");
        m.Entity<ProductAttributeValue>().ToTable("PRODUCT_ATTRIBUTE_VALUES", "HEHE");


        // ================== EXTRA CONSTRAINTS ==================
        m.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        m.Entity<ProductVariant>().HasIndex(x => x.Sku).IsUnique();
        m.Entity<Order>().HasIndex(x => x.Code).IsUnique();
        m.Entity<DeviceUnit>().HasIndex(x => x.Imei).IsUnique();
        m.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();

        // Composite PK Inventory
        m.Entity<Inventory>().HasKey(x => new { x.VariantId, x.WarehouseId });


        // Hàm generic set precision cho decimal/decimal?
        m.Entity<Order>().Property(x => x.Subtotal).HasMoney();
        m.Entity<Order>().Property(x => x.DiscountTotal).HasMoney();
        m.Entity<Order>().Property(x => x.TaxTotal).HasMoney();
        m.Entity<Order>().Property(x => x.ShippingFee).HasMoney();
        m.Entity<Order>().Property(x => x.GrandTotal).HasMoney();

        m.Entity<OrderItem>().Property(x => x.UnitPrice).HasMoney();
        m.Entity<OrderItem>().Property(x => x.TaxAmount).HasMoney();
        m.Entity<OrderItem>().Property(x => x.DiscountAmount).HasMoney();

        m.Entity<ProductPrice>().Property(x => x.ListPrice).HasMoney();
        m.Entity<ProductPrice>().Property(x => x.SalePrice).HasMoney();    // nullable cũng OK


        // CHECK constraints
        m.Entity<StockMovement>().ToTable(t => t.HasCheckConstraint("CK_SM_Type",
            "MOVEMENT_TYPE IN ('IN','OUT','ADJUST')"));
        m.Entity<StockMovement>().ToTable(t => t.HasCheckConstraint("CK_SM_QtyDeltaSign",
            "(MOVEMENT_TYPE='IN' AND QTY_DELTA>0) OR " +
            "(MOVEMENT_TYPE='OUT' AND QTY_DELTA<0) OR " +
            "(MOVEMENT_TYPE='ADJUST' AND QTY_DELTA<>0)"));

        m.Entity<Inventory>().ToTable(t => t.HasCheckConstraint("CK_INV_NonNegative",
            "QTY_ON_HAND>=0 AND QTY_RESERVED>=0"));

        m.Entity<ProductImage>().ToTable(t => t.HasCheckConstraint("CK_IMG_OneOwner",
            "(PRODUCT_ID IS NOT NULL AND VARIANT_ID IS NULL) OR " +
            "(PRODUCT_ID IS NULL AND VARIANT_ID IS NOT NULL)"));

        m.Entity<ProductAttributeValue>().ToTable(t => t.HasCheckConstraint("CK_ATTRVAL_OneValue",
            " (CASE WHEN INT_VALUE IS NOT NULL THEN 1 ELSE 0 END) + " +
            " (CASE WHEN DEC_VALUE IS NOT NULL THEN 1 ELSE 0 END) + " +
            " (CASE WHEN BOOL_VALUE IS NOT NULL THEN 1 ELSE 0 END) + " +
            " (CASE WHEN TEXT_VALUE IS NOT NULL THEN 1 ELSE 0 END) = 1"));
        m.Entity<ProductAttributeValue>().HasIndex(x => new { x.ProductId, x.AttributeId }).IsUnique();

        // INDEX gợi ý
        m.Entity<ProductPrice>().HasIndex(x => new { x.VariantId, x.StartsAt, x.EndsAt });
        m.Entity<ProductImage>().HasIndex(x => new { x.ProductId, x.IsPrimary });
        m.Entity<ProductImage>().HasIndex(x => new { x.VariantId, x.IsPrimary });
        m.Entity<OrderItem>().HasIndex(x => x.OrderId);
        m.Entity<Order>().HasIndex(x => new { x.CustomerId, x.PlacedAt });


        base.OnModelCreating(m);

    }

}
