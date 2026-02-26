using Microsoft.Xna.Framework;
using System;
using Terraria;


namespace SpinningTopsMod
{
    // This class contains utility Utils that can be used across the mod.
    // For example, you can create a function to scale a value from one range to another.
    // This can be useful for various calculations in your mod.
    
    public static partial class Utils
    {
        
            /// <summary>
            /// <para> This function scales a value from one range to another. </para>
            /// <para> It takes a value, a minimum and maximum of the original range, and a minimum and maximum of the target range. </para>
            /// <para> It returns the scaled value in the target range. </para>
            /// <para> For example, if you want to scale a value from 0-100 to 0-1, you can use this function like this: </para>
            /// <para> float scaledValue = Utils.ScaleToRange(value, 0f, 100f, 0f, 1f);</para>
            /// </summary>
            public static float ScaleToRange(float value, float min, float max, float rangeMin, float rangeMax)
            {
                return rangeMin + (Math.Clamp(value, min, max) - min) * (rangeMax - rangeMin) / (max - min);
            }

            /// <summary>
            /// <para> This function checks if two Vector2 positions 
            /// have similar indexes. </para>
            /// <para> It takes two Vector2 positions and a threshold distance as input. </para>
            /// <para> It returns true if any index of pos1 is within the threshold distance of pos2,
            /// and false if every single index of pos1 and pos2 are different </para>
            /// </summary>
            public static bool areClose(Vector2 pos1, Vector2 pos2, float threshold)
            {
                float[] pos1Array = new float[] { pos1.X, pos1.Y };
                float[] pos2Array = new float[] { pos2.X, pos2.Y };

                for (int i = 0; i < pos1Array.Length; i++)
                {
                    if (areClose(pos1Array[i], pos2Array[i], threshold))
                    {
                        return true;
                    }

                }
                return false;
            }
            /// <summary>
            /// <para> This function checks if two float values are within a
            /// certain threshold of each other. </para>
            /// <para> It takes two float values and a threshold as input. </para>
            /// <para> It returns true if the absolute difference between the two values is
            /// less than or equal to the threshold, and false otherwise. </para>
            /// </summary>
            public static bool areClose(float value1, float value2, float threshold)
            {
                return Math.Abs(value1 - value2) <= threshold;
            }

        /// <summary>
        /// Determines the angular distance between two vectors based on dot product comparisons. This method ensures underlying normalization is performed safely.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        public static float AngleBetween(this Vector2 v1, Vector2 v2) => (float)Math.Acos(Vector2.Dot
        (v1.SafeNormalize(Vector2.Zero), v2.SafeNormalize(Vector2.Zero)));
        
         /// <summary>
        /// Uses a rewritten horizontal range formula to determine the direction to fire a projectile in order for it to hit a destination. Falls back on a certain value if no such direction can exist. If no fallback is provided, a clamp is used.
        /// </summary>
        /// <param name="shootingPosition">The starting position of the projectile.</param>
        /// <param name="destination">The destination for the projectile to land at.</param>
        /// <param name="gravity">The gravity of the projectile.</param>
        /// <param name="shootSpeed">The magnitude </param>
        /// <param name="nanFallback">The direction to fall back to if the calculations result in any NaNs. If nothing is specified, a clamp is performed to prevent any chance of NaNs at all.</param>
        public static Vector2 GetProjectilePhysicsFiringVelocity(Vector2 shootingPosition, Vector2 destination, float gravity, float shootSpeed, Vector2? nanFallback = null)
        {
            // Ensure that the gravity has the right sign for Terraria's coordinate system.
            gravity = -Math.Abs(gravity);

            float horizontalRange = MathHelper.Distance(shootingPosition.X, destination.X);
            float fireAngleSine = gravity * horizontalRange / (float)Math.Pow(shootSpeed, 2);

            // Clamp the sine if no fallback is provided.
            if (nanFallback is null)
                fireAngleSine = MathHelper.Clamp(fireAngleSine, -1f, 1f);

            float fireAngle = (float)Math.Asin(fireAngleSine) * 0.5f;

            // Get out of here if no valid firing angle exists. This can only happen if a fallback does indeed exist.
            if (float.IsNaN(fireAngle))
                return nanFallback.Value * shootSpeed;

            Vector2 fireVelocity = new Vector2(0f, -shootSpeed).RotatedBy(fireAngle);
            fireVelocity.X *= (destination.X - shootingPosition.X < 0).ToDirectionInt();
            return fireVelocity;
        }

        public static int FrequencyToFrames(float frequency, float updatesPerFrame = 1)
        {
            return (int)((60f * updatesPerFrame) / frequency);
        }

        public static bool blockedByWall(Vector2 nowVelocity, Vector2 oldVelocity)
        {
            return Math.Abs(nowVelocity.X - oldVelocity.X) > float.Epsilon;
        }

        public static bool blockedByFloor(Vector2 nowVelocity, Vector2 oldVelocity)
        {
            return Math.Abs(nowVelocity.Y - oldVelocity.Y) > float.Epsilon;
        }

        public static int secondsToFrames(float seconds, int numUpdates = 1)
        {
            return (int)(60*numUpdates*seconds);
        }
        /// <summary>
        /// Converts a number of tiles to pixels. NOTE: point coordinates are in TILES, not pixels.
        /// Don't use for world coordinates directly.
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static float tilesToPixels(byte tiles)
        {
            return tiles * 16f;
        }

        public static float tilesPerSecondToPixelsPerFrame(float tilesPerSecond, int numUpdates = 1)
        {
            return (tilesPerSecond * 16f) / (60f * numUpdates);
        }
        /// <summary>
        /// Sinusoidal interpolator for smoothing (ease in/out).
        /// Input t is expected in [0,1]. Output is also in [0,1].
        /// Use like: MathHelper.Lerp(a, b, Utils.Sinusoidal(t));
        /// </summary>
        public static float Sinusoidal(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return (float)(0.5 * (1 - Math.Cos(Math.PI * t)));
        }

        /// <summary>
        /// Power interpolator. Returns t^power (clamped) so you can get ease-in behavior
        /// for power &gt; 1, or ease-out by using 1 - (1-t)^power, etc. Example: Utils.Power(t, 2) for quadratic.
        /// Use like: MathHelper.Lerp(a, b, Utils.Power(t, 3));
        /// </summary>
        public static float Power(float t, float power)
        {
            t = Math.Clamp(t, 0f, 1f);
            return (float)Math.Pow(t, power);
        }
        /// <summary>
        /// Returns a Vector2 that is tanget to the input vector.
        /// Is turned 90 degrees anti-clockwise by defualt.
        /// Just multiply by -1 to get clockwise tanget.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 getTangetToVector(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }

        public static float degreesToRadians(float degrees)
        {
            return degrees/360f * twoPi;
        }
    }
}