using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System;

using stm;
using Python.Runtime;


public class PipelineWindow : EditorWindow
{
    public Pipeline pipeline = null; 

    private Vector2 scrollPosition = Vector2.zero;

    private List<bool> foldouts = new List<bool>();

    private bool showImport = true;
    private bool showProcess = false;
    private bool showMeshGeneration = false;
    private bool showLoaded = false;
    private bool showExport = false;

    private bool showMetadata = false;
    private Vector2 metadataScrollPosition = Vector2.zero;



    [MenuItem("Tools/ScanToMesh/Show Pipeline")]
    public static void ShowWindow()
    {
        GetWindow<PipelineWindow>("Scan To Mesh");
    }

    private void OnDisable()
    {
        try
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
        catch (PythonException e) { Debug.Log(e.Message); }
        catch (Exception e) { Debug.Log(e.Message); }
    }

    private void OnEnable()
    {
        pipeline = null;
        EditorApplication.delayCall += SafeInit;
    }

    private void SafeInit() 
    { 
        if (!PythonInteropInit.Initialized())
        {
            Debug.LogWarning("Python not initialized yet. Retrying...");
            EditorApplication.delayCall += SafeInit; // Retry on next frame
            return;
        }

        pipeline = new Pipeline();
        pipeline.outputFolder = "Assets";
    }

    private void Space(int spacing) { EditorGUILayout.Space(spacing); }
    private void Space() { EditorGUILayout.Space(); }

    private void DrawSeparator()
    {
        Space();
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;

        EditorGUI.DrawRect(rect, Color.gray);
    }

    private void OnGUI()
    {
        if (pipeline == null)
        {
            EditorGUILayout.LabelField("Waiting for Python Engine");
            return;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        DrawImport();
        DrawSeparator();

        DrawProcessors();
        DrawSeparator();

        DrawMeshGen();
        DrawSeparator();

        DrawInfos();
        DrawSeparator();

        DrawExport();

        EditorGUILayout.EndScrollView();
    }

    private void DrawImport()
    {
        showImport = EditorGUILayout.Foldout(showImport, "Import", true);
        if (!showImport) return;


        Space(); // Image Stack selection

        EditorGUILayout.LabelField("Select Image Stack", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        string oldFolderPath = pipeline.reader.GetFolderPath();
        string newFolderPath = EditorGUILayout.TextField(new GUIContent("Folder Path", "Select to path to your image stack folder"), oldFolderPath);
        if (newFolderPath != oldFolderPath)
        {
            pipeline.SetFolderPath(newFolderPath);
        }
        Rect dropArea = GUILayoutUtility.GetLastRect();
        HandleFolderDND(dropArea);

        if (GUILayout.Button(new GUIContent("...", "Brows files"), GUILayout.Width(20)))
        {
            string folder = EditorUtility.OpenFolderPanel("Choose Folder", "", "");
            if (!string.IsNullOrEmpty(folder))
            {
                pipeline.SetFolderPath(folder);
            }
        }
        EditorGUILayout.EndHorizontal();


        Space(); // File Extension selection

        DrawExtensionsUI();


        Space(); // Final Import Stage

        EditorGUILayout.LabelField("Number of Files:", pipeline.reader.fileStack.Count().ToString());

        EditorGUILayout.BeginHorizontal();
        pipeline.reader.startIndex = EditorGUILayout.IntField(new GUIContent("Start Index", "Select the range of the stack you want to import"), pipeline.reader.startIndex);
        pipeline.reader.endIndex = EditorGUILayout.IntField(new GUIContent("End Index", "Select the range of the stack you want to import"), pipeline.reader.endIndex);
        EditorGUILayout.EndHorizontal();

        GUI.enabled = pipeline.reader.fileStack.Count() > 0;
        if (GUILayout.Button(new GUIContent("Read Images", "Import the image stack you selected")))
        {
            pipeline.Read();
            if (pipeline.image != null)
            {
                showLoaded = true;
            }
        }
        GUI.enabled = true;
    }
    private void DrawExtensionsUI()
    {
        var extensions = pipeline.reader.fileStack._extensions;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Accepted File Extensions", "Select file extensions, you want to limit to (e.g.: \".tif\")\nLeave empty, if you do not want to filter"), EditorStyles.boldLabel);
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
            string newExtension = EditorGUILayout.TextField(new GUIContent($"Extension {i + 1}", "Enter extension (e.g.: \".tif\")"), extensions[i]);
            if (newExtension != oldExtension)
            {
                extensions[i] = newExtension;
                
                if (pipeline.reader.fileStack.IsInitialized())
                {
                    pipeline.reader.fileStack.Init();
                }
            }

            if (GUILayout.Button(new GUIContent("-", "Remove entry"), GUILayout.Width(25)))
            {
                extensions.RemoveAt(i);
                if (pipeline.reader.fileStack.IsInitialized())
                {
                    pipeline.reader.fileStack.Init();
                }
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawProcessors()
    {
        showProcess = EditorGUILayout.Foldout(showProcess, "Image Processing", true);
        if (!showProcess) return;

        Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Image Processors", "Add image processor steps"), EditorStyles.boldLabel);
        if (GUILayout.Button(new GUIContent("Add Processor", "Add ImageProcessor step")))
        {
            pipeline.processors.Add(new stm.ImageProcessor());
        }
        EditorGUILayout.EndHorizontal();

        Space();

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
            EditorGUILayout.BeginHorizontal();
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], $"Processor {i + 1}: {proc.name ?? "(unnamed)"}", true);

            // Remove
            if (GUILayout.Button(new GUIContent("Remove", "Remove enty"), GUILayout.Width(90)))
            {
                pipeline.processors.RemoveAt(i);
                foldouts.RemoveAt(i);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                break;
            }
            EditorGUILayout.EndHorizontal();

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

            }

            EditorGUILayout.EndVertical();
        }

        Space();

        GUI.enabled = pipeline.image != null;
        if (GUILayout.Button(new GUIContent("Run Pipeline", "Run pipeline with all Image Processors above")))
        {
            pipeline.Process();
            if (pipeline.processedImage != null)
            {
                showLoaded = true;
            }
        }
        GUI.enabled = true;
    }
    private string DrawPythonPathField(string currentPath, ImageProcessor processor)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Python File", "Select a python file, that is an ImageProcessor class"), GUILayout.Width(80));
        string result = EditorGUILayout.TextField(currentPath);

        Rect dropArea = GUILayoutUtility.GetLastRect();
        HandleProcessorDND(dropArea, processor);

        if (GUILayout.Button(new GUIContent("...", "Browse files"), GUILayout.Width(30)))
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

    private void DrawMeshGen()
    {
        showMeshGeneration = EditorGUILayout.Foldout(showMeshGeneration, "Mesh Generation", true);
        if (!showMeshGeneration) return;

        Space();

        pipeline.MeshGen.isoVal = EditorGUILayout.FloatField(new GUIContent("Iso Value", "Iso Value for marching cubes algorithm"), pipeline.MeshGen.isoVal);
        if (GUILayout.Button(new GUIContent("Generate", "Generate a mesh by using marching cubes algorithm")))
        {
            pipeline.GenerateMesh();
            if (pipeline.mesh != null)
            {
                showLoaded = true;
            }
        }
    }


    private void DrawInfos()
    {

        showLoaded = EditorGUILayout.Foldout(showLoaded, "Loaded Data", true);
        if (!showLoaded) return;

        Space();

        //pipeline.image == null ? Color.gray : Color.green;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Loaded Image", EditorStyles.boldLabel);
        GUI.enabled = pipeline.image != null;
        if (GUILayout.Button(new GUIContent("Render", "Show a 3D view of the imported image")))
        {
            pipeline.Render();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(pipeline.imageInfo);

        Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Processed Image", EditorStyles.boldLabel);
        GUI.enabled = pipeline.processedImage != null;
        if (GUILayout.Button(new GUIContent("Render", "Show a 3D view of the processed image")))
        {
            pipeline.RenderProcessed();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(pipeline.processedImageInfo);

        Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Mesh", EditorStyles.boldLabel);
        GUI.enabled = pipeline.mesh != null;
        if (GUILayout.Button(new GUIContent("Render", "Show a 3D view of the generated mesh")))
        {
            pipeline.RenderMesh();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(pipeline.meshInfo);

        Space();

        EditorGUI.indentLevel++;
        showMetadata = EditorGUILayout.Foldout(showMetadata, new GUIContent("Image Metadata", "Metadata that was attached to the image file(s)"));
        if (showMetadata)
        {
            metadataScrollPosition = EditorGUILayout.BeginScrollView(metadataScrollPosition, GUILayout.Height(150));
            pipeline.imageMetadata = EditorGUILayout.TextArea(pipeline.imageMetadata, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
        EditorGUI.indentLevel--;
    }

    private void DrawExport()
    {
        showExport = EditorGUILayout.Foldout(showExport, "Export", true);
        if (!showExport) return;

        Space();

        pipeline.outputFolder = EditorGUILayout.TextField(new GUIContent("Output Folder", "The output folder, where the files are written to"), pipeline.outputFolder);
        pipeline.dataName = EditorGUILayout.TextField(new GUIContent("Data Name", "Name of the data-set, used for the export name, e.g.:\nData Name: \"BrainVessel\"\nOutput: \"BrainVessel.obj\""), pipeline.dataName);
        pipeline.writeMetaFile = EditorGUILayout.Toggle(new GUIContent("Write Meta File", "Write the Image Metadata to a seperate .meta.txt file"), pipeline.writeMetaFile);

        Space();

        EditorGUILayout.LabelField("Export Image Stack");
        GUI.enabled = pipeline.processedImage != null || pipeline.image != null;
        if (GUILayout.Button(new GUIContent("Save as TIFF Stack", "Write the image to a series of .tif files")))
        {
            pipeline.WriteImageStack();
            AssetDatabase.Refresh();
        }
        GUI.enabled = true;

        Space();

        EditorGUILayout.LabelField("Export Mesh");
        GUI.enabled = pipeline.mesh != null;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Save as STL", "Write the mesh to a .stl file")))
        {
            pipeline.WriteSTL();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(new GUIContent("Save as OBJ", "Write the mesh to a .obj file")))
        {
            pipeline.WriteOBJ();
            AssetDatabase.Refresh();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

}
