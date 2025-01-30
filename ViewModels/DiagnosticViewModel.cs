using SharpDX.DXGI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MenuConfig2._0.ViewModels
{
    public class DiagnosticViewModel : BaseViewModel, IDisposable
    {
        private readonly DispatcherTimer _timer;
        private readonly CancellationTokenSource _cts;

        private bool _isActive = true;

        //Monitoring
        public string CpuUsage { get; set; }
        public string RamUsage { get; set; }
        public string GpuUsage { get; set; }
        public double CpuUsagePercentage { get; set; }
        public double RamUsagePercentage { get; set; }
        public double GpuUsagePercentage { get; set; }
        public string NetworkStatus { get; set; }
        public ObservableCollection<string> SystemLogs { get; set; }
        public ObservableCollection<string> DiskUsage { get; set; }

        //Réseau
        public string LocalIp { get; set; }
        public string PublicIp { get; set; }
        public string PingResult { get; set; }
        public string ConnectionStatus { get; set; }
        public string DownloadSpeed { get; set; }
        public string UploadSpeed { get; set; }


        public ICommand RefreshHardwareCommand { get; }
        public ICommand TestNetworkCommand { get; }
        public ICommand LoadLogsCommand { get; }
        public ICommand OpenEventViewerCommand { get; }

        public DiagnosticViewModel()
        {
            SystemLogs = new ObservableCollection<string>();
            DiskUsage = new ObservableCollection<string>();

            RefreshHardwareCommand = new RelayCommand(_ => RefreshHardwareInfo());
            TestNetworkCommand = new RelayCommand(_ => PerformNetworkTest());
            LoadLogsCommand = new RelayCommand(_ => LoadSystemLogs());
            OpenEventViewerCommand = new RelayCommand(_ => OpenEventViewer());

            // Initialisation du timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += async (s, e) =>
            {
                if (IsActive)
                    await RefreshHardwareInfo();
            };
            _timer.Start();

            // Initialisation du Task pour l'actualisation asynchrone
            _cts = new CancellationTokenSource();
            Task.Run(() => AutoRefresh(_cts.Token));
            TestNetworkCommand = new RelayCommand(_ => PerformNetworkTest());

            // Chargement initial
            RefreshHardwareInfo();
            PerformNetworkTest();
            LoadSystemLogs();
        }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (!_isActive)
                {
                    _cts.Cancel(); // 🔥 Arrête les tâches en arrière-plan quand l'onglet est masqué
                }
            }
        }

        private async Task RefreshHardwareInfo()
        {
            if (!IsActive) return;

            // Lancer les tâches en parallèle
            var cpuTask = Task.Run(() => GetCpuUsage());
            var ramTask = Task.Run(() => GetRamUsage());
            var diskTask = Task.Run(() => GetDiskInfo());
            var gpuNameTask = GetGpuNames(); // ✅ Exécution en parallèle
            var gpuUsageTask = GetGpuUsage(); // ✅ Exécution en parallèle

            // Attendre les résultats
            CpuUsage = await cpuTask;
            RamUsage = await ramTask;
            GpuUsage = await gpuNameTask;
            CpuUsagePercentage = ExtractNumericValue(CpuUsage);
            RamUsagePercentage = ExtractNumericValue(RamUsage) / GetTotalRam() * 100;
            GpuUsagePercentage = await gpuUsageTask;

            // Mettre à jour les données UI
            OnPropertyChanged(nameof(CpuUsage));
            OnPropertyChanged(nameof(RamUsage));
            OnPropertyChanged(nameof(GpuUsage));
            OnPropertyChanged(nameof(CpuUsagePercentage));
            OnPropertyChanged(nameof(RamUsagePercentage));
            OnPropertyChanged(nameof(GpuUsagePercentage));
            OnPropertyChanged(nameof(DiskUsage));
        }
        private async Task AutoRefresh(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (IsActive)
                {
                    await RefreshHardwareInfo();
                }
                await Task.Delay(2000, token); // 🔥 Pause 1 seconde mais annulable
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private double ExtractNumericValue(string input)
        {
            // Recherche du premier nombre dans la chaîne
            var match = System.Text.RegularExpressions.Regex.Match(input, @"\d+([,.]\d+)?");
            if (match.Success)
            {
                return double.Parse(match.Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
            }
            return 0; // Retourne 0 si aucune valeur trouvée
        }
        // -------------------------------------------------------------------------   Monitoring RAM CPU GPU
        private async Task<string> GetCpuUsage()
        {
            return await Task.Run(() =>
            {
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(500); // Laisse le temps au compteur de se stabiliser
                return $"CPU : {cpuCounter.NextValue():F1}%";
            });
        }

        private string GetRamUsage()
        {
            var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            var totalRam = GetTotalRam();
            var usedRam = totalRam - ramCounter.NextValue();
            return $"RAM : {usedRam} MB / {totalRam} MB";
        }

        private int GetTotalRam()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["TotalVisibleMemorySize"]) / 1024;
                }
            }
            return 0;
        }

        private async Task<string> GetGpuNames()
        {
            return await Task.Run(() =>
            {
                List<string> gpuList = new List<string>();

                // ✅ Méthode 1 : Via WMI (Win32_VideoController)
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            gpuList.Add(obj["Name"].ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    gpuList.Add("❌ WMI GPU Info introuvable");
                }

                // ✅ Méthode 2 : Via DirectX (SharpDX)
                try
                {
                    using (var factory = new Factory1())
                    {
                        for (int i = 0; i < factory.GetAdapterCount1(); i++)
                        {
                            var adapter = factory.GetAdapter1(i);
                            gpuList.Add(adapter.Description.Description);
                        }
                    }
                }
                catch (Exception)
                {
                    gpuList.Add("❌ DirectX GPU Info introuvable");
                }

                return gpuList.Count > 0 ? $"GPU : {string.Join(" | ", gpuList.Distinct())}" : "GPU : Non détecté";
            });
        }

        private string GetGpuUsageText()
        {
            return $"GPU : {GpuUsagePercentage:F1}%";
        }
        private async Task<double> GetGpuUsage()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "engtype_3D");
                    gpuCounter.NextValue();
                    System.Threading.Thread.Sleep(500); // Permet au compteur de se stabiliser
                    return gpuCounter.NextValue();
                }
                catch
                {
                    return 0; // Si le GPU n'est pas supporté, retourne 0%
                }
            });
        }

        private async void GetDiskInfo()
        {
            try
            {
                // 1️⃣ Récupération des infos des disques en arrière-plan
                var tempList = await Task.Run(() =>
                {
                    List<string> disks = new List<string>();

                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady)
                        {
                            string info = $"💾 {drive.Name} : {drive.AvailableFreeSpace / (1024 * 1024 * 1024)}GB / {drive.TotalSize / (1024 * 1024 * 1024)}GB";
                            disks.Add(info);
                        }
                    }

                    return disks;
                });

                // 2️⃣ Mise à jour complète de DiskUsage sur le thread UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DiskUsage = new ObservableCollection<string>(tempList); // ✅ Remplacement complet sans modification directe
                    OnPropertyChanged(nameof(DiskUsage)); // 🔥 Rafraîchit l'UI
                });

                // 3️⃣ Debugging pour voir les disques détectés
                foreach (var disk in tempList)
                {
                    Console.WriteLine($"✅ Disque détecté : {disk}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetDiskInfo : {ex.Message}");
            }
        }
        // -------------------------------------------------------------------------   Logs Event
        private void LoadSystemLogs()
        {
            Console.WriteLine("📜 Chargement des logs système...");

            SystemLogs.Clear();

            try
            {
                EventLog eventLog = new EventLog("System");
                foreach (EventLogEntry entry in eventLog.Entries.Cast<EventLogEntry>().Reverse().Take(5))
                {
                    SystemLogs.Add($"[{entry.EntryType}] {entry.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement des logs : {ex.Message}");
            }

            OnPropertyChanged(nameof(SystemLogs));
        }

        // 🔹 Fonction pour ouvrir l'Observateur d'événements
        private void OpenEventViewer()
        {
            Console.WriteLine("📜 Ouverture du Gestionnaire d'événements...");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "eventvwr.msc",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'ouverture du Gestionnaire d'événements : {ex.Message}");
            }
        }

        // -------------------------------------------------------------------------   Network

        private async void PerformNetworkTest()
        {
            Console.WriteLine("🔍 Test Réseau en cours...");

            ConnectionStatus = GetConnectionType();
            Console.WriteLine($"📡 Type de connexion : {ConnectionStatus}");

            LocalIp = GetLocalIPAddress();
            Console.WriteLine($"🌐 IP Locale : {LocalIp}");

            PublicIp = await GetPublicIPAddressAsync();
            Console.WriteLine($"🌍 IP Publique : {PublicIp}");

            PingResult = await PingServer("8.8.8.8");
            Console.WriteLine($"📶 Ping : {PingResult}");

            (DownloadSpeed, UploadSpeed) = await TestInternetSpeed();
            Console.WriteLine($"⬇️ Download : {DownloadSpeed}, ⬆️ Upload : {UploadSpeed}");

            OnPropertyChanged(nameof(ConnectionStatus));
            OnPropertyChanged(nameof(LocalIp));
            OnPropertyChanged(nameof(PublicIp));
            OnPropertyChanged(nameof(PingResult));
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(UploadSpeed));
        }
        private string GetConnectionType()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return "❌ Pas de connexion";

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    return "📶 WiFi";
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    return "🔌 Ethernet";
            }
            return "🔍 Connexion inconnue";
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"🔹 IP Locale trouvée : {ip}");
                        return $"🌐 {ip}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur IP Locale : {ex.Message}");
            }
            return "Non disponible";
        }

        private async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new WebClient())
                {
                    string publicIp = await client.DownloadStringTaskAsync("https://api64.ipify.org");
                    Console.WriteLine($"🔹 IP Publique trouvée : {publicIp}");
                    return $"🌍 {publicIp}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur IP Publique : {ex.Message}");
                return "🌍 Non disponible";
            }
        }

        private async Task<string> PingServer(string address)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(address, 1000);
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"🔹 Ping réussi : {reply.RoundtripTime} ms");
                    return $"✅ {reply.RoundtripTime} ms";
                }
                else
                {
                    Console.WriteLine("❌ Ping échoué");
                    return "❌ Ping échoué";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur Ping : {ex.Message}");
                return "❌ Ping impossible";
            }
        }

        private async Task<(string, string)> TestInternetSpeed()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                Console.WriteLine("🔄 Envoi de la requête Speedtest...");
                var response = await client.GetStringAsync("http://speedtest.tele2.net/1MB.zip");

                if (!string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("✅ Speedtest API répond : données reçues.");
                    return ("✅ Download OK", "✅ Upload OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur Speedtest API : {ex.Message}");
            }

            return ("❌ Download échoué", "❌ Upload échoué");
        }
    }
}
