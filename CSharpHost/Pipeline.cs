using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace stm
{
    [Serializable]
    public class Pipeline : IDisposable
    {
        public ImageSeriesReader reader = new ImageSeriesReader();
        public List<ImageProcessor> processors = new List<ImageProcessor>();
        public VolumeRenderer volumeRenderer = new VolumeRenderer();
        public ImageInfo ImInfo = new ImageInfo();
        public ImageCopy ImCopy = new ImageCopy();
        public MeshInfo MshInfo = new MeshInfo();
        public ImageMetadata ImMetadata = new ImageMetadata();

        public MarchingCubesGenerator MeshGen = new MarchingCubesGenerator();
        public MeshRenderer meshRenderer = new MeshRenderer();
        public STLWriter STLWriter = new STLWriter();
        public OBJWriter OBJWriter = new OBJWriter();
        public ImageSeriesWriter writer = new ImageSeriesWriter();
        public bool writeMetaFile = true;

        public PyObject image = null;
        public PyObject processedImage = null;
        public PyObject mesh = null;

        public string imageInfo = "null";
        public string processedImageInfo = "null";
        public string meshInfo = "null";
        public string imageMetadata = "null";

        public string dataName = "null";

        public string outputFolder = "null";

        public void UpdateImageInfo()
        {
            if (image != null)
            {
                imageInfo = ImInfo.Get(image);
            }
            else
            {
                imageInfo = "null";
            }
        }
        public void UpdatePImageInfo()
        {
            if (processedImage != null)
            {
                processedImageInfo = ImInfo.Get(processedImage);
            }
            else
            {
                processedImageInfo = "null";
            }
        }
        public void UpdateMeshInfo()
        {
            if (mesh != null)
            {
                meshInfo = MshInfo.Get(mesh);
            }
            else
            {
                meshInfo= "null";
            }
        }
        public void UpdateImageMetadata()
        {
            if (processedImage != null)
            {
                // use processed image
                imageMetadata = ImMetadata.Get(processedImage);
            }
            else
            {
                // use non processed
                if (image != null)
                {
                    imageMetadata = ImMetadata.Get(image);
                }
                else
                {
                    imageMetadata = "null";
                }
            }
        }

        public void SetFolderPath(string folderPath)
        {
            reader.SetFolderPath(folderPath);

            string folder = reader.GetFolderPath();
            if (folder != null )
            {
                dataName = System.IO.Path.GetFileName(folder);
            }
        }

        public void Read()
        {
            if (reader.fileStack.Count() < 1)
            {
                Console.WriteLine("No files to read");
                return;
            }

            try
            {
                PyObject result = reader.Read();
                if (result == null)
                {
                    Console.WriteLine("Image is null");
                    result.Dispose();
                    result = null;
                }
                else
                {
                    if (image != null)
                    image.Dispose();

                    image = result;
                    UpdateImageInfo();
                    UpdateImageMetadata();
                }

            }
            catch (PythonException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        public void Process()
        {
            try
            {
                if (processedImage != null)
                {
                    processedImage.Dispose();
                }
                processedImage = image;
                bool changed = false;
                foreach (ImageProcessor processor in processors)
                {
                    if (!processor.IsValid())
                    {
                        Console.WriteLine($"Skipping invalid processor: {processor.name}");
                        continue;
                    }
                    changed = true;
                    processedImage = processor.Process(processedImage);
                }
                if (!changed)
                {
                    processedImage = null;
                }
                UpdatePImageInfo();
            }
            catch (PythonException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message);  }
        }

        public void Render()
        {
            try
            {
                if (image != null)
                volumeRenderer.Render(image);
            }
            catch (PythonException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
        public void RenderProcessed()
        {
            try
            {
                if (processedImage != null)
                    volumeRenderer.Render(processedImage);
            }
            catch (PythonException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        public void GenerateMesh()
        {
            try
            {
                if (processedImage != null)
                {
                    // use processed image
                    if (mesh != null) mesh.Dispose();

                    mesh = MeshGen.Generate(processedImage);
                }
                else
                {
                    // use image
                    if (image == null)
                    {
                        Console.WriteLine("No image to perform Mesh Generation");
                        return;
                    }

                    if (mesh != null) mesh.Dispose();

                    mesh = MeshGen.Generate(image);
                }

                UpdateMeshInfo();
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        public void RenderMesh()
        {
            try
            {
                if (mesh == null)
                {
                    Console.WriteLine("No mesh to render");
                    return;
                }

                meshRenderer.Render(mesh);
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        public void WriteImageStack()
        {
            try
            {
                if (processedImage != null)
                {
                    // using processed
                    writer.fileName = dataName;
                    writer.folderPath = outputFolder;

                    writer.Write(processedImage, writeMetaFile);
                }
                else
                {
                    // using non processed
                    if (image != null)
                    {
                        writer.fileName = dataName;
                        writer.folderPath = outputFolder;

                        writer.Write(image, writeMetaFile);
                    }
                    else
                    {
                        Console.WriteLine("No Image to write");
                        return;
                    }
                }
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        public void WriteSTL()
        {
            try
            {
                if (mesh == null)
                {
                    Console.WriteLine("No mesh to write");
                    return;
                }

                STLWriter.folderPath = outputFolder;
                STLWriter.fileName = dataName;

                STLWriter.Write(mesh, writeMetaFile ? image : null);
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        public void WriteOBJ()
        {
            try
            {
                if (mesh == null)
                {
                    Console.WriteLine("No mesh to write");
                    return;
                }

                OBJWriter.folderPath = outputFolder;
                OBJWriter.fileName = dataName;

                OBJWriter.Write(mesh, writeMetaFile ? image : null);
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        public void Dispose()
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    return;
                }

                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                if (processedImage != null)
                {
                    processedImage.Dispose();
                    processedImage = null;
                }
                if (mesh != null)
                {
                    mesh.Dispose();
                    mesh = null;
                }

                ImInfo.Dispose();
                ImCopy.Dispose();
                MshInfo.Dispose();
                ImMetadata.Dispose();

                MeshGen.Dispose();
                meshRenderer.Dispose();
                STLWriter.Dispose();
                OBJWriter.Dispose();
                writer.Dispose();

                reader.Dispose();
                volumeRenderer.Dispose();
                foreach (ImageProcessor processor in processors)
                {
                    processor.Dispose();
                }
            }
            catch (Exception e) {Console.WriteLine(e.Message); }
        }

    }
}
