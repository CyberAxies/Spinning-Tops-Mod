using System;
using System.Collections.Generic;
using System.Linq;
using SpinningTopsMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SpinningTopsMod
{
    public static partial class Utils
    {
        /// <summary>
        /// Detects nearby hostile NPCs from a given point
        /// </summary>
        /// <param name="origin">The position where we wish to check for nearby NPCs</param>
        /// <param name="maxDistanceToCheck">Maximum amount of pixels to check around the origin</param>
        /// <param name="ignoreTiles">Whether to ignore tiles when finding a target or not</param>
        /// <param name="bossPriority">Whether bosses should be prioritized in targetting or not</param>
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck = 1200, bool ignoreTiles = false, bool bossPriority = true)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;
            if (bossPriority)
            {
                bool bossFound = false;
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    // If we've found a valid boss target, ignore ALL targets which aren't bosses.
                    if (bossFound && !(Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye))
                        continue;

                    if (Main.npc[index].CanBeChasedBy(null, false))
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            if (Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye)
                                bossFound = true;

                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            else
            {
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    if (Main.npc[index].CanBeChasedBy(null, false))
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            return closestTarget;
        }

        /// <summary>
        /// Detects the hostile NPC that is closest angle-wise to the rotation vector
        /// </summary>
        /// <param name="origin">The position that will be used to find the rotation vector to NPCs</param>
        /// <param name="checkRotationVector">The rotation vector that the other rotation vectors to NPCs will be compared to</param>
        /// <param name="maxDistanceToCheck">Maximum amount of pixels to check around the origin</param>
        /// <param name="wantedHalfCone">When the angle between the rotation vector and the vector to the NPC is less than or equal to this, NPCs start getting ranked by distance. Set to 0 or less to ignore</param>
        /// <param name="ignoreTiles">Whether or not to ignore tiles when finding a target</param>
        /// <returns>The NPC that best fits the parameters. Null if no NPC is found</returns>
        public static NPC ClosestNPCToAngle(this Vector2 origin, Vector2 checkRotationVector, float maxDistanceToCheck, float wantedHalfCone = 0.125f, bool ignoreTiles = true)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;
            float angle = MathHelper.Pi;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(null, false))
                    continue;

                float checkDist = origin.Distance(npc.Center);
                if (checkDist >= distance) // Immediately disqualify anything beyond the distance that must be beaten
                    continue;

                float angleBetween = checkRotationVector.AngleBetween(npc.Center - origin);
                if (angleBetween > angle) // Narrow down to the closest npc to the angle
                    continue;

                if (!ignoreTiles && !Collision.CanHit(origin, 1, 1, npc.Center, 1, 1)) // Tile LoS check if wanted
                    continue;

                if (angle <= wantedHalfCone)
                {
                    angle = wantedHalfCone; 
                    distance = checkDist; // We are within the cone. Now npcs are further narrowed down by distance
                    closestTarget = npc;
                }
                else
                {
                    angle = angleBetween;
                    closestTarget = npc;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// Detects nearby hostile NPCs from a given point
        /// Returns a list of all detected NPCs
        /// </summary>
        /// <param name="origin">The position where we wish to check for nearby NPCs</param>
        /// <param name="maxDistanceToCheck">Maximum amount of pixels to check around the origin</param>
        /// <param name="ignoreTiles">Whether to ignore tiles when finding a target or not</param>
        public static List<NPC> AllNPCsWithinRangeOfPoint(this Vector2 origin, float maxDistanceToCheck = 1200, bool ignoreTiles = false)
        {
            List<NPC> allTargets = new List<NPC>();
            float distSquared = maxDistanceToCheck * maxDistanceToCheck;

            for (int index = 0; index < Main.npc.Length; index++)
            {
                if (Main.npc[index].CanBeChasedBy(null, false))
                {
                    float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);
                    float checkDistSquared = Vector2.DistanceSquared(origin, Main.npc[index].Center);

                    bool canHit = true;
                    if (extraDistance < maxDistanceToCheck && !ignoreTiles)
                        canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                    if (checkDistSquared < distSquared && canHit)
                    {
                        allTargets.Add(Main.npc[index]);
                    }
                }
            }

            return allTargets;
        }
        /// <summary>
        /// Loops over every NPC within 100 tiles and detects if their
        /// hitbox intersects with the given rectangle.
        /// Can optionally ignore tiles between the rectangle and the npcs
        /// </summary>
        /// <param name="center"> the center of the rectangle</param>
        /// <param name="size"> Vector2 that hold the width in the X and the height in Y.
        /// Width and height are the total side lengths of the rectangle</param>
        /// <param name="ignoreTiles"></param>
        /// <returns></returns> <summary>
        public static List<NPC> allNPCsInRectangle(Vector2 center, Vector2 size, bool ignoreTiles = false)
        {
            List<NPC> allTargets = new List<NPC>();
            float distSquared = 1600 * 1600; // 100 tiles is 1600 pixels, way more than the screen size, so it's enough for any rectangle
            
            Rectangle area = new Rectangle( 
                (int)(center.X - size.X/2), // Top left corner X
                (int)(center.Y - size.Y/2), // Top left corner Y
                (int)size.X, // Width
                (int)size.Y ); // Height

            for (int index = 0; index < Main.npc.Length; index++)
            {
                if (Main.npc[index].CanBeChasedBy(null, false) && // Can we hit this NPC?
                    Vector2.DistanceSquared(center, Main.npc[index].Center) < distSquared) // Is the NPC close?
                {
                    // Check if the NPCs hitbox intersects with the area rectangle
                    Rectangle npcHitbox = Main.npc[index].Hitbox;
                    NPC npc = Main.npc[index];
                    if (!area.Intersects(npcHitbox))
                        continue;

                    bool canHit = true;
                    if (!ignoreTiles)
                        // Check line of sight from the center of the area to the center of the NPC
                        canHit = Collision.CanHit(center - new Vector2(16, 16), 32, 32, npc.Center, npcHitbox.Width, npcHitbox.Height);

                    if (canHit)
                    {
                        allTargets.Add(Main.npc[index]);
                    }
                }
            }

            return allTargets;
        }
        /// <summary>
        /// Overload that handles rectangle input instead of center and size,
        /// Be careful! XNA Rectangles position is defined by the top-left corner, not the center
        /// </summary>
        /// <param name="area"></param>
        /// <param name="ignoreTiles"></param>
        /// <returns></returns>
        public static List<NPC> allNPCsInRectangle(Rectangle area, bool ignoreTiles = false)
        {
            List<NPC> allTargets = new List<NPC>();
            float distSquared = 1600 * 1600;

            for (int index = 0; index < Main.npc.Length; index++)
            {
                if (Main.npc[index].CanBeChasedBy(null, false) &&
                    Vector2.DistanceSquared(area.Center.ToVector2(), Main.npc[index].Center) < distSquared)
                {
                    Rectangle npcHitbox = Main.npc[index].Hitbox;
                    NPC npc = Main.npc[index];
                    if (!area.Intersects(npcHitbox))
                        continue;

                    bool canHit = true;
                    if (!ignoreTiles)
                        canHit = Collision.CanHit(area.Center.ToVector2(), 32, 32, npc.Center, npcHitbox.Width, npcHitbox.Height);

                    if (canHit)
                    {
                        allTargets.Add(Main.npc[index]);
                    }
                }
            }

            return allTargets;
        }

    }
}