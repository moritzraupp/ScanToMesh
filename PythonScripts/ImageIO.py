import os
import json

import itk
import tifffile

import FileStack


def extract_image_descriptions(tiff_path):
    descriptions = []
    with tifffile.TiffFile(tiff_path) as tif:
        for page in tif.pages:
            description = None
            for tag in page.tags.values():
                if tag.name == 'ImageDescription':
                    try:
                        description = tag.value
                    except Exception as e:
                        description = f"[Error reading ImageDescription: {e}]"
                    break
            if description:
                descriptions.append(f"Page {page.index} ImageDescription:\n{description}\n")
            else:
                descriptions.append(f"Page {page.index} has no ImageDescription tag.\n")
    return "\n".join(descriptions)


def get_metadata(image: itk.Image):
    meta_dict = image.GetMetaDataDictionary()
    if meta_dict.HasKey("ImageDescription"):
        return meta_dict["ImageDescription"]
    else:
        return "no metadata"


def clone_metadata(from_image, to_image):
    if from_image is not None and to_image is not None:
        to_image["ImageDescription"] = get_metadata(from_image)


def get_image_info(image):
    size = tuple(image.GetBufferedRegion().GetSize())
    pixel_type, dimension = itk.template(image)[1]
    return f"Size: {size}, PixelType: {pixel_type}, Dimension: {dimension}"


def read_image(file_path):
    image_description = extract_image_descriptions(file_path)
    image = itk.imread(file_path)
    image["ImageDescription"] = image_description
    return image


def write_image(file_path, image):
    itk.imwrite(image, file_path)


def read_image_stack(file_stack: FileStack.FileStack, start_index=0, end_index=None):
    if file_stack.is_empty():
        print("FileStack is empty!")
        return None

    probe_image = read_image(file_stack[start_index])
    image_template = itk.template(probe_image)
    pixel_type, dimension = image_template[1]

    image_description = extract_image_descriptions(file_stack[start_index])

    ImageType = itk.Image[pixel_type, 3]

    reader = itk.ImageSeriesReader[ImageType].New()
    end = len(file_stack) - 1
    if end_index is not None:
        end = end_index
    reader.SetFileNames(file_stack.get_range(start_index, end))
    reader.Update()

    image = reader.GetOutput()
    image["ImageDescription"] = image_description

    return image


def write_image_stack(directory: str, base_file_name: str, image: itk.Image, meta_file=True):

    os.makedirs(directory, exist_ok=True)

    Dimension = 3
    PixelType = itk.template(image)[1][0]
    ImageType3D = itk.Image[PixelType, Dimension]
    ImageType2D = itk.Image[PixelType, Dimension - 1]

    WriterType = itk.ImageSeriesWriter[ImageType3D, ImageType2D]
    writer = WriterType.New()

    size = itk.size(image)
    num_slices = size[2]
    filenames = [os.path.join(directory, f"{base_file_name}_{i:04d}.tif") for i in range(num_slices)]

    writer.SetFileNames(filenames)
    writer.SetInput(image)

    writer.Update()

    if meta_file:
        metadata_str = get_metadata(image)
        meta_filename = os.path.join(directory, f"{base_file_name}.meta.txt")
        with open(meta_filename, 'w', encoding='utf-8', newline='') as f:
            f.write(metadata_str)

    return


def write_image_stack_with_metadata(directory: str, base_file_name: str, image: itk.Image, meta_file=True):

    os.makedirs(directory, exist_ok=True)

    image_description = get_metadata(image)

    size = itk.size(image)
    num_slices = size[2]

    ExtractFilterType = itk.ExtractImageFilter[type(image), itk.Image[itk.template(image)[1][0], 2]]

    metadata_str = get_metadata(image)

    if meta_file:
        meta_filename = os.path.join(directory, f"{base_file_name}.meta.txt")
        with open(meta_filename, 'w', encoding='utf-8', newline='') as f:
            f.write(metadata_str)

    for i in range(num_slices):
        extraction_region = list(size)
        extraction_region[2] = 0
        index = [0, 0, i]

        extractor = ExtractFilterType.New()
        extractor.SetInput(image)
        extractor.SetDirectionCollapseToIdentity()
        extractor.SetExtractionRegion(itk.ImageRegion[3](index, extraction_region))
        extractor.Update()

        slice_2d = extractor.GetOutput()
        np_slice = itk.array_view_from_image(slice_2d)

        filename = os.path.join(directory, f"{base_file_name}_{i:04d}.tiff")

        # Embed metadata string in TIFF ImageDescription tag
        tifffile.imwrite(filename, np_slice, description=metadata_str)