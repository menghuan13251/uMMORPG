﻿using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerChat))]
[DisallowMultipleComponent]
public class PlayerGuild : MonoBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerChat chat;

    [Header("Text Meshes")]
    public TextMeshPro overlay;
    public string overlayPrefix = "[";
    public string overlaySuffix = "]";

    // .guild is a copy for easier reading/syncing. Use GuildSystem to manage
    // guilds!
    [Header("Guild")]
    [ HideInInspector] public string inviteFrom = "";
   [ HideInInspector] public Guild guild; // TODO SyncToOwner later but need to sync guild name to everyone!
    public float inviteWaitSeconds = 3;

    void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // notify guild members that we are online. this also updates the client's
        // own guild info via targetrpc automatically
        // -> OnStartServer is too early because it's not spawned there yet
        if (isServer)
            SetOnline(true);
    }

    void Update()
    {
        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly)
        {
            if (overlay != null)
                overlay.text = !string.IsNullOrWhiteSpace(guild.name) ? overlayPrefix + guild.name + overlaySuffix : "";
        }
    }

    void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // notify guild members that we are offline
        if (isServer)
            SetOnline(false);
    }

    // guild ///////////////////////////////////////////////////////////////////
    public bool InGuild() => !string.IsNullOrWhiteSpace(guild.name);

    // ServerCALLBACk to ignore the warning if it's called while server isn't
    // active, which happens if OnDestroy->SetOnline(false) is called while
    // shutting down.
    
    public void SetOnline(bool online)
    {
        // validate
        if (InGuild())
            GuildSystem.SetGuildOnline(guild.name, name, online);
    }

   
    public void CmdInviteTarget()
    {
        // validate
        if (player.target != null &&
            player.target is Player targetPlayer &&
            InGuild() && !targetPlayer.guild.InGuild() &&
            guild.CanInvite(name, targetPlayer.name) &&
            NetworkTime.time >= player.nextRiskyActionTime &&
            Utils.ClosestDistance(player, targetPlayer) <= player.interactionRange)
        {
            // send an invite
            targetPlayer.guild.inviteFrom = name;
            Debug.Log(name + " invited " + player.target.name + " to guild");
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
    }

   
    public void CmdInviteAccept()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InGuild() && inviteFrom != "" &&
            Player.onlinePlayers.TryGetValue(inviteFrom, out Player sender) &&
            sender.guild.InGuild())
        {
            // try to add. GuildSystem does all the checks.
            GuildSystem.AddToGuild(sender.guild.guild.name, sender.name, name, player.level.current);
        }

        // reset guild invite in any case
        inviteFrom = "";
    }

   
    public void CmdInviteDecline()
    {
        inviteFrom = "";
    }

   
    public void CmdKick(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.KickFromGuild(guild.name, name, memberName);
    }

   
    public void CmdPromote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.PromoteMember(guild.name, name, memberName);
    }

   
    public void CmdDemote(string memberName)
    {
        // validate
        if (InGuild())
            GuildSystem.DemoteMember(guild.name, name, memberName);
    }

   
    public void CmdSetNotice(string notice)
    {
        // validate
        // (only allow changes every few seconds to avoid bandwidth issues)
        if (InGuild() && NetworkTime.time >= player.nextRiskyActionTime)
        {
            // try to set notice
            GuildSystem.SetGuildNotice(guild.name, name, notice);
        }

        // reset risky time no matter what. even if set notice failed, we don't
        // want people to spam attempts all the time.
        player.nextRiskyActionTime = NetworkTime.time + GuildSystem.NoticeWaitSeconds;
    }

    // helper function to check if we are near a guild manager npc
    public bool IsGuildManagerNear()
    {
        return player.target != null &&
               player.target is Npc npc &&
               npc.guildManagement != null && // only if Npc offers guild management
               Utils.ClosestDistance(player, player.target) <= player.interactionRange;
    }

    
    public void CmdTerminate()
    {
        // validate
        if (InGuild() && IsGuildManagerNear())
            GuildSystem.TerminateGuild(guild.name, name);
    }

   
    public void CmdCreate(string guildName)
    {
        // validate
        if (player.health.current > 0 && player.gold >= GuildSystem.CreationPrice &&
            !InGuild() && IsGuildManagerNear())
        {
            // try to create the guild. pay for it if it worked.
            if (GuildSystem.CreateGuild(name, player.level.current, guildName))
                player.gold -= GuildSystem.CreationPrice;
            else
                chat.TargetMsgInfo("Guild name invalid!");
        }
    }

   
    public void CmdLeave()
    {
        // validate
        if (InGuild())
            GuildSystem.LeaveGuild(guild.name, name);
    }
}
