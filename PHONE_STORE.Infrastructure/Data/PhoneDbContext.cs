using Microsoft.EntityFrameworkCore;
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


        base.OnModelCreating(m);
    }

}
