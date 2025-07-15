import os

import itk
import vtk

import ImageIO as io


class MeshGen:

    @staticmethod
    def marching_cubes(image: itk.Image, iso_value=255):
        vtk_image = itk.vtk_image_from_image(image)

        contour = vtk.vtkMarchingCubes()
        contour.SetInputData(vtk_image)
        contour.SetValue(0, iso_value)
        contour.Update()

        return contour.GetOutput()


def get_mesh_info(mesh):

    if not isinstance(mesh, vtk.vtkPolyData):
        return f"Invalid mesh type: {type(mesh)}"

    num_points = mesh.GetNumberOfPoints()
    num_cells = mesh.GetNumberOfCells()
    bounds = mesh.GetBounds()  # (xmin, xmax, ymin, ymax, zmin, zmax)
    center = mesh.GetCenter()

    return (
        f"Points: {num_points}, "
        f"Cells: {num_cells}, "
        f"Bounds: ({bounds[0]:.2f}, {bounds[1]:.2f}, "
        f"{bounds[2]:.2f}, {bounds[3]:.2f}, "
        f"{bounds[4]:.2f}, {bounds[5]:.2f}), "
        f"Center: ({center[0]:.2f}, {center[1]:.2f}, {center[2]:.2f})"
    )


def smooth_mesh(mesh):
    smoother = vtk.vtkSmoothPolyDataFilter()
    smoother.SetInputData(mesh)
    smoother.SetNumberOfIterations(5)
    smoother.SetRelaxationFactor(0.1)
    smoother.FeatureEdgeSmoothingOff()
    smoother.BoundarySmoothingOn()
    smoother.Update()

    return smoother.GetOutput()


def write_stl(directory, file_name, mesh, reference_image=None):
    path = os.path.join(directory, f"{file_name}.stl")

    stl_writer = vtk.vtkSTLWriter()
    stl_writer.SetInputData(mesh)
    stl_writer.SetFileName(path)
    stl_writer.Write()

    if reference_image is not None:
        metadata_str = io.get_metadata(reference_image)
        meta_filename = path + ".meta.txt"
        os.makedirs(os.path.dirname(meta_filename), exist_ok=True)
        with open(meta_filename, 'w', encoding='utf-8', newline='') as f:
            f.write(metadata_str)

    return


def write_obj(directory, file_name, mesh, reference_image=None):
    path = os.path.join(directory, f"{file_name}.obj")

    obj_writer = vtk.vtkOBJWriter()
    obj_writer.SetInputData(mesh)
    obj_writer.SetFileName(path)
    obj_writer.Write()

    if reference_image is not None:
        metadata_str = io.get_metadata(reference_image)
        meta_filename = path + ".meta.txt"
        os.makedirs(os.path.dirname(meta_filename), exist_ok=True)
        with open(meta_filename, 'w', encoding='utf-8', newline='') as f:
            f.write(metadata_str)

    return
