using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF_SP.Models;

namespace WPF_SP.ViewModels;

public partial class TareaEditViewModel : ObservableValidator
{
    public int TareaID { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsValido))]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    private string _titulo = string.Empty;

    [ObservableProperty]
    private string? _descripcion;

    [ObservableProperty]
    private bool _completada;

    public bool EsNueva => TareaID == 0;
    
    public string TituloVentana => EsNueva ? "Crear nueva tarea" : "Editar tarea existente";

    public bool EsValido => !string.IsNullOrWhiteSpace(Titulo);

     <summary>
     </summary>
    public event Action<bool>? CerrarVentanaSolicitado;

    public TareaEditViewModel(Tarea? tarea = null)
    {
        if (tarea is not null)
        {
            TareaID = tarea.TareaID;
            _titulo = tarea.Titulo ?? string.Empty;
            _descripcion = tarea.Descripcion;
            _completada = tarea.Completada;
        }
    }

    [RelayCommand(CanExecute = nameof(EsValido))]
    private void Guardar()
    {
        if (!EsValido) return;
        
        CerrarVentanaSolicitado?.Invoke(true);
    }

    [RelayCommand]
    private void Cancelar()
    {
        CerrarVentanaSolicitado?.Invoke(false);
    }

     <summary>
    </summary>
    public Tarea ToTarea()
    {
        return new Tarea
        {
            TareaID = this.TareaID,
            Titulo = this.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(this.Descripcion) ? null : this.Descripcion.Trim(),
            Completada = this.Completada
        };
    }
}