from ImageProcessor import ImageProcessor

import itk

import ImageIO as io

class RescaleImageFilter(ImageProcessor):
    def __init__(self, out_min=0, out_max=255, out_type="UC"):
        super().__init__()
        self.out_min = out_min
        self.out_max = out_max
        self.out_type = out_type

    def process(self):
        if self.image is None:
            raise Exception('ImageProcessor.process() called without an image')

        if 'rgb' in str(self.pixel_type).lower():
            raise Exception('RescaleImageFilter.process() only works with grayscale images!')

        pixel_type_map = {
            "UC": itk.UC,
            "US": itk.US,
        }

        pt = pixel_type_map.get(self.out_type)
        if pt is None:
            allowed_types = ", ".join(pixel_type_map.keys())
            raise Exception(f"Unsupported pixel type: {self.out_type}. Currently allowed types: {allowed_types}")

        InputImageType = itk.Image[self.pixel_type, self.dimension]
        OutputImageType = itk.Image[pt, self.dimension]


        input_image = self.image

        rescale = itk.RescaleIntensityImageFilter[InputImageType, OutputImageType].New()
        rescale.SetInput(input_image)
        rescale.SetOutputMinimum(self.out_min)
        rescale.SetOutputMaximum(self.out_max)
        rescale.Update()

        rescale_image = rescale.GetOutput()

        io.clone_metadata(input_image, rescale_image)

        return rescale_image