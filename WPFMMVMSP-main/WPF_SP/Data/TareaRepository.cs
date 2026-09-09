using System.Data;
using Microsoft.Data.SqlClient;
using WPF_SP.Models;

namespace WPF_SP.Data;

public class TareaRepository : ITareaRepository
{
    private readonly string _connectionString;

    public TareaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CrearAsync(string titulo, string? descripcion)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_Crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = titulo;
        command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, -1).Value = (object?)descripcion ?? DBNull.Value;

        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<Tarea?> ObtenerPorIdAsync(int tareaId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_ObtenerPorId", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapTarea(reader) : null;
    }

    public async Task<List<Tarea>> ListarTodasAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_ListarTodas", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        var tareas = new List<Tarea>();
        while (await reader.ReadAsync())
        {
            tareas.Add(MapTarea(reader));
        }
        return tareas;
    }

    public async Task<List<Tarea>> ListarPorEstadoAsync(bool completada)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_ListarPorEstado", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Completada", SqlDbType.Bit).Value = completada;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        var tareas = new List<Tarea>();
        while (await reader.ReadAsync())
        {
            tareas.Add(MapTarea(reader));
        }
        return tareas;
    }

    public async Task ActualizarAsync(int tareaId, string titulo, string? descripcion)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_Actualizar", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;
        command.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = titulo;
        command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, -1).Value = (object?)descripcion ?? DBNull.Value;

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task MarcarCompletadaAsync(int tareaId, bool completada)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_MarcarCompletada", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;
        command.Parameters.Add("@Completada", SqlDbType.Bit).Value = completada;

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int tareaId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_Tarea_Eliminar", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static Tarea MapTarea(SqlDataReader reader)
    {
        return new Tarea
        {
            TareaID = reader.GetInt32(reader.GetOrdinal("TareaID")),
            Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
            Completada = reader.GetBoolean(reader.GetOrdinal("Completada")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
            FechaCompletada = reader.IsDBNull(reader.GetOrdinal("FechaCompletada")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaCompletada"))
        };
    }
}
