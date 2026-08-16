import subprocess, re, os, glob

ROOT = r"E:\GitHub Unity Projects\Boltz-Adventure"
os.chdir(ROOT)


def root_active(text, name):
    """m_IsActive of the GameObject block whose m_Name matches `name`."""
    for blk in text.split("--- !u!"):
        if not blk.startswith("1 &"):      # !u!1 == GameObject
            continue
        m = re.search(r"^\s*m_Name:\s*(.+?)\s*$", blk, re.M)
        a = re.search(r"^\s*m_IsActive:\s*(\d)", blk, re.M)
        if m and a and m.group(1) == name:
            return a.group(1)
    return "?"


print("%-22s %6s %6s" % ("BALL", "HEAD", "DISK"))
for f in sorted(glob.glob("Assets/Prefab/Balls/*.prefab")):
    name = os.path.splitext(os.path.basename(f))[0]
    gitpath = "HEAD:" + f.replace("\\", "/")
    head = subprocess.run(["git", "show", gitpath],
                          capture_output=True, text=True).stdout
    disk = open(f, encoding="utf-8", errors="ignore").read()
    print("%-22s %6s %6s" % (name, root_active(head, name), root_active(disk, name)))
