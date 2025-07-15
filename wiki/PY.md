Return to [main page](../README.md)

# Standalone Python

STM comes with a complete packaged Python environment (ver. 3.13) where Python code can be executed

Make sure, that all requirements are installed ([Install Python Requirements](../README.md#2-install-python-requirements))

## Setup

### 1. Python Console

1. Run `python_runtime/python.exe`

2. Make sure that python can find the all modules and scripts (see [Example Code](#example-code-pythonscriptspythondemopy) below)

3. Start Programming in Python


### 2. IDE

1. Load Repository as a project in your IDE

2. Set Reference to interpreter (`python_runtime/python.exe`)

3. Make sure that python can find the all modules and scripts (see [Example Code](#example-code-pythonscriptspythondemopy) below)

4. Start Programming in Python

## Example Code ([PythonScripts/PythonDemo.py](../PythonScripts/PythonDemo.py))


```python
import sys
sys.path.append("../python_runtime/Lib/site-packages")
sys.path.append("./")
sys.path.append("./ImageProcessors")

# make sure that python can find the modules

# -------------------------------------------------- #

# Import

import ImageIO as io
from FileStack import FileStack
import Rendering as ren

stack = FileStack("D:\\Thesis\\data\\PP_20150928_15758 PM\\PP_w0", "tif")
print(stack)

image = io.read_image_stack(stack, 1384-1024, 1384)
original_image = image
print(io.get_image_info(image))
print(io.get_metadata(image))
ren.render_volume(image)

# -------------------------------------------------- #

# Image Processors

from ImageProcessors import Rescale
from ImageProcessors import Threshold
from ImageProcessors import Shrink

# Rescaling (to 8bit)
rescale = Rescale.RescaleImageFilter(0, 255)
rescale.set_image(image)
image = rescale.process()
print(io.get_image_info(image))
ren.render_volume(image)

# Shrinking (merge 4 pixels in each direction)
shrink = Shrink.ShrinkImageFilter(4)
shrink.set_image(image)
image = shrink.process()
print(io.get_image_info(image))
ren.render_volume(image)

# Thresholding
thresholdFilter = Threshold.ThresholdImageFilter()
thresholdFilter.upper = 25
thresholdFilter.set_image(image)
tImage = thresholdFilter.process()  # dont override image for further usage
print(io.get_image_info(tImage))
ren.render_volume(tImage)

# -------------------------------------------------- #

# Mesh Generation

import Mesh

mesh = Mesh.MeshGen.marching_cubes(image, 25)  # using non threshold image for smoothing
print(Mesh.get_mesh_info(mesh))
ren.render_mesh(mesh)

# -------------------------------------------------- #

# Export

# Export threshold image as TIFF stack
default_name = "PP_w0000_z"
out_dir = "out/threshold"
io.write_image_stack(out_dir, default_name, tImage)

# Export non threshold image as TIFF stack with metadata
default_name = "PP_w0000_z"
out_dir = "out/processed"
io.write_image_stack_with_metadata(out_dir, default_name, image)  # function will write metadata from source into each tif image

# Export Mesh
out_dir = "out"
file_name = "mesh"
Mesh.write_stl(out_dir, file_name, mesh)
Mesh.write_obj(out_dir, file_name, mesh, reference_image=original_image)  # use reference image to save metadata

```


##
Back to [top](#standalone-python)

Return to [main page](../README.md)