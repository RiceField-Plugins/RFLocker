using System;
using System.IO;
using Rocket.Core.Logging;

namespace RFVault.DatabaseManagers
{
    internal class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "vault.db";
        private static readonly string LiteDB_FilePath = Path.Combine(Plugin.Inst.Directory, LiteDB_FileName);
        internal static readonly string LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";

        internal static string MySql_ConnectionString = Plugin.Conf.MySqlConnectionString;

        internal readonly VaultManager VaultManager;

        internal DatabaseManager()
        {
            try
            {
                VaultManager = new VaultManager();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] DatabaseManager Initializing: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }
    }
}