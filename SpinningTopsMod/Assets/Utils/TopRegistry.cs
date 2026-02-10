using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace SpinningTopsMod
{
    /// <summary>
    /// Packed, SoA-style registry for active spinning tops.
    /// Maintains three (well, several) packed lists and an ID->index reverse map
    /// so add/remove are O(1) (swap-and-pop) and iteration is cache-friendly.
    /// </summary>
    public static class TopRegistry
    {
        // Spatial hash for quick neighbor queries (default 64px cells)
        static SpatialHashGrid grid = new SpatialHashGrid(64);

        // Struct-of-Arrays fields
        public static readonly List<Vector2> positions = new List<Vector2>();
        public static readonly List<Vector2> velocities = new List<Vector2>();
        public static readonly List<int> projIds = new List<int>();
        public static readonly List<int> owners = new List<int>();
        public static readonly List<float> radiuses = new List<float>();

        // Reverse map: projectile.whoAmI -> index in the lists, or -1 if not present
        static int[] idToIndex = Array.Empty<int>();
        static bool initialized = false;

        static void EnsureInit()
        {
            if (initialized)
                return;
            idToIndex = Enumerable.Repeat(-1, Main.maxProjectiles).ToArray();

            // Pre-allocate backing arrays to avoid runtime reallocations and improve cache behavior.
            positions.Capacity = Math.Max(positions.Capacity, Main.maxProjectiles);
            velocities.Capacity = Math.Max(velocities.Capacity, Main.maxProjectiles);
            projIds.Capacity = Math.Max(projIds.Capacity, Main.maxProjectiles);
            owners.Capacity = Math.Max(owners.Capacity, Main.maxProjectiles);
            radiuses.Capacity = Math.Max(radiuses.Capacity, Main.maxProjectiles);

            grid.Clear();
            initialized = true;
        }

        /// <summary>
        /// Set the spatial-hash cell size (pixels). Clears the grid when changed.
        /// </summary>
        public static void SetGridCellSize(int cellSize)
        {
            EnsureInit();
            grid.SetCellSize(cellSize);
        }

        /// <summary>
        /// Ensure internal lists have capacity for at least <paramref name="expected"/> entries.
        /// Call this if you expect many tops and want to avoid dynamic growth.
        /// </summary>
        public static void EnsureCapacity(int expected)
        {
            EnsureInit();
            if (expected <= 0) return;
            if (positions.Capacity < expected) positions.Capacity = expected;
            if (velocities.Capacity < expected) velocities.Capacity = expected;
            if (projIds.Capacity < expected) projIds.Capacity = expected;
            if (owners.Capacity < expected) owners.Capacity = expected;
            if (radiuses.Capacity < expected) radiuses.Capacity = expected;
        }

        public static bool IsRegistered(int projWhoAmI)
        {
            EnsureInit();
            if (projWhoAmI < 0 || projWhoAmI >= idToIndex.Length) return false;
            return idToIndex[projWhoAmI] != -1;
        }

        public static void Register(Projectile proj, float radius)
        {
            EnsureInit();
            int id = proj.whoAmI;
            if (id < 0 || id >= idToIndex.Length)
                return;

            if (idToIndex[id] != -1)
                return; // already registered

            int idx = positions.Count;
            positions.Add(proj.Center);
            velocities.Add(proj.velocity);
            projIds.Add(id);
            owners.Add(proj.owner);
            radiuses.Add(radius);
            idToIndex[id] = idx;
            // Add to spatial hash
            grid.Add(idx, proj.Center);
        }

        public static void UpdateFromProjectile(Projectile proj)
        {
            EnsureInit();
            int id = proj.whoAmI;
            if (id < 0 || id >= idToIndex.Length) return;
            int idx = idToIndex[id];
            if (idx == -1) return;

            Vector2 oldPos = positions[idx];
            Vector2 newPos = proj.Center;
            positions[idx] = newPos;
            velocities[idx] = proj.velocity;
            // Update spatial hash if moved across cells
            grid.Move(idx, oldPos, newPos);
            // owners/projIds/radiuses are mostly static, but could be updated here if needed
        }

        /// <summary>
        /// Clears the registry. Useful when unloading or resetting state between tests.
        /// </summary>
        public static void ClearAll()
        {
            positions.Clear();
            velocities.Clear();
            projIds.Clear();
            owners.Clear();
            radiuses.Clear();
            if (idToIndex != null && idToIndex.Length > 0)
                for (int i = 0; i < idToIndex.Length; i++) idToIndex[i] = -1;
        }

        public static void Unregister(int projWhoAmI)
        {
            EnsureInit();
            if (projWhoAmI < 0 || projWhoAmI >= idToIndex.Length) return;
            int idx = idToIndex[projWhoAmI];
            if (idx == -1) return;

            int last = positions.Count - 1;

            // Remove this packed index from the spatial hash first
            Vector2 removePos = positions[idx];
            grid.Remove(idx, removePos);

            if (idx != last)
            {
                Vector2 lastPos = positions[last];
                int lastPacked = projIds[last];

                // Remove last's old entry from grid
                grid.Remove(last, lastPos);

                // Move last into idx
                positions[idx] = positions[last];
                velocities[idx] = velocities[last];
                projIds[idx] = projIds[last];
                owners[idx] = owners[last];
                radiuses[idx] = radiuses[last];

                // Update reverse map for moved projectile
                int movedProjId = projIds[idx];
                if (movedProjId >= 0 && movedProjId < idToIndex.Length)
                    idToIndex[movedProjId] = idx;

                // Add moved packed index back into the grid at its (unchanged) position
                grid.Add(idx, lastPos);
            }

            // Pop last
            positions.RemoveAt(last);
            velocities.RemoveAt(last);
            projIds.RemoveAt(last);
            owners.RemoveAt(last);
            radiuses.RemoveAt(last);

            idToIndex[projWhoAmI] = -1;
        }

        public static int Count => positions.Count;

        // Iterate packed list with an action that receives index and projectile id
        public static void ForEach(Action<int, int> action)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                action(i, projIds[i]);
            }
        }

        // Non-allocating broad-phase neighbor search: fills the provided list with nearby indices.
        // This avoids per-frame allocations; caller should reuse a List<int>.
        public static void QueryNearbyIndicesNonAlloc(int sourceIndex, float radius, List<int> outList)
        {
            if (outList == null) throw new ArgumentNullException(nameof(outList));
            outList.Clear();
            if (sourceIndex < 0 || sourceIndex >= positions.Count) return;

            float r2 = radius * radius;
            Vector2 p = positions[sourceIndex];

            // Use spatial hash to collect candidates and then filter by precise radius
            grid.Query(p, radius, outList);
            // Filter out self and any candidates outside exact radius
            for (int i = outList.Count - 1; i >= 0; i--)
            {
                int packed = outList[i];
                if (packed == sourceIndex)
                {
                    outList.RemoveAt(i);
                    continue;
                }
                if (packed < 0 || packed >= positions.Count)
                {
                    outList.RemoveAt(i);
                    continue;
                }
                if (Vector2.DistanceSquared(p, positions[packed]) > r2)
                    outList.RemoveAt(i);
            }
        }

        // Backwards-compatible allocing helper (use sparingly)
        public static List<int> QueryNearbyIndices(int sourceIndex, float radius)
        {
            var tmp = new List<int>();
            QueryNearbyIndicesNonAlloc(sourceIndex, radius, tmp);
            return tmp;
        }

        /// <summary>
        /// Try to get the packed index for a projectile whoAmI. Returns true and sets index if registered.
        /// </summary>
        public static bool TryGetIndex(int projWhoAmI, out int index)
        {
            EnsureInit();
            index = -1;
            if (projWhoAmI < 0 || projWhoAmI >= idToIndex.Length) return false;
            int v = idToIndex[projWhoAmI];
            if (v == -1) return false;
            index = v;
            return true;
        }
    }
}
