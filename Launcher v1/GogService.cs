
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Launcher_v1
{
    internal class GogService : IGamePlatform
    {
        public string PlatformName => "GOG";

        public List<Game> ScanInstalledGames()
        {
            List<Game> games = new();

            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (string registryPath in registryPaths)
            {
                using RegistryKey baseKey =
                    Registry.LocalMachine.OpenSubKey(registryPath);

                if (baseKey == null)
                    continue;

                foreach (string subKeyName in baseKey.GetSubKeyNames())
                {
                    using RegistryKey gameKey =
                        baseKey.OpenSubKey(subKeyName);

                    if (gameKey == null)
                        continue;

                    string name =
                        gameKey.GetValue("DisplayName") as string;

                    string installDir =
                        gameKey.GetValue("InstallLocation") as string;

                    string publisher =
                        gameKey.GetValue("Publisher") as string;

                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(installDir))
                        continue;

                    if (!Directory.Exists(installDir))
                        continue;

                    if (publisher == null ||
                        !publisher.Contains(
                            "GOG",
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!TryReadGogInfo(
                        installDir,
                        out string gameId,
                        out string gameName,
                        out string executablePath))
                    {
                        continue;
                    }

                    games.Add(new Game
                    {
                        Id = gameId,
                        Name = gameName,
                        Platform = "GOG",
                        InstallDir = installDir,
                        ExecutablePath = executablePath,
                        LaunchType = GameLaunchType.Gog
                    });
                }
            }

            return games;
        }

        private bool TryReadGogInfo(
            string installDir,
            out string gameId,
            out string gameName,
            out string executablePath)
        {
            gameId = "";
            gameName = "";
            executablePath = "";

            string[] infoFiles = Directory.GetFiles(
                installDir,
                "goggame-*.info",
                SearchOption.TopDirectoryOnly
            );

            if (infoFiles.Length == 0)
                return false;

            using JsonDocument document =
                JsonDocument.Parse(File.ReadAllText(infoFiles[0]));

            JsonElement root = document.RootElement;

            gameId = root.GetProperty("gameId").GetString() ?? "";
            gameName = root.GetProperty("name").GetString() ?? "";

            if (!root.TryGetProperty(
                "playTasks",
                out JsonElement tasks))
                return false;

            JsonElement task = default;

            foreach (JsonElement currentTask in tasks.EnumerateArray())
            {
                if (currentTask.TryGetProperty(
                    "isPrimary",
                    out JsonElement primary) &&
                    primary.ValueKind == JsonValueKind.True)
                {
                    task = currentTask;
                    break;
                }
            }

            if (task.ValueKind == JsonValueKind.Undefined)
                return false;

            string relativePath =
                task.GetProperty("path").GetString() ?? "";

            executablePath = Path.Combine(
                installDir,
                relativePath
            );

            return !string.IsNullOrWhiteSpace(gameId) &&
                   File.Exists(executablePath);
        }

        public void LaunchGame(Game game)
        {
            if (game == null)
                return;

            string galaxyPath =
                @"D:\Programas\GOG Galaxy\GalaxyClient.exe";

            MessageBox.Show(
                $"Ruta buscada: \n{galaxyPath}\n\n" +
                $"ID del juego: {game.Id}\n" + 
                $"Existe?: {File.Exists(galaxyPath)}\n\n" 
                );

            if (!File.Exists(galaxyPath))
            {
                MessageBox.Show("No se pudo encontrar GOG Galaxy.");
                return;
            }

            if (string.IsNullOrWhiteSpace(game.Id))
            {
                MessageBox.Show("El juego no tiene un ID válido de GOG.");
                return;
            }

            string arguments =
                $"/command=runGame /gameId={game.Id} " +
                $"/path=\"{game.InstallDir}\"";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = galaxyPath,
                    Arguments = arguments,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al intentar lanzar el juego:\n{ex.Message}"
                );
            }
        }
    }
}
