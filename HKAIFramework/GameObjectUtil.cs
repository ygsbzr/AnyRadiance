using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace HKAIFramework
{
    public static class GameObjectUtil
    {
        public static GameObject Child(this GameObject parent,string name)
        {
            return parent.transform.Find(name).gameObject;
        }
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if(go.GetComponent<T>() != null)
            {
                return go.GetComponent<T>();
            }
            else
            {
                return go.AddComponent<T>();
            }
        }
    }
}
