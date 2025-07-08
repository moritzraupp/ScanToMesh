import itk


def read_image(filename):
    return itk.imread(filename)


def write_image(filename, image):
    itk.imwrite(filename, image)


def read_image_stack(folder_path, limit_extensions=0):


    return 0
