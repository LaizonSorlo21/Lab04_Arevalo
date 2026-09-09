using System.Windows;
using WPF_SP.ViewModels;

namespace WPF_SP.Views;

public partial class TareaEditWindow : Window
{
    public TareaEditWindow(TareaEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => TituloBox.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var vm = (TareaEditViewModel)DataContext;
        if (string.IsNullOrWhiteSpace(vm.Titulo))
        {
            MessageBox.Show(this, "El título es obligatorio.", "Datos incompletos",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
