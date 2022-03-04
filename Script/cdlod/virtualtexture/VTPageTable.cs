using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    /// ƽ������
    /// </summary>
    public Vector2Int m_RegionSize;

    /// <summary>
    /// Tile��� ������
    /// </summary>
    public int m_TilePadding;

    /// <summary>
    /// Tile�ߴ�
    /// </summary>
    public int m_TileSize;

    /// <summary>
    /// ����padding���ܳ���
    /// </summary>
    public int TileSizeWithPadding
    {
        get
        {
            return m_TileSize + m_TilePadding * 2;
        }
    }


    /// <summary>
    /// �����Ĳ�����node
    /// </summary>
    /// <param name="node"></param>
    public void ActiveNode(Node node)
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
