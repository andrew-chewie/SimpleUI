using System;
using UnityEngine;

namespace SimpleUI
{
    [Serializable]
    public class FilterData
    {
        public bool Active = false;
        public string Name;

        [HideInInspector]
        public long ID;


        public FilterData(long id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}