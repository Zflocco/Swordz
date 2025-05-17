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
    public class ToolTunneler : Item
    {

        public void destroyBlocks(IWorldAccessor world, BlockPos offset, IPlayer player)
        {
            BlockPos tempPos = new BlockPos();
            int x = offset.X;
            int y = offset.Y;
            int z = offset.Z;
            Block block = world.BlockAccessor.GetBlock(x, y, z);

            if (block.BlockMaterial != EnumBlockMaterial.Mantle)
            {
                tempPos.Set(x, y, z);
                if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
                    world.BlockAccessor.SetBlock(0, tempPos);
                else
                    world.BlockAccessor.BreakBlock(tempPos, player);
            }
        }
        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            if (base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel))
            {
                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
                    // checks what side of the block your looking at and calls the destroyBlocks method with the correct offset
                    switch (blockSel.Face.Axis)
                    {
                        case EnumAxis.X: // looking at east or west side of block
                            destroyBlocks(world, blockSel.Position.AddCopy(0, -1, 0), player);
                            break;
                        case EnumAxis.Y: //looking at top or bottom of block // TODO: removes 1 block in the direction the player is facing
                            destroyBlocks(world, blockSel.Position.AddCopy(0, 0, 0), player);
                            break;
                        case EnumAxis.Z: // looking at north or south side of block
                            destroyBlocks(world, blockSel.Position.AddCopy(0, -1, 0), player);
                            break;
                    }
                }
                return true;
            }
            return false;
        }
    }
}