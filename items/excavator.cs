using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace swordz.src
{
    public class ToolExcavator : Item
    {

        public void destroyBlocks(IWorldAccessor world, BlockPos min, BlockPos max, IPlayer player, BlockSelection blockSel)
        {
            BlockPos tempPos = new BlockPos();


            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        Block block = world.BlockAccessor.GetBlock(x,y,z);
                        
                        // checks what block your trying to destroy
                        if (block.BlockMaterial == EnumBlockMaterial.Soil | block.BlockMaterial == EnumBlockMaterial.Sand | block.BlockMaterial == EnumBlockMaterial.Gravel | block.BlockMaterial == EnumBlockMaterial.Snow)
                        {
                            tempPos.Set(x, y, z);
                            if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
                                world.BlockAccessor.SetBlock(0, tempPos);
                            else
                                world.BlockAccessor.BreakBlock(tempPos, player);
                        }
                    }
                }
            }
        }
        public void destroyUpDownBlocks(IWorldAccessor world, BlockSelection blockSel, IPlayer player, Entity entity)
        {
            EntityPos pos = entity.World.Side == EnumAppSide.Server ? entity.ServerPos : entity.Pos;
            switch (pos.Yaw)
            {
                case < 0.785398f and > 0 or > 5.67232f and < 6.28319f: // east
                    destroyBlocks(world, blockSel.Position.AddCopy(0, 0, -1), blockSel.Position.AddCopy(1, 0, 1), player, blockSel);
                    break;
                case < 5.67232f and > 3.92699f: // north
                    destroyBlocks(world, blockSel.Position.AddCopy(-1, 0, 0), blockSel.Position.AddCopy(1, 0, 1), player, blockSel);
                    break;
                case < 3.92699f and > 2.35619f: // west
                    destroyBlocks(world, blockSel.Position.AddCopy(-1, 0, -1), blockSel.Position.AddCopy(0, 0, 1), player, blockSel);
                    break;
                case < 2.35619f and > 0.785398f: // south
                    destroyBlocks(world, blockSel.Position.AddCopy(-1, 0, -1), blockSel.Position.AddCopy(1, 0, 0), player, blockSel);
                    break;
            }
        }
        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            if (base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel))
            {
                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
                    EntityPlayer ePlayer = (EntityPlayer)byEntity;
                    Entity entity = (EntityPlayer)byEntity;
                    // checks what side of the block your looking at and calls the destroyBlocks method with the correct offset
                    switch (blockSel.Face.Axis)
                    {
                        case EnumAxis.X: // looking at east or west side of block
                            destroyBlocks(world, blockSel.Position.AddCopy(0, -1, -1), blockSel.Position.AddCopy(0, 0, 1), player, blockSel);
                            break;

                        case EnumAxis.Y: //looking at top or bottom of block
                                 destroyUpDownBlocks(world, blockSel, player, entity);
                                    break;
                        case EnumAxis.Z: // looking at north or south side of block
                            destroyBlocks(world, blockSel.Position.AddCopy(-1, -1, 0), blockSel.Position.AddCopy(1, 0, 0), player, blockSel);
                            break;
                    }
                }
                return true;
            }
            return false;
        }

    }
}