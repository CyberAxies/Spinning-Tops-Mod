using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpinningTopsMod
{
    /// <summary>
    /// Simple fixed-size spatial hash (uniform grid) storing packed indices.
    /// Note: uses a Dictionary of cell-key -> List<int> of packed indices.
    /// </summary>
    public class SpatialHashGrid
    {
        readonly Dictionary<long, List<int>> cells = new Dictionary<long, List<int>>();
        public int CellSize { get; private set; }

        public SpatialHashGrid(int cellSize)
        {
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
            CellSize = cellSize;
        }

        public void SetCellSize(int cellSize)
        {
            if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
            if (cellSize == CellSize) return;
            CellSize = cellSize;
            Clear();
        }

        long Key(int cx, int cy) => ((long)cx << 32) | (uint)cy;

        (int cx, int cy) CellCoords(Vector2 pos)
        {
            int cx = (int)Math.Floor(pos.X / CellSize);
            int cy = (int)Math.Floor(pos.Y / CellSize);
            return (cx, cy);
        }

        public void Clear()
        {
            cells.Clear();
        }

        public void Add(int packedIndex, Vector2 pos)
        {
            var (cx, cy) = CellCoords(pos);
            long k = Key(cx, cy);
            if (!cells.TryGetValue(k, out var list))
            {
                list = new List<int>(4);
                cells[k] = list;
            }
            if (!list.Contains(packedIndex))
                list.Add(packedIndex);
        }

        public void Remove(int packedIndex, Vector2 pos)
        {
            var (cx, cy) = CellCoords(pos);
            long k = Key(cx, cy);
            if (!cells.TryGetValue(k, out var list)) return;
            int idx = list.IndexOf(packedIndex);
            if (idx >= 0)
            {
                int last = list.Count - 1;
                if (idx != last)
                    list[idx] = list[last];
                list.RemoveAt(last);
                if (list.Count == 0)
                    cells.Remove(k);
            }
        }

        public void Move(int packedIndex, Vector2 oldPos, Vector2 newPos)
        {
            var (ocx, ocy) = CellCoords(oldPos);
            var (ncx, ncy) = CellCoords(newPos);
            if (ocx == ncx && ocy == ncy)
                return;
            Remove(packedIndex, oldPos);
            Add(packedIndex, newPos);
        }

        // Gather candidate packed indices inside the bounding box radius (may contain duplicates across cells)
        public void Query(Vector2 pos, float radius, List<int> outList)
        {
            if (outList == null) throw new ArgumentNullException(nameof(outList));
            outList.Clear();

            int minCx = (int)Math.Floor((pos.X - radius) / CellSize);
            int maxCx = (int)Math.Floor((pos.X + radius) / CellSize);
            int minCy = (int)Math.Floor((pos.Y - radius) / CellSize);
            int maxCy = (int)Math.Floor((pos.Y + radius) / CellSize);

            var seen = new HashSet<int>();
            for (int cx = minCx; cx <= maxCx; cx++)
            {
                for (int cy = minCy; cy <= maxCy; cy++)
                {
                    long k = Key(cx, cy);
                    if (!cells.TryGetValue(k, out var list)) continue;
                    for (int i = 0; i < list.Count; i++)
                    {
                        int packed = list[i];
                        if (seen.Add(packed)) outList.Add(packed);
                    }
                }
            }
        }
    }
}
