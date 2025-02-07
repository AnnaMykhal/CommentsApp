using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentsApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Comments",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Comments",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true,
                oldType: "char(36)");
        }
    }
}
