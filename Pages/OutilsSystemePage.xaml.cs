using MenuConfig2._0.Helpers;
using MenuConfig2._0.ViewModels;
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
using System.Globalization;

namespace MenuConfig2._0.Pages
{
    /// <summary>
    /// Logique d'interaction pour OutilsSystemePage.xaml
    /// </summary>
    public partial class OutilsSystemePage : Page
    {
        public OutilsSystemePage()
        {
            InitializeComponent();

            // 🔹 Forçage du DataContext
            this.DataContext = new OutilsSystemeViewModel();

            // 🔹 Appliquer le convertisseur en C# à chaque bouton Radio
            SetRadioButtonBindings();
            var viewModel = (OutilsSystemeViewModel)this.DataContext;
        }

        private void SetRadioButtonBindings()
        {
            // Vérifie que le ViewModel est bien attaché
            if (this.DataContext is OutilsSystemeViewModel viewModel)
            {
                var converter = new IntToBoolConverter();

                // 🔹 Niveau 1
                Binding level1Binding = new Binding("RepairLevel")
                {
                    Source = viewModel,
                    Mode = BindingMode.TwoWay,
                    Converter = converter,
                    ConverterParameter = "1"
                };
                Niveau1RadioButton.SetBinding(RadioButton.IsCheckedProperty, level1Binding);

                // 🔹 Niveau 2
                Binding level2Binding = new Binding("RepairLevel")
                {
                    Source = viewModel,
                    Mode = BindingMode.TwoWay,
                    Converter = converter,
                    ConverterParameter = "2"
                };
                Niveau2RadioButton.SetBinding(RadioButton.IsCheckedProperty, level2Binding);

                // 🔹 Niveau 3
                Binding level3Binding = new Binding("RepairLevel")
                {
                    Source = viewModel,
                    Mode = BindingMode.TwoWay,
                    Converter = converter,
                    ConverterParameter = "3"
                };
                Niveau3RadioButton.SetBinding(RadioButton.IsCheckedProperty, level3Binding);
            }
        }
    }
}
