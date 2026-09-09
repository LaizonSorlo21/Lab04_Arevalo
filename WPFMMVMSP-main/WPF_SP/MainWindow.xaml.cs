using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WPF_SP.Data;

namespace WPF_SP;

public partial class MainWindow : Window
{
    private int _productoId, _categoriaId, _proveedorId, _pedidoId;
    public MainWindow() { InitializeComponent(); Loaded += async (_, _) => await InicializarAsync(); }

    private async Task InicializarAsync()
    {
        dpDesde.SelectedDate = DateTime.Today.AddMonths(-1); dpHasta.SelectedDate = DateTime.Today;
        dpFechaPedido.SelectedDate = DateTime.Today;
        try { await CargarCombosAsync(); await RecargarTodoAsync(); Estado("Listo."); }
        catch (Exception ex) { Estado("No se pudo conectar: " + ex.Message); }
    }
    private async Task RecargarTodoAsync()
    {
        dgProductos.ItemsSource = await TablaAsync("dbo.usp_Producto_Listar");
        dgCategorias.ItemsSource = await TablaAsync("dbo.usp_Categoria_Listar");
        dgProveedores.ItemsSource = await TablaAsync("dbo.usp_Proveedor_Listar");
        dgPedidos.ItemsSource = await TablaAsync("dbo.usp_Pedido_Listar");
    }
    private async Task CargarCombosAsync()
    {
        cbProductoProveedor.ItemsSource = await ListaAsync("SELECT ProveedorID, CompaniaNombre FROM dbo.Proveedores ORDER BY CompaniaNombre", "ProveedorID", "CompaniaNombre");
        cbProductoCategoria.ItemsSource = await ListaAsync("SELECT CategoriaID, NombreCategoria FROM dbo.Categorias ORDER BY NombreCategoria", "CategoriaID", "NombreCategoria");
        cbPedidoCliente.ItemsSource = await ListaAsync("SELECT ClienteID, Empresa FROM dbo.Clientes ORDER BY Empresa", "ClienteID", "Empresa");
        cbPedidoEmpleado.ItemsSource = await ListaAsync("SELECT EmpleadoID, CONCAT(Nombre, ' ', Apellidos) AS Nombre FROM dbo.Empleados ORDER BY Nombre", "EmpleadoID", "Nombre");
        cbPedidoTransportista.ItemsSource = await ListaAsync("SELECT TransportistaID, CompaniaNombre FROM dbo.Transportistas ORDER BY CompaniaNombre", "TransportistaID", "CompaniaNombre");
    }
    private async Task<List<Opcion>> ListaAsync(string sql, string id, string nombre)
    {
        var lista = new List<Opcion>(); await using var cn = new SqlConnection(DbConfig.ConnectionString); await using var cmd = new SqlCommand(sql, cn); await cn.OpenAsync(); await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) lista.Add(new Opcion(rd.GetInt32(rd.GetOrdinal(id)), rd.GetString(rd.GetOrdinal(nombre)))); return lista;
    }
    private async Task<DataView> TablaAsync(string sp, params SqlParameter[] parametros)
    {
        var dt = new DataTable(); await using var cn = new SqlConnection(DbConfig.ConnectionString); await using var cmd = new SqlCommand(sp, cn) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddRange(parametros); await cn.OpenAsync(); using var rd = await cmd.ExecuteReaderAsync(); dt.Load(rd); return dt.DefaultView;
    }
    private async Task EjecutarAsync(string sp, params SqlParameter[] parametros)
    {
        await using var cn = new SqlConnection(DbConfig.ConnectionString); await using var cmd = new SqlCommand(sp, cn) { CommandType = CommandType.StoredProcedure }; cmd.Parameters.AddRange(parametros); await cn.OpenAsync(); await cmd.ExecuteNonQueryAsync();
    }
    private static SqlParameter P(string n, object? v) => new(n, v ?? DBNull.Value);
    private static object? Valor(ComboBox cb) => cb.SelectedValue is int id ? id : null;
    private static string Texto(TextBox box) => string.IsNullOrWhiteSpace(box.Text) ? string.Empty : box.Text.Trim();
    private static short Numero(TextBox box) => short.TryParse(box.Text, out var n) ? n : (short)0;
    private static decimal Decimal(TextBox box) => decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var n) ? n : 0;
    private void Estado(string text) => lblEstado.Text = text;
    private bool Confirmar(string texto) => MessageBox.Show(texto, "NeptunoDB", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    private async Task Seguro(Func<Task> accion) { try { await accion(); } catch (Exception ex) { Estado("Error: " + ex.Message); MessageBox.Show(ex.Message, "Operación no realizada", MessageBoxButton.OK, MessageBoxImage.Error); } }

    private void ProductoNuevo_Click(object s, RoutedEventArgs e) { _productoId = 0; txtProductoNombre.Clear(); txtCantidadUnidad.Clear(); txtPrecio.Text = "0"; txtExistencias.Text = txtEnPedido.Text = txtReorden.Text = "0"; cbProductoProveedor.SelectedIndex = cbProductoCategoria.SelectedIndex = -1; chkDescontinuado.IsChecked = false; }
    private async void ProductoGuardar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (Texto(txtProductoNombre) == "") throw new InvalidOperationException("El nombre del producto es obligatorio."); var sp = _productoId == 0 ? "dbo.usp_Producto_Insertar" : "dbo.usp_Producto_Actualizar"; var p = new List<SqlParameter> { P("@NombreProducto", Texto(txtProductoNombre)), P("@ProveedorID", Valor(cbProductoProveedor)), P("@CategoriaID", Valor(cbProductoCategoria)), P("@CantidadPorUnidad", Texto(txtCantidadUnidad)), P("@PrecioUnidad", Decimal(txtPrecio)), P("@UnidadesEnExistencia", Numero(txtExistencias)), P("@UnidadesEnPedido", Numero(txtEnPedido)), P("@NivelDeReorden", Numero(txtReorden)), P("@Descontinuado", chkDescontinuado.IsChecked == true) }; if (_productoId > 0) p.Insert(0, P("@ProductoID", _productoId)); await EjecutarAsync(sp, p.ToArray()); await RecargarTodoAsync(); ProductoNuevo_Click(s, e); Estado("Producto guardado."); });
    private async void ProductoEliminar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (_productoId == 0 || !Confirmar("¿Eliminar el producto seleccionado?")) return; await EjecutarAsync("dbo.usp_Producto_Eliminar", P("@ProductoID", _productoId)); await RecargarTodoAsync(); ProductoNuevo_Click(s, e); Estado("Producto eliminado."); });
    private void Productos_SelectionChanged(object s, SelectionChangedEventArgs e) { if (dgProductos.SelectedItem is not DataRowView r) return; _productoId = (int)r["ProductoID"]; txtProductoNombre.Text = r["NombreProducto"].ToString(); cbProductoProveedor.SelectedValue = Db(r, "ProveedorID"); cbProductoCategoria.SelectedValue = Db(r, "CategoriaID"); txtCantidadUnidad.Text = r["CantidadPorUnidad"].ToString(); txtPrecio.Text = r["PrecioUnidad"].ToString(); txtExistencias.Text = r["UnidadesEnExistencia"].ToString(); txtEnPedido.Text = r["UnidadesEnPedido"].ToString(); txtReorden.Text = r["NivelDeReorden"].ToString(); chkDescontinuado.IsChecked = (bool)r["Descontinuado"]; }

    private void CategoriaNuevo_Click(object s, RoutedEventArgs e) { _categoriaId = 0; txtCategoriaNombre.Clear(); txtCategoriaDescripcion.Clear(); }
    private async void CategoriaGuardar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (Texto(txtCategoriaNombre) == "") throw new InvalidOperationException("El nombre de categoría es obligatorio."); await EjecutarAsync(_categoriaId == 0 ? "dbo.usp_Categoria_Insertar" : "dbo.usp_Categoria_Actualizar", _categoriaId == 0 ? new[] { P("@NombreCategoria", Texto(txtCategoriaNombre)), P("@Descripcion", Texto(txtCategoriaDescripcion)) } : new[] { P("@CategoriaID", _categoriaId), P("@NombreCategoria", Texto(txtCategoriaNombre)), P("@Descripcion", Texto(txtCategoriaDescripcion)) }); await CargarCombosAsync(); await RecargarTodoAsync(); CategoriaNuevo_Click(s, e); Estado("Categoría guardada."); });
    private async void CategoriaEliminar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (_categoriaId == 0 || !Confirmar("¿Eliminar la categoría seleccionada?")) return; await EjecutarAsync("dbo.usp_Categoria_Eliminar", P("@CategoriaID", _categoriaId)); await CargarCombosAsync(); await RecargarTodoAsync(); CategoriaNuevo_Click(s, e); Estado("Categoría eliminada."); });
    private void Categorias_SelectionChanged(object s, SelectionChangedEventArgs e) { if (dgCategorias.SelectedItem is not DataRowView r) return; _categoriaId = (int)r["CategoriaID"]; txtCategoriaNombre.Text = r["NombreCategoria"].ToString(); txtCategoriaDescripcion.Text = r["Descripcion"].ToString(); }

    private void ProveedorNuevo_Click(object s, RoutedEventArgs e) { _proveedorId = 0; foreach (var b in new[] { txtProveedorCompania, txtProveedorContacto, txtProveedorCargo, txtProveedorDireccion, txtProveedorCiudad, txtProveedorPostal, txtProveedorPais, txtProveedorTelefono, txtProveedorFax }) b.Clear(); }
    private async void ProveedorGuardar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (Texto(txtProveedorCompania) == "") throw new InvalidOperationException("La compañía es obligatoria."); var p = new List<SqlParameter> { P("@CompaniaNombre", Texto(txtProveedorCompania)), P("@NombreContacto", Texto(txtProveedorContacto)), P("@CargoContacto", Texto(txtProveedorCargo)), P("@Direccion", Texto(txtProveedorDireccion)), P("@Ciudad", Texto(txtProveedorCiudad)), P("@CodigoPostal", Texto(txtProveedorPostal)), P("@Pais", Texto(txtProveedorPais)), P("@Telefono", Texto(txtProveedorTelefono)), P("@Fax", Texto(txtProveedorFax)) }; if (_proveedorId > 0) p.Insert(0, P("@ProveedorID", _proveedorId)); await EjecutarAsync(_proveedorId == 0 ? "dbo.usp_Proveedor_Insertar" : "dbo.usp_Proveedor_Actualizar", p.ToArray()); await CargarCombosAsync(); await RecargarTodoAsync(); ProveedorNuevo_Click(s, e); Estado("Proveedor guardado."); });
    private async void ProveedorEliminar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (_proveedorId == 0 || !Confirmar("¿Eliminar el proveedor seleccionado?")) return; await EjecutarAsync("dbo.usp_Proveedor_Eliminar", P("@ProveedorID", _proveedorId)); await CargarCombosAsync(); await RecargarTodoAsync(); ProveedorNuevo_Click(s, e); Estado("Proveedor eliminado."); });
    private void Proveedores_SelectionChanged(object s, SelectionChangedEventArgs e) { if (dgProveedores.SelectedItem is not DataRowView r) return; _proveedorId = (int)r["ProveedorID"]; txtProveedorCompania.Text = r["CompaniaNombre"].ToString(); txtProveedorContacto.Text = r["NombreContacto"].ToString(); txtProveedorCargo.Text = r["CargoContacto"].ToString(); txtProveedorDireccion.Text = r["Direccion"].ToString(); txtProveedorCiudad.Text = r["Ciudad"].ToString(); txtProveedorPostal.Text = r["CodigoPostal"].ToString(); txtProveedorPais.Text = r["Pais"].ToString(); txtProveedorTelefono.Text = r["Telefono"].ToString(); txtProveedorFax.Text = r["Fax"].ToString(); }
    private async void BuscarProveedores_Click(object s, RoutedEventArgs e) => await Seguro(async () => { dgBusquedaProveedores.ItemsSource = await TablaAsync("dbo.usp_Proveedor_Buscar", P("@NombreContacto", Texto(txtBuscarContacto)), P("@Ciudad", Texto(txtBuscarCiudad))); Estado("Búsqueda actualizada."); });
    private void LimpiarBusquedaProveedores_Click(object s, RoutedEventArgs e) { txtBuscarContacto.Clear(); txtBuscarCiudad.Clear(); dgBusquedaProveedores.ItemsSource = null; }

    private void PedidoNuevo_Click(object s, RoutedEventArgs e) { _pedidoId = 0; cbPedidoCliente.SelectedIndex = cbPedidoEmpleado.SelectedIndex = cbPedidoTransportista.SelectedIndex = -1; dpFechaPedido.SelectedDate = DateTime.Today; dpFechaRequerida.SelectedDate = dpFechaEnvio.SelectedDate = null; txtDestinatario.Clear(); txtCiudadDestino.Clear(); txtPaisDestino.Clear(); }
    private async void PedidoGuardar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (dpFechaPedido.SelectedDate is null) throw new InvalidOperationException("La fecha de pedido es obligatoria."); var p = new List<SqlParameter> { P("@ClienteID", Valor(cbPedidoCliente)), P("@EmpleadoID", Valor(cbPedidoEmpleado)), P("@FechaPedido", dpFechaPedido.SelectedDate.Value), P("@FechaRequerida", dpFechaRequerida.SelectedDate), P("@FechaEnvio", dpFechaEnvio.SelectedDate), P("@TransportistaID", Valor(cbPedidoTransportista)), P("@Destinatario", Texto(txtDestinatario)), P("@CiudadDestino", Texto(txtCiudadDestino)), P("@PaisDestino", Texto(txtPaisDestino)) }; if (_pedidoId > 0) p.Insert(0, P("@PedidoID", _pedidoId)); await EjecutarAsync(_pedidoId == 0 ? "dbo.usp_Pedido_Insertar" : "dbo.usp_Pedido_Actualizar", p.ToArray()); await RecargarTodoAsync(); PedidoNuevo_Click(s, e); Estado("Pedido guardado."); });
    private async void PedidoEliminar_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (_pedidoId == 0 || !Confirmar("¿Eliminar el pedido y sus detalles?")) return; await EjecutarAsync("dbo.usp_Pedido_Eliminar", P("@PedidoID", _pedidoId)); await RecargarTodoAsync(); PedidoNuevo_Click(s, e); Estado("Pedido eliminado."); });
    private void Pedidos_SelectionChanged(object s, SelectionChangedEventArgs e) { if (dgPedidos.SelectedItem is not DataRowView r) return; _pedidoId = (int)r["PedidoID"]; cbPedidoCliente.SelectedValue = Db(r, "ClienteID"); cbPedidoEmpleado.SelectedValue = Db(r, "EmpleadoID"); cbPedidoTransportista.SelectedValue = Db(r, "TransportistaID"); dpFechaPedido.SelectedDate = (DateTime)r["FechaPedido"]; dpFechaRequerida.SelectedDate = Fecha(r, "FechaRequerida"); dpFechaEnvio.SelectedDate = Fecha(r, "FechaEnvio"); txtDestinatario.Text = r["Destinatario"].ToString(); txtCiudadDestino.Text = r["CiudadDestino"].ToString(); txtPaisDestino.Text = r["PaisDestino"].ToString(); }
    private async void Reporte_Click(object s, RoutedEventArgs e) => await Seguro(async () => { if (dpDesde.SelectedDate is null || dpHasta.SelectedDate is null) throw new InvalidOperationException("Indique ambas fechas."); if (dpDesde.SelectedDate > dpHasta.SelectedDate) throw new InvalidOperationException("La fecha inicial no puede ser mayor que la final."); dgReporte.ItemsSource = await TablaAsync("dbo.usp_Reporte_DetallePedidosPorFechas", P("@FechaInicio", dpDesde.SelectedDate.Value), P("@FechaFin", dpHasta.SelectedDate.Value)); Estado("Reporte generado."); });
    private static object? Db(DataRowView r, string c) => r[c] == DBNull.Value ? null : r[c]; private static DateTime? Fecha(DataRowView r, string c) => r[c] == DBNull.Value ? null : (DateTime)r[c];
    private sealed record Opcion(int Id, string Nombre);
}
