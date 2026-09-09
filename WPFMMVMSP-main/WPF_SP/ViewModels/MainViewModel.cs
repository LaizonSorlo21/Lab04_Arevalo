using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPF_SP.Data;
using WPF_SP.Models;
using WPF_SP.Services;

namespace WPF_SP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITareaRepository _repository;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Tarea> Tareas { get; } = new();

    [ObservableProperty]
    private string _nuevoTitulo = string.Empty;

    [ObservableProperty]
    private string? _nuevaDescripcion;

    [ObservableProperty]
    private EstadoFiltro _currentFilter = EstadoFiltro.Todas;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasTareas;

    /// <summary>Se dispara cuando el usuario pide editar una tarea; la vista es responsable de mostrar el diálogo.</summary>
    public event Action<TareaEditViewModel>? EditarSolicitado;

    public MainViewModel(ITareaRepository repository, IDialogService dialogService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    partial void OnCurrentFilterChanged(EstadoFiltro value) => _ = CargarAsync();

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var resultado = CurrentFilter switch
            {
                EstadoFiltro.Pendientes => await _repository.ListarPorEstadoAsync(false),
                EstadoFiltro.Completadas => await _repository.ListarPorEstadoAsync(true),
                _ => await _repository.ListarTodasAsync()
            };

            cancellationToken.ThrowIfCancellationRequested();

            Tareas.Clear();
            foreach (var tarea in resultado)
            {
                Tareas.Add(tarea);
            }
            HasTareas = Tareas.Count > 0;
        }
        catch (OperationCanceledException)
        {
            // Ignorar cancelación normal de tareas
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo cargar la lista: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeAgregar))]
    private async Task AgregarAsync()
    {
        ErrorMessage = null;
        try
        {
            await _repository.CrearAsync(
                NuevoTitulo.Trim(), 
                string.IsNullOrWhiteSpace(NuevaDescripcion) ? null : NuevaDescripcion.Trim());
            
            NuevoTitulo = string.Empty;
            NuevaDescripcion = null;
            await CargarAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo crear la tarea: {ex.Message}";
        }
    }

    private bool PuedeAgregar() => !string.IsNullOrWhiteSpace(NuevoTitulo);

    partial void OnNuevoTituloChanged(string value) => AgregarAsyncCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private async Task EditarAsync(Tarea? tarea)
    {
        if (tarea is null) return;

        ErrorMessage = null;
        try
        {
            var actual = await _repository.ObtenerPorIdAsync(tarea.TareaID);
            if (actual is null)
            {
                ErrorMessage = "La tarea ya no existe.";
                await CargarAsync();
                return;
            }

            EditarSolicitado?.Invoke(new TareaEditViewModel(actual));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo abrir la tarea: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task GuardarEdicionAsync(TareaEditViewModel? edicion)
    {
        if (edicion is null || string.IsNullOrWhiteSpace(edicion.Titulo)) return;

        ErrorMessage = null;
        try
        {
            await _repository.ActualizarAsync(
                edicion.TareaID, 
                edicion.Titulo.Trim(),
                string.IsNullOrWhiteSpace(edicion.Descripcion) ? null : edicion.Descripcion.Trim());
            
            await CargarAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo actualizar la tarea: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ToggleCompletadaAsync(Tarea? tarea)
    {
        if (tarea is null) return;

        ErrorMessage = null;
        try
        {
            await _repository.MarcarCompletadaAsync(tarea.TareaID, tarea.Completada);
            if (CurrentFilter != EstadoFiltro.Todas)
            {
                await CargarAsync();
            }
        }
        catch (Exception ex)
        {
            tarea.Completada = !tarea.Completada; // Revertir en caso de fallo
            ErrorMessage = $"No se pudo actualizar el estado: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EliminarAsync(Tarea? tarea)
    {
        if (tarea is null) return;

        bool confirmado = _dialogService.Confirmar(
            $"¿Eliminar la tarea \"{tarea.Titulo}\"?", 
            "Confirmar eliminación");

        if (!confirmado) return;

        ErrorMessage = null;
        try
        {
            await _repository.EliminarAsync(tarea.TareaID);
            Tareas.Remove(tarea);
            HasTareas = Tareas.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"No se pudo eliminar la tarea: {ex.Message}";
        }
    }
}