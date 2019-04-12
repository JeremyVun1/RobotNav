import os

OLD_BASE = "C:/Users/elode/Documents/swinburne/2018_sem2/COS20007 oop/custom proj/TaskForceUltra/src"
NEW_BASE = "C:/Users/elode/Documents/swinburne/2018_sem2/COS20007 oop/custom proj/TaskForceUltra/copy"
NEW_NAME = 'merged.cs'

def merge_files(infiles, outfile):
    with open(outfile, 'wb') as fo:
        for infile in infiles:
            with open(infile, 'rb') as fi:
                fo.write(fi.read())


for (dirpath, dirnames, filenames) in os.walk(OLD_BASE):
    base, tail = os.path.split(dirpath)
    if base != OLD_BASE: continue  # Don't operate on OLD_BASE, only children directories

    # Build infiles list
    infiles = sorted([os.path.join(dirpath, filename) for filename in filenames])

    # Create output directory
    new_dir =  os.path.join(NEW_BASE, tail)
    os.mkdir(new_dir)  # This will raise an OSError if the directory already exists

    # Build outfile name
    outfile = os.path.join(new_dir, NEW_NAME)

    # Merge
    merge_files(infiles, outfile)