import os
import glob


class FileStack:
    def __init__(self, directory, extensions=None, sort_files=False):
        self.directory = directory
        self.extensions = extensions if extensions else []
        self.sort_files = sort_files
        self.files = self._gather_files()

    def _gather_files(self):
        all_files = set()

        if self.extensions:
            for ext in self.extensions:
                pattern = os.path.join(self.directory, f'*{ext}')
                all_files.update(glob.glob(pattern))
        else:
            # No extensions specified: include all files
            all_files = glob.glob(os.path.join(self.directory, '*'))

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
