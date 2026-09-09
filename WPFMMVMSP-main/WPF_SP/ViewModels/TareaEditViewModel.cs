using CommunityToolkit.Mvvm.ComponentModel;
using WPF_SP.Models;

namespace WPF_SP.ViewModels;

public partial class TareaEditViewModel : ObservableObject
{
    public int TareaID { get; }

    [ObservableProperty]
    private string titulo;

    [ObservableProperty]
    private string? descripcion;

    public bool EsNueva => TareaID == 0;
    public string TituloVentana => EsNueva ? "Nueva tarea" : "Editar tarea";

    public TareaEditViewModel(Tarea? tarea = null)
    {
        TareaID = tarea?.TareaID ?? 0;
        titulo = tarea?.Titulo ?? string.Empty;
        descripcion = tarea?.Descripcion;
    }
}
