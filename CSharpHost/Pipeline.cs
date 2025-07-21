using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace stm
{
    public class Pipeline : IDisposable
    {
        public ImageSeriesReader reader = new ImageSeriesReader();
        public List<ImageProcessor> processors = new List<ImageProcessor>();
        public VolumeRenderer volumeRenderer = new VolumeRenderer();
        public ImageInfo ImInfo = new ImageInfo();
        public ImageCopy ImCopy = new ImageCopy();
        public MeshInfo MshInfo = new MeshInfo();
        public ImageMetadata ImMetadata = new ImageMetadata();
        public ImageMetadataSetter ImMetaSetter = new ImageMetadataSetter();

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
            if (processedImage != null & false) // nnn
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

        private void ClearMetadata_ImageProcessing()
        {
            string pattern = @"((?:\s*\r?\n){0,2})?\[Image Processing Start\].*?\[Image Processing End\]";
            imageMetadata = Regex.Replace(imageMetadata, pattern, string.Empty, RegexOptions.Singleline);
        }
        private void ClearMetadata_MeshGen()
        {
            string pattern = @"((?:\s*\r?\n){0,2})?\[Mesh Generation Start\].*?\[Mesh Generation End\]";
            imageMetadata = Regex.Replace(imageMetadata, pattern, string.Empty, RegexOptions.Singleline);
        }
        private void AddMetadata_ImageProcessing(string metadata)
        {
            if (metadata == null) return;
            ClearMetadata_ImageProcessing();

            string stringToAdd = "\n\n[Image Processing Start]\n" + metadata + "\n[Image Processing End]\n";
            imageMetadata += stringToAdd;
        }
        private void AddMetadata_MeshGen(string metadata)
        {
            if (metadata == null) return;
            ClearMetadata_MeshGen();

            string stringToAdd = "\n\n[Mesh Generation Start]\n" + metadata + "\n[Mesh Generation End]\n";
            imageMetadata += stringToAdd;
        }

        public void ClearLoaded()
        {
            if (mesh != null)
            {
                mesh.Dispose();
                mesh = null;
            }
            if (processedImage != null)
            {
                processedImage.Dispose();
                processedImage = null;
            }
            if (image != null)
            {
                image.Dispose();
                image = null;
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
                    ClearLoaded();

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
                processedImage = ImCopy.Copy(image);
                bool changed = false;
                string metadata = "";
                foreach (ImageProcessor processor in processors)
                {
                    if (!processor.IsValid())
                    {
                        Console.WriteLine($"Skipping invalid processor: {processor.name}");
                        continue;
                    }
                    changed = true;
                    processedImage = processor.Process(processedImage);

                    metadata += processor.OutString + "\n";
                }
                if (!changed)
                {
                    if (processedImage == null) processedImage.Dispose();
                    processedImage = null;
                }
                else
                {
                    AddMetadata_ImageProcessing(metadata);
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
                    AddMetadata_MeshGen(MeshGen.OutString);
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
                    AddMetadata_MeshGen(MeshGen.OutString);
                }

                UpdateMeshInfo();
            }
            catch (PythonException e) { Console.WriteLine(e.Message, e.StackTrace); }
            catch (Exception e) { Console.Write(e.Message, e.StackTrace); }
        }

        private void UpdateMetaData()
        {
            if (imageMetadata == null) return;
            if (image != null)
            {
                ImMetaSetter.Set(image, imageMetadata);
            }
            if (processedImage != null)
            {
                ImMetaSetter.Set(processedImage, imageMetadata);
            }
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
            UpdateMetaData();
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
            UpdateMetaData();
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
            UpdateMetaData();
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
                MshInfo.Dispose();
                ImCopy.Dispose();
                ImMetadata.Dispose();
                ImMetaSetter.Dispose();

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
