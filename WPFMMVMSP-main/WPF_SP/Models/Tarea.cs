using CommunityToolkit.Mvvm.ComponentModel;

namespace WPF_SP.Models;

public partial class Tarea : ObservableObject
{
    [ObservableProperty]
    private int tareaID;

    [ObservableProperty]
    private string titulo = string.Empty;

    [ObservableProperty]
    private string? descripcion;

    [ObservableProperty]
    private bool completada;

    [ObservableProperty]
    private DateTime fechaCreacion;

    [ObservableProperty]
    private DateTime? fechaCompletada;
}
