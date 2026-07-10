using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToursAyacuchoPeruAPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "PaquetesTuristicos",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.PaqueteImagenes', N'U') IS NOT NULL
BEGIN
    UPDATE p
    SET ImagenUrl = i.ImagenUrl
    FROM dbo.PaquetesTuristicos p
    INNER JOIN dbo.PaqueteImagenes i ON i.PaqueteId = p.PaqueteId
    WHERE i.EsPrincipal = 1
      AND p.ImagenUrl IS NULL;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "PaquetesTuristicos");
        }
    }
}
