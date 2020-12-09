using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

public class DataUtility
{

    public static JsonData ReadData(string path)
    {
        path = Application.dataPath + path;

        if (!File.Exists(path))
            return null;

        string jsonString = File.ReadAllText(Application.dataPath + "/" + path);
        JsonData data = JsonMapper.ToObject(jsonString);

        return data;
    }

    void WriteData(object data, string path)
    {
        JsonData jsonData = JsonMapper.ToJson(data);
        File.WriteAllText(Application.dataPath + "/" + path, jsonData.ToString());
    }
}
