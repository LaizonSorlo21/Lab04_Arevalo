using WPF_SP.Models;

namespace WPF_SP.Data;

public interface ITareaRepository
{
    Task<int> CrearAsync(string titulo, string? descripcion);
    Task<Tarea?> ObtenerPorIdAsync(int tareaId);
    Task<List<Tarea>> ListarTodasAsync();
    Task<List<Tarea>> ListarPorEstadoAsync(bool completada);
    Task ActualizarAsync(int tareaId, string titulo, string? descripcion);
    Task MarcarCompletadaAsync(int tareaId, bool completada);
    Task EliminarAsync(int tareaId);
}
