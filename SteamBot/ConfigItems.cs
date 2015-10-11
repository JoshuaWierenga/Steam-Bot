using SteamKit2;
using System.Collections.Generic;
namespace SteamBot
{
    public class ConfigItems
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public List<ulong> Admins { get; set; }
    }
}