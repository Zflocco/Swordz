﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class ItemInsertNameHere: Item, IContainedMeshSource
    {
        float offY;
        float curOffY = 0;
        ICoreClientAPI capi;

        Dictionary<int, MultiTextureMeshRef> meshrefs => ObjectCacheUtil.GetOrCreate(api, "shieldmeshrefs", () => new Dictionary<int, MultiTextureMeshRef>());

        public string Construction => Variant["construction"];

        Dictionary<string, Dictionary<string, int>> durabilityGains;


        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            curOffY = offY = FpHandTransform.Translation.Y;
            capi = api as ICoreClientAPI;

            durabilityGains = Attributes["durabilityGains"].AsObject<Dictionary<string, Dictionary<string, int>>>();

            AddAllTypesToCreativeInventory();
        }

        public override int GetMaxDurability(ItemStack itemstack)
        {
            int gain = 0;

            foreach (var val in durabilityGains)
            {
                string mat = itemstack.Attributes.GetString(val.Key);
                if (mat != null)
                {
                    val.Value.TryGetValue(mat, out var matgain);
                    gain += matgain;
                }
            }

            return base.GetMaxDurability(itemstack) + gain;
        }

        public void AddAllTypesToCreativeInventory()
        {
            List<JsonItemStack> stacks = new List<JsonItemStack>();

            var vg = Attributes["variantGroups"].AsObject<Dictionary<string, string[]>>();

            foreach (var metal in vg["metal"])
            {
                switch (Construction)
                {
                    case "woodmetal":
                        foreach (var wood in vg["wood"])
                        {
                            stacks.Add(genJstack(string.Format("{{ wood: \"{0}\", metal: \"{1}\", deco: \"none\" }}", wood, metal)));
                        }
                        break;

                    case "woodmetalleather":
                        foreach (var color in vg["color"])
                        {
                            stacks.Add(genJstack(string.Format("{{ wood: \"{0}\", metal: \"{1}\", color: \"{2}\", deco: \"none\" }}", "generic", metal, color)));
                            if (color != "redblack") stacks.Add(genJstack(string.Format("{{ wood: \"{0}\", metal: \"{1}\", color: \"{2}\", deco: \"ornate\" }}", "generic", metal, color)));
                        }
                        break;
                }
            }

            this.CreativeInventoryStacks = new CreativeTabAndStackList[]
            {
                new CreativeTabAndStackList() { Stacks = stacks.ToArray(), Tabs = new string[]{ "general", "items", "tools" } }
            };
        }


        private JsonItemStack genJstack(string json)
        {
            var jstack = new JsonItemStack()
            {
                Code = this.Code,
                Type = EnumItemClass.Item,
                Attributes = new JsonObject(JToken.Parse(json))
            };

            jstack.Resolve(api.World, "shield type");

            return jstack;
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            if (target == EnumItemRenderTarget.HandFp)
            {
                bool sneak = capi.World.Player.Entity.Controls.Sneak;

                curOffY += ((sneak ? 0.4f : offY) - curOffY) * renderinfo.dt * 8;

                renderinfo.Transform.Translation.X = curOffY;
                renderinfo.Transform.Translation.Y = curOffY * 1.2f;
                renderinfo.Transform.Translation.Z = curOffY * 1.2f;
            }

            int meshrefid = itemstack.TempAttributes.GetInt("meshRefId");
            if (meshrefid == 0 || !meshrefs.TryGetValue(meshrefid, out renderinfo.ModelRef))
            {
                int id = meshrefs.Count+1;
                var modelref = capi.Render.UploadMultiTextureMesh(GenMesh(itemstack, capi.ItemTextureAtlas));
                renderinfo.ModelRef = meshrefs[id] = modelref;

                itemstack.TempAttributes.SetInt("meshRefId", id);
            }

            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }

        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas)
        {
            var cnts = new ContainedTextureSource(api as ICoreClientAPI, targetAtlas, new Dictionary<string, AssetLocation>(), string.Format("For render in shield {0}", Code));
            
            MeshData mesh;
            cnts.Textures.Clear();

            string wood = itemstack.Attributes.GetString("wood");
            string metal = itemstack.Attributes.GetString("metal");
            string color = itemstack.Attributes.GetString("color");
            string deco = itemstack.Attributes.GetString("deco");


            if (wood == null || wood == "") wood = "generic";
            //                                                              Crossguard+Pommel for zwei is "cgm"
            cnts.Textures["metal"] = cnts.Textures["handle"] = cnts.Textures["cgm"] = new AssetLocation("block/wood/planks/generic.png");

            var shape = capi.TesselatorManager.GetCachedShape(this.Shape.Base);

            foreach (var ctex in shape.Textures)
            {
                cnts.Textures[ctex.Key] = ctex.Value;
            }
            //      Four zwei variants : Blunt/Dull/Sharp/Honed (Zweihander-honed) {Attributes : {metal for blade} , {material for handle}, {decoration}
            //      Head/Blade : {Metal} | Crossguard/Pommel : {Material} | {Decoration} if applicable | {handle}
            //              Textures , Change the keywords to what is specifically within a shape/model (Rim to "Blade" or "Metal" etc)
            //      concatination?
            switch (Construction)
            {
                case "woodmetal":
                    if (wood != "generic")
                    {
                        cnts.Textures["handle"] = cnts.Textures["back"] = cnts.Textures["front"] = new AssetLocation("block/wood/debarked/" + wood + ".png");
                    }
                    cnts.Textures["metal"] = new AssetLocation("block/metal/sheet/" + metal + "1.png");
                    cnts.Textures["handle"] = new AssetLocation("block/metal/sheet/" + metal + "1.png");

                    if (deco == "ornate")
                    {
                        cnts.Textures["front"] = new AssetLocation("item/tool/shield/ornate/" + color + ".png");
                    }
                    break;
                case "woodmetalleather":
                    if (wood != "generic")
                    {
                        cnts.Textures["handle"] = cnts.Textures["back"] = cnts.Textures["front"] = new AssetLocation("block/wood/debarked/" + wood + ".png");
                    }
                    cnts.Textures["front"] = new AssetLocation("item/tool/shield/leather/" + color + ".png");
                    cnts.Textures["rim"] = new AssetLocation("block/metal/sheet/" + metal + "1.png");

                    if (deco == "ornate")
                    {
                        cnts.Textures["front"] = new AssetLocation("item/tool/shield/ornate/" + color + ".png");
                    }

                    break;
            }

            capi.Tesselator.TesselateItem(this, out mesh, cnts);

            return mesh;
        }

         
        public override string GetHeldItemName(ItemStack itemStack)
        {
            bool ornate = itemStack.Attributes.GetString("deco") == "ornate";
            string metal = itemStack.Attributes.GetString("metal");
            string wood = itemStack.Attributes.GetString("wood");
            string color = itemStack.Attributes.GetString("color");

            switch (Construction)
            {
                case "woodmetal":
                    if (wood == "generic")  //      do something here
                    {
                        return ornate ? Lang.Get("Ornate wooden shield") : Lang.Get("Wooden shield");
                    }
                    if (wood == "aged")
                    {
                        return ornate ? Lang.Get("Aged ornate shield") : Lang.Get("Aged wooden shield");
                    }
                    return ornate ? Lang.Get("Ornate {0} shield", Lang.Get("material-" + wood)) : Lang.Get("{0} shield", Lang.Get("material-" + wood));
                case "woodmetalleather":
                    return ornate ? Lang.Get("Ornate leather reinforced wooden shield") : Lang.Get("Leather reinforced wooden shield");
            }

            return base.GetHeldItemName(itemStack);
        }


        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            var attr = inSlot.Itemstack?.ItemAttributes?["shield"];
            if (attr == null || !attr.Exists) return;

            float acdmgabsorb = attr["damageAbsorption"]["active"].AsFloat(0);
            float acchance = attr["protectionChance"]["active"].AsFloat(0);

            float padmgabsorb = attr["damageAbsorption"]["passive"].AsFloat(0);
            float pachance = attr["protectionChance"]["passive"].AsFloat(0);

            dsc.AppendLine(Lang.Get("shield-stats", (int)(100*acchance), (int)(100*pachance), acdmgabsorb, padmgabsorb));

            switch (Construction)
            {
                case "woodmetal":
                    dsc.AppendLine(Lang.Get("shield-woodtype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("wood"))));
                    dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("metal"))));
                    break;

                case "woodmetalleather":
                    dsc.AppendLine(Lang.Get("shield-metaltype", Lang.Get("material-" + inSlot.Itemstack.Attributes.GetString("metal"))));
                    break;
            }
        }

        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
        {
            return GenMesh(itemstack, targetAtlas);
        }

        public string GetMeshCacheKey(ItemStack itemstack)
        {
            string wood = itemstack.Attributes.GetString("wood");
            string metal = itemstack.Attributes.GetString("metal");
            string color = itemstack.Attributes.GetString("color");
            string deco = itemstack.Attributes.GetString("deco");

            return Code.ToShortString() + "-" + wood + "-" + metal + "-" + color + "-" + deco;
        }
    }
}
