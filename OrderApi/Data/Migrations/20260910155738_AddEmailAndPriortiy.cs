using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndPriortiy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
        name: "CustomerEmail",
        table: "Orders",
        type: "nvarchar(200)",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "OrderPriority",
        table: "Orders",
        type: "nvarchar(20)",
        nullable: false,
        defaultValue: "Normal");

    // Populate EXISTING rows
    migrationBuilder.Sql(@"
        UPDATE Orders
        SET
            CustomerEmail = CASE CustomerName
                WHEN 'Mahesh' THEN 'mahesh@gmail.com'
                WHEN 'Virat' THEN 'virat@gmail.com'
                ELSE LOWER(CustomerName) + '@company.com'
            END,
            OrderPriority = CASE
                WHEN Price >= 800 THEN 'High'
                ELSE 'Normal'
            END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderPriority",
                table: "Orders");
        }
    }
}
