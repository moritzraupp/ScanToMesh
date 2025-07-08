from ImageProcessor import ImageProcessor

import itk


class RescaleImageFilter(ImageProcessor):
    def __init__(self, out_min=0, out_max=255, out_type=itk.UC):
        super().__init__()
        self.out_min = out_min
        self.out_max = out_max
        self.out_type = out_type

    def process(self):
        if self.image is None:
            raise Exception('ImageProcessor.process() called without an image')

        if 'rgb' in str(self.pixel_type).lower():
            raise Exception('RescaleImageFilter.process() only works with grayscale images!')

        InputImageType = itk.Image[self.pixel_type, self.dimension]
        OutputImageType = itk.Image[self.out_type, self.dimension]


        input_image = self.image

        rescale = itk.RescaleIntensityImageFilter[InputImageType, OutputImageType].New()
        rescale.SetInput(input_image)
        rescale.SetOutputMinimum(self.out_min)
        rescale.SetOutputMaximum(self.out_max)
        rescale.Update()

        return rescale.GetOutput()