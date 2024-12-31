using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;  
#endif
public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/GenerateMap")]
    public static void GenerateMap()
    {
        GenerateMapFunc("Assets/Resources/Map");
        GenerateMapFunc("../../../code/derekopP1Server/Common/Map");
    }
    public static void GenerateMapFunc(string path)
    {
        GameObject[] gos = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach(GameObject go in gos)
        {
            GameObject go2 = go.transform.Find("TileCollision").gameObject;
            Tilemap tileMap = go2.GetComponent<Tilemap>();

            using (var writer = File.CreateText(path+$"/{go.name}.txt"))
            {
                writer.WriteLine(tileMap.cellBounds.xMin);
                writer.WriteLine(tileMap.cellBounds.xMax-1);
                writer.WriteLine(tileMap.cellBounds.yMin+1);
                writer.WriteLine(tileMap.cellBounds.yMax);
                for(int y = tileMap.cellBounds.yMax;y>=tileMap.cellBounds.yMin+1;y--)
                {
                    for(int x=  tileMap.cellBounds.xMin;x<=tileMap.cellBounds.xMax-1;x++)
                    {
                        TileBase tile = tileMap.GetTile(new Vector3Int(x, y-1, 0));
                        if(tile == null)
                        {
                            writer.Write(0);
                        }
                        else
                        {
                            writer.Write(1);
                        }
                    }
                    writer.WriteLine();
                }
            }
        }
    }
    #endif
    
}
