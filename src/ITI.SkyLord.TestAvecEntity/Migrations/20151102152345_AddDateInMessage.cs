using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace ITI.SkyLord.TestAvecEntity.Migrations
{
    public partial class AddDateInMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Tchat",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                type: "date"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Date", table: "Tchat");
        }
    }
}
