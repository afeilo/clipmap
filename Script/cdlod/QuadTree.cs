

using System;
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
    List<SelectNode> selectNodeList;
    
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
                var x = topNodeSize * i - mapSize / 2 + topNodeSize / 2;
                var y = topNodeSize * j - mapSize / 2 + topNodeSize / 2;
                var node = InitNode(x, y, config.startLevel, config.endLevel);
                topLevelNode.Add(node);
            }
        }
    }

    public List<SelectNode> Select(Vector3 cameraPosition)
    {
        if (null == selectNodeList)
            selectNodeList = new List<SelectNode>();
        else
            selectNodeList.Clear();

        for (int i = 0; i < topLevelNode.Count; i++)
        {
            var node = topLevelNode[i];
            if (!SelectNode(node, cameraPosition))
            {
                selectNodeList.Add(new SelectNode() {node = node , chooseBit = 0});
            }
        }
        return selectNodeList;
    }

    bool SelectNode(Node node, Vector3 cameraPosition)
    {
        //Debug.Log(node.CaclMinLerpValue(cameraPosition) + "-" + node.x + "-" + node.y + "-" + node.size);
        //TODO 视锥裁剪
        if (node.CaclMinLerpValue(cameraPosition)<= node.size * node.size * 4)
        {
            if (null == node.subTL)
            {
                selectNodeList.Add(new SelectNode() { node = node, chooseBit = 0 });
            }
            else
            {
                byte b = 0;
                if (SelectNode(node.subTL, cameraPosition))
                    b += 1;
                if (SelectNode(node.subTR, cameraPosition))
                    b += 2;
                if (SelectNode(node.subBL, cameraPosition))
                    b += 4;
                if (SelectNode(node.subBR, cameraPosition))
                    b += 8;
                if (15 != b)
                {
                    selectNodeList.Add(new SelectNode() { node = node , chooseBit = b});
                }
                //else
                //{
                //    if ((b & 1) == 0)
                //        selectNodeList.Add(node.subTL);
                //    if ((b & 2) == 0)
                //        selectNodeList.Add(node.subTR);
                //    if ((b & 4) == 0)
                //        selectNodeList.Add(node.subBL);
                //    if ((b & 8) == 0)
                //        selectNodeList.Add(node.subBR);
                //}
            }
            return true;
        }
        return false;
    }

    Node InitNode(int x, int y, int level, int endLevel)
    {
        if (level < endLevel)
            return null;
        Node node = new Node();
        var size = 1 << level;
        node.x = x;
        node.y = y;
        node.size = size;
        node.subTL = InitNode(x - size / 4, y + size / 4, level - 1, endLevel);
        node.subTR = InitNode(x + size / 4, y + size / 4, level - 1, endLevel);
        node.subBL = InitNode(x - size / 4, y - size / 4, level - 1, endLevel);
        node.subBR = InitNode(x + size / 4, y - size / 4, level - 1, endLevel);
        return node;
    }
}

public struct SelectNode
{
    public Node node;
    public byte chooseBit;
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
        float dist = 0.0f;

        if (pos.x < minx)
        {
            float d = pos.x - minx;
            dist += d * d;
        }
        else if (pos.x > maxx)
        {
            float d = pos.x - maxx;
            dist += d * d;
        }
        //y轴
        {
            float d = pos.y;
            dist += d * d;
        }

        if (pos.z < miny)
        {
            float d = pos.z - miny;
            dist += d * d;
        }
        else if (pos.z > maxy)
        {
            float d = pos.z - maxy;
            dist += d * d;
        }

        return dist;

        //int minx = x - size / 2;
        //int maxx = x + size / 2;
        //int miny = y - size / 2;
        //int maxy = y + size / 2;
        //if (pos.x >= minx && pos.x <= maxx)
        //{
        //    if (pos.z >= miny && pos.z <= miny)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        return Mathf.Min(Mathf.Abs(pos.z - miny), Mathf.Abs(pos.z - maxy));
        //    }
        //}
        //else if (pos.z >= miny && pos.z <= maxy)
        //{
        //    return Mathf.Min(Mathf.Abs(pos.x - minx), Mathf.Abs(pos.x - maxx));
        //}
        //else
        //{
        //    var p = new Vector2(pos.x, pos.z);
        //    var dtl = Vector2.Distance(p, new Vector2(minx, maxy));
        //    var dtr = Vector2.Distance(p, new Vector2(maxx, maxy));
        //    var dbl = Vector2.Distance(p, new Vector2(minx, miny));
        //    var dbr = Vector2.Distance(p, new Vector2(maxx, miny));
        //    return Mathf.Min(dtl, dtr, dbl, dbr);
        //    //return Mathf.Max(Math.Min(Mathf.Abs(pos.z - miny), Mathf.Abs(pos.z - maxy)), Math.Min(Mathf.Abs(pos.x - minx), Mathf.Abs(pos.x - maxx)));
        //}
    }
}

[Serializable]
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