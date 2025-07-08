import itk

import FileStack


def print_image_info(image):
    size = tuple(image.GetBufferedRegion().GetSize())
    pixel_type, dimension = itk.template(image)[1]
    print(f"Size: {size}, PixelType: {pixel_type}, Dimension: {dimension}")


def read_image(file_path):
    return itk.imread(file_path)


def write_image(file_path, image):
    itk.imwrite(image, file_path)


def read_image_stack(file_stack=FileStack.FileStack, start_index=None, end_index=None):
    if file_stack.is_empty():
        print("FileStack is empty!")
        return None

    probe_image = read_image(file_stack[0])
    image_template = itk.template(probe_image)
    pixel_type, dimension = image_template[1]

    ImageType = itk.Image[pixel_type, 3]

    reader = itk.ImageSeriesReader[ImageType].New()
    start = 0
    end = len(file_stack) - 1
    if start_index is not None:
        start = start_index
    if end_index is not None:
        end = end_index
    reader.SetFileNames(file_stack.get_range(start, end))
    reader.Update()

    return reader.GetOutput()