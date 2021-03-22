using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ScheduleBot.Data.Migrations
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recurrency",
                table: "Lessons");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Recurrency",
                table: "Lessons",
                type: "interval",
                nullable: true);
        }
    }
}
