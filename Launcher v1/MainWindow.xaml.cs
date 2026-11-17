
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher_v1
{
    public partial class MainWindow : Window
    {
        private List<Game> games = new List<Game>();

        GogService gog = new GogService();
        SteamService steam = new SteamService();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Lista.SelectedItem is not Game selectedGame)
            {
                MessageBox.Show("Por favor, selecciona un juego de la lista.");
                return;
            }

            btnLaunch.Content = "Iniciando...";
            btnLaunch.IsEnabled = false;

            try
            {
                vis.Content =
                    $"Juego: {selectedGame.Name} - " +
                    $"Id: {selectedGame.Id} - " +
                    $"Plataforma: {selectedGame.Platform} - " +
                    $"Directorio: {selectedGame.InstallDir} - "+
                    $"Ejecutable: {selectedGame.ExecutablePath} - "+
                    $"Tipo de lanzamiento: {selectedGame.LaunchType} - " +
                    $"Nombre del proceso: {selectedGame.ProcessName}"
                    ;

                if (selectedGame.Platform == "GOG")
                {
                    gog.LaunchGame(selectedGame);
                }
                else if (selectedGame.Platform == "Steam")
                {
                    steam.LaunchGame(selectedGame);
                }
                else
                {
                    MessageBox.Show("Plataforma no soportada.");
                    return;
                }

                // Esperar a que el juego aparezca y después termine
                await WaitForGameToCloseAsync(selectedGame);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al intentar lanzar el juego: {ex.Message}"
                );
            }
            finally
            {
                btnLaunch.Content = "Lanzar Juego";
                btnLaunch.IsEnabled = true;
            }
        }

        private async Task WaitForGameToCloseAsync(Game game)
        {
            if (string.IsNullOrWhiteSpace(game.ProcessName))
            {
                MessageBox.Show("No se pudo identificar el proceso del juego.");
                return;
            }

            int attempts = 0;

            // Esperar hasta que el juego aparezca
            while (Process.GetProcessesByName(game.ProcessName).Length == 0)
            {
                await Task.Delay(500);
                attempts++;

                if (attempts >= 120)
                {
                    MessageBox.Show("No se detectó el proceso del juego.");
                    return;
                }
            }

            // Esperar hasta que el juego se cierre
            while (Process.GetProcessesByName(game.ProcessName).Length > 0)
            {
                await Task.Delay(1000);
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            games.Clear();

            games.AddRange(steam.ScanInstalledGames());
            games.AddRange(gog.ScanInstalledGames());

            Lista.ItemsSource = null;
            Lista.ItemsSource = games;

            MessageBox.Show(
                "Escaneo completado. Juegos encontrados: " + games.Count
            );
        }
    }
}
