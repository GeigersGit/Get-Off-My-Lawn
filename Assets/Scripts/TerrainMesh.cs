using System;
using System.Linq;
using Assets.Scripts;
using MathUtil;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainMesh : MonoBehaviour 
{
	// Size in pixels of one tile and also of its texture
	public int TileSize = 128;

	// Dimensions of the entire mesh
    public int MeshWidth = 4096;
    public int MeshHeight = 4096;
    public Vector2I Origin { get { return new Vector2I(-MeshWidth / 2, -MeshHeight / 2); } }

    // Material to apply to tiles
    public Material Material;

    // Generated mesh tile container
    public TerrainMeshTiles Tiles;

    // Actual terrain data
    public World Terrain;
    
    public void Awake()
    {
        GenerateMesh();
    }

    public void Update()
    {
        Profiler.BeginSample("TerrainMesh.Update");
        foreach (var byTile in this.Terrain.EnumerateChangeList().Select(qe => qe).GroupBy(pix => this.Tiles.GetTileAtCoords(pix)))
        {
            if (byTile.Key != null)
                byTile.Key.Paint(byTile, this.Terrain);
        }
        this.Terrain.FlushChangeList();
        Profiler.EndSample();
    }

    public void InitializeWithTerrainTexture(Texture2D texture)
    {
        this.Terrain = new World(this.Origin, new Vector2I(MeshWidth, MeshHeight), this, texture);
        this.Tiles.InitializeWithTerrain(this.Terrain.TerrainLayer);
    }

    // Use this for initialization
	private void GenerateMesh () 
    {
        var meshData = new MeshComponents();
        this.Tiles = new TerrainMeshTiles(this.Origin, new Vector2I(MeshWidth, MeshHeight), TileSize, Material);

        var materials = new List<Material>();
	    foreach (var tile in this.Tiles.TileArray)
	    {
	        meshData.GenerateSquareAt(tile.FromX, tile.FromY, tile.SizeX);
            materials.Add(tile.Material);
	    }

	    var mesh = this.GetComponent<MeshFilter>().mesh;
        meshData.BuildMesh(mesh);
	    this.GetComponent<Renderer>().materials = materials.ToArray();
    }
}

public class TerrainMeshTiles
{
    public Vector2I Origin { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int TileSize { get; private set; }
    public readonly TerrainMeshTile[,] TileArray;

    // Creates a mesh of desired size composed of submeshes of size Width * Height so each can have its own texture
    public TerrainMeshTiles(Vector2I origin, Vector2I worldSize, int tileSize, Material material)
    {
        this.Width = (int)worldSize.x;
        this.Height = (int)worldSize.y;
        this.TileSize = tileSize;
        this.Origin = origin; 
        
        if ((int)worldSize.x % tileSize != 0 || (int)worldSize.y % tileSize != 0)
            Debug.LogError("MeshDimensions must be a multiple of TileSize");

        int tilesHorizontal = (int)worldSize.x / tileSize;
        int tilesVertical = (int)worldSize.y / tileSize;
        
        // Create tiles of size Width * Height to fill the requested area
        this.TileArray = new TerrainMeshTile[tilesHorizontal, tilesVertical];
        for (int x = 0; x < tilesHorizontal; ++x)
            for (int y = 0; y < tilesVertical; ++y)
            {
                this.TileArray[x, y] = new TerrainMeshTile(new Vector2I(origin.x + x * tileSize, origin.y + y * tileSize), new Vector2I(tileSize, tileSize), material);
            }
    }

    // Apply this texture to the entire mesh, splitting between tile boundaries
    public void InitializeWithTerrain(Assets.Scripts.TerrainLayer terrainLayer)
    {
        if (terrainLayer.Width != this.Width || terrainLayer.Height != this.Height)
            Debug.LogWarning(String.Format("World texture dimensions [{0}x{1}] do not match world mesh dimensions [{2}x{3}]", terrainLayer.Width, terrainLayer.Height, this.Width, this.Height));

        // Iterate over all tiles to distribute texture data
        foreach (var tile in TileArray)
        {
            // Harvest pixel data from world texture
            var tilePixels = new Color32[tile.SizeX*tile.SizeY];
            for (int i = 0; i < tilePixels.Length; ++i)
            {
                int x = tile.FromX + (i % tile.SizeX);
                int y = tile.FromY + (i / tile.SizeX);
                tilePixels[i] = terrainLayer.GetAt(new Vector2I(x, y)); 
            }

            // Create smaller tile texture from pixel data
            var tileTexture = new Texture2D(tile.SizeX, tile.SizeY, TextureFormat.RGBA32, false);
            tileTexture.wrapMode = TextureWrapMode.Clamp;
            tileTexture.SetPixels32(tilePixels);
            tileTexture.Apply(false);
            // Become its material
            if (tile.Material == null)
                Debug.LogWarning("Did not set material to terrain, cannot apply terrain texture");
            else
                tile.Material.mainTexture = tileTexture;
        }
    }

    public TerrainMeshTile GetTileAtCoords(Vector2I coord)
    {
        if (coord.x < this.Origin.x || coord.x > this.Origin.x + this.Width || coord.y < this.Origin.y || coord.y > this.Origin.y + this.Height)
            return null;
        return this.TileArray[(coord.x - this.Origin.x)/TileSize, (coord.y - this.Origin.y)/TileSize];
    }

    public void Update()
    {
        
    }

}


public class TerrainMeshTile
{
    public int FromX { get; private set; }
    public int FromY { get; private set; }
    public int ToX { get; private set; }
    public int ToY { get; private set; }

    public int SizeX { get { return ToX - FromX; } }
    public int SizeY { get { return ToY - FromY; } }

    public Material Material { get; set; }

    public TerrainMeshTile(Vector2I from, Vector2I size, Material material)
    {
        this.FromX = from.x;
        this.FromY = from.y;
        this.ToX = from.x + size.x;
        this.ToY = from.y + size.y;
        this.Material = new Material(material);
    }

    public void Paint(IEnumerable<Vector2I> modifiedPixels, World world)
    {
        foreach (var pixel in modifiedPixels)
        {
#if DEBUG
            if (pixel.x < this.FromX || pixel.x > this.ToX || pixel.y < this.FromY || pixel.y > this.ToY)
            {
                Debug.LogError("Painting outside tile bounds.");
                continue;
            }
#endif
            ((Texture2D)this.Material.mainTexture).SetPixel(pixel.x - this.FromX, pixel.y - this.FromY, world.GetPixelAt(pixel));
        }
        ((Texture2D)this.Material.mainTexture).Apply(false);
    }
}



// Items used for mesh construction
public class MeshComponents
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
        
    // List of submesh indices
    public List<List<int>> Indices = new List<List<int>>();

    public void GenerateSquareAt(float x, float y, float size)
    {
        int firstIndex = Vertices.Count;
        
        Vertices.Add(new Vector3(x, y + size, 0));
        Vertices.Add(new Vector3(x + size, y + size, 0));
        Vertices.Add(new Vector3(x + size, y, 0));
        Vertices.Add(new Vector3(x, y, 0));

        Indices.Add(new List<int>
                    {
                        firstIndex,
                        firstIndex + 1,
                        firstIndex + 3,
                        firstIndex + 1,
                        firstIndex + 2,
                        firstIndex + 3,
                    });

        Uvs.Add(new Vector2(0, 1));
        Uvs.Add(new Vector2(1, 1));
        Uvs.Add(new Vector2(1, 0));
        Uvs.Add(new Vector2(0, 0));
    }

    public void BuildMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = Vertices.ToArray();
        mesh.uv = Uvs.ToArray();
        mesh.subMeshCount = Indices.Count;
        int submesh = 0;
        foreach (var indexList in Indices)
        {
            mesh.SetTriangles(indexList.ToArray(), submesh++);
        }
        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void Clear()
    {
        this.Vertices.Clear();
        this.Uvs.Clear();
        this.Indices.Clear();
    }
}
    