using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Pet;

public class Pet : BasePlugin
{
    public override string ModuleName => "pet";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Wangsir";
    public override string ModuleDescription => "Create a model that follows player";
    private readonly List<CCSPlayerController> Players = new();
    private readonly Dictionary<CCSPlayerController, bool> playerpet = new();
    private readonly Dictionary<CCSPlayerController, CDynamicProp> entitys = new();
    private readonly Dictionary<CCSPlayerController, QAngle> entitys_angle = new();

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var player in Players.Where(player => player is { IsValid: true, IsBot: false, PawnIsAlive: true }))
            {
                telepet(player);
            }
        });
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValid || player.IsBot) return HookResult.Continue;
        Players.Add(player);
        playerpet[player] = false;
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValid || player.IsBot) return HookResult.Continue;
        Players.Remove(player);
        playerpet.Remove(player);
        entitys.TryGetValue(player, out var pet);
        pet?.Remove();
        entitys.Remove(player);
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValid || player.IsBot) return HookResult.Continue;
        if (Players.Contains(player) == false)Players.Add(player);
        playerpet[player] = false;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValid || player.IsBot) return HookResult.Continue;
        playerpet[player] = false;
        return HookResult.Continue;
    }
    public void telepet(CCSPlayerController? player)
    {
        if (!player.IsValid || !player.PawnIsAlive) return;
        if (playerpet[player] == false)
        {
            if(entitys.TryGetValue(player, out var pet))
            {
                pet?.Remove();
                entitys.Remove(player);
            }
            return;
        }
        if((entitys[player].AbsOrigin-player.PlayerPawn.Value!.AbsOrigin).Length()<=200)return;
        entitys[player].Teleport(
            new Vector(
                player.PlayerPawn.Value!.AbsOrigin!.X-(float)(Math.Cos(player.PlayerPawn.Value!.EyeAngles!.Y*(Math.PI / 180.0))*10),
                player.PlayerPawn.Value!.AbsOrigin!.Y-(float)(Math.Sin(player.PlayerPawn.Value!.EyeAngles!.Y*(Math.PI / 180.0))*10),
                player.PlayerPawn.Value!.AbsOrigin!.Z
            ),
            new QAngle(
                0,
                entitys_angle[player].Y>0?entitys_angle[player].Y-180:180+entitys_angle[player].Y,
                0
            ),
            player.PlayerPawn.Value!.AbsVelocity
        );
    }

    [ConsoleCommand("css_pet", "show pet")]
    //[RequiresPermissions("@css/root")]
    public void showpet(CCSPlayerController? player, CommandInfo command)
    {
        if (!player.IsValid || !player.PawnIsAlive) return;
        playerpet[player] = !playerpet[player];
        if(playerpet[player]==false)return;
        var entity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (entity == null || !entity.IsValid)return;
        entity.Teleport(
            new Vector(
                player.PlayerPawn.Value!.AbsOrigin!.X+(float)(Math.Cos(player.PlayerPawn.Value!.EyeAngles!.Y*(Math.PI / 180.0))*10),
                player.PlayerPawn.Value!.AbsOrigin!.Y+(float)(Math.Sin(player.PlayerPawn.Value!.EyeAngles!.Y*(Math.PI / 180.0))*10),
                player.PlayerPawn.Value!.AbsOrigin!.Z
            ),
            new QAngle(
                0,
                player.PlayerPawn.Value!.EyeAngles!.Y>0?player.PlayerPawn.Value!.EyeAngles!.Y-180:180+player.PlayerPawn.Value!.EyeAngles!.Y,
                0
            ),
            player.PlayerPawn.Value!.AbsVelocity
        );
        entity.SetModel("D:\\cs2_servers/game/csgo/models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl_c");
        entity.DispatchSpawn();
        entitys[player]=entity;
        entitys_angle[player]=new QAngle(
                0,
                player.PlayerPawn.Value!.EyeAngles.Y,
                0
            );
    }
}