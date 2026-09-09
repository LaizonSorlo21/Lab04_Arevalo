using System.Data;
using Microsoft.Data.SqlClient;
using WPF_SP.Models;

namespace WPF_SP.Data;

public class TareaRepository : ITareaRepository
{
    private readonly string _connectionString;

    public TareaRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<int> CrearAsync(string titulo, string? descripcion, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_Crear", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = titulo;
            command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, -1).Value = (object?)descripcion ?? DBNull.Value;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }, cancellationToken);
    }

    public async Task<Tarea?> ObtenerPorIdAsync(int tareaId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_ObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? MapTarea(reader) : null;
        }, cancellationToken);
    }

    public async Task<List<Tarea>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_ListarTodas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var tareas = new List<Tarea>();
            while (await reader.ReadAsync(cancellationToken))
            {
                tareas.Add(MapTarea(reader));
            }
            return tareas;
        }, cancellationToken);
    }

    public async Task<List<Tarea>> ListarPorEstadoAsync(bool completada, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_ListarPorEstado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@Completada", SqlDbType.Bit).Value = completada;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var tareas = new List<Tarea>();
            while (await reader.ReadAsync(cancellationToken))
            {
                tareas.Add(MapTarea(reader));
            }
            return tareas;
        }, cancellationToken);
    }

    public async Task ActualizarAsync(int tareaId, string titulo, string? descripcion, CancellationToken cancellationToken = default)
    {
        await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_Actualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;
            command.Parameters.Add("@Titulo", SqlDbType.NVarChar, 150).Value = titulo;
            command.Parameters.Add("@Descripcion", SqlDbType.NVarChar, -1).Value = (object?)descripcion ?? DBNull.Value;

            await command.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }

    public async Task MarcarCompletadaAsync(int tareaId, bool completada, CancellationToken cancellationToken = default)
    {
        await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_MarcarCompletada", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;
            command.Parameters.Add("@Completada", SqlDbType.Bit).Value = completada;

            await command.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }

    public async Task EliminarAsync(int tareaId, CancellationToken cancellationToken = default)
    {
        await ExecuteWithConnectionAsync(async connection =>
        {
            await using var command = new SqlCommand("dbo.usp_Tarea_Eliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add("@TareaID", SqlDbType.Int).Value = tareaId;

            await command.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Método auxiliar privado para centralizar la apertura y disposición de la conexión SQL de manera segura.
    /// </summary>
    private async Task<T> ExecuteWithConnectionAsync<T>(Func<SqlConnection, Task<T>> action, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await action(connection);
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