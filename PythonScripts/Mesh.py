import itk
import vtk


class MeshGen:

    @staticmethod
    def marching_cubes(image: itk.Image, iso_value=255):
        vtk_image = itk.vtk_image_from_image(image)

        contour = vtk.vtkMarchingCubes()
        contour.SetInputData(vtk_image)
        contour.SetValue(0, iso_value)
        contour.Update()

        return contour.GetOutput()


def smooth_mesh(mesh):
    smoother = vtk.vtkSmoothPolyDataFilter()
    smoother.SetInputData(mesh)
    smoother.SetNumberOfIterations(5)
    smoother.SetRelaxationFactor(0.1)
    smoother.FeatureEdgeSmoothingOff()
    smoother.BoundarySmoothingOn()
    smoother.Update()

    return smoother.GetOutput()


def write_stl(path, mesh):
    stl_writer = vtk.vtkSTLWriter()
    stl_writer.SetInputData(mesh)
    stl_writer.SetFileName(path)
    stl_writer.Write()

    return
