from ImageProcessor import ImageProcessor
import ImageIO as io
import itk


class TemplateProcessor(ImageProcessor):  # must inherit from ImageProcessor
    def __init__(self, some_parameter=0, another_parameter=True, another_parameter_2="Always set default values"):
        super().__init__()
        self.some_parameter = some_parameter
        self.another_parameter = another_parameter
        self.another_parameter_2 = another_parameter_2

    def process(self):  # must implement the process function
        if self.image is None:
            raise Exception('TemplateProcessor.process() called without an image')

        if 'rgb' in str(self.pixel_type).lower():  # optional
            raise Exception('TemplateProcessor.process() only works with grayscale images!')


        InputImageType = itk.Image[self.pixel_type, self.dimension]
        input_image = self.image

        """
        Implement your own processing function
        
        see https://examples.itk.org/src
        
        Do something with the image
        """
        result = io.copy_image(input_image)  # do not return the original image

        io.clone_metadata(input_image, result)  # always copy over the metadata
        return result  # must return an itk.Image
