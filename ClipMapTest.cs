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
    public Transform target;
    public float distance;

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

    // Update is called once per frame
    void Update()
    {
        
        float Gap = 12.8f * 2;
        var forward = camera.transform.forward.normalized;
        camera.transform.position = target.transform.position - forward * distance;
        Vector3 cameraOffset = new Vector3(Mathf.FloorToInt(target.transform.position.x / Gap) * Gap, 0, Mathf.FloorToInt(target.transform.position.z / Gap) * Gap);
        var cameraOffsetMatrix = Matrix4x4.Translate(cameraOffset);

        matrices.Clear();
        int size = 16;
        float scale = 1.6f;

        var scaleMatrix = Matrix4x4.Scale(Vector3.one * scale);

        var offset = 8;
        ///�м��
        var matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset)) * scaleMatrix;
        matrices.Add(cameraOffsetMatrix * matrix);
        matrix = Matrix4x4.Translate(new Vector3(offset, 0, -offset)) * scaleMatrix;
        matrices.Add(cameraOffsetMatrix * matrix);
        matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset)) * scaleMatrix;
        matrices.Add(cameraOffsetMatrix * matrix);
        matrix = Matrix4x4.Translate(new Vector3(-offset, 0, offset)) * scaleMatrix;
        matrices.Add(cameraOffsetMatrix * matrix);

        //var lt = new Vector2Int(-16, 16);
        //var rt = new Vector2Int(16, 16);
        //var lb = new Vector2Int(-16, 16);
        //var rb = new Vector2Int(-16, 16);

        //matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset + size / 2)) * scaleMatrix;
        //matrices.Add(matrix);
        //matrix = Matrix4x4.Translate(new Vector3(offset, 0, -offset - size / 2)) * scaleMatrix;
        //matrices.Add(matrix);
        //matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset - size / 2)) * scaleMatrix;
        //matrices.Add(matrix);
        //matrix = Matrix4x4.Translate(new Vector3(-offset, 0, offset + size / 2)) * scaleMatrix;
        //matrices.Add(matrix);


        size = 8;
        ///��� �����Ĳ� ->
        for (int i = 0; i < 5; i++)
        {
            //offset += size / 2;
            if (0 == i)
            {
                size = 16;
                offset += size;
            }
            else
            {
                offset += (size / 2 * 3);
                size *= 2;
                scale *= 2;
                scaleMatrix = Matrix4x4.Scale(Vector3.one * scale);
            }
            

            for (int j = 0; j < 3; j++)
            {
                matrix = Matrix4x4.Translate(new Vector3(-offset + j * size, 0, offset)) * scaleMatrix;
                matrices.Add(cameraOffsetMatrix * matrix);
            }
            for (int j = 0; j < 3; j++)
            {
                matrix = Matrix4x4.Translate(new Vector3(offset, 0, offset - j * size)) * scaleMatrix;
                matrices.Add(cameraOffsetMatrix * matrix);
            }
            for (int j = 0; j < 3; j++)
            {
                matrix = Matrix4x4.Translate(new Vector3(offset - j * size, 0, -offset)) * scaleMatrix;
                matrices.Add(cameraOffsetMatrix * matrix);
            }
            for (int j = 0; j < 3; j++)
            {
                matrix = Matrix4x4.Translate(new Vector3(-offset, 0, -offset + j * size)) * scaleMatrix;
                matrices.Add(cameraOffsetMatrix * matrix);
            }

            
            
            
        }

        Graphics.DrawMeshInstanced(mesh, 0, material2, matrices);
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
        
    }

    [UnityEditor.MenuItem("Create/�����߶�ͼ")]
    static void ExportHeight()
    {
        for (int i = 0; i < 16; i++)
        {
            //var go = GameObject.Find("heightfield_" + i);
            //var terrain = go.GetComponent<Terrain>();
            //var renderTexture = terrain.terrainData.heightmapTexture;
            //int width = renderTexture.width;
            //int height = renderTexture.height;
            //Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
            ////terrain.normalmapTexture
            ////Debug.Log(terrain.hei);
            //for (int w = 0; w < width; w++)
            //{
            //    for (int h = 0; h < height; h++)
            //    {
            //        var _normal = terrain.terrainData.GetInterpolatedNormal(w / (float)width, h / (float)height).normalized;
            //        texture2D.SetPixel(w, h, new Color((_normal.x + 1) / 2.0f, (_normal.y + 1) / 2.0f, (_normal.z + 1) / 2.0f));
            //    }
            //}
            //var bytes = texture2D.EncodeToPNG();
            //var file = File.Open("Assets/Clipmap/NormalMap/normal_" + i + ".png", FileMode.Create);
            //var binary = new BinaryWriter(file);
            //binary.Write(bytes);
            //file.Close();


            var go = GameObject.Find("heightfield_" + i);
            var terrain = go.GetComponent<Terrain>();
            var renderTexture = terrain.terrainData.heightmapTexture;
            int width = renderTexture.width;
            int height = renderTexture.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.RG16, false);

            //Debug.Log(terrain.hei);
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var _h = terrain.terrainData.GetHeight(w, h) / 257f;
                        Debug.Log(terrain.terrainData.GetHeight(w, h));
                    texture2D.SetPixel(w, h, new Color(_h, _h, 0));
                }
            }

            return;

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
