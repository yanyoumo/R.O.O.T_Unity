using System.Collections;
using System.Collections.Generic;
using ROOT.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT.UI
{
    public class LevelSelectionBSTMaster : MonoBehaviour
    {
        public Transform LevelSelectionPanel;
        public GameObject LevelQuadTemplate;
        
        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}