import sys
sys.path.append("../python_runtime/Lib/site-packages")
sys.path.append("./")
sys.path.append("./ImageProcessors")



import ImageIO as io
import Rendering as ren

from FileStack import FileStack
import Mesh as m

from ImageProcessors import Rescale
from ImageProcessors import Threshold
from ImageProcessors import Shrink

stack = FileStack("D:\\Thesis\\data\\PP_20150928_15758 PM\\PP_w0", "tif")
#tack = FileStack("D:\\Thesis\\data\\PP_20150928_15758 PM\\PP_20150928_15758 PM", ["tif"])

print(stack)

image = io.read_image_stack(stack, 1384-1024, 1384)
# image = io.read_image("D:\\Thesis\\data\\PP_20150928_15758 PM\\PP_w0\\PP_w0000_z0716.tif")

io.print_image_info(image)
ren.render_volume(image)


rescale = Rescale.RescaleImageFilter(0, 255)
rescale.set_image(image)
image = rescale.process()

io.print_image_info(image)

shrink = Shrink.ShrinkImageFilter(2)
shrink.set_image(image)
image = shrink.process()
io.print_image_info(image)


io.print_image_info(image)
ren.render_volume(image)


thresholdFilter = Threshold.ThresholdImageFilter()
thresholdFilter.upper = 25
thresholdFilter.set_image(image)

tImage = thresholdFilter.process()

del image


io.print_image_info(tImage)

ren.render_volume(tImage)

#raise Exception("aa")



mesh = m.MeshGen.marching_cubes(tImage, 255)
del tImage

ren.render_mesh(mesh)

m.write_stl('C:\\Users\\Moritz\\Downloads\\out.stl', mesh)


# io.write_image('C:\\Users\\Moritz\\Downloads\\out.png', tImage)