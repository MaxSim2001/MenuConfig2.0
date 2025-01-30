using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MenuConfig2._0.ViewModels;


namespace MenuConfig2._0.Pages
{
    /// <summary>
    /// Logique d'interaction pour DiagnosticPage.xaml
    /// </summary>
    public partial class DiagnosticPage : Page
    {
        private DiagnosticViewModel _viewModel;

        public DiagnosticPage()
        {
            InitializeComponent();

            // 🔥 Assigne manuellement le ViewModel pour éviter le NULL
            this.DataContext = new DiagnosticViewModel();

            _viewModel = DataContext as DiagnosticViewModel;

            this.Loaded += DiagnosticPage_Loaded;
            this.Unloaded += DiagnosticPage_Unloaded;
        }

        private void DiagnosticPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.IsActive = true;
                Console.WriteLine("✅ Onglet Diagnostic chargé : IsActive = true");
            }
            else
            {
                Console.WriteLine("❌ DataContext toujours NULL après initialisation !");
            }
        }

        private void DiagnosticPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.IsActive = false;
                Console.WriteLine("⛔ Onglet Diagnostic quitté : IsActive = false");
            }
        }
    }
}
