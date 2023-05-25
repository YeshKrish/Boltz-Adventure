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

    public void OverrideJson(int key, int starsCollected)
    {
        string readFromFilePath = Application.persistentDataPath + "/levelAndStar.txt";
        string[] lines = File.ReadAllLines(readFromFilePath);
        bool foundKey = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('\t');
            if (parts.Length == 2 && int.TryParse(parts[0], out int existingKey))
            {
                if (existingKey == key)
                {
                    foundKey = true;
                    if (int.TryParse(parts[1], out int existingStars))
                    {
                        if (starsCollected > existingStars)
                        {
                            lines[i] = key + "\t" + starsCollected.ToString();
                        }
                        break;
                    }
                }
            }
        }

        if (!foundKey)
        {
            string content = key + "\t" + starsCollected.ToString();
            File.AppendAllText(readFromFilePath, content + "\n");
        }
        else
        {
            File.WriteAllLines(readFromFilePath, lines);
        }
    }

    public int GetTotalStars(Dictionary<int, int> dict, int currentLevelCompleted)
    {
        int totalStars = 0;

        // Calculate the starting level to consider
        int startLevel = currentLevelCompleted - 5;
        startLevel = Mathf.Max(startLevel, 0); // Ensure startLevel is not less than 1

        // Iterate over the levels from startLevel to currentLevelCompleted
        for (int level = startLevel; level <= currentLevelCompleted; level++)
        {
            if (dict.ContainsKey(level))
            {
                totalStars += dict[level]; // Add the stars for the level to the total
            }
        }

        return totalStars;
    }
}


