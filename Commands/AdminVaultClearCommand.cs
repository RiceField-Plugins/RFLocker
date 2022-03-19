﻿using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Plugins;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using SDG.Unturned;
using Steamworks;
using ThreadUtil = RFRocketLibrary.Utils.ThreadUtil;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Both)]
    [RFRocketLibrary.Plugins.CommandName("adminvaultclear")]
    [Permissions("adminvaultclear")]
    [Aliases("adminlockerclear")]
    [CommandInfo("Clear selected player vault.", "/adminvaultclear <playerId|playerName> <vaultName>")]
    public class AdminVaultClearCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var requestedVault = Vault.Parse(context.CommandRawArguments[1]);
            if (requestedVault == null)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            if (ulong.TryParse(context.CommandRawArguments[0], out var steamId))
            {
                var playerVault = VaultManager.Get(steamId, requestedVault.Name);
                if (playerVault == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(), steamId,
                            requestedVault.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                playerVault.VaultContent = new ItemsWrapper();
                var player = PlayerTool.getPlayer(new CSteamID(steamId));
                if (player != null)
                {
                    var cPlayer = player.GetComponent<PlayerComponent>();
                    if (cPlayer.PlayerVault != null && cPlayer.PlayerVault.VaultName == requestedVault.Name)
                    {
                        if (cPlayer.PlayerVaultItems != null)
                        {
                            cPlayer.PlayerVaultItems = null;
                            player.inventory.closeStorageAndNotifyClient();
                        }

                        cPlayer.PlayerVault.VaultContent = new ItemsWrapper();
                    }
                }

                await VaultManager.UpdateAsync(playerVault);
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.ADMIN_VAULT_CLEAR.ToString(), steamId,
                        requestedVault.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
            }
            else
            {
                var player = PlayerTool.getPlayer(context.CommandRawArguments[0]);
                if (player == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_NOT_FOUND.ToString(), context.CommandRawArguments[0]),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                var playerVault = VaultManager.Get(player.channel.owner.playerID.steamID.m_SteamID,
                    requestedVault.Name);
                if (playerVault == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(),
                            player.channel.owner.playerID.characterName, requestedVault.Name), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                playerVault.VaultContent = new ItemsWrapper();
                var cPlayer = player.GetComponent<PlayerComponent>();
                if (cPlayer.PlayerVault != null && cPlayer.PlayerVault.VaultName == requestedVault.Name)
                {
                    if (cPlayer.PlayerVaultItems != null)
                    {
                        cPlayer.PlayerVaultItems = null;
                        player.inventory.closeStorageAndNotifyClient();
                    }

                    cPlayer.PlayerVault.VaultContent = new ItemsWrapper();
                }

                await VaultManager.UpdateAsync(playerVault);
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.ADMIN_VAULT_CLEAR.ToString(),
                        cPlayer.Player.CharacterName, requestedVault.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
            }
        }
    }
}