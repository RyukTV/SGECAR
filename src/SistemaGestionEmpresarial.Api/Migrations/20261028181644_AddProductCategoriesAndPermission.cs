using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionEmpresarial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoriesAndPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PuedeGestionarCategorias",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoriasProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProducto", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                EXEC sp_executesql N'
                    INSERT INTO CategoriasProducto (Nombre)
                    SELECT DISTINCT
                        CASE
                            WHEN NULLIF(LTRIM(RTRIM(Categoria)), N'''') IS NULL THEN N''Sin categoría''
                            ELSE LTRIM(RTRIM(Categoria))
                        END
                    FROM Productos;

                    UPDATE p
                    SET p.CategoriaId = c.Id
                    FROM Productos p
                    INNER JOIN CategoriasProducto c
                        ON c.Nombre = CASE
                            WHEN NULLIF(LTRIM(RTRIM(p.Categoria)), N'''') IS NULL THEN N''Sin categoría''
                            ELSE LTRIM(RTRIM(p.Categoria))
                        END;
                ';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaId",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProducto_Nombre",
                table: "CategoriasProducto",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_CategoriasProducto_CategoriaId",
                table: "Productos",
                column: "CategoriaId",
                principalTable: "CategoriasProducto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Productos");

            migrationBuilder.Sql(
                """
                UPDATE Roles
                SET PuedeGestionarCategorias = 1
                WHERE Nombre = N'Administrador';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.Categoria = c.Nombre
                FROM Productos p
                INNER JOIN CategoriasProducto c ON c.Id = p.CategoriaId;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Productos_CategoriasProducto_CategoriaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PuedeGestionarCategorias",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "CategoriasProducto");
        }
    }
}
