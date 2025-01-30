using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MenuConfig2._0.Helpers;
using MenuConfig2._0.ViewModels;

namespace MenuConfig2._0.ViewModels
{
    public class OutilsSystemeViewModel : BaseViewModel
    {
        public ICommand StartRepairCommand { get; }
        public ICommand SetRepairLevelCommand { get; }
        public ICommand VerifierWindowsUpdateCommand { get; }
        public ICommand GererServicesCommand { get; }
        public ICommand OuvrirExplorateurCommand { get; }

        private int _repairLevel = 1;
        public int RepairLevel
        {
            get => _repairLevel;
            set
            {
                _repairLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDiskSelectionVisible));
                Console.WriteLine($"✅ Niveau de réparation sélectionné : {RepairLevel}");
            }
        }

        public bool IsDiskSelectionVisible => RepairLevel == 3;

        private string _selectedDisk = "C:";
        public string SelectedDisk
        {
            get => _selectedDisk;
            set
            {
                _selectedDisk = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailableDisks { get; } = new ObservableCollection<string>();

        public OutilsSystemeViewModel()
        {

            var testConverter = new IntToBoolConverter(); // ✅ Vérifie si Visual Studio reconnaît la classe
            Console.WriteLine("IntToBoolConverter accessible !");

            Console.WriteLine("OutilsSystemeViewModel chargé"); // 🔥 Debug

            StartRepairCommand = new RelayCommand(StartRepair);
            SetRepairLevelCommand = new RelayCommand(SetRepairLevel);
            VerifierWindowsUpdateCommand = new RelayCommand(VerifierWindowsUpdate);
            GererServicesCommand = new RelayCommand(GererServices);
            OuvrirExplorateurCommand = new RelayCommand(OuvrirExplorateurWindows);
            
            RefreshCommands();
            LoadAvailableDisks();
        }

        private void SetRepairLevel(object? level)
        {
            if (level is string levelStr && int.TryParse(levelStr, out int lvl))
            {
                RepairLevel = lvl;
                Console.WriteLine($"🔹 Niveau de réparation changé à {RepairLevel}");
            }
        }

        private void StartRepair(object? parameter)
        {
            Console.WriteLine($"✅ Exécution de la réparation : Niveau {RepairLevel}");
            MessageBox.Show($"Lancement de la réparation (Niveau {RepairLevel})", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                string command = "";
                switch (RepairLevel)
                {
                    case 1:
                        command = "sfc /scannow";
                        break;
                    case 2:
                        command = "Dism /Online /Cleanup-Image /RestoreHealth && sfc /scannow";
                        break;
                    case 3:
                        command = $"chkdsk C: /f /r"; // 🚀 Modifier pour permettre la sélection du disque plus tard
                        break;
                }

                if (!string.IsNullOrEmpty(command))
                {
                    foreach (var process in Process.GetProcessesByName("cmd"))
                    {
                        process.Kill();
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/k \"{command}\"",
                        Verb = "runas",
                        UseShellExecute = true
                    });

                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'exécution de la commande : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartRepairWithPowerShell(object? parameter)
        {
            Console.WriteLine("✅ Exécution via PowerShell !");
            MessageBox.Show("Commande PowerShell en cours d'exécution...", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoExit -Command \"sfc /scannow\"",
                    Verb = "runas", // Exécute en admin
                    UseShellExecute = true
                };

                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur PowerShell : {ex.Message}");
                MessageBox.Show($"Erreur PowerShell : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableDisks()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Fixed)
                    .Select(d => d.Name.TrimEnd('\\'))
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableDisks.Clear();
                    foreach (var drive in drives)
                    {
                        AvailableDisks.Add(drive);
                    }
                    if (AvailableDisks.Count > 0)
                    {
                        SelectedDisk = AvailableDisks[0];
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors du chargement des disques : {ex.Message}");
            }
        }

        private void SetRepairLevel(object? level)
        {
            if (level is string levelStr && int.TryParse(levelStr, out int lvl))
            {
                RepairLevel = lvl;
                Console.WriteLine($"✅ Niveau de réparation changé : {RepairLevel}"); // 🔥 Debug
            }
        }

        private void VerifierWindowsUpdate(object? parameter)
        {
            RunCommand("wuauclt /detectnow");
        }

        private void GererServices(object? parameter)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "mmc.exe",
                Arguments = "services.msc",
                UseShellExecute = true,
                Verb = "runas" // Exécute en mode administrateur
            });
        }

        private void OuvrirExplorateurWindows(object? parameter)
        {
            Process.Start("explorer.exe");
        }

        private void RunCommand(string command)
        {
            Console.WriteLine($"Exécution de la commande : {command}"); // 🔥 Debug

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    Verb = "runas",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution de la commande : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RefreshCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

    }
}
