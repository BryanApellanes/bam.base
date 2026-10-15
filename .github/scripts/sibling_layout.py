"""Fetch the sibling framework repos a checkout's projects reference, each at the commit bamtk records.

Bam framework repos reference each other as siblings (..\\..\\bam.test\\bam.test\\bam.test.csproj), the
layout bamtk's submodules/ directory gives them. CI has only this repo, so this script rebuilds that
layout next to it: it walks the ProjectReferences of every project under the checkout, transitively, and
for each sibling it fetches the commit bamtk's default branch records for submodules/<name>. Pinning to
bamtk's gitlinks means CI builds against the framework versions bamtk ships, not whatever each repo's
default branch holds today.

Usage: python sibling_layout.py <checkout-dir> [bamtk-ref]
"""
import os
import re
import subprocess
import sys

OWNER_URL = "https://github.com/BryanApellanes/{}.git"
REFERENCE = re.compile(r'Include="\.\.[\\/]\.\.[\\/]([^\\/"]+)[\\/]')


def run(*args, cwd=None):
    return subprocess.run(args, cwd=cwd, check=True, capture_output=True, text=True).stdout


def referenced_siblings(directory):
    found = set()
    for root, dirs, files in os.walk(directory):
        dirs[:] = [d for d in dirs if d not in ("bin", "obj", ".git")]
        for name in files:
            if name.endswith(".csproj"):
                with open(os.path.join(root, name), encoding="utf-8-sig") as project:
                    found.update(REFERENCE.findall(project.read()))
    return found


def main():
    checkout = os.path.abspath(sys.argv[1])
    bamtk_ref = sys.argv[2] if len(sys.argv) > 2 else "bamtk"
    parent = os.path.dirname(checkout)
    own = os.path.basename(checkout)
    index = os.path.join(parent, ".bamtk-index")
    if not os.path.isdir(index):
        run("git", "clone", "--quiet", "--depth", "1", "--filter=blob:none", "--no-checkout",
            "--branch", bamtk_ref, OWNER_URL.format("bamtk"), index)

    pending = sorted(referenced_siblings(checkout))
    done = {own}
    while pending:
        name = pending.pop()
        if name in done:
            continue
        done.add(name)
        tree = run("git", "ls-tree", "HEAD", "submodules/" + name, cwd=index).split()
        if len(tree) < 3 or tree[1] != "commit":
            sys.exit(f"bamtk records no submodule named {name}; cannot pin it")
        sha = tree[2]
        target = os.path.join(parent, name)
        run("git", "init", "--quiet", target)
        run("git", "fetch", "--quiet", "--depth", "1", OWNER_URL.format(name), sha, cwd=target)
        run("git", "checkout", "--quiet", "FETCH_HEAD", cwd=target)
        print(f"{name} @ {sha}")
        pending.extend(sorted(referenced_siblings(target) - done))


if __name__ == "__main__":
    main()
