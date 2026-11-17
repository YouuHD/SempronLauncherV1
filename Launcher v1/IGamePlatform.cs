using System;
using System.Collections.Generic;
using System.Text;

namespace Launcher_v1
{
    public interface IGamePlatform
    {
        string PlatformName { get; }
        List<Game> ScanInstalledGames();
        void LaunchGame(Game game);
    }
}
