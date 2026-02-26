
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
    public struct RicoshotTarget
    {
        public Vector2 pos;
        public int entityID;

        public RicoshotTarget()
        {
            pos = -Vector2.One; // -1, -1
            entityID = -1;
        }

    }
    public static partial class Utils
    {


        // Stolen from Calamity Mod's Github
        public static bool AnyProjectiles(int projectileID)
        {
            // Efficiently loop through all projectiles, using a specially designed continue continue that attempts to minimize the amount of OR
            // checks per iteration.
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != projectileID)
                    continue;

                return true;
            }

            return false;
        }


        public static int CountProjectiles(int projectileID) => Main.projectile.Count(proj => proj.type == projectileID && proj.active);

        public static bool FinalExtraUpdate(this Projectile proj) => proj.numUpdates == -1;

        public static Vector2 RandomVelocity(float directionMult, float speedLowerLimit, float speedCap, float speedMult = 0.1f)
        {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-directionMult, directionMult), Main.rand.NextFloat(-directionMult, directionMult));
            //Rerolling to avoid dividing by zero
            while (velocity.X == 0f && velocity.Y == 0f)
            {
                velocity = new Vector2(Main.rand.NextFloat(-directionMult, directionMult), Main.rand.NextFloat(-directionMult, directionMult));
            }
            velocity.Normalize();
            velocity *= Main.rand.NextFloat(speedLowerLimit, speedCap) * speedMult;
            return velocity;
        }
        
        #region Projectile Ai
        public static void ExpandHitboxBy(this Projectile projectile, int width, int height)
        {
            projectile.position = projectile.Center;
            projectile.width = width;
            projectile.height = height;
            projectile.position -= projectile.Size * 0.5f;
        }
        public static void ExpandHitboxBy(this Projectile projectile, int newSize) => projectile.ExpandHitboxBy(newSize, newSize);
        public static void ExpandHitboxBy(this Projectile projectile, Vector2 newSize) => projectile.ExpandHitboxBy((int)newSize.X, (int)newSize.Y);
        public static void ExpandHitboxBy(this Projectile projectile, float expandRatio) => projectile.ExpandHitboxBy((int)(projectile.width * expandRatio), (int)(projectile.height * expandRatio));
        public static void HomeInOnNPC(Projectile projectile, bool ignoreTiles, float distanceRequired, float homingVelocity, float inertia)
        {
            if (!projectile.friendly)
                return;


            Vector2 destination = projectile.Center;
            float maxDistance = distanceRequired;
            bool locatedTarget = false;
            int defaultExtraUpdates = projectile.extraUpdates;

            // Find the closest target.
            float npcDistCompare = 25000f; // Initializing the value to a large number so the first entry is basically guaranteed to replace it.
            int index = -1;
            foreach (NPC n in Main.ActiveNPCs)
            {
                float extraDistance = (n.width / 2) + (n.height / 2);
                if (!n.CanBeChasedBy(projectile, false) || !projectile.WithinRange(n.Center, maxDistance + extraDistance))
                    continue;

                float currentNPCDist = Vector2.Distance(n.Center, projectile.Center);
                if ((currentNPCDist < npcDistCompare) && (ignoreTiles || Collision.CanHit(projectile.Center, 1, 1, n.Center, 1, 1)))
                {
                    npcDistCompare = currentNPCDist;
                    index = n.whoAmI;
                }
            }
            // If the index was never changed, don't do anything. Otherwise, tell the projectile where to home.
            if (index != -1)
            {
                destination = Main.npc[index].Center;
                locatedTarget = true;
            }

            if (locatedTarget)
            {
                // Increase amount of extra updates to greatly increase homing velocity.
                projectile.extraUpdates++;

                // Home in on the target.
                Vector2 homeDirection = (destination - projectile.Center).SafeNormalize(Vector2.UnitY);
                projectile.velocity = (projectile.velocity * inertia + homeDirection * homingVelocity) / (inertia + 1f);
            }
            else
            {
                // Set amount of extra updates to default amount.
                projectile.extraUpdates = defaultExtraUpdates;
            }
        }

        // NOTE - Do not under any circumstance use these predictive methods for enemies or bosses. It is intended for minions and player-created projectiles.
        // Due to its extremely precise nature, it will have little openings that allow players to react without dashing through the enemy.
        // The results will be neither fun nor fair.

        /// <summary>
        /// Calculates a velocity that approximately predicts where some target will be in the future based on Euler's Method.
        /// </summary>
        /// <param name="startingPosition">The starting position from where the movement is calculated.</param>
        /// <param name="targetPosition">The position of the target to hit.</param>
        /// <param name="targetVelocity">The velocity of the target to hit.</param>
        /// <param name="shootSpeed">The speed of the predictive velocity.</param>
        /// <param name="iterations">The number of iterations to perform. The more iterations, the more precise the results are.</param>
        public static Vector2 CalculatePredictiveAimToTarget(Vector2 startingPosition, Vector2 targetPosition, Vector2 targetVelocity, float shootSpeed, int iterations = 4)
        {
            float previousTimeToReachDestination = 0f;
            Vector2 currentTargetPosition = targetPosition;
            for (int i = 0; i < iterations; i++)
            {
                float timeToReachDestination = Vector2.Distance(startingPosition, currentTargetPosition) / shootSpeed;
                currentTargetPosition += targetVelocity * (timeToReachDestination - previousTimeToReachDestination);
                previousTimeToReachDestination = timeToReachDestination;
            }
            return (currentTargetPosition - startingPosition).SafeNormalize(Vector2.UnitY) * shootSpeed;
        } 
        
        /// <summary>
        /// Calculates a velocity that approximately predicts where some target will be in the future based on Euler's Method. This takes into account the projectile's max updates.
        /// </summary>
        /// <param name="startingPosition">The starting position from where the movement is calculated.</param>
        /// <param name="targetPosition">The position of the target to hit.</param>
        /// <param name="targetVelocity">The velocity of the target to hit.</param>
        /// <param name="shootSpeed">The speed of the predictive velocity.</param>
        /// <param name="projMaxUpdates">How many extra updates the resulting projectile will have.</param>
        /// <param name="iterations">The number of iterations to perform. The more iterations, the more precise the results are.</param>
        public static Vector2 CalculatePredictiveAimToTargetMaxUpdates(Vector2 startingPosition, Vector2 targetPosition, Vector2 targetVelocity, float shootSpeed, int projMaxUpdates, int iterations = 4)
        {
            float previousTimeToReachDestination = 0f;
            Vector2 currentTargetPosition = targetPosition;
            for (int i = 0; i < iterations; i++)
            {
                float timeToReachDestination = Vector2.Distance(startingPosition, currentTargetPosition) / shootSpeed / projMaxUpdates;
                currentTargetPosition += targetVelocity * (timeToReachDestination - previousTimeToReachDestination);
                previousTimeToReachDestination = timeToReachDestination;
            }
            return (currentTargetPosition - startingPosition).SafeNormalize(Vector2.UnitY) * shootSpeed;
        }

        /// <summary>
        /// Calculates a velocity that approximately predicts where some target will be in the future based on Euler's Method. This takes into account the projectile's max updates.
        /// </summary>
        /// <param name="startingPosition">The starting position from where the movement is calculated.</param>
        /// <param name="target">The target to hit.</param>
        /// <param name="shootSpeed">The speed of the predictive velocity.</param>
        /// <param name="projMaxUpdates">How many extra updates the resulting projectile will have.</param>
        /// <param name="iterations">The number of iterations to perform. The more iterations, the more precise the results are.</param>
        public static Vector2 CalculatePredictiveAimToTargetMaxUpdates(Vector2 startingPosition, Entity target, float shootSpeed, int projMaxUpdates, int iterations = 4)
        {
            return CalculatePredictiveAimToTargetMaxUpdates(startingPosition, target.Center, target.velocity, shootSpeed, projMaxUpdates, iterations);
        }

        /// <summary>
        /// Makes a projectile home in such a way that it attempts to fractionally move towards a target's expected future position.
        /// This is based on the results of the <see cref="CalculatePredictiveAimToTarget"/> method.
        /// </summary>
        /// <param name="projectile">The projectile that should home.</param>
        /// <param name="target">The target.</param>
        /// <param name="inertia">The inertia of the movement change.</param>
        /// <param name="predictionStrength">The ratio for how much the projectile aims ahead of the target. 1f is normal predictiveness. 0.01f is the lowest possible value, equating to no practical predictiveness.</param>
        public static Vector2 SuperhomeTowardsTarget(this Projectile projectile, NPC target, float homingSpeed, float inertia, float predictionStrength = 1f)
        {
            if (predictionStrength < 0.01f) { predictionStrength = 0.01f; }
            Vector2 idealVelocity = CalculatePredictiveAimToTarget(projectile.Center, targetPosition: target.position, targetVelocity: target.velocity, homingSpeed / predictionStrength) * predictionStrength;
            return (projectile.velocity * (inertia - 1f) + idealVelocity) / inertia;
        }
        #endregion
        ///////////////////////////////////
        #region Projectile Spawning

        public static Projectile[] projTripleShot(IEntitySource source, Vector2 spawnPosition, int projectileType, Vector2 targetDir, float projSpeed, int damage, float knockBack = 0, float spreadAngle = 60f)
        {
            Projectile[] projArray = new Projectile[3];
            Vector2 velocity = targetDir * projSpeed;
            float angle = degreesToRadians(spreadAngle);
            velocity.RotatedBy(-angle);
            for (int i = 0; i < 3; i++)
            {
                Vector2 newVel = velocity.RotatedBy(i*angle);
                projArray[i] = Projectile.NewProjectileDirect(source, spawnPosition, newVel, projectileType, damage, knockBack);

            }
            return projArray;
        }

        public static Projectile ProjectileRain(IEntitySource source, Vector2 targetPos, float xLimit, float xVariance, float yLimitLower, float yLimitUpper, float projSpeed, int projType, int damage, float knockback, int owner)
        {
            float x = targetPos.X + Main.rand.NextFloat(-xLimit, xLimit);
            float y = targetPos.Y - Main.rand.NextFloat(yLimitLower, yLimitUpper);
            Vector2 spawnPosition = new Vector2(x, y);
            Vector2 velocity = targetPos - spawnPosition;
            velocity.X += Main.rand.NextFloat(-xVariance, xVariance);
            float speed = projSpeed;
            float targetDist = velocity.Length();
            targetDist = speed / targetDist;
            velocity.X *= targetDist;
            velocity.Y *= targetDist;
            return Projectile.NewProjectileDirect(source, spawnPosition, velocity, projType, damage, knockback, owner);
        }

        public static Projectile ProjectileBarrage(IEntitySource source, Vector2 originVec, Vector2 targetPos, bool fromRight, float xOffsetMin, float xOffsetMax, float yOffsetMin, float yOffsetMax, float projSpeed, int projType, int damage, float knockback, int owner, bool clamped = false, float inaccuracyOffset = 5f)
        {
            float xPos = originVec.X + Main.rand.NextFloat(xOffsetMin, xOffsetMax) * fromRight.ToDirectionInt();
            float yPos = originVec.Y + Main.rand.NextFloat(yOffsetMin, yOffsetMax) * Main.rand.NextBool().ToDirectionInt();
            Vector2 spawnPosition = new Vector2(xPos, yPos);
            Vector2 velocity = targetPos - spawnPosition;
            velocity.X += Main.rand.NextFloat(-inaccuracyOffset, inaccuracyOffset);
            velocity.Y += Main.rand.NextFloat(-inaccuracyOffset, inaccuracyOffset);
            velocity.Normalize();
            velocity *= projSpeed * (clamped ? 150f : 1f);
            //This clamp means the spawned projectiles only go at diagnals and are not accurate
            if (clamped)
            {
                velocity.X = MathHelper.Clamp(velocity.X, -15f, 15f);
                velocity.Y = MathHelper.Clamp(velocity.Y, -15f, 15f);
            }
            return Projectile.NewProjectileDirect(source, spawnPosition, velocity, projType, damage, knockback, owner);
        }

        public static Projectile SpawnOrb(Projectile projectile, int damage, int projType, float distanceRequired, float speedMult, bool gsPhantom = false)
        {
            float ai1 = Main.rand.NextFloat() + 0.5f;
            int[] array = new int[Main.maxNPCs];
            int targetArrayA = 0;
            int targetArrayB = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(projectile, false))
                {
                    float enemyDist = Vector2.Distance(projectile.Center, npc.Center);
                    if (enemyDist < distanceRequired)
                    {
                        if (Collision.CanHit(projectile.position, 1, 1, npc.position, npc.width, npc.height) && enemyDist > 50f)
                        {
                            array[targetArrayB] = npc.whoAmI;
                            targetArrayB++;
                        }
                        else if (targetArrayB == 0)
                        {
                            array[targetArrayA] = npc.whoAmI;
                            targetArrayA++;
                        }
                    }
                }
            }
            if (targetArrayA == 0 && targetArrayB == 0)
            {
                return Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ProjectileType<NobodyKnows>(), 0, 0f, projectile.owner);
            }
            int target = targetArrayB <= 0 ? array[Main.rand.Next(targetArrayA)] : array[Main.rand.Next(targetArrayB)];
            Vector2 velocity = RandomVelocity(100f, speedMult, speedMult, 1f);
            Projectile orb = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), projectile.Center, velocity, projType, damage, 0f, projectile.owner, gsPhantom ? 0f : target, gsPhantom ? ai1 : 0f);
            return orb;
        }

         public static void MagnetSphereHitscan(Projectile projectile, float distanceRequired, float homingVelocity, float projectileTimer, int maxTargets, int spawnedProjectile, double damageMult = 1D, bool attackMultiple = false)
        {
            // Only shoot once every N frames.
            projectile.localAI[1] += 1f;
            if (projectile.localAI[1] > projectileTimer)
            {
                projectile.localAI[1] = 0f;

                // Only search for targets if projectiles could be fired.
                float maxDistance = distanceRequired;
                bool homeIn = false;
                int[] targetArray = new int[maxTargets];
                int targetArrayIndex = 0;

                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.CanBeChasedBy(projectile, false))
                    {
                        float extraDistance = (n.width / 2) + (n.height / 2);

                        bool canHit = true;
                        if (extraDistance < maxDistance)
                            canHit = Collision.CanHit(projectile.Center, 1, 1, n.Center, 1, 1);

                        if (projectile.WithinRange(n.Center, maxDistance + extraDistance) && canHit)
                        {
                            if (targetArrayIndex < maxTargets)
                            {
                                targetArray[targetArrayIndex] = n.whoAmI;
                                targetArrayIndex++;
                                homeIn = true;
                            }
                            else
                                break;
                        }
                    }
                }

                // If there is anything to actually shoot at, pick targets at random and fire.
                if (homeIn)
                {
                    int randomTarget = Main.rand.Next(targetArrayIndex);
                    randomTarget = targetArray[randomTarget];

                    projectile.localAI[1] = 0f;
                    Vector2 spawnPos = projectile.Center + projectile.velocity * 4f;
                    Vector2 velocity = Vector2.Normalize(Main.npc[randomTarget].Center - spawnPos) * homingVelocity;

                    if (attackMultiple)
                    {
                        for (int i = 0; i < targetArrayIndex; i++)
                        {
                            velocity = Vector2.Normalize(Main.npc[targetArray[i]].Center - spawnPos) * homingVelocity;

                            if (projectile.owner == Main.myPlayer)
                            {
                                int projectile2 = Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, velocity, spawnedProjectile, (int)(projectile.damage * damageMult), projectile.knockBack, projectile.owner, 0f, 0f);

                                
                            }
                        }

                        return;
                    }

                    
                    if (projectile.owner == Main.myPlayer)
                    {
                        int projectile2 = Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, velocity, spawnedProjectile, (int)(projectile.damage * damageMult), projectile.knockBack, projectile.owner, 0f, 0f);

                    }
                }
            }
        }

        #endregion

        #region Killing & Despawning
        public static void KillShootProjectiles(bool shouldBreak, int projType, Player player)
        {
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                Projectile proj = Main.projectile[x];
                if (proj.active && proj.owner == player.whoAmI && proj.type == projType)
                {
                    proj.Kill();
                    if (shouldBreak)
                        break;
                }
            }
        }
        
        #endregion

    
    }
}
