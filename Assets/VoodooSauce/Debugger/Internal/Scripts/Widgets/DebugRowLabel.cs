using System;
using UnityEngine;
using UnityEngine.UI;

namespace Voodoo.Sauce.Debugger.Widgets
{
    public class DebugRowLabel : Widget
    {
        [SerializeField] private Text rowName;
        [SerializeField] private Text rowValuePrefab;
        [SerializeField] private Transform rowValuesParent;

        public void SetRowName(string text)
        {
            rowName.text = text;
        }

        public void AddRows(string[] texts)
        {
            foreach (string text in texts) {
                Text textObj = Instantiate(rowValuePrefab, rowValuesParent);
                textObj.text = text;   
            }
            
            rowValuePrefab.gameObject.SetActive(false);
        }
    }
}