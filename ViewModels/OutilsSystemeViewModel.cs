using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MenuConfig2._0.ViewModels
{
    public class OutilsSystemeViewModel
    {
        public ICommand ReparFichiersSystemeCommand { get; }
        public ICommand GererServicesCommand { get; }
        public ICommand VerifierWindowsUpdateCommand { get; }
        public ICommand OuvrirExplorateurCommand { get; }

        public OutilsSystemeViewModel()
        {
            ReparFichiersSystemeCommand = new RelayCommand(ReparerFichiersSysteme);
            GererServicesCommand = new RelayCommand(GererServicesWindows);
            VerifierWindowsUpdateCommand = new RelayCommand(VerifierWindowsUpdate);
            OuvrirExplorateurCommand = new RelayCommand(OuvrirExplorateurWindows);
        }

        private void ReparerFichiersSysteme(object? obj)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c sfc /scannow",
                Verb = "runas", // Exécute en mode administrateur
                UseShellExecute = true
            });
        }

        private void GererServicesWindows(object? obj)
        {
            Process.Start("services.msc");
        }

        private void VerifierWindowsUpdate(object? obj)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c wuauclt /detectnow",
                Verb = "runas",
                UseShellExecute = true
            });
        }

        private void OuvrirExplorateurWindows(object? obj)
        {
            Process.Start("explorer.exe");
        }
    }
}
