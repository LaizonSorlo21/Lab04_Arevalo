/* ============================================================
   My TodoListDB  
   ============================================================ */
 
IF DB_ID(N'TodoListDB') IS NULL
BEGIN
    CREATE DATABASE TodoListDB;
END
GO

USE TodoListDB;
GO
 
 

CREATE TABLE dbo.Tareas (
    TareaID         INT IDENTITY(1,1) PRIMARY KEY,
    Titulo          NVARCHAR(150)  NOT NULL,
    Descripcion     NVARCHAR(MAX)  NULL,
    Completada      BIT            NOT NULL DEFAULT 0,
    FechaCreacion   DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    FechaCompletada DATETIME2      NULL
);
GO

/* ============================================================
   STORED PROCEDURES  
   ============================================================ */

-- Crear tarea
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_Crear
    @Titulo      NVARCHAR(150),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Tareas (Titulo, Descripcion)
        VALUES (@Titulo, @Descripcion);

        SELECT SCOPE_IDENTITY() AS TareaID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Obtener tarea por ID
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_ObtenerPorId
    @TareaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Tareas WHERE TareaID = @TareaID;
END
GO

-- Listar todas las tareas
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_ListarTodas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Tareas ORDER BY FechaCreacion DESC;
END
GO

-- Listar solo pendientes o solo completadas
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_ListarPorEstado
    @Completada BIT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Tareas
    WHERE Completada = @Completada
    ORDER BY FechaCreacion DESC;
END
GO

-- Actualizar tarea (título/descripción)
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_Actualizar
    @TareaID     INT,
    @Titulo      NVARCHAR(150),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Tareas
        SET Titulo = @Titulo,
            Descripcion = @Descripcion
        WHERE TareaID = @TareaID;

        IF @@ROWCOUNT = 0
            THROW 50001, 'Tarea no encontrada.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Marcar como completada o no completada
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_MarcarCompletada
    @TareaID    INT,
    @Completada BIT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.Tareas
        SET Completada = @Completada,
            FechaCompletada = CASE WHEN @Completada = 1 THEN SYSDATETIME() ELSE NULL END
        WHERE TareaID = @TareaID;

        IF @@ROWCOUNT = 0
            THROW 50002, 'Tarea no encontrada.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Eliminar tarea
CREATE OR ALTER PROCEDURE dbo.usp_Tarea_Eliminar
    @TareaID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.Tareas WHERE TareaID = @TareaID;

        IF @@ROWCOUNT = 0
            THROW 50003, 'Tarea no encontrada.', 1;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

/* ============================================================
   Datos de prueba  
   ============================================================ */

INSERT INTO dbo.Tareas (Titulo, Descripcion) VALUES
(N'Comprar víveres', N'Leche, pan, huevos'),
(N'Estudiar SQL', N'Repasar stored procedures'),
(N'Llamar al dentista', N'Agendar cita de control');
GO