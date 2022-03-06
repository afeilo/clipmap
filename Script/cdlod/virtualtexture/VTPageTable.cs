using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using VirtualTexture;
/// <summary>
///  ���裺
///  1��Active Node
///  2������PageTable��λ�ã�LRUCache��
///  3������LodͼƬ
///  4���決��PageTable��
///  5������LookUp��
/// </summary>
public class VTPageTable : MonoBehaviour
{
    /// <summary>
    /// ƽ������(x*x)
    /// </summary>
    public int m_RegionSize;

    /// <summary>
    /// Tile��� ������
    /// </summary>
    public int m_TilePadding;

    /// <summary>
    /// Tile�ߴ�
    /// </summary>
    public int m_TileSize;

    public Shader m_DrawTextureShader;
    /// <summary>
    /// ����padding���ܳ���
    /// </summary>
    public int TileSizeWithPadding
    {
        get
        {
            return m_TileSize + m_TilePadding;
        }
    }

    /// <summary>
    /// ҳ�泤��
    /// </summary>
    public int PageSize
    {
        get
        {
            return m_RegionSize * TileSizeWithPadding;
        }
    }

    /// <summary>
    /// �ϲ�texture�Ĳ���
    /// </summary>
    private Material m_DrawTextureMateral;
    /// <summary>
    /// ��ǰ�����Page
    /// </summary>
    private LruCache lruCache;
    /// <summary>
    /// ƽ����ͼ����
    /// </summary>
    public RenderTexture m_TileTexture;
    private static Mesh s_FullscreenMesh;
    public static Mesh fullscreenMesh
    {
        get
        {
            if (s_FullscreenMesh != null)
                return s_FullscreenMesh;

            float topV = 1.0f;
            float bottomV = 0.0f;

            s_FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
            s_FullscreenMesh.SetVertices(new List<Vector3>
                {
                    new Vector3(-1.0f, -1.0f, 0.0f),
                    new Vector3(-1.0f,  1.0f, 0.0f),
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector3(1.0f,  1.0f, 0.0f)
                });

            s_FullscreenMesh.SetUVs(0, new List<Vector2>
                {
                    new Vector2(0.0f, bottomV),
                    new Vector2(0.0f, topV),
                    new Vector2(1.0f, bottomV),
                    new Vector2(1.0f, topV)
                });

            s_FullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
            s_FullscreenMesh.UploadMeshData(true);
            return s_FullscreenMesh;
        }
    }
    void Awake()
    {
        lruCache = new LruCache();
        lruCache.Init(m_RegionSize);

        m_TileTexture = new RenderTexture(PageSize, PageSize, 0);
        m_TileTexture.useMipMap = false;
        m_TileTexture.wrapMode = TextureWrapMode.Clamp;
        Shader.SetGlobalTexture("_VTTiledTex", m_TileTexture);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// �����Ĳ�����node
    /// </summary>
    /// <param name="node"></param>
    async public void ActiveNode(Node node)
    {
        while (node.path == null)
        {
            node = node.parent;
        }
        int hashCode = node.GetHashCode();
        var lruNode = lruCache.GetNode(hashCode);
        if (null != lruNode)
        {
            lruCache.SetActive(hashCode);
            return;
        }
        lruNode = lruCache.SetActive(hashCode);
        //���ض�ӦNode;
        var handle = Addressables.LoadAssetAsync<Texture2D>(node.path);
        await handle.Task;
        //�決��ӦNode��
        var texture2d = handle.Result;
        lruNode = lruCache.GetNode(hashCode);
        if (null == lruNode)
        {
            return;
        }
        DrawTexture(texture2d, m_TileTexture, new RectInt(lruNode.x * TileSizeWithPadding, lruNode.y * TileSizeWithPadding, TileSizeWithPadding, TileSizeWithPadding));
        Addressables.Release(handle);
    }

    public void GetNodeOffsetScale(Node node, out Vector4 nodeOffsetScale, out Vector4 pageOffsetScale)
    {
        nodeOffsetScale = new Vector4(0, 0, 1, 1);
        int hashCode = node.GetHashCode();
        var lruNode = lruCache.GetNode(hashCode);
        while (lruNode == null)
        {
            nodeOffsetScale.x = node.offset.x + nodeOffsetScale.x * 0.5f;
            nodeOffsetScale.y = node.offset.y + nodeOffsetScale.y * 0.5f;
            nodeOffsetScale.z = nodeOffsetScale.z * 0.5f;
            nodeOffsetScale.w = nodeOffsetScale.w * 0.5f;
            node = node.parent;
            hashCode = node.GetHashCode();
            lruNode = lruCache.GetNode(hashCode);
        }
        var scale = 1.0f / m_RegionSize;
        var uvOffset = scale * m_TilePadding / (TileSizeWithPadding * 2.0f);
        pageOffsetScale = new Vector4(lruNode.x * scale + uvOffset, lruNode.y * scale + uvOffset, (float)m_TileSize / PageSize, (float)m_TileSize / PageSize);
    }

    private void DrawTexture(Texture source, RenderTexture target, RectInt position)
    {
        if (source == null || target == null || m_DrawTextureShader == null)
            return;

        // ��ʼ�����Ʋ���
        if (m_DrawTextureMateral == null)
            m_DrawTextureMateral = new Material(m_DrawTextureShader);

        // �����任����
        float l = position.x * 2.0f / target.width - 1;
        float r = (position.x + position.width) * 2.0f / target.width - 1;
        float b = position.y * 2.0f / target.height - 1;
        float t = (position.y + position.height) * 2.0f / target.height - 1;

        var mat = new Matrix4x4();
        mat.m00 = r - l;
        mat.m03 = l;
        mat.m11 = t - b;
        mat.m13 = b;
        mat.m23 = -1;
        mat.m33 = 1;

        // ������ͼ
        m_DrawTextureMateral.SetMatrix(Shader.PropertyToID("_ImageMVP"), GL.GetGPUProjectionMatrix(mat, true));

        target.DiscardContents();
        Graphics.Blit(source, target, m_DrawTextureMateral);



        //// ������ͼ
        //m_DrawTextureMateral.SetMatrix(Shader.PropertyToID("_ImageMVP"), GL.GetGPUProjectionMatrix(mat, true));
        //m_DrawTextureMateral.SetTexture("_MainTex", source);
        //var tempCB = CommandBufferPool.Get("VTDraw");
        //tempCB.SetRenderTarget(target, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        //tempCB.DrawMesh(fullscreenMesh, Matrix4x4.identity, m_DrawTextureMateral, 0, 0);
        //Graphics.ExecuteCommandBuffer(tempCB);//DEBUG
        //CommandBufferPool.Release(tempCB);
    }

}
