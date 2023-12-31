using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;

namespace TabTimeleft;

public class TabTimeleft : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName { get; } = "TabTimeleft";
    public override string ModuleVersion { get; } = "1.0.0";
    public override string ModuleAuthor { get; } = "iks";

    public PluginConfig Config { get; set; }

    public List<CCSPlayerController> connectedPlayers =  new List<CCSPlayerController>();

    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>(ModuleName);
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnTick>(() =>
        {
            CCSGameRules gameRules = GetGameRules();
            
            int gameStart = (int)gameRules.GameStartTime;
            int timelimit = (int)ConVar.Find("mp_timelimit").GetPrimitiveValue<float>() * 60;
            int currentTime = (int)Server.CurrentTime;
            int timeleft = timelimit - (currentTime - gameStart);
            if (timelimit == 0)
            {
                return;
            }
            
            TimeSpan time = TimeSpan.FromSeconds(timeleft);
            DateTime dateTime = DateTime.Today.Add(time);
            string displayTime = dateTime.ToString("mm:ss");
            foreach (var player in connectedPlayers)
            {
                if (player.IsValid || !player.IsBot)
                {
                    var buttons = player.Buttons;
                
                    if (buttons != 0 && buttons.ToString().Contains("858993"))
                    {
                        if (!player.IsBot)
                        {
                            if (Config.PluginMode == 0)
                            {
                                player.PrintToCenter(timeleft <= 0 ? Localizer["main.LastRound"] :  Localizer["main.Timeleft"].ToString().Replace("{timeleft}", displayTime));
                            }
        
                            if (Config.PluginMode == 1)
                            {
                                player.PrintToCenterHtml($"<br><br> {(timeleft <= 0 ?  Localizer["main.LastRound"] : $" {Localizer["main.Timeleft"].ToString().Replace("{timeleft}", displayTime)}")}", 1);
                            }
                        }
                    }
                }
            }
        });
    }
    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_tabtimeleft_reload")]
    public void OnReloadCommand(CCSPlayerController? controller, CommandInfo info)
    {
        OnConfigParsed(Config);
        Console.Write("[Tab Timeleft] Config reloaded!");
        if (controller != null)
        {
            controller.PrintToChat($" {ChatColors.Gold}[Tab Timeleft] {ChatColors.Green}Config reloaded!");
        }
    }
    
    [GameEventHandler]
    public HookResult OnPlayerConnectedFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!player.IsBot)
        {
            connectedPlayers.Add(player);
            Console.Write($"{player.PlayerName} added in connectedPlayers");
            return HookResult.Continue;
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        connectedPlayers.Remove(player);
        Console.Write($"{player.PlayerName} removed in connectedPlayers");
        return HookResult.Continue;
    }
}
