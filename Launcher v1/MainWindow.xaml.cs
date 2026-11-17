using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Launcher_v1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Game> games = new List<Game>();
        public MainWindow()
        {
            InitializeComponent();
            //SteamGame selectedGame = Lista.SelectedItem as SteamGame;
            

                
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(Lista.SelectedItem is Game selectedGame)
            {
                btnLaunch.Content = "Iniciando...";
                btnLaunch.IsEnabled = false;
                vis.Content = $"Juego: {selectedGame.Name} - Id: {selectedGame.AppId}";
                var steam = new SteamService();
                steam.LaunchGame(selectedGame.AppId);
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un juego de la lista.");
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var steam = new SteamService();
            games = steam.ScanInstalledGames();
            Lista.ItemsSource = games;
            MessageBox.Show("Escaneo completado. Juegos encontrados: " + games.Count);

        }
    }
}