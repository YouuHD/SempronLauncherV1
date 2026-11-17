using System;
using System.Collections.Generic;
using System.Text;

namespace Launcher_v1
{
    public class Game
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;
        public string InstallDir { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;

        public string LaunchArguments { get; set; } = string.Empty;

        public GameLaunchType LaunchType { get; set; }  

        public string ProcessName { get; set; } = string.Empty;

    }
    public enum GameLaunchType
    {
        Executable,
        Steam,
        Epic,
        Gog,
        WindowsApp
    }
}
