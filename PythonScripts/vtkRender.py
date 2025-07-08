import vtk

def render_image(image):

    mapper = vtk.vtkImageMapper()
    mapper.SetInputData(image)

    imageActor = vtk.vtkImageActor()
    imageActor.SetInputData(image)

    renderer = vtk.vtkRenderer()
    renderer.AddActor(imageActor)
    renderer.ResetCamera()

    rendererWindow = vtk.vtkRenderWindow()
    rendererWindow.AddRenderer(renderer)
    rendererWindow.SetSize(1024, 1024)

    interactor = vtk.vtkRenderWindowInteractor()
    interactor.SetRenderWindow(rendererWindow)

    rendererWindow.Render()
    interactor.Start()