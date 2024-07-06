using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace swordz.src
{
    public class ToolSledgehammer : Item
    {

        public void DestroyBlocks(IWorldAccessor world, BlockPos min, BlockPos max, IPlayer player)
        {
            BlockPos tempPos = new BlockPos();
            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        Block block = world.BlockAccessor.GetBlock(x, y, z);
                        tempPos.Set(x, y, z);
                        if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
                        {
                            world.BlockAccessor.SetBlock(0, tempPos);
                        }
                        else
                        {
                            if (block.BlockMaterial != EnumBlockMaterial.Mantle && block.BlockMaterial != EnumBlockMaterial.Soil && block.BlockMaterial != EnumBlockMaterial.Gravel && block.BlockMaterial != EnumBlockMaterial.Sand && block.BlockMaterial != EnumBlockMaterial.Snow)
                            {
                                world.BlockAccessor.BreakBlock(tempPos, player);
                            }
                        }
                    }
                }
            }
        }
        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 0.5f)
        {
            if (base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel))
            {
                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
                    switch (blockSel.Face.Axis)
                    {
                        case EnumAxis.X:
                            DestroyBlocks(world, blockSel.Position.AddCopy(0, -1, -1), blockSel.Position.AddCopy(0, 1, 1), player);
                            break;
                        case EnumAxis.Y:
                            DestroyBlocks(world, blockSel.Position.AddCopy(-1, 0, -1), blockSel.Position.AddCopy(1, 0, 1), player);
                            break;
                        case EnumAxis.Z:
                            DestroyBlocks(world, blockSel.Position.AddCopy(-1, -1, 0), blockSel.Position.AddCopy(1, 1, 0), player);
                            break;
                    }
                }
                return true;
            }
            return false;
        }

    }
}
