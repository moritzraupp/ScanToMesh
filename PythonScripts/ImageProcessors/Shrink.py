from ImageProcessor import ImageProcessor

import itk

import ImageIO as io

class ShrinkImageFilter(ImageProcessor):
    def __init__(self, shrink_factor=2):
        super().__init__()
        self.shrink_factor = shrink_factor

    def process(self):
        if self.image is None:
            raise Exception('ImageProcessor.process() called without an image')

        if 'rgb' in str(self.pixel_type).lower():
            raise Exception('ShrinkImageFilter.process() only works with grayscale images!')

        InputImageType = itk.Image[self.pixel_type, self.dimension]

        input_image = self.image

        shrink = itk.BinShrinkImageFilter[InputImageType, InputImageType].New()
        shrink.SetInput(input_image)
        shrink.SetShrinkFactors([self.shrink_factor] * self.dimension)
        shrink.Update()

        shrink_image = shrink.GetOutput()

        io.clone_metadata(input_image, shrink_image)

        return shrink_image