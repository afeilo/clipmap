using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteAlways]
public class ClipMapTest : MonoBehaviour
{
    public TerrainData data;
    public Mesh mesh;
    public Material material;
    public Material material2;
    public Camera camera;

    const int TILE_RESOLUTION = 32;
    const int PATCH_VERT_RESOLUTION = TILE_RESOLUTION + 1;
    const int CLIPMAP_RESOLUTION = TILE_RESOLUTION * 4 + 1;
    const int CLIPMAP_VERT_RESOLUTION = CLIPMAP_RESOLUTION + 1;
    const int NUM_CLIPMAP_LEVELS = 5;
    public Mesh tileMesh;

    List<Matrix4x4> matrices = new List<Matrix4x4>();
    RenderTexture highTexture;
    // Start is called before the first frame update
    void Start()
    {
        highTexture = data.heightmapTexture;
        var width = data.heightmapResolution;
        var height = data.heightmapResolution;
        for (int i = 0; i < 4; i++)
        {
            var layer = data.terrainLayers[i];
            material.SetTexture("_Splat" + i, layer.diffuseTexture);
            material.SetTextureOffset("_Splat" + i, layer.tileOffset);
            material.SetTextureScale("_Splat" + i, layer.tileSize);
            material.SetTexture("_Normal" + i, layer.normalMapTexture);
            material.SetTexture("_Mask" + i, layer.maskMapTexture);
            
        }
        material.EnableKeyword("_NORMALMAP");
        material.EnableKeyword("_MASKMAP");
        material.SetVector("_LayerHasMask", new Vector4(1, 1, 1, 1));
        Vector4 v = Vector4.zero;
        for (int i = 0; i < 4; i++)
        {
            if (data.terrainLayers.Length <= i + 4)
                break;
            var layer = data.terrainLayers[i + 4];
            material2.SetTexture("_Splat" + i, layer.diffuseTexture);
            material2.SetTextureOffset("_Splat" + i, layer.tileOffset);
            material2.SetTextureScale("_Splat" + i, layer.tileSize);
            material2.SetTexture("_Normal" + i, layer.normalMapTexture);
            material2.SetTexture("_Mask" + i, layer.maskMapTexture);
            v[i] = 1;
        }
        material2.SetVector("_LayerHasMask", v);
        material2.EnableKeyword("_NORMALMAP");
        material2.EnableKeyword("_MASKMAP");
        material2.EnableKeyword("TERRAIN_SPLAT_ADDPASS");
    }

    private void OnEnable()
    {
        // generate tile mesh
        {
            List<Vector3> vertices = new List<Vector3>();
            int n = 0;
            for (int y = 0; y < PATCH_VERT_RESOLUTION; y++)
            {
                for (int x = 0; x < PATCH_VERT_RESOLUTION; x++)
                {
                    n++;
                    vertices.Add(new Vector3(x, 0, y));
                }
            }
            List<int> indices = new List<int>(TILE_RESOLUTION * TILE_RESOLUTION * 6);
            n = 0;
            for (int y = 0; y < TILE_RESOLUTION; y++)
            {
                for (int x = 0; x < TILE_RESOLUTION; x++)
                {
                    indices.Add(patch2d(x, y));
                    indices.Add(patch2d(x, y + 1));
                    indices.Add(patch2d(x + 1, y + 1));
                    indices.Add(patch2d(x, y));
                    indices.Add(patch2d(x + 1, y + 1));
                    indices.Add(patch2d(x + 1, y));
                }
            }
            tileMesh = new Mesh();
            tileMesh.SetVertices(vertices);
            tileMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
    }

    int patch2d(int x, int y)
    {
        return y * PATCH_VERT_RESOLUTION + x;
    }

    // Update is called once per frame
    void Update()
    {



    MaterialPropertyBlock block = new MaterialPropertyBlock();
    List<Vector4> dims = new List<Vector4>();
        matrices.Clear();
        var t = Camera.main.transform.position;
        //Vector3 position = new Vector3(t.x, 0, t.z);


        float Gap = 16;
        Vector3 position = new Vector3(Mathf.FloorToInt(t.x / Gap) * Gap, 0, Mathf.FloorToInt(t.z / Gap) * Gap);

        for (int l = 0; l < NUM_CLIPMAP_LEVELS; l++)
        {
            float scale = 1 << l;
            Vector3 snapped_pos = position;

            // draw tiles

            var tile_scale = TILE_RESOLUTION << l;
            Vector3 tile_size = Vector3.one * tile_scale;
            Vector3 base_pos = snapped_pos - new Vector3(TILE_RESOLUTION << (l + 1), 0, TILE_RESOLUTION << (l + 1));
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    // draw a 4x4 set of tiles. cut out the middle 2x2 unless we're at the finest level
                    if (l != 0 && (x == 1 || x == 2) && (y == 1 || y == 2))
                    {
                        continue;
                    }

                    Vector3 fill = new Vector3(x >= 2 ? 1 : 0, 0, y >= 2 ? 1 : 0) * scale;
                    Vector3 tile_tl = base_pos + new Vector3(x, 0, y) * tile_scale;// + fill;

                    // draw a low poly tile if the tile is entirely outside the world
                    Vector3 tile_br = tile_tl + tile_size;


                    var transM = Matrix4x4.Translate(tile_tl);
                    var scaleM = Matrix4x4.Scale(Vector3.one * scale);
                    matrices.Add(transM * scaleM);

                    dims.Add(new Vector4(l, scale, tile_scale, TILE_RESOLUTION));

                    //block.SetVector("CLIP_DIM", new Vector4(l, scale, tile_scale, TILE_RESOLUTION));
                    //material.SetVector("CLIP_DIM", new Vector4(l, scale, tile_scale, TILE_RESOLUTION));
                    //material2.SetVector("CLIP_DIM", new Vector4(l, scale, tile_scale, TILE_RESOLUTION));
                    ////matrices.Add(transM * scaleM);
                    //Graphics.DrawMesh(mesh, transM * scaleM, material, 0);
                    //Graphics.DrawMesh(mesh, transM * scaleM, material2, 0);
                }
            }

        }

        block.SetVectorArray("CLIP_DIM", dims);
        Graphics.DrawMeshInstanced(tileMesh, 0, material, matrices, block);
        Graphics.DrawMeshInstanced(tileMesh, 0, material2, matrices, block);
        //var t = Camera.main.transform.position;
        //Vector3 cameraOffset = new Vector3(t.x, 0, t.z);



        ////float Gap = 12.8f * 2;
        ////var forward = camera.transform.forward.normalized;
        ////var x = camera.transform.position.y / forward.y;
        ////Vector3 cameraOffset = new Vector3(Mathf.FloorToInt((camera.transform.position.x - forward.x * x) / Gap) * Gap, 0, Mathf.FloorToInt((camera.transform.position.z - forward.z * x) / Gap) * Gap);
        //var cameraOffsetMatrix = Matrix4x4.Translate(cameraOffset);

        //matrices.Clear();
        //int size = 16;
        //float scale = 1.6f;

        //var scaleMatrix = Matrix4x4.Scale(Vector3.one * scale);

        //var offset = 8;
        /////�м��
        //var matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset)) * scaleMatrix;
        //matrices.Add(cameraOffsetMatrix * matrix);
        //matrix = Matrix4x4.Translate(new Vector3(offset, 0, -offset)) * scaleMatrix;
        //matrices.Add(cameraOffsetMatrix * matrix);
        //matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset)) * scaleMatrix;
        //matrices.Add(cameraOffsetMatrix * matrix);
        //matrix = Matrix4x4.Translate(new Vector3(-offset, 0, offset)) * scaleMatrix;
        //matrices.Add(cameraOffsetMatrix * matrix);

        ////var lt = new Vector2Int(-16, 16);
        ////var rt = new Vector2Int(16, 16);
        ////var lb = new Vector2Int(-16, 16);
        ////var rb = new Vector2Int(-16, 16);

        ////matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset + size / 2)) * scaleMatrix;
        ////matrices.Add(matrix);
        ////matrix = Matrix4x4.Translate(new Vector3(offset, 0, -offset - size / 2)) * scaleMatrix;
        ////matrices.Add(matrix);
        ////matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset - size / 2)) * scaleMatrix;
        ////matrices.Add(matrix);
        ////matrix = Matrix4x4.Translate(new Vector3(-offset, 0, offset + size / 2)) * scaleMatrix;
        ////matrices.Add(matrix);


        //size = 8;
        /////��� �����Ĳ� ->
        //for (int i = 0; i < 5; i++)
        //{
        //    //offset += size / 2;
        //    if (0 == i)
        //    {
        //        size = 16;
        //        offset += size;
        //    }
        //    else
        //    {
        //        offset += (size / 2 * 3);
        //        size *= 2;
        //        scale *= 2;
        //        scaleMatrix = Matrix4x4.Scale(Vector3.one * scale);
        //    }


        //    for (int j = 0; j < 3; j++)
        //    {
        //        matrix = Matrix4x4.Translate(new Vector3(-offset + j * size, 0, offset)) * scaleMatrix;
        //        matrices.Add(cameraOffsetMatrix * matrix);
        //    }
        //    for (int j = 0; j < 3; j++)
        //    {
        //        matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset - j * size)) * scaleMatrix;
        //        matrices.Add(cameraOffsetMatrix * matrix);
        //    }
        //    for (int j = 0; j < 3; j++)
        //    {
        //        matrix = Matrix4x4.Translate(new Vector3(offset - j * size, 0, -offset)) * scaleMatrix;
        //        matrices.Add(cameraOffsetMatrix * matrix);
        //    }
        //    for (int j = 0; j < 3; j++)
        //    {
        //        matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset + j * size)) * scaleMatrix;
        //        matrices.Add(cameraOffsetMatrix * matrix);
        //    }




        //}

        ////Graphics.DrawMeshInstanced(mesh, 0, material2, matrices);
        //Graphics.DrawMeshInstanced(mesh, 0, material, matrices);

    }

    [UnityEditor.MenuItem("Create/�����߶�ͼ")]
    static void ExportHeight()
    {
        for (int i = 0; i < 16; i++)
        {
            var go = GameObject.Find("heightfield_" + i);
            var terrain = go.GetComponent<Terrain>();
            var renderTexture = terrain.terrainData.heightmapTexture;
            int width = renderTexture.width;
            int height = renderTexture.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
            //terrain.normalmapTexture
            //Debug.Log(terrain.hei);
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var _normal = terrain.terrainData.GetInterpolatedNormal(w / (float)width, h / (float)height).normalized;
                    texture2D.SetPixel(w, h, new Color((_normal.x + 1) / 2.0f, (_normal.y + 1) / 2.0f, (_normal.z + 1) / 2.0f));
                }
            }
            var bytes = texture2D.EncodeToPNG();
            var file = File.Open("Assets/Clipmap/NormalMap/normal_" + i + ".png", FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();


            //var go = GameObject.Find("heightfield_" + i);
            //var terrain = go.GetComponent<Terrain>();
            //var renderTexture = terrain.terrainData.heightmapTexture;
            //int width = renderTexture.width;
            //int height = renderTexture.height;
            //Texture2D texture2D = new Texture2D(width, height, TextureFormat.RG16, false);

            ////Debug.Log(terrain.hei);
            //for (int w = 0; w < width; w++)
            //{
            //    for (int h = 0; h < height; h++)
            //    {
            //        var _h = terrain.terrainData.GetHeight(w, h) / 257f;

            //        texture2D.SetPixel(w, h, new Color(_h, _h, 0));
            //    }
            //}



            //var bytes = texture2D.EncodeToPNG();
            //var file = File.Open("Assets/HeightMap/height_" + i + ".png", FileMode.Create);
            //var binary = new BinaryWriter(file);
            //binary.Write(bytes);
            //file.Close();

            //for (int j = 0; j < 2; j++)
            //{
            //    bytes = terrain.terrainData.alphamapTextures[j].EncodeToPNG();
            //    file = File.Open("Assets/SplitMap" + j + "/split_" + i + ".png", FileMode.Create);
            //    binary = new BinaryWriter(file);
            //    binary.Write(bytes);
            //    file.Close();
            //}

        }
        UnityEditor.AssetDatabase.Refresh();
    }
}
