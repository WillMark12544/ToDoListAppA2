using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoListAppA2.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedArchivePropertyToToDoListModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "ToDoLists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archived",
                table: "ToDoLists");
        }
    }
}
