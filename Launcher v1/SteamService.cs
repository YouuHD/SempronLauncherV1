using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace Launcher_v1
{
    public class SteamService
    {
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

                    games.Add(new Game
                    {
                        AppId = appId,
                        Name = name,
                        InstallDir = gamePath
                    });
                }
            }

            return games;
        
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
        public void LaunchGame(int appId)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = $"steam://rungameid/{appId}",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}
