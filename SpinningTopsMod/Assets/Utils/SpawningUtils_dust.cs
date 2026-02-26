using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SpinningTopsMod;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpinningTopsMod
{
    public static partial class Utils
    {   
        public static float twoPi = (float)(2 * Math.PI);

        public static float PiOver2 = (float)(Math.PI / 2);

        public static float PiOver4 = (float)(Math.PI / 4);

        public static float GoldenRatio = (1f + (float)Math.Sqrt(5)) / 2f;

        public static float InverseGoldenRatio = 1f / GoldenRatio;

        public static float GoldenAngle = twoPi * (1f - InverseGoldenRatio);

        /// <summary>
        /// Returns a Vector2 from an angle and radius, using sin/cos 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 GetVector2FromAngle(float angle, float radius)
        {
            return new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
        }
        /// <summary>
        /// <para> This function calculates the angle step for evenly spacing points around a circle. </para>
        /// </summary>
        /// <param name="divisions"> The number of equal angles to divide the circle into</param>
        /// <returns></returns>
        public static float angleEvenlySpaced_circle(int divisions)
        {
            return twoPi / divisions;
        }
        /// <summary>
        /// <para> This function calculates the angle steps for evenly spacing points around a circle. </para>
        /// <para> If iterate is true, it returns an array of angles for each division. If iterate is false- just use the float variant of angleEvenlySpaced_circle </para>
        /// </summary>
        /// <param name="divisions"></param>
        /// <param name="iterate"></param>
        /// <returns></returns>
        public static float[] angleEvenlySpaced_circle(int divisions, bool iterate = true)
        {   
            if(iterate)
            {
                float[] angles = new float[divisions];
                for (int i = 0; i < divisions; i++)
                {
                    angles[i] = angleEvenlySpaced_circle(divisions);
                }
                return angles;
            }
            return new float[] { angleEvenlySpaced_circle(divisions) };
        }
        /// <summary>
        /// Creates a ring of dust particles around a given position.
        /// Returns an array of the created Dust objects for direct access.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="radius"></param>
        /// <param name="dustAmount"></param>
        /// <param name="dustSpeed"> How fast the dust particles move tanget to the circle</param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"> Enables gravity for dust. True sets Dust.noGravity to false, and vice versa. </param>
        /// <param name="dustGrow"> The velocity of the dust away from the center, making the circle grow </param>
        /// <returns></returns>
        public static Dust[] DustRing(Vector2 position, int dustType, float radius, int dustAmount, float dustSpeed = 0, float dustGrow = 0, Color? dustColor = null, bool gravity = false)
        {
            float angleStep = angleEvenlySpaced_circle(dustAmount);
            Dust[] dusts = new Dust[dustAmount];
            for (int i = 0; i < dustAmount; i++)
            {
                Vector2 offset = GetVector2FromAngle(i * angleStep, radius);
                Dust dust = Dust.NewDustPerfect(position + offset,
                    dustType, 
                    Vector2.Zero, 
                    100, 
                    dustColor.HasValue ? dustColor.Value : default, 
                    1.5f);
                dust.noGravity = !gravity;
                Vector2 tanget = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
                dust.customData = dustSpeed;
                Vector2 tangetVelocity = dustSpeed == 0 ? Vector2.Zero : tanget * dustSpeed;
                Vector2 growVelocity = dustGrow == 0 ? Vector2.Zero : offset.SafeNormalize(Vector2.Zero) * dustGrow;
                dust.velocity = tangetVelocity + growVelocity;
                dusts[i] = dust;
            }
            return dusts;
        }
       
       
        /// <summary>
        /// Spawns an explosion of dust particles from a given position, with random velocities.
        /// </summary>
        /// <param name="position"> Explosion center</param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"> Number of dust instances</param>
        /// <param name="minSpeed"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"></param>
        /// <param name="dustScale"></param> <summary>
        public static void DustExplosion(Vector2 position, int dustType, int dustAmount, float minSpeed, float maxSpeed, Color? dustColor = null, bool gravity = true, float dustScale = 1f)
        {
            for (int i = 0; i < dustAmount; i++)
            {
                Dust dust = Dust.NewDustPerfect(position,
                    dustType,
                    Vector2.Zero,
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale);
                dust.noGravity = !gravity;
                float speed = Main.rand.NextFloat(minSpeed, maxSpeed);
                float PerfectAngle = i*angleEvenlySpaced_circle(dustAmount);
                float angle = PerfectAngle + Main.rand.NextFloat(0, PerfectAngle/2f);
                Vector2 velocity = GetVector2FromAngle(angle, speed);
                dust.velocity = velocity;
            }
            
        }
        /// <summary>
        /// Emits out dust in a spiral pattern based on the golden ratio
        /// </summary>
        /// <param name="position"> Spiral center</param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"></param>
        /// <param name="speed"> The speed each dust gets shot out at</param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"></param>
        /// <param name="dustScale"></param>
        public static void DustGoldenRatioEmitter(Vector2 position, int dustType, int dustAmount, float speed, Color? dustColor = null, bool gravity = false, float dustScale = 1f)
        {
            float angleStep = GoldenAngle;
            for (int i = 0; i < dustAmount; i++)
            {
                Dust dust = Dust.NewDustPerfect(position,
                    dustType,
                    GetVector2FromAngle(i*angleStep, speed),
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale);
                dust.noGravity = !gravity;
            }  
        }
        /// <summary>
        /// Horizontal puff of smoke/dust at the edges of a box
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"></param>
        /// <param name="radius"></param>
        /// <param name="dustColor"></param>
        /// <param name="dustScale"></param>
        /// <param name="speed"></param>
        public static void dustPuffAtEdges(Vector2 position, int dustType, int dustAmount, float radius, Color? dustColor = null, float dustScale = 1f, float speed = 2f)
        {
            for (int i = 0; i < dustAmount; i++)
            {
                int dir = i%2 == 0 ? 1 : -1;
                Vector2 offset =  new Vector2(dir * radius, 0);
                Vector2 pos = position + offset;
                Vector2 velocity =  new Vector2(dir*speed, -speed);
                Dust dust = Dust.NewDustPerfect(pos,
                    dustType,
                    velocity,
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale);
                    dust.noGravity = true;
            }
        }
        /// <summary>
        /// Horizontal slam of dust outwards from a surface, where the dust travels vertically paralell to the surface
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"></param>
        /// <param name="minSpeed"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="spreadRange"></param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"></param>
        /// <param name="dustScale"></param>
        public static void dustSlamOutwards(Vector2 position, int dustType, int dustAmount, float minSpeed, float maxSpeed, float spreadRange ,Color? dustColor = null, bool gravity = true, float dustScale = 1f)
        {
            for (int i = 0; i < dustAmount; i++)
            {
                float speed = Main.rand.NextFloat(minSpeed, maxSpeed);
                float pos =  position.X + Main.rand.NextFloat(-spreadRange, spreadRange);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(), speed);

                Dust particalSpray = Dust.NewDustPerfect(new Vector2(pos, position.Y),
                    dustType,
                    velocity,
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale + Main.rand.NextFloat(-0.5f, 1f));
                particalSpray.noGravity = !gravity;
                
            }   
        }
        /// <summary>
        /// Spawns a vertical slam of dust outwards from a vertical surface, where dust travels horizontally
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"></param>
        /// <param name="minSpeed"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="spreadRange"></param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"></param>
        /// <param name="dustScale"></param>
        public static void dustSlamVertical(Vector2 position, int dustType, int dustAmount, float minSpeed, float maxSpeed, float spreadRange ,Color? dustColor = null, bool gravity = true, float dustScale = 1f)
        {
            for (int i = 0; i < dustAmount; i++)
            {
                float speed = Main.rand.NextFloat(minSpeed, maxSpeed);
                float pos =  position.Y + Main.rand.NextFloat(-spreadRange, spreadRange);
                Vector2 velocity = new Vector2(0, speed);

                Dust particalSpray = Dust.NewDustPerfect(new Vector2(position.X, pos),
                    dustType,
                    velocity,
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale + Main.rand.NextFloat(-0.5f, 1f));
                particalSpray.noGravity = !gravity;
                
            }  
        }
        /// <summary>
        /// Spawns a vertical puff of dust at the edges of a box, where the dust travels horizontally away from the center
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="dustAmount"></param>
        /// <param name="radius"></param>
        /// <param name="dustColor"></param>
        /// <param name="dustScale"></param>
        /// <param name="speed"></param>
        public static void dustPuffEdgesVertical(Vector2 position, int dustType, int dustAmount, float radius, Color? dustColor = null, float dustScale = 1f, float speed = 2f)
        {
            for (int i = 0; i < dustAmount; i++)
            {
                int dir = i%2 == 0 ? 1 : -1;
                Vector2 offset =  new Vector2(0, dir * radius);
                Vector2 pos = position + offset;
                Vector2 velocity =  new Vector2(speed, dir*speed);
                Dust dust = Dust.NewDustPerfect(pos,
                    dustType,
                    velocity,
                    100,
                    dustColor.HasValue ? dustColor.Value : default,
                    dustScale);
                    dust.noGravity = true;
            }
        }
    }
}