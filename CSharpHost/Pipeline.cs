using Python.Runtime;
using System;
using System.Collections.Generic;
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

        public PyObject image = null;
        public PyObject processedImage = null;

        public string imageInfo = "null";
        public string processedImageInfo = "null";

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

        public void SetFolderPath(string folderPath)
        {
            reader.SetFolderPath(folderPath);
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

                ImInfo.Dispose();
                ImCopy.Dispose();

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
