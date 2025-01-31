using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentsApp.Migrations
{
    public partial class AddNullableUserIdToComment1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Змінюємо поле UserId на nullable
            migrationBuilder.AlterColumn<Guid?>(
                name: "UserId", 
                table: "Comments",
                nullable: true, 
                oldClrType: typeof(Guid), 
                oldType: "uniqueidentifier", 
                oldNullable: false); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
