using System;
using UnityEngine;

namespace UniFramework.Runtime
{
    [Serializable]
    public sealed class UIGroupData
    {
        [SerializeField]
        private string m_Name = null;

        [SerializeField]
        private int m_Depth = 0;

        public UIGroupData(string name, int depth)
        {
            m_Name = name;
            m_Depth = depth;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public int Depth
        {
            get
            {
                return m_Depth;
            }
        }
    }
}