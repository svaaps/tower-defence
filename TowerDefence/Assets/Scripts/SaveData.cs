using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public static readonly string directoryPath = Application.persistentDataPath + "/saves";

    public static readonly string saveExtension = "save";

    public string name;

    public static SaveData[] ReadSaveData()
    {
        List<SaveData> dataList = new List<SaveData>();
        string[] filePaths = Directory.GetFiles(directoryPath + "/", "*." + saveExtension);
        foreach(string filePath in filePaths)
        {
            FileStream fileStream = File.OpenRead(filePath);

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                SaveData data = (SaveData)formatter.Deserialize(fileStream);
                dataList.Add(data);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to deserialize. Reason: " + e.Message);
            }
            catch (InvalidCastException e)
            {
                Debug.LogError("Failed to cast deserialized object." + e.Message);
            }
            finally
            {
                fileStream.Close();
            }
        }
        return dataList.ToArray();
    }
    
    public static bool ValidFileName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    public static bool Save(SaveData data, string name, bool overwrite)
    {
        if (data == null)
            return false;

        if (!ValidFileName(name))
            return false;

        string path = directoryPath + "/" + name + "." + saveExtension;
        FileStream file;
        if (File.Exists(path))
        {
            if (!overwrite)
                return false;
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(file, data);
        }
        catch (SerializationException e)
        {
            Debug.LogError("Failed to serialize. Reason: " + e.Message);
            return false;
        }
        finally
        {
            file.Close();
        }

        return true;
    }
}
