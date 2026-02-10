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
                Vector2 tangetVelocity = dustSpeed == 0 ? Vector2.Zero : tanget * dustSpeed;
                Vector2 growVelocity = dustGrow == 0 ? Vector2.Zero : offset.SafeNormalize(Vector2.Zero) * dustGrow;
                dust.velocity = tangetVelocity + growVelocity;
                dusts[i] = dust;
            }
            return dusts;
        }
        /// <summary>
        /// Spawns a ring of dust particles around a given position.
        /// Dust particles velocity is 0 and cannot be controlled.
        /// Returns null.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dustType"></param>
        /// <param name="radius"></param>
        /// <param name="dustAmount"></param>
        /// <param name="dustColor"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static Dust DustRing(Vector2 position, int dustType, float radius, int dustAmount, Color? dustColor = null, bool gravity = false)
        {
            DustRing(position, dustType, radius, dustAmount, 0, 0, dustColor, gravity);
            return null;
        }

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

        public static void DustGoldenRatioEmitter(Vector2 position, int dustType, int dustAmount, float speed, Color? dustColor = null, bool gravity = true, float dustScale = 1f)
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
    }
}