using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ROOT
{
    [System.Serializable]
    struct ScoreData
    {
        public string name;
        public int time;
        public int current;
    }

    public class ScoreWriting : MonoBehaviour
    {
        private ScoreData score;

        // Start is called before the first frame update
        void Start()
        {
            JsonUtility.ToJson(score);
        }

        public void Save()
        {
            score.name = "AAA";
            score.time = 60;
            score.current = 1000;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
            bf.Serialize(file, score);
            file.Close();
        }

        public void Read()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            ScoreData save = (ScoreData)bf.Deserialize(file);
            file.Close();

            Debug.Log(save.name);
        }
    }
}