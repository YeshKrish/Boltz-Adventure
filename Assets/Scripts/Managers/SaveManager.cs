using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
public class SaveManager : MonoBehaviour
{
    [SerializeField]
    private LevelAndStar _levelAndStar;

    public static SaveManager Instance;

    [HideInInspector]
    public List<string> _loadedLines = new List<string>();

    Dictionary<int, int> dict = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }


    public void SaveJson()
    {
        using(StreamWriter file = new StreamWriter(Application.dataPath + "/levelAndStar.txt"))
        {
            foreach (int stars in _levelAndStar.LevelAndStarDict.Keys)
            {
                foreach(int level in _levelAndStar.LevelAndStarDict.Values)
                {
                    file.WriteLine(stars.ToString() + "\t" + level.ToString());
                }

            }
            file.Close();
        }
        //string json = JsonUtility.ToJson(_levelAndStar);
        //string filePath = Application.dataPath + "/levelAndStar.json";
        //File.WriteAllText(filePath, json);
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

}


