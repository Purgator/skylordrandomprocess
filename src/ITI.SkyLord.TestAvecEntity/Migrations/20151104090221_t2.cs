using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace ITI.SkyLord.TestAvecEntity.Migrations
{
    public partial class t2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "age",
                table: "Tchat",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "age", table: "Tchat");
        }
    }
}
