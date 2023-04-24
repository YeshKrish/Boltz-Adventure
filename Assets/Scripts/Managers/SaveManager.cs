using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    Dictionary<int, int> dict = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }


    public void SaveJson(int star, int level)
    {
        string readFromFilePath = Application.persistentDataPath + "/levelAndStar.txt";
        string content = level + "\t" + star + "\n"; // add a newline character to separate from previous content
        File.AppendAllText(readFromFilePath, content);
    }

    public Dictionary<int, int> LoadJson()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        string filePath = Application.persistentDataPath + "/levelAndStar.txt";
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('\t'); // Replace '\t' with your delimiter character
                int key = int.Parse(parts[0]);
                int value = int.Parse(parts[1]);
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
                else
                {
                    if (value > dict[key])
                    {
                        dict[key] = value;
                    }
                }
            }

            return dict;
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }
    }

    public void OverrideJson(int key, int star)
    {
        string readFromFilePath = Application.persistentDataPath + "/levelAndStar.txt";
        string content = key + "\t" + star + "\n"; // add a newline character to separate from previous content
        File.AppendAllText(readFromFilePath, content);

    }
}


