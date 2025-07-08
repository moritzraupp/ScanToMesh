# ScanToMesh (stm)

**ScanToMesh** is a Python-based bioimaging pipeline tool designed for seamless integration with C# applications via PythonNet. It leverages powerful libraries like **ITK** and **VTK** to import, process, render, and convert high-resolution bioimaging scans into detailed 3D meshes.

## Features

- Import and preprocess bioimaging scan data  
- Advanced image processing powered by ITK  
- High-quality visualization and rendering with VTK  
- Mesh generation for 3D modeling  
- Easy integration with C# using the `stm` namespace and PythonNet  

## Getting Started

1. Use the bundled standalone Python environment for consistent deployment.  
2. Run the setup script to install required dependencies (`itk`, `vtk`, etc.).  
3. Utilize the Python API via the `stm` namespace in your C# projects through PythonNet.  

## Requirements

- Standalone Python environment (embeddable recommended)  
- ITK  
- VTK  
- PythonNet  
- C# development tools  

## PythonNet

This tool uses pythonnet to integrate Python into C#. For more information see [here](CSharpHost/dlls/pythonnet/README.md).

## Usage

Check out the example scripts and C# samples included in this repo to quickly get started with importing scans, processing data, rendering visuals, and exporting meshes.
