using TMPro;
using UnityEngine;

namespace ROOT
{
    [ExecuteInEditMode]
    public class TutorialGoalEntry : MonoBehaviour
    {
        public Material tick;
        public Material cross;
        public MeshRenderer CheckBox;
        public TextMeshPro content;
        private bool _completed;

        public bool Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                CheckBox.material = _completed ? tick : cross;
            }
        }

        public string Content
        {
            set => content.text = value;
        }
    }
}