import itk
import vtk


from enum import Enum


class RenderMode(Enum):
    RAW = 0
    GRAYSCALE = 1
    BINARY = 2


class TransferFunction:
    def __init__(self, color_points=None, opacity_points=None):
        self.color_points = color_points or []
        self.opacity_points = opacity_points or []

    def build(self, min_val, max_val):
        color_tf = vtk.vtkColorTransferFunction()
        for p in self._rescale(self.color_points, min_val, max_val):
            color_tf.AddRGBPoint(*p)

        opacity_tf = vtk.vtkPiecewiseFunction()
        for p in self._rescale(self.opacity_points, min_val, max_val):
            opacity_tf.AddPoint(*p)

        return color_tf, opacity_tf

    @staticmethod
    def _rescale(points, min_val, max_val):
        """Convert normalized [0,1] to real value range."""
        return [(min_val + (max_val - min_val) * p[0], *p[1:]) for p in points]

    @staticmethod
    def from_mode(mode: RenderMode):
        if mode == RenderMode.BINARY:
            return TransferFunction(
                color_points=[(0.0, 0, 0, 0), (1.0, 1, 1, 1)],
                opacity_points=[(0.0, 0.0), (1.0, 1.0)]
            )
        elif mode == RenderMode.GRAYSCALE:
            return TransferFunction(
                color_points=[(0.0, 0, 0, 0), (1.0, 1, 1, 1)],
                opacity_points=[
                    (0.0, 0.0),
                    (0.1, 0.1),
                    (0.4, 0.4),
                    (1.0, 1.0)
                ]
            )
        elif mode == RenderMode.RAW:
            return TransferFunction(
                color_points=[(0.0, 0, 0, 0), (1.0, 1, 1, 1)],
                opacity_points=[(0.0, 0.0), (1.0, 1.0)]
            )
        else:
            raise ValueError(f'Unknown mode: {mode}')


def render_volume(image: itk.Image, transfer_function=TransferFunction.from_mode(RenderMode.GRAYSCALE)):
    vtk_image = itk.vtk_image_from_image(image)

    # Get image range
    stats_filter = itk.StatisticsImageFilter[type(image)].New()
    stats_filter.SetInput(image)
    stats_filter.Update()
    min_val = stats_filter.GetMinimum()
    max_val = stats_filter.GetMaximum()

    # Set transfer function
    color_tf, opacity_tf = transfer_function.build(min_val, max_val)

    # Volume mapper
    volume_mapper = vtk.vtkSmartVolumeMapper()
    volume_mapper.SetInputData(vtk_image)

    # Volume properties
    volume_property = vtk.vtkVolumeProperty()
    volume_property.SetColor(color_tf)
    volume_property.SetScalarOpacity(opacity_tf)
    volume_property.ShadeOn()
    volume_property.SetInterpolationTypeToLinear()

    volume = vtk.vtkVolume()
    volume.SetMapper(volume_mapper)
    volume.SetProperty(volume_property)

    # Renderer and render window
    renderer = vtk.vtkRenderer()
    renderer.AddVolume(volume)
    renderer.SetBackground(0, 0, 0)

    render_window = vtk.vtkRenderWindow()
    render_window.AddRenderer(renderer)
    render_window.SetSize(800, 600)

    interactor = vtk.vtkRenderWindowInteractor()
    interactor.SetRenderWindow(render_window)

    # Initialize and start
    render_window.Render()
    interactor.Initialize()
    interactor.Start()


def render_mesh(mesh):
    mapper = vtk.vtkPolyDataMapper()
    mapper.SetInputData(mesh)

    actor = vtk.vtkActor()
    actor.SetMapper(mapper)
    actor.GetProperty().SetColor(1.0, 0.8, 0.3)  # Optional color

    renderer = vtk.vtkRenderer()
    renderer.AddActor(actor)
    renderer.SetBackground(0.1, 0.1, 0.1)

    render_window = vtk.vtkRenderWindow()
    render_window.AddRenderer(renderer)
    render_window.SetSize(800, 600)

    interactor = vtk.vtkRenderWindowInteractor()
    interactor.SetRenderWindow(render_window)

    render_window.Render()
    interactor.Start()