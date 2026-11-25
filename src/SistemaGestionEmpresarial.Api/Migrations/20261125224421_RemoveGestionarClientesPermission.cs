using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionEmpresarial.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGestionarClientesPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuedeGestionarClientes",
                table: "Roles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PuedeGestionarClientes",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Restaura el estado previo: AddClientesAndGestionarClientes concedía el permiso al Administrador.
            migrationBuilder.Sql(
                "UPDATE [Roles] SET [PuedeGestionarClientes] = 1 WHERE [Nombre] = N'Administrador'");
        }
    }
}
