using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PHONE_STORE.Infrastructure; 

#nullable disable

namespace PHONE_STORE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNTS",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    PHONE = table.Column<string>(type: "NVARCHAR2(450)", nullable: true),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACCOUNTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLES",
                columns: table => new
                {
                    USER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    ROLE_ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLES", x => new { x.USER_ID, x.ROLE_ID });
                });

            migrationBuilder.CreateIndex(
                name: "UX_USER_ACCOUNTS_EMAIL",
                table: "USER_ACCOUNTS",
                column: "EMAIL",
                unique: true,
                filter: "\"EMAIL\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_USER_ACCOUNTS_PHONE",
                table: "USER_ACCOUNTS",
                column: "PHONE",
                unique: true,
                filter: "\"PHONE\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_ROLE_ID",
                table: "USER_ROLES",
                column: "ROLE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROLES");

            migrationBuilder.DropTable(
                name: "USER_ACCOUNTS");

            migrationBuilder.DropTable(
                name: "USER_ROLES");
        }
    }
}
