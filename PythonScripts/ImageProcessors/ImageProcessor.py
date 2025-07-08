from abc import ABC, abstractmethod

import itk


class ImageProcessor(ABC):

    def __init__(self):
        self._image = None
        self._pixel_type = None
        self._dimension = None

    @property
    def image(self):
        return self._image

    @property
    def dimension(self):
        return self._dimension

    @property
    def pixel_type(self):
        return self._pixel_type

    @abstractmethod
    def process(self):
        pass

    def set_image(self, image):
        if not isinstance(image, itk.Image):
            raise Exception("Image must be an instance of itk.Image")

        self._image = image
        self._dimension = image.GetImageDimension()
        self._pixel_type = itk.template(image)[1][0]
