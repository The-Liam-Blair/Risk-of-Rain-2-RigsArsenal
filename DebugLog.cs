using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace RigsArsenal
{
    // Displaying debug messages in the game console.
    internal static class DebugLog
    {
        private static ManualLogSource _logger;

        internal static void Init(ManualLogSource logger)
        {
            _logger = logger;
        }

        internal static void Log(object data) => _logger.LogInfo(data);
    }
}