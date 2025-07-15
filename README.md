# ScanToMesh (STM)

**ScanToMesh** is a Python-based bioimaging pipeline tool designed for seamless integration with C# applications via PythonNet. It leverages powerful libraries like **ITK** and **VTK** to import, process, render, and convert high-resolution bioimaging scans into detailed 3D meshes.

## Features

- Import and preprocess bioimaging scan data  
- Advanced image processing powered by ITK  
- High-quality visualization and rendering with VTK  
- Mesh generation for 3D modeling  
- Easy integration with C# using the `stm` namespace and PythonNet  

## Project Structure

```
ScanToMesh/  
├── CSharpHost/
│	├── dlls/				// DLL files that are used by C#, e.g.: Python.Runtime.dll
│	├── out/				// Output folder for building CSharpHost.csproj
│	├── CSharpHost.csproj			// Project file to load in IDE and build from
│	└── *.cs				// C# source files
│
├── python_runtime/ 				// Standalone Python environment
│	├── Lib/
│	│	└── site-packages/
│	├── python.exe				// Run python from here
│	└── requirements.txt			// requirements file
│
├── PythonScripts/				// Source folder for all pythons scripts
│	└── PythonDemo.py			// Showcase for python usage
│
├── STM_out/
│	└── ScanToMesh/				// Copy this into your C# project
│
├── UnityImplementation/			// Development project for Unity Layer
│	└── Assets/				
│		└── EditorScripts/		// Unity Layer scripts
│		└── ScanToMesh/			// Target of copy_stm_and_unity_to_unity_out.bat
│
├── Unity_out/
│	└── ScanToMesh/				// Copy this into YourUnityProject/Assets/
│
│
├── copy_stm_to_out.bat				// Automation for copying STM into STM_out/
├── copy_stm_to_unity.bat			// Automation for copying STM into UnityIntegration/Asset/ScanToMesh/
├── copy_stm_and_unity_to_unity_out.bat		// Automation for copying STM and UnityIntegration/Asset/EditorScripts into Unity_out/ScanToMesh 
│
├── install_python_requirements.bat		// Autimation for running python requirements install
│
├── README.md  
└── .gitignore
```

## Getting Started

### 1. Clone or Copy this Repository

### 2. Install python requirements:
1. Manual Installation:
```cmd
python_runtime\python.exe -m pip install -r python_runtime\requirements.txt
```
2. Automated: Run Script: **install_python_requirements.bat**

## Using STM
### For standalone **Python** usage see [here](wiki/PY.md).
### For Standalone **C#** usage see [here](wiki/CS.md).
### For Using The Tool in **Unity** see [here](wiki/Unity.md).


## Requirements


| Systen Requirements | |
| ---|---|
| Hardware | x64 architecture |
| Windows | 10 / 11 |

## Tools Used

[**Python.Net**](https://pythonnet.github.io/) is a bridge between Python and .NET that allows you to run Python code directly from C#. This makes it easy to write complex data processing steps in Python—using libraries like ITK and VTK—and integrate them dynamically into a C# application at runtime. For more information see [here](CSharpHost/dlls/pythonnet/README.md).

[**ITK (Insight Toolkit)**](https://itk.org/) is a powerful library for medical image analysis and processing. It provides advanced tools for filtering, segmentation, registration, and working with multi-dimensional images—making it ideal for bioimaging workflows.

[**VTK (Visualization Toolkit)**](https://vtk.org/) is a 3D graphics and visualization library used for rendering complex datasets, including medical images and geometric models. It enables interactive visualization and mesh generation, making it a key component in image-to-mesh pipelines.

### 

[![Python](https://img.shields.io/badge/Python-3776AB?logo=python&logoColor=fff)](#) [![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)](#) [![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#) [![Unity](https://img.shields.io/badge/Unity-%23000000.svg?logo=unity&logoColor=white)](#)


##
Back to [top](#scantomesh-stm) 