using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using ITI.SkyLord.TestAvecEntity.Models;

namespace ITI.SkyLord.TestAvecEntity.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20151104102839_1")]
    partial class _1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Annotation("ProductVersion", "7.0.0-beta8-15964")
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ITI.SkyLord.TestAvecEntity.Models.Tchat", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Message");

                    b.Property<string>("Personne");

                    b.HasKey("ID");
                });
        }
    }
}
