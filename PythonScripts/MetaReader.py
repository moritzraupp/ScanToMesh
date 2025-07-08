import os
import importlib.util
import inspect


def get_class_params_from_file(file_path):
    module_name = os.path.splitext(os.path.basename(file_path))[0]

    spec = importlib.util.spec_from_file_location(module_name, file_path)
    if spec is None or spec.loader is None:
        raise ImportError(f'Could not find the spec module{file_path}')

    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)

    # find first class
    for name, obj in vars(module).items():
        if inspect.isclass(obj) and obj.__module__ == module.__name__:
            class_name = name
            cls = obj
            break
    else:
        raise Exception("No class found in Python file")

    # get constructor parameters
    sig = inspect.signature(cls.__init__)
    params = []
    for pname, param in sig.parameters.items():
        if pname == "self":
            continue
        default = param.default if param.default is not inspect.Parameter.empty else None
        params.append({
            'name': pname,
            'default': default,
            'type': str(param.annotation) if param.annotation != inspect.Parameter.empty else None
        })

    return {
        'class_name': class_name,
        'params': params
    }
