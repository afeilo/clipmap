

using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 四叉树
/// </summary>
public class QuadTree 
{
    /// <summary>
    /// 头节点
    /// </summary>
    List<Node> topLevelNode;
    /// <summary>
    /// 视野选中节点
    /// </summary>
    List<Node> selectNodeList;
    
    public void Create(QuadTreeConfig config)
    {
        topLevelNode = new List<Node>();
        int length = (config.maxLevel - config.startLevel) << 1;
        int mapSize = 1 << config.maxLevel;
        int topNodeSize = 1 << config.startLevel;
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                var x = topNodeSize * i + mapSize / 2;
                var y = topNodeSize * j + mapSize / 2;
                var node = InitNode(x, y, config.startLevel, config.endLevel);
                topLevelNode.Add(node);
            }
        }
    }

    public List<Node> Select(Vector3 cameraPosition)
    {
        if (null == selectNodeList)
            selectNodeList = new List<Node>();
        else
            selectNodeList.Clear();

        return selectNodeList;
    }


    Node InitNode(int x, int y, int level, int endLevel)
    {
        if (level == endLevel)
            return null;
        Node node = new Node();
        var size = 1 << level;
        node.x = x;
        node.y = y;
        node.size = size;
        node.subTL = InitNode(x - size / 2, y + size / 2, level - 1, endLevel);
        node.subTR = InitNode(x + size / 2, y + size / 2, level - 1, endLevel);
        node.subBL = InitNode(x - size / 2, y - size / 2, level - 1, endLevel);
        node.subBR = InitNode(x + size / 2, y - size / 2, level - 1, endLevel);
        return node;
    }
}

public class Node
{
    /// <summary>
    /// 位置
    /// </summary>
    public int x, y;
    /// <summary>
    /// 格子尺寸
    /// </summary>
    public int size;
    /// <summary>
    /// 四个子节点
    /// </summary>
    public Node subTL;
    public Node subTR;
    public Node subBL;
    public Node subBR;

    public float CaclMinLerpValue(Vector3 pos)
    {
        int minx = x - size / 2;
        int maxx = x + size / 2;
        int miny = y - size / 2;
        int maxy = y + size / 2;
        if (pos.x >= minx && pos.x <= maxx)
        {
            if (pos.z >= miny && pos.z <= miny)
            {
                return 0;
            }
            else
            {
                return Mathf.Min(Mathf.Abs(pos.z - miny), Mathf.Abs(pos.z - maxy));
            }
        }
        else if (pos.z >= miny && pos.z <= miny)
        {
            return Mathf.Min(Mathf.Abs(pos.x - minx), Mathf.Abs(pos.x - maxx));
        }
        else
        {
            var p = new Vector2(pos.x, pos.z);
            var dtl = Vector2.Distance(p, new Vector2(x - size / 2, y + size / 2));
            var dtr = Vector2.Distance(p, new Vector2(x + size / 2, y + size / 2));
            var dbl = Vector2.Distance(p, new Vector2(x - size / 2, y - size / 2));
            var dbr = Vector2.Distance(p, new Vector2(x + size / 2, y - size / 2));
            return Mathf.Min(dtl, dtr, dbl, dbr);
        }
    }
}

public struct QuadTreeConfig
{
    /// <summary>
    /// 中心位置x,y
    /// </summary>
    public int x, y;
    public int maxLevel; // 12 = 4096
    public int startLevel; // 10 = 1024
    public int endLevel; // 5 = 32
}