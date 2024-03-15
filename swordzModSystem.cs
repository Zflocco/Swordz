using swordz.src;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace swordz
{
    public class swordzModSystem : ModSystem
    {






        public SwordzConfig Config;
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            api.Logger.Notification("Tis but a flesh wound!: " + api.Side);
            
            
            api.RegisterItemClass("ItemMithrilBloom", typeof(ItemMithrilBloom));
            base.Start(api);
            api.RegisterItemClass("Tunneler", typeof(ToolTunneler));
            api.RegisterItemClass("Pila", typeof(Pila));

            api.RegisterItemClass("ItemTrainingDummy", typeof(ItemTrainingDummy));
            api.RegisterEntity("EntityTrainingDummy", typeof(EntityTrainingDummy));

            api.RegisterItemClass("Sledgehammer", typeof(ToolSledgehammer));
            api.RegisterItemClass("Excavator", typeof(ToolExcavator));

            //  api.RegisterBlockClass("BlockFantasyMeteorite",typeof (BlockOre));


         
            try
            {
                Config = api.LoadModConfig<SwordzConfig>("SwordzConfig.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    Config = new SwordzConfig();
                }
            }
            catch
            {
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
                Config = new SwordzConfig();
            }
            finally
            {
                api.StoreModConfig(Config, "SwordzConfig.json");
            }
            api.World.Config.SetBool("stainlessenabled", Config.stainlessenabled);
            api.World.Config.SetBool("titaniumenabled", Config.titaniumenabled);
            api.World.Config.SetBool("fantasymetalsenabled", Config.fantasymetalsenabled);
            api.World.Config.SetBool("bombsdisabled", Config.bombsdisabled);
            api.World.Config.SetBool("sterilizedbandageenabled", Config.sterilizedbandageenabled);
            api.World.Config.SetBool("metalblocksdisabled", Config.metalblocksdisabled);





        }
        public class SwordzConfig
        {
            public bool stainlessenabled = true;
            public bool titaniumenabled = true;
            public bool fantasymetalsenabled = true;
            public bool bombsdisabled = true;
            public bool sterilizedbandageenabled = true;
            public bool metalblocksdisabled = true;




        }














        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Logger.Notification("Hello from template mod server side: " + Lang.Get("swordz:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Logger.Notification("Hello from template mod client side: " + Lang.Get("swordz:hello"));
        }

    }
}
