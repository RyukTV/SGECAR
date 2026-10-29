IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [PuedeAgregar] bit NOT NULL,
        [PuedeModificar] bit NOT NULL,
        [PuedeEliminar] bit NOT NULL,
        [PuedeConsultar] bit NOT NULL,
        [PuedeGestionarUsuarios] bit NOT NULL,
        [PuedeGestionarRoles] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [NombreUsuario] nvarchar(100) NOT NULL,
        [NombreCompleto] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Activo] bit NOT NULL,
        [RolId] int NOT NULL,
        [IntentosFallidos] int NOT NULL DEFAULT 0,
        [BloqueadoHasta] datetime2 NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [Roles] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260921224548_InitialUsersAndRoles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260921224548_InitialUsersAndRoles', N'8.0.31');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018025318_AddClientesAndGestionarClientes'
)
BEGIN
    ALTER TABLE [Roles] ADD [PuedeGestionarClientes] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018025318_AddClientesAndGestionarClientes'
)
BEGIN
    UPDATE [Roles] SET [PuedeGestionarClientes] = 1 WHERE [Nombre] = N'Administrador'
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018025318_AddClientesAndGestionarClientes'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Apellido] nvarchar(100) NOT NULL,
        [Telefono] nvarchar(30) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [Direccion] nvarchar(250) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018025318_AddClientesAndGestionarClientes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Correo] ON [Clientes] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261018025318_AddClientesAndGestionarClientes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261018025318_AddClientesAndGestionarClientes', N'8.0.31');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027175429_AddProductos'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [Stock] int NOT NULL,
        [Categoria] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027175429_AddProductos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027175429_AddProductos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027175429_AddProductos', N'8.0.31');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    ALTER TABLE [Roles] ADD [PuedeGestionarCategorias] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    ALTER TABLE [Productos] ADD [CategoriaId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    CREATE TABLE [CategoriasProducto] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_CategoriasProducto] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'CategoriaId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Productos] ALTER COLUMN [CategoriaId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CategoriasProducto_Nombre] ON [CategoriasProducto] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_CategoriasProducto_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [CategoriasProducto] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'Categoria');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Productos] DROP COLUMN [Categoria];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    UPDATE Roles
    SET PuedeGestionarCategorias = 1
    WHERE Nombre = N'Administrador';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028181644_AddProductCategoriesAndPermission'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028181644_AddProductCategoriesAndPermission', N'8.0.31');
END;
GO

COMMIT;
GO

