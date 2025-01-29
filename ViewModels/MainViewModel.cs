using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MenuConfig2._0.ViewModels
{
    public class MainViewModel
    {
        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);
        }

        private void Navigate(object? page)
        {
            Console.WriteLine($"Navigation vers : {page}"); // Debug

            if (page is string pageName)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null && mainWindow.MainFrame != null)
                    {
                        Uri pageUri = new Uri($"/Pages/{pageName}.xaml", UriKind.Relative);
                        mainWindow.MainFrame.NavigationService.Navigate(pageUri);

                        // Supprime la page précédente de la mémoire
                        mainWindow.MainFrame.NavigationService.RemoveBackEntry();
                    }
                });
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged;
    }

}
