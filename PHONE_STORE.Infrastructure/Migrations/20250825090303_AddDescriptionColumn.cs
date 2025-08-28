using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PHONE_STORE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ROLES",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ROLES");
        }
    }
}
