import os
import glob

class FileStack:
    def __init__(self, directory, extensions=[".tif"], sort_files=False):
        self.directory = directory
        self.extensions = extensions if extensions else []
        self.sort_files = sort_files
        self.files = self._gather_files()

    def _gather_files(self):
        allowed_exts = set(ext.lower() for ext in self.extensions) if self.extensions else None
        all_files = []

        for entry in os.listdir(self.directory):
            full_path = os.path.join(self.directory, entry)
            if not os.path.isfile(full_path):
                continue

            _, ext = os.path.splitext(entry)
            if allowed_exts and ext.lower() not in allowed_exts:
                continue

            all_files.append(full_path)

        if self.sort_files:
            return sorted(all_files)

        return list(all_files)

    def __len__(self):
        return len(self.files)

    def __getitem__(self, index):
        return self.files[index]

    def __iter__(self):
        return iter(self.files)

    def __repr__(self):
        return f"<FileStack: {len(self.files)} files from directory: {self.directory}>"

    def is_empty(self):
        return len(self) == 0

    def get_range(self, start, end):
        """Return a list of file paths from index `start` up to but not including `end`."""
        return self.files[start:end]
