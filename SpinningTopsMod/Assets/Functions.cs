namespace SpinningTopsMod
{
    // This class contains utility functions that can be used across the mod.
    // For example, you can create a function to scale a value from one range to another.
    // This can be useful for various calculations in your mod.

    public static class Functions
    {   /// <summary>
        /// <para> This function scales a value from one range to another. </para>
        /// <para> It takes a value, a minimum and maximum of the original range, and a minimum and maximum of the target range. </para>
        /// <para> It returns the scaled value in the target range. </para>
        /// <para> For example, if you want to scale a value from 0-100 to 0-1, you can use this function like this: </para>
        /// <para> float scaledValue = Functions.ScaleToRange(value, 0f, 100f, 0f, 1f);</para>
        /// </summary>
        public static float ScaleToRange(float value, float min, float max, float rangeMin, float rangeMax)
        {
            return rangeMin + (value - min) * (rangeMax - rangeMin) / (max - min);
        }
        

    }
}