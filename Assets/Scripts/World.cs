using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MathUtil;
using UnityEngine;

namespace Assets.Scripts
{
    // Represents the world terrain data
    public class World
    {
        public Vector2I Minimum { get; private set; }
        public Vector2I Maximum { get; private set; }
        public int Width { get { return this.Maximum.x - this.Minimum.x; } }
        public int Height { get { return this.Maximum.y - this.Minimum.y; } }
        
        public TerrainLayer TerrainLayer { get; private set; }
        public DynamicLayer DynamicLayer { get; private set; }

        public TerrainMesh Mesh { get; private set; }

        public World(Vector2I origin, Vector2I dimensions, TerrainMesh mesh, Texture2D texture = null)
        {
            this.Minimum = origin;
            this.Maximum = origin + dimensions - new Vector2I(1, 1);
            this.Mesh = mesh;
            this.TerrainLayer = new TerrainLayer(origin, dimensions, texture);
            this.DynamicLayer = new DynamicLayer();
        }

        public bool IsInside(Vector2I coord)
        {
            return coord.x >= Minimum.x && coord.x <= Maximum.x && coord.y >= Minimum.y && coord.y <= Maximum.y;
        }

        public Color32 GetPixelAt(Vector2I coord)
        {
            Color32? dynamicPixel = this.DynamicLayer.GetAt(coord);
            if (dynamicPixel.HasValue)
                return dynamicPixel.Value;
            return this.TerrainLayer.GetAt(coord);
        }

        public IEnumerable<Vector2I> EnumerateChangeList()
        {
            foreach (var terrainPixel in this.TerrainLayer.ChangeList)
            {
                yield return terrainPixel;
            }
            foreach (var dynamicPixel in this.DynamicLayer.ChangeList)
            {
                yield return dynamicPixel;
            }
        }

        public void FlushChangeList()
        {
            this.TerrainLayer.ChangeList.Clear();
            this.DynamicLayer.ChangeList.Clear();
        }

        // All changes are queued and committed on next Update
        public void ChangeTerrainColorAt(Vector2I position, Color32 newColorData)
        {
            this.TerrainLayer.SetAt(position, newColorData);
        }

        public void ChangeTerrainAlphaAt(Vector2I position, float newAlpha)
        {
            this.TerrainLayer.SetAlphaAt(position, newAlpha);
        }
        public DynamicLayerEntry CreateDecal(Vector3I position, Color32 color)
        {
            return this.DynamicLayer.CreateDynamicEntry(position, color);
        }
        public void RemoveDecal(DynamicLayerEntry dynamicEntry)
        {
            this.DynamicLayer.RemoveDynamicEntry(dynamicEntry);
        }
    }

    // Contains the all pixels of the terrain layer
    public class TerrainLayer
    {
        public Color32[] Data { get; private set; }
        public Vector2I Origin { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public List<Vector2I> ChangeList { get; private set; }

        public TerrainLayer(Vector2I origin, Vector2I dimensions, Texture2D initialTexture = null)
        {
            this.Origin = origin;
            this.Width = dimensions.x;
            this.Height = dimensions.y;
            this.Data = new Color32[Width * Height];
            this.ChangeList = new List<Vector2I>();
            if (initialTexture != null)
            {
                if (initialTexture.width != this.Width || initialTexture.height != this.Height)
                {
                    // Dimensions don't match, we will repeat the texture over and over
                    var pixels = initialTexture.GetPixels32();
                    for (int y = 0; y < this.Height; ++y)
                    {
                        int xRemain = this.Width;
                        while (xRemain > 0)
                        {
                            Array.Copy(pixels, (y % initialTexture.height)*initialTexture.width, this.Data, y*this.Width + this.Width - xRemain, Math.Min(xRemain, initialTexture.width));
                            xRemain -= initialTexture.width;
                        }
                    }
                }
                else
                    Array.Copy(initialTexture.GetPixels32(), this.Data, Width * Height);
            }
        }

        public Color32 GetAt(Vector2I pos)
        {
            return this.Data[DataArrayIndex(pos)];
        }

        public void SetAt(Vector2I pos, Color32 value)
        {
            this.Data[DataArrayIndex(pos)] = value;
            this.ChangeList.Add(pos);
        }

        public Color32 SetAlphaAt(Vector2I pos, float alpha)
        {
            var value = GetAt(pos);
            value.a = (byte)(255*alpha);
            this.Data[DataArrayIndex(pos)] = value;
            this.ChangeList.Add(pos);
            return value;
        }

        private int DataArrayIndex(Vector2I pos)
        {
            return pos.x - Origin.x + ((pos.y - Origin.y))*this.Width;
        }
    }

    // Contains movable pixels of animated effects, drawn over terrain layer
    public class DynamicLayer
    {
        public List<DynamicLayerEntry> Data { get; private set; }
        public List<Vector2I> ChangeList { get; private set; }

        public DynamicLayer()
        {
            this.Data = new List<DynamicLayerEntry>();
            this.ChangeList = new List<Vector2I>();
        }

        public Color32? GetAt(Vector2I coord)
        {
            for (int i = Data.Count - 1; i >= 0; --i)
            {
                if (this.Data[i].Position.x == coord.x && this.Data[i].Position.y == coord.y)
                    return this.Data[i].Color;
            }
            return null;
        }

        public Color32? GetTopAt(Vector2I coord)
        {
            DynamicLayerEntry result = null;
            for (int i = Data.Count - 1; i >= 0; ++i)
            {
                if (this.Data[i].Position.x == coord.x && this.Data[i].Position.y == coord.y)
                {
                    if (result == null || (result.Position.z < this.Data[i].Position.z))
                        result = this.Data[i];
                }
            }
            if (result != null)
                return result.Color;
            return null;
        }

        public DynamicLayerEntry CreateDynamicEntry(Vector3I coord, Color32 color)
        {
            var entry = new DynamicLayerEntry(coord, color, this);
            this.Data.Add(entry);
            return entry;
        }

        public void RemoveDynamicEntry(DynamicLayerEntry entry)
        {
            entry.Delete();
            bool removed = this.Data.Remove(entry);
            if (!removed)
                Debug.LogWarning("RemoveDynamicEntry could not find any such entry");
        }
    }

    // One movable pixel of the terrain layer
    public class DynamicLayerEntry
    {
        public Vector3I Position { get; private set; }
        public Color32 Color { get; private set; }
        public DynamicLayer ParentLayer { get; private set; }

        public DynamicLayerEntry(Vector3I position, Color32 color, DynamicLayer parent)
        {
            this.Color = color;
            this.Position = position;
            this.ParentLayer = parent;
            InvalidateCurrentPosition();
        }

        public void Delete()
        {
            InvalidateCurrentPosition();
            this.ParentLayer = null;
        }
        /*
        public int CompareTo(DynamicLayerEntry other)
        {
            return this.Position.z.CompareTo(other.Position.z);
        }
        */

        public void MoveTo(Vector2I newPosition)
        {
            InvalidateCurrentPosition();
            this.Position = new Vector3I(newPosition.x, newPosition.y, this.Position.z);
            InvalidateCurrentPosition();
        }
        public void MoveTo(Vector3I newPosition)
        {
            InvalidateCurrentPosition();
            this.Position = newPosition;
            InvalidateCurrentPosition();
        }

        public void ChangeColor(Color32 newColor)
        {
            this.Color = newColor;
            InvalidateCurrentPosition();
        }

        private void InvalidateCurrentPosition()
        {
            if (this.ParentLayer == null)
            {
                Debug.LogError("Attempted to modify an already deleted DynamicLayerEntry");
                return;
            }
            this.ParentLayer.ChangeList.Add(this.Position.ToVector2I());
        }
    }
}
