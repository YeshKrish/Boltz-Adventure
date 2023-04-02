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
        string readFromFilePath = Application.dataPath + "/levelAndStar.txt";
        string content = level + "\t" + star + "\n"; // add a newline character to separate from previous content
        File.AppendAllText(readFromFilePath, content);
    }

    public Dictionary<int, int> LoadJson()
    {
        if(File.Exists(Application.dataPath + "/levelAndStar.txt"))
        {
            string readFromFilePath = Application.dataPath + "/levelAndStar.txt";
            StreamReader reader = new StreamReader(readFromFilePath);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t'); // Replace '\t' with your delimiter character
                int key = int.Parse(parts[0]);
                int value = int.Parse(parts[1]);
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
                else
                {
                    if(value > dict[key])
                    {
                        dict[key] = value;
                    }
                }
            }

            reader.Close();
            return dict;
        }
        return null;
    }

    public void OverrideJson(int key, int star)
    {
        string readFromFilePath = Application.dataPath + "/levelAndStar.txt";
        string content = key + "\t" + star + "\n"; // add a newline character to separate from previous content
        File.AppendAllText(readFromFilePath, content);

    }
}


