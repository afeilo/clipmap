using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///  步骤：
///  1、Active Node
///  2、分配PageTable的位置（LRUCache）
///  3、加载Lod图片
///  4、烘焙到PageTable上
///  5、更新LookUp表
/// </summary>
public class VTPageTable : MonoBehaviour
{
    /// <summary>
    /// 平铺数量
    /// </summary>
    public Vector2Int m_RegionSize;

    /// <summary>
    /// Tile间距 单个的
    /// </summary>
    public int m_TilePadding;

    /// <summary>
    /// Tile尺寸
    /// </summary>
    public int m_TileSize;

    /// <summary>
    /// 加上padding的总长度
    /// </summary>
    public int TileSizeWithPadding
    {
        get
        {
            return m_TileSize + m_TilePadding * 2;
        }
    }


    /// <summary>
    /// 激活四叉树的node
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
