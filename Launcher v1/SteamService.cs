using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Launcher_v1
{
    public class SteamService : IGamePlatform
    {

        public string PlatformName => "Steam";
        public List<Game> ScanInstalledGames()
        {
            List<Game> games = new();
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            string steamPath = key?.GetValue("SteamPath") as string;
            
            if (string.IsNullOrWhiteSpace(steamPath) || !Directory.Exists(steamPath)){
                return games;
            }

            HashSet<string> libraries = new(System.StringComparer.OrdinalIgnoreCase);
            
            libraries.Add(steamPath);

            string libraryFile = Path.Combine(
                steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFile))
            {
                string content = File.ReadAllText(libraryFile);

                // Leer las rutas de las bibliotecas adicionales
                MatchCollection matches = Regex.Matches(
                    content,
                    "\"path\"\\s+\"([^\"]+)\""
                );

                foreach (Match match in matches)
                {
                    string path = match.Groups[1].Value
                        .Replace(@"\\", @"\");

                    if (Directory.Exists(path))
                    {
                        libraries.Add(path);
                    }
                }
            }

            // 3. Recorrer los manifiestos de cada biblioteca
            foreach (string library in libraries)
            {
                string steamApps = Path.Combine(
                    library,
                    "steamapps"
                );

                if (!Directory.Exists(steamApps))
                    continue;

                foreach (string manifest in
                    Directory.EnumerateFiles(
                        steamApps,
                        "appmanifest_*.acf"
                    ))
                {
                    string data = File.ReadAllText(manifest);
          
                    string appIdText = GetValue(data, "appid");
                    string name = GetValue(data, "name");
                    string installDir = GetValue(data, "installdir");

                    if (!int.TryParse(appIdText, out int appId))
                        continue;

                    string gamePath = Path.Combine(
                        steamApps,
                        "common",
                        installDir
                    );

                    if (!Directory.Exists(gamePath))
                        continue;

                    string executablePath = FindExecutable(installDir);


                    games.Add(new Game
                    {
                        Id = appId.ToString(),
                        Name = name,
                        Platform = "Steam",
                        InstallDir = gamePath,
                        LaunchType = GameLaunchType.Steam,
                        ProcessName = executablePath != null ? Path.GetFileNameWithoutExtension(executablePath) : string.Empty,


                    });
                }
            }

            return games;
            //return new List<Game>();
        
        }

        private string GetValue(string content, string key)
        {
            Match match = Regex.Match(
                content,
                "\"" + Regex.Escape(key) +
                "\"\\s+\"([^\"]*)\""
            );

            return match.Success ? match.Groups[1].Value : "";
        }

        private string FindExecutable(string installDir)
        {
            if (!Directory.Exists(installDir))
                return null;

            string[] excluded =
            {
        "unins",
        "setup",
        "config",
        "crash",
        "launcher"
    };

            var executables = Directory.GetFiles(
                installDir,
                "*.exe",
                SearchOption.AllDirectories
            );

            foreach (string exe in executables)
            {
                string fileName = Path.GetFileNameWithoutExtension(exe);

                if (excluded.Any(x =>
                    fileName.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                return exe;
            }

            return null;
        }
        public void LaunchGame(Game game)
        {
            if(game == null) {
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = $"steam://rungameid/{game.Id}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}
