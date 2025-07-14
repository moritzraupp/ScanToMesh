using UnityEngine;
using UnityEditor;

using stm;
using static UnityEditor.Progress;
using Unity.VisualScripting.ReorderableList.Internal;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;


public class PipelineWindow : EditorWindow
{
    public Pipeline pipeline = null; 

    private List<bool> foldouts = new List<bool>();

    class UnityTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Debug.Log(value);
        }

        public override void Write(string value)
        {
            Debug.Log(value);
        }
    }


    [MenuItem("Tools/ScanToMesh")]
    public static void ShowWindow()
    {
        GetWindow<PipelineWindow>("Scan To Mesh");
    }

    private void OnDisable()
    {
        if (pipeline != null)
        {
            pipeline.Dispose();
            pipeline = null;

            GC.Collect();
            GC.WaitForPendingFinalizers(); 
            GC.Collect();
        }
    }

    private void OnEnable()
    {
        Console.SetOut(new UnityTextWriter());
        Console.SetError(new UnityTextWriter());

        pipeline = new Pipeline(); 
    }

    private void OnGUI()
    { 
        DrawReaderUI();
        DrawProcessorsUI();
        DrawImageInfos();
    }

    private void DrawReaderUI()
    {
        EditorGUILayout.LabelField("Image Series Reader", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        string oldFolderPath = pipeline.reader.GetFolderPath();
        string newFolderPath = EditorGUILayout.TextField("Folder Path", oldFolderPath);
        if (newFolderPath != oldFolderPath)
        {
            pipeline.SetFolderPath(newFolderPath);
        }
        Rect dropArea = GUILayoutUtility.GetLastRect();
        HandleFolderDND(dropArea);

        if (GUILayout.Button("...", GUILayout.Width(20)))
        {
            string folder = EditorUtility.OpenFolderPanel("Choose Folder", "", "");
            if (!string.IsNullOrEmpty(folder))
            {
               pipeline.SetFolderPath(folder);
            }
        }
        EditorGUILayout.EndHorizontal();
        DrawExtensionsUI();
        EditorGUILayout.LabelField("Number of Files:", pipeline.reader.fileStack.Count().ToString());

        EditorGUILayout.BeginHorizontal();
        pipeline.reader.startIndex = EditorGUILayout.IntField("Start Index", pipeline.reader.startIndex);
        pipeline.reader.endIndex = EditorGUILayout.IntField("End Index", pipeline.reader.endIndex);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Read Images"))
        {
            pipeline.Read();
        }
    }

    private void DrawExtensionsUI()
    {
        var extensions = pipeline.reader.fileStack._extensions;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Accepted File Extensions", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Add Extension"))
        {
            extensions.Add(""); // Add a new empty entry
        }
        EditorGUILayout.EndHorizontal();

        if (extensions == null)
            return;

        for (int i = 0; i < extensions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            string oldExtension = extensions[i];
            string newExtension = EditorGUILayout.TextField($"Extension {i + 1}", extensions[i]);
            if (newExtension != oldExtension)
            {
                extensions[i] = newExtension;
                
                if (pipeline.reader.fileStack.IsInitialized())
                {
                    pipeline.reader.fileStack.Init();
                }
            }

            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                extensions.RemoveAt(i);
                if (pipeline.reader.fileStack.IsInitialized())
                {
                    pipeline.reader.fileStack.Init();
                }
                break; // Important to avoid GUI layout errors
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawProcessorsUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Image Processors", EditorStyles.boldLabel);

        var processors = pipeline.processors;

        // Ensure foldouts list matches processors
        while (foldouts.Count < processors.Count)
            foldouts.Add(true); // default expanded

        while (foldouts.Count > processors.Count) 
            foldouts.RemoveAt(foldouts.Count - 1);

        for (int i = 0; i < processors.Count; i++)
        {
            var proc = processors[i];

            EditorGUILayout.BeginVertical("box");
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], $"Processor {i + 1}: {proc.name ?? "(unnamed)"}", true);

            if (foldouts[i])
            {

                // Python path field
                string oldPath = proc.GetPythonPath();
                string newPath = DrawPythonPathField(oldPath, proc);

                if (newPath != oldPath && System.IO.File.Exists(newPath))
                {
                    try
                    {
                        proc.SetPythonPath(newPath);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to set Python path: {e.Message}");
                    }
                }

                // Parameters
                for (int p = 0; p < proc.parameters.Count; p++)
                {
                    var param = proc.parameters[p];
                    string newValue = EditorGUILayout.TextField(param.name, param.value);
                    if (newValue != param.value)
                        proc.parameters[p] = new stm.ImageProcessor.Parameter { name = param.name, value = newValue };
                }

                // Remove
                if (GUILayout.Button("Remove Processor"))
                {
                    pipeline.processors.RemoveAt(i);
                    foldouts.RemoveAt(i);
                    EditorGUILayout.EndVertical();
                    break;
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Processor"))
        {
            pipeline.processors.Add(new stm.ImageProcessor());
        }

        if (GUILayout.Button("Run Pipeline"))
        {
            pipeline.Process();
        }
    }

    private string DrawPythonPathField(string currentPath, ImageProcessor processor)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Python File", GUILayout.Width(80));
        string result = EditorGUILayout.TextField(currentPath);

        Rect dropArea = GUILayoutUtility.GetLastRect();
        HandleProcessorDND(dropArea, processor);

        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string selected = EditorUtility.OpenFilePanel("Choose Python Script", "", "py");
            if (!string.IsNullOrEmpty(selected))
                result = selected;
        }

        EditorGUILayout.EndHorizontal();
        return result;
    }

    private void HandleProcessorDND(Rect dropArea, ImageProcessor processor)
    {
        if (processor == null) return;
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string draggedObject in DragAndDrop.paths)
                    {
                        if (System.IO.File.Exists(draggedObject))
                        {
                            processor.SetPythonPath(draggedObject);
                            Repaint();
                            break;
                        }
                    }
                }
                evt.Use();
            }
        }
    }

    private void HandleFolderDND(Rect dropArea)
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (string draggedObject in DragAndDrop.paths)
                    {
                        if (System.IO.Directory.Exists(draggedObject))
                        {
                            pipeline.reader.SetFolderPath(draggedObject);
                            Repaint();
                            break;
                        }
                    }
                }
                evt.Use();
            }
        }
    }


    private void DrawImageInfos()
    {
        EditorGUILayout.Space();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Loaded Image", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(pipeline.image == null);
        if (GUILayout.Button("Render"))
        {
            pipeline.Render();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(pipeline.imageInfo);

        EditorGUILayout.Space();



        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Processed Image", EditorStyles.boldLabel);
        GUI.enabled = pipeline.image != null;
        if (GUILayout.Button("Render"))
        {
            pipeline.RenderProcessed();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(pipeline.processedImageInfo);
    }

}
