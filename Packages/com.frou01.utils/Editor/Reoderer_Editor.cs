using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Reorderer), false), CanEditMultipleObjects]
public class railModelTiler_Editor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        Reorderer reorderer = (Reorderer)target;
        if (reorderer == null) return;
        DrawDefaultInspector();
        if (GUILayout.Button("Perform"))
        {
            ArrayList sorting = new ArrayList();
            ArrayList sorted = new ArrayList();
            foreach (Reorderer n in targets.OfType<Reorderer>())
            {
                sorting.Add(n);
            }
            int selectedNum = 0;
            int currentSiblingIndex = 0;
            while (sorting.Count > 0)
            {
                selectedNum++;
                if (selectedNum > 1000) return;
                Reorderer target = null;
                foreach (Reorderer n in sorting)
                {
                    if (n.index <= currentSiblingIndex) target = n;
                }
                if (target == null)
                {
                    currentSiblingIndex += 1;
                    continue;
                }
                Debug.Log(currentSiblingIndex);
                sorted.Add(target);
                sorting.Remove(target);
            }

            foreach (Reorderer n in sorted)
            {
                Debug.Log(n.index);
                n.transform.parent.GetChild(n.index).SetSiblingIndex(n.transform.GetSiblingIndex());
                n.transform.SetSiblingIndex(n.index);
            }
        }
        if (GUILayout.Button("Get"))
        {
            foreach (Reorderer n in targets.OfType<Reorderer>())
            {
                n.index = n.transform.GetSiblingIndex();
            }
        }
    }
}
