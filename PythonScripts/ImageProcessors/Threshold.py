from ImageProcessor import ImageProcessor

import itk

import ImageIO as io

class ThresholdImageFilter(ImageProcessor):
    def __init__(self, lower=0, upper=255, inside=0, outside=255):
        super().__init__()
        self.lower = lower
        self.upper = upper
        self.inside = inside
        self.outside = outside

    def process(self):
        if self.image is None:
            raise Exception('ImageProcessor.process() called without an image')

        if 'rgb' in str(self.pixel_type).lower():
            raise Exception('ThresholdingImageFilter.process() only works with grayscale images!')

        InputImageType = itk.Image[self.pixel_type, self.dimension]
        OutputImageType = itk.Image[itk.UC, self.dimension]

        input_image = self.image

        threshold_filter = itk.BinaryThresholdImageFilter[InputImageType, OutputImageType].New()
        threshold_filter.SetInput(input_image)
        threshold_filter.SetLowerThreshold(self.lower)
        threshold_filter.SetUpperThreshold(self.upper)
        threshold_filter.SetInsideValue(self.inside)
        threshold_filter.SetOutsideValue(self.outside)
        threshold_filter.Update()

        threshold_image = threshold_filter.GetOutput()

        io.clone_metadata(input_image, threshold_image)

        return threshold_image