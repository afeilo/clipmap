using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClipMapTest2 : MonoBehaviour
{

    public Mesh tileMesh;
    public Mesh fillerMesh;
    public Mesh trimMesh;
    public Mesh emptyMesh;
    public Mesh crossMesh;
    public Mesh seamMesh;
    public Camera camera;
    const int TILE_RESOLUTION = 48;
    const int PATCH_VERT_RESOLUTION = TILE_RESOLUTION + 1;
    const int CLIPMAP_RESOLUTION = TILE_RESOLUTION* 4 + 1;
    const int CLIPMAP_VERT_RESOLUTION = CLIPMAP_RESOLUTION + 1;
    const int NUM_CLIPMAP_LEVELS = 5;

    public Material material;
    public Material material2;
    public TerrainData data;
    void Start()
    {
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


        // generate filler mesh
        {

            List<Vector3> vertices = new List<Vector3>(PATCH_VERT_RESOLUTION * 8);
            int offset = TILE_RESOLUTION;

            for (int i = 0; i < PATCH_VERT_RESOLUTION; i++)
            {
                vertices.Add(new Vector3(offset + i + 1, 0, 0));
                vertices.Add(new Vector3(offset + i + 1, 0, 1));
            }

            for (int i = 0; i < PATCH_VERT_RESOLUTION; i++)
            {
                vertices.Add(new Vector3(1, 0, offset + i + 1));
                vertices.Add(new Vector3(0, 0, offset + i + 1));
            }

            for (int i = 0; i < PATCH_VERT_RESOLUTION; i++)
            {
                vertices.Add(new Vector3(-(offset + i), 0, 1));
                vertices.Add(new Vector3(-(offset + i), 0, 0));
            }

            for (int i = 0; i < PATCH_VERT_RESOLUTION; i++)
            {
                vertices.Add(new Vector3(0, 0, -(offset + i)));
                vertices.Add(new Vector3(1, 0, -(offset + i)));
            }


            List<int> indices = new List<int>(TILE_RESOLUTION * 24);

            for (int i = 0; i < TILE_RESOLUTION * 4; i++)
            {
                // the arms shouldn't be connected to each other
                int arm = i / TILE_RESOLUTION;

                int bl = (arm + i) * 2 + 0;
                int br = (arm + i) * 2 + 1;
                int tl = (arm + i) * 2 + 2;
                int tr = (arm + i) * 2 + 3;

                if (arm % 2 == 0)
                {
                    indices.Add(br);
                    indices.Add(tr);
                    indices.Add(bl);
                    indices.Add(bl);
                    indices.Add(tr);
                    indices.Add(tl);
                }
                else
                {
                    indices.Add(br);
                    indices.Add(tl);
                    indices.Add(bl);
                    indices.Add(br);
                    indices.Add(tr);
                    indices.Add(tl);
                }
            }

            fillerMesh = new Mesh();
            fillerMesh.SetVertices(vertices);
            fillerMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }

        // generate trim mesh
        {

            List<Vector3> vertices = new List<Vector3>((CLIPMAP_VERT_RESOLUTION * 2 + 1) * 2);
            int n = 0;

            // vertical part of L
            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION + 1; i++)
            {
                vertices.Add(new Vector3(0, 0, CLIPMAP_VERT_RESOLUTION - i));
                vertices.Add(new Vector3(1, 0, CLIPMAP_VERT_RESOLUTION - i));
                n += 2;
            }

            int start_of_horizontal = n;

            // horizontal part of L
            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION; i++)
            {
                vertices.Add(new Vector3(i + 1, 0, 0));
                vertices.Add(new Vector3(i + 1, 0, 1));
            }

            var offset = new Vector3(0.5f * (CLIPMAP_VERT_RESOLUTION + 1), 0, 0.5f * (CLIPMAP_VERT_RESOLUTION + 1));

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] -= offset;
            }


            List<int> indices = new List<int>((CLIPMAP_VERT_RESOLUTION * 2 - 1) * 6);
            n = 0;

            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION; i++)
            {
                indices.Add((i + 0) * 2 + 1);
                indices.Add((i + 1) * 2 + 0);
                indices.Add((i + 0) * 2 + 0);

                indices.Add((i + 1) * 2 + 1);
                indices.Add((i + 1) * 2 + 0);
                indices.Add((i + 0) * 2 + 1);
            }

            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION - 1; i++)
            {
                indices.Add(start_of_horizontal + (i + 0) * 2 + 1);
                indices.Add(start_of_horizontal + (i + 1) * 2 + 0);
                indices.Add(start_of_horizontal + (i + 0) * 2 + 0);

                indices.Add(start_of_horizontal + (i + 1) * 2 + 1);
                indices.Add(start_of_horizontal + (i + 1) * 2 + 0);
                indices.Add(start_of_horizontal + (i + 0) * 2 + 1);
            }
            trimMesh = new Mesh();
            trimMesh.SetVertices(vertices);
            trimMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }


        // generate empty tile mesh
        {

            List<Vector3> vertices = new List<Vector3>(TILE_RESOLUTION * 4 + 1);
            for (int i = 0; i < TILE_RESOLUTION * 4 + 1; i++)
            {
                vertices.Add(Vector3.zero);
            }

            // vertices[ 0 ] is the centre of the fan
            vertices[0] = new Vector3(TILE_RESOLUTION / 2.0f, 0, TILE_RESOLUTION / 2.0f);

            for (int i = 0; i < TILE_RESOLUTION; i++)
            {
                vertices[i + 1 + 0 * TILE_RESOLUTION] = new Vector3(i, 0, 0);
                vertices[i + 1 + 1 * TILE_RESOLUTION] = new Vector3(TILE_RESOLUTION, 0, i);
                vertices[i + 1 + 2 * TILE_RESOLUTION] = new Vector3(TILE_RESOLUTION - i, 0, TILE_RESOLUTION);
                vertices[i + 1 + 3 * TILE_RESOLUTION] = new Vector3(0, 0, TILE_RESOLUTION - i);
            }

            List<int> indices = new List<int>(TILE_RESOLUTION * 12);

            for (int i = 0; i < TILE_RESOLUTION * 12; i++)
            {
                indices.Add(-1);
            }
            for (int i = 0; i < TILE_RESOLUTION * 4; i++)
            {
                indices[i * 3 + 0] = 0;
                indices[i * 3 + 1] = i + 2;
                indices[i * 3 + 2] = i + 1;
            }

            // make the last triangle wrap around
            indices[indices.Count - 2] = 1;

            emptyMesh = new Mesh();
            emptyMesh.SetVertices(vertices);
            emptyMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        }
        // generate cross mesh
        {

            List<Vector3> vertices = new List<Vector3>(PATCH_VERT_RESOLUTION * 8);
            int n = 0;

            // horizontal vertices
            for (int i = 0; i < PATCH_VERT_RESOLUTION * 2; i++)
            {
                vertices.Add(new Vector3(i - (TILE_RESOLUTION), 0, 0));
                vertices.Add(new Vector3(i - (TILE_RESOLUTION), 0, 1));
            }

            int start_of_vertical = PATCH_VERT_RESOLUTION * 4;

            // vertical vertices
            for (int i = 0; i < PATCH_VERT_RESOLUTION * 2; i++)
            {
                vertices.Add(new Vector3(0, 0, i - (TILE_RESOLUTION)));
                vertices.Add(new Vector3(1, 0, i - (TILE_RESOLUTION)));
            }


            List<int> indices = new List<int>(TILE_RESOLUTION * 24 + 6);
            n = 0;

            // horizontal indices
            for (int i = 0; i < TILE_RESOLUTION * 2 + 1; i++)
            {
                int bl = i * 2 + 0;
                int br = i * 2 + 1;
                int tl = i * 2 + 2;
                int tr = i * 2 + 3;

                indices.Add(br);
                indices.Add(tr);
                indices.Add(bl);
                indices.Add(bl);
                indices.Add(tr);
                indices.Add(tl);
            }

            // vertical indices
            for (int i = 0; i < TILE_RESOLUTION * 2 + 1; i++)
            {
                if (i == TILE_RESOLUTION)
                    continue;

                int bl = i * 2 + 0;
                int br = i * 2 + 1;
                int tl = i * 2 + 2;
                int tr = i * 2 + 3;

                indices.Add(start_of_vertical + br);
                indices.Add(start_of_vertical + bl);
                indices.Add(start_of_vertical + tr);
                indices.Add(start_of_vertical + bl);
                indices.Add(start_of_vertical + tl);
                indices.Add(start_of_vertical + tr);
            }
            crossMesh = new Mesh();
            crossMesh.SetVertices(vertices);
            crossMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }

        // generate seam mesh
        {

            List<Vector3> vertices = new List<Vector3>(CLIPMAP_VERT_RESOLUTION * 4);
            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION * 4; i++)
            {
                vertices.Add(Vector3.zero);
            }

            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION; i++)
            {
                vertices[CLIPMAP_VERT_RESOLUTION * 0 + i] = new Vector3(i, 0, 0);
                vertices[CLIPMAP_VERT_RESOLUTION * 1 + i] = new Vector3(CLIPMAP_VERT_RESOLUTION, 0, i);
                vertices[CLIPMAP_VERT_RESOLUTION * 2 + i] = new Vector3(CLIPMAP_VERT_RESOLUTION - i, 0, CLIPMAP_VERT_RESOLUTION);
                vertices[CLIPMAP_VERT_RESOLUTION * 3 + i] = new Vector3(0, 0, CLIPMAP_VERT_RESOLUTION - i);
            }

            List<int> indices = new List<int>(CLIPMAP_VERT_RESOLUTION * 6);
            int n = 0;

            for (int i = 0; i < CLIPMAP_VERT_RESOLUTION * 4; i += 2)
            {
                indices.Add(i + 1);
                indices.Add(i + 2);
                indices.Add(i);
            }

            // make the last triangle wrap around
            indices[indices.Count - 2] = 0;


            seamMesh = new Mesh();
            seamMesh.SetVertices(vertices);
            seamMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
    }

    int patch2d(int x, int y)
    {
        return y * PATCH_VERT_RESOLUTION + x;
    }

    Vector3 floorf(Vector3 p)
    {
        return new Vector3(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z));
    }

    Matrix4x4[] rotations = new Matrix4x4[] {
        Matrix4x4.Rotate(Quaternion.Euler(0, 0, 0)),
        Matrix4x4.Rotate(Quaternion.Euler(0, 270, 0)),
        Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0)),
        Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0)),
    };
    bool intervals_overlap(float a0, float a1, float b0, float b1)
    {
        return a0 <= b1 && b0 <= a1;
    }
    // Update is called once per frame
    void Update()
    {
        var t = Camera.main.transform.position;
        Vector3 position = new Vector3(t.x, 0, t.z);
        //draw cross
        {
            Vector3 snapped_pos = floorf(position);
            var transM = Matrix4x4.Translate(snapped_pos);
            var scaleM = Matrix4x4.Scale(Vector3.one);
            var rotM = rotations[0];
            Graphics.DrawMesh(crossMesh, transM * scaleM * rotM, material, 0);
            Graphics.DrawMesh(crossMesh, transM * scaleM * rotM, material2, 0);

            //render_state.set_uniform("model", rotation_uniforms[0]);
            //render_state.set_uniform("clipmap", renderer_uniforms(snapped_pos, 1.0f));
            //renderer_draw_mesh(clipmap.gpu.cross, render_state);
        }

        for (int l = 0; l < NUM_CLIPMAP_LEVELS; l++)
        {
            float scale = 1 << l;
            Vector3 snapped_pos = floorf(position / scale) * scale;

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
                    Vector3 tile_tl = base_pos + new Vector3(x, 0, y) * tile_scale + fill;

                    // draw a low poly tile if the tile is entirely outside the world
                    Vector3 tile_br = tile_tl + tile_size;
                    bool inside = true;
                    if (!intervals_overlap(tile_tl.x, tile_br.x, 0, 4096))
                        inside = false;
                    if (!intervals_overlap(tile_tl.y, tile_br.y, 0, 4096))
                        inside = false;

                    Mesh mesh = inside ? tileMesh : tileMesh;

                    var transM = Matrix4x4.Translate(tile_tl);
                    var scaleM = Matrix4x4.Scale(Vector3.one * scale);
                    var rotM = rotations[0];
                    Graphics.DrawMesh(mesh, transM * scaleM * rotM, material, 0);
                    Graphics.DrawMesh(mesh, transM * scaleM * rotM, material2, 0);
                }
            }

            // draw filler
            {

                var transM = Matrix4x4.Translate(snapped_pos);
                var scaleM = Matrix4x4.Scale(Vector3.one * scale);
                var rotM = rotations[0];
                Graphics.DrawMesh(fillerMesh, transM * scaleM * rotM, material, 0);
                Graphics.DrawMesh(fillerMesh, transM * scaleM * rotM, material2, 0);
                //render_state.set_uniform("model", rotation_uniforms[0]);
                //render_state.set_uniform("clipmap", renderer_uniforms(snapped_pos, scale));
                //renderer_draw_mesh(clipmap.gpu.filler, render_state);
            }

            if (l != NUM_CLIPMAP_LEVELS - 1)
            {
                float next_scale = scale * 2.0f;
                Vector3 next_snapped_pos = floorf(position / next_scale) * next_scale;

                // draw trim
                {
                    Vector3 tile_centre = snapped_pos + new Vector3(scale * 0.5f, 0, scale * 0.5f);

                    Vector3 d = position - next_snapped_pos;
                    int r = 0;
                    r |= d.x >= scale ? 0 : 1;
                    r |= d.z >= scale ? 0 : 2;

                    var transM = Matrix4x4.Translate(tile_centre);
                    var scaleM = Matrix4x4.Scale(Vector3.one * scale);
                    var rotM = rotations[r];
                    Graphics.DrawMesh(trimMesh, transM * scaleM * rotM, material, 0);
                    Graphics.DrawMesh(trimMesh, transM * scaleM * rotM, material2, 0);
                    //render_state.set_uniform("model", rotation_uniforms[r]);
                    //render_state.set_uniform("clipmap", renderer_uniforms(tile_centre, scale));
                    //renderer_draw_mesh(clipmap.gpu.trim, render_state);
                }

                // draw seam
                {
                    Vector3 next_base = next_snapped_pos - new Vector3(TILE_RESOLUTION << (l + 1), 0, TILE_RESOLUTION << (l + 1));

                    var transM = Matrix4x4.Translate(next_base);
                    var scaleM = Matrix4x4.Scale(Vector3.one * scale);
                    var rotM = rotations[0];
                    Graphics.DrawMesh(seamMesh, transM * scaleM * rotM, material, 0);
                    Graphics.DrawMesh(seamMesh, transM * scaleM * rotM, material2, 0);
                    //render_state.set_uniform("model", rotation_uniforms[0]);
                    //render_state.set_uniform("clipmap", renderer_uniforms(next_base, scale));
                    //renderer_draw_mesh(clipmap.gpu.seam, render_state);
                }
            }
        }

    }
}
