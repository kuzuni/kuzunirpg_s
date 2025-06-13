using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Kumapet
{
    public class Kumapet_PanelControl : MonoBehaviour
    {
        public TextMeshProUGUI m_text;
        public Transform tf_parent;

        List<GameObject> list_go = new List<GameObject>();
        int m_Index = 0;

        private void Awake()
        {
            for (int i = 0; i < tf_parent.childCount - 1; i++)
                list_go.Add(tf_parent.GetChild(i).gameObject);
            Set(0);
        }

        public void Set(int arrow)
        {
            m_Index += arrow;
            if (m_Index >= list_go.Count)
                m_Index = 0;
            else if (m_Index < 0)
                m_Index = list_go.Count - 1;

            list_go.ForEach(f => f.SetActive(false));
            list_go[m_Index].SetActive(true);
            m_text.text = list_go[m_Index].name;
        }
    }
}