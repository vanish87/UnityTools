using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityTools.ConfigureGUI;

namespace UnityTools.GUITool
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NoneGUIAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Field)]
    public class GUIDisplayNameAttribute : Attribute
    {
        internal string guiName;
        public GUIDisplayNameAttribute(string name)
        {
            this.guiName = name;
        }
    }
    public abstract class GUIContainer
    {
        public class VariableInfo
        {
            public FieldInfo fieldInfo;
            public object defaultValue;
            public object lastValidValue;
            public string displayName;

        }
        private List<VariableInfo> variableList = new List<VariableInfo>();
        private Dictionary<string, string> unParsedString = new Dictionary<string, string>();
        private string classHashString;

        public GUIContainer()
        {
            var bindingFlags = BindingFlags.Instance  |
                               BindingFlags.NonPublic |
                               BindingFlags.Public;

            var variableList = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field => !Attribute.IsDefined(field, typeof(NoneGUIAttribute)));

            foreach(var v in variableList)
            {
                var name = v.Name;
                if (Attribute.IsDefined(v, typeof(GUIDisplayNameAttribute)))
                {
                    var attrib = Attribute.GetCustomAttribute(v, typeof(GUIDisplayNameAttribute)) as GUIDisplayNameAttribute;
                    name = attrib.guiName;
                }
                this.variableList.Add(new VariableInfo()
                {
                    fieldInfo = v,
                    defaultValue = v.GetValue(this),
                    lastValidValue = v.GetValue(this),
                    displayName = name
                });
            }

            this.classHashString = Environment.StackTrace;
        }

        public void ResetToDefault()
        {
            foreach(var v in this.variableList)
            {
                v.fieldInfo.SetValue(this, v.defaultValue);
            }
        }
        

        public virtual void OnGUI()
        {
            this.OnGUIInternal();

            // foreach (var v in this.variableList)
            // {
            //     var method = this.GetType().BaseType.GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Instance);
            //     var typeFunc = method.MakeGenericMethod(v.fieldInfo.FieldType);
            //     typeFunc.Invoke(this, new object[1] { v });

            //     // if (v.fieldInfo.FieldType == typeof(int)) this.HandleFieldValue<int>(v.fieldInfo, v.displayName);
            //     // else if (v.fieldInfo.FieldType == typeof(int)) this.HandleFieldValue<int>(v.fieldInfo, v.displayName);
            //     // else if (v.fieldInfo.FieldType == typeof(int)) this.HandleFieldValue<int>(v.fieldInfo, v.displayName);
            //     // if (v.fieldInfo.FieldType == typeof(int)) this.HandleFieldValue<int>(v.fieldInfo, v.displayName);
            //     // if (v.fieldInfo.FieldType == typeof(int)) this.HandleFieldValue<int>(v.fieldInfo, v.displayName);
            // }


        }
        private void HandleBool(VariableInfo v)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            var toggle = (bool)v.fieldInfo.GetValue(this);
            using (var h = new GUILayout.HorizontalScope())
            {
                toggle = GUILayout.Toggle(toggle, v.displayName);
            }
            v.fieldInfo.SetValue(this, toggle);
        }
        
        private void HandleFieldValue<T>(VariableInfo variable)
        {
            var t = variable.fieldInfo.FieldType;
            if (t == typeof(bool))
            {
                this.HandleBool(variable);
            }
            else
            if (t == typeof(Vector2))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector2)variable.fieldInfo.GetValue(this);
                    var lv = (Vector2)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    variable.fieldInfo.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            if (t == typeof(Vector3))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector3)variable.fieldInfo.GetValue(this);
                    var lv = (Vector3)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    this.OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z);
                    variable.fieldInfo.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            if (t == typeof(Vector4))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector4)variable.fieldInfo.GetValue(this);
                    var lv = (Vector4)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    this.OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z);
                    this.OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w);
                    variable.fieldInfo.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            {
                var v = (T)variable.fieldInfo.GetValue(this);
                var lv = (T)variable.lastValidValue;
                this.OnFieldGUI<T>(ref v, variable.displayName, ref lv);
                variable.fieldInfo.SetValue(this, v);
                variable.lastValidValue = lv;
            }
        }

        private void OnFieldGUI<T>(ref T v, string displayName, ref T lastValidValue)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName);
                var hash = displayName;
                var target = v.ToString();
                var hasUnparsedStr = this.unParsedString.ContainsKey(hash);
                if (hasUnparsedStr)
                {
                    target = this.unParsedString[hash];
                }
                var color = hasUnparsedStr ? Color.red : GUI.color;

                using (var cs = new ColorScope(color))
                {
                    var ret = GUILayout.TextField(target, op);

                    var newValue = default(T);
                    var canParse = false;
                    try
                    {
                        newValue = (T)Convert.ChangeType(ret, typeof(T));
                        canParse = newValue.ToString() == ret;
                    }
                    catch (Exception) { }

                    if (canParse)
                    {
                        v = newValue;
                        lastValidValue = newValue;
                        if (hasUnparsedStr) this.unParsedString.Remove(hash);
                    }
                    else
                    {
                        this.unParsedString[hash] = ret;
                    }
                    if (hasUnparsedStr && GUILayout.Button("Reset"))
                    {
                        v = lastValidValue;
                        this.unParsedString.Remove(hash);
                    }
                }
            }
        }

        private void OnGUIInternal()
        {
            foreach (var v in this.variableList)
            {
                var method = this.GetType().BaseType.GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Instance);
                var typeFunc = method.MakeGenericMethod(v.fieldInfo.FieldType);
                typeFunc.Invoke(this, new object[1] { v });
            }
        }
        
    }

}