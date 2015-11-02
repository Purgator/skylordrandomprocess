using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace ITI.SkyLord.TestAvecEntity.Migrations
{
    public partial class MyFirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tchat",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    Message = table.Column<string>(nullable: true),
                    Personne = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tchat", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Tchat");
        }
    }
}