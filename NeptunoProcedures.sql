/* Ejecutar este archivo después de NeptunoDB.sql */
USE NeptunoDB;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Listar AS SELECT CategoriaID, NombreCategoria, Descripcion FROM dbo.Categorias ORDER BY NombreCategoria;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Insertar @NombreCategoria NVARCHAR(30), @Descripcion NVARCHAR(200)=NULL AS INSERT dbo.Categorias(NombreCategoria,Descripcion) VALUES(@NombreCategoria,NULLIF(@Descripcion,N''));
GO
CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Actualizar @CategoriaID INT,@NombreCategoria NVARCHAR(30),@Descripcion NVARCHAR(200)=NULL AS UPDATE dbo.Categorias SET NombreCategoria=@NombreCategoria,Descripcion=NULLIF(@Descripcion,N'') WHERE CategoriaID=@CategoriaID;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Eliminar @CategoriaID INT AS DELETE dbo.Categorias WHERE CategoriaID=@CategoriaID;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Listar AS SELECT ProveedorID,CompaniaNombre,NombreContacto,CargoContacto,Direccion,Ciudad,CodigoPostal,Pais,Telefono,Fax FROM dbo.Proveedores ORDER BY CompaniaNombre;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Insertar @CompaniaNombre NVARCHAR(60),@NombreContacto NVARCHAR(40)=NULL,@CargoContacto NVARCHAR(40)=NULL,@Direccion NVARCHAR(80)=NULL,@Ciudad NVARCHAR(30)=NULL,@CodigoPostal NVARCHAR(10)=NULL,@Pais NVARCHAR(30)=NULL,@Telefono NVARCHAR(24)=NULL,@Fax NVARCHAR(24)=NULL AS INSERT dbo.Proveedores VALUES(@CompaniaNombre,NULLIF(@NombreContacto,N''),NULLIF(@CargoContacto,N''),NULLIF(@Direccion,N''),NULLIF(@Ciudad,N''),NULLIF(@CodigoPostal,N''),NULLIF(@Pais,N''),NULLIF(@Telefono,N''),NULLIF(@Fax,N''));
GO
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Actualizar @ProveedorID INT,@CompaniaNombre NVARCHAR(60),@NombreContacto NVARCHAR(40)=NULL,@CargoContacto NVARCHAR(40)=NULL,@Direccion NVARCHAR(80)=NULL,@Ciudad NVARCHAR(30)=NULL,@CodigoPostal NVARCHAR(10)=NULL,@Pais NVARCHAR(30)=NULL,@Telefono NVARCHAR(24)=NULL,@Fax NVARCHAR(24)=NULL AS UPDATE dbo.Proveedores SET CompaniaNombre=@CompaniaNombre,NombreContacto=NULLIF(@NombreContacto,N''),CargoContacto=NULLIF(@CargoContacto,N''),Direccion=NULLIF(@Direccion,N''),Ciudad=NULLIF(@Ciudad,N''),CodigoPostal=NULLIF(@CodigoPostal,N''),Pais=NULLIF(@Pais,N''),Telefono=NULLIF(@Telefono,N''),Fax=NULLIF(@Fax,N'') WHERE ProveedorID=@ProveedorID;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Eliminar @ProveedorID INT AS DELETE dbo.Proveedores WHERE ProveedorID=@ProveedorID;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Proveedor_Buscar @NombreContacto NVARCHAR(40)=NULL,@Ciudad NVARCHAR(30)=NULL AS SELECT ProveedorID,CompaniaNombre,NombreContacto,CargoContacto,Ciudad,Pais,Telefono FROM dbo.Proveedores WHERE (NULLIF(@NombreContacto,N'') IS NULL OR NombreContacto LIKE N'%'+@NombreContacto+N'%') AND (NULLIF(@Ciudad,N'') IS NULL OR Ciudad LIKE N'%'+@Ciudad+N'%') ORDER BY CompaniaNombre;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Listar AS
SELECT p.ProductoID, p.NombreProducto, p.ProveedorID, p.CategoriaID,
       ISNULL(pr.CompaniaNombre, N'Sin proveedor') AS Proveedor,
       ISNULL(c.NombreCategoria, N'Sin categoria') AS Categoria,
       p.CantidadPorUnidad, p.PrecioUnidad, p.UnidadesEnExistencia,
       p.UnidadesEnPedido, p.NivelDeReorden, p.Descontinuado
FROM dbo.Productos p
LEFT JOIN dbo.Proveedores pr ON pr.ProveedorID = p.ProveedorID
LEFT JOIN dbo.Categorias c ON c.CategoriaID = p.CategoriaID
ORDER BY p.NombreProducto;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Insertar @NombreProducto NVARCHAR(60),@ProveedorID INT=NULL,@CategoriaID INT=NULL,@CantidadPorUnidad NVARCHAR(30)=NULL,@PrecioUnidad DECIMAL(10,2),@UnidadesEnExistencia SMALLINT=0,@UnidadesEnPedido SMALLINT=0,@NivelDeReorden SMALLINT=0,@Descontinuado BIT=0 AS INSERT dbo.Productos VALUES(@NombreProducto,@ProveedorID,@CategoriaID,NULLIF(@CantidadPorUnidad,N''),@PrecioUnidad,@UnidadesEnExistencia,@UnidadesEnPedido,@NivelDeReorden,@Descontinuado);
GO
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Actualizar @ProductoID INT,@NombreProducto NVARCHAR(60),@ProveedorID INT=NULL,@CategoriaID INT=NULL,@CantidadPorUnidad NVARCHAR(30)=NULL,@PrecioUnidad DECIMAL(10,2),@UnidadesEnExistencia SMALLINT=0,@UnidadesEnPedido SMALLINT=0,@NivelDeReorden SMALLINT=0,@Descontinuado BIT=0 AS UPDATE dbo.Productos SET NombreProducto=@NombreProducto,ProveedorID=@ProveedorID,CategoriaID=@CategoriaID,CantidadPorUnidad=NULLIF(@CantidadPorUnidad,N''),PrecioUnidad=@PrecioUnidad,UnidadesEnExistencia=@UnidadesEnExistencia,UnidadesEnPedido=@UnidadesEnPedido,NivelDeReorden=@NivelDeReorden,Descontinuado=@Descontinuado WHERE ProductoID=@ProductoID;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Eliminar @ProductoID INT AS DELETE dbo.Productos WHERE ProductoID=@ProductoID;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Listar AS SELECT PedidoID,ClienteID,EmpleadoID,FechaPedido,FechaRequerida,FechaEnvio,TransportistaID,Destinatario,CiudadDestino,PaisDestino FROM dbo.Pedidos ORDER BY FechaPedido DESC,PedidoID DESC;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Insertar @ClienteID INT=NULL,@EmpleadoID INT=NULL,@FechaPedido DATE,@FechaRequerida DATE=NULL,@FechaEnvio DATE=NULL,@TransportistaID INT=NULL,@Destinatario NVARCHAR(60)=NULL,@CiudadDestino NVARCHAR(30)=NULL,@PaisDestino NVARCHAR(30)=NULL AS INSERT dbo.Pedidos VALUES(@ClienteID,@EmpleadoID,@FechaPedido,@FechaRequerida,@FechaEnvio,@TransportistaID,NULLIF(@Destinatario,N''),NULLIF(@CiudadDestino,N''),NULLIF(@PaisDestino,N''));
GO
CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Actualizar @PedidoID INT,@ClienteID INT=NULL,@EmpleadoID INT=NULL,@FechaPedido DATE,@FechaRequerida DATE=NULL,@FechaEnvio DATE=NULL,@TransportistaID INT=NULL,@Destinatario NVARCHAR(60)=NULL,@CiudadDestino NVARCHAR(30)=NULL,@PaisDestino NVARCHAR(30)=NULL AS UPDATE dbo.Pedidos SET ClienteID=@ClienteID,EmpleadoID=@EmpleadoID,FechaPedido=@FechaPedido,FechaRequerida=@FechaRequerida,FechaEnvio=@FechaEnvio,TransportistaID=@TransportistaID,Destinatario=NULLIF(@Destinatario,N''),CiudadDestino=NULLIF(@CiudadDestino,N''),PaisDestino=NULLIF(@PaisDestino,N'') WHERE PedidoID=@PedidoID;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Pedido_Eliminar @PedidoID INT AS BEGIN DELETE dbo.DetallePedidos WHERE PedidoID=@PedidoID; DELETE dbo.Pedidos WHERE PedidoID=@PedidoID; END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Reporte_DetallePedidosPorFechas @FechaInicio DATE,@FechaFin DATE AS SELECT p.PedidoID,p.FechaPedido,p.Destinatario,pr.NombreProducto,d.PrecioUnidad,d.Cantidad,d.Descuento,CAST(d.PrecioUnidad*d.Cantidad*(1-d.Descuento) AS DECIMAL(12,2)) AS Importe FROM dbo.DetallePedidos d INNER JOIN dbo.Pedidos p ON p.PedidoID=d.PedidoID INNER JOIN dbo.Productos pr ON pr.ProductoID=d.ProductoID WHERE p.FechaPedido BETWEEN @FechaInicio AND @FechaFin ORDER BY p.FechaPedido,p.PedidoID;
GO
