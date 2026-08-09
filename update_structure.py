import re

path = r'C:\Users\franc\Desktop\TileStories\.clinerules\10-structure.md'
content = open(path, encoding='utf-8').read()

# 1. Add MarkerHierarchyResolver.cs after StatusRamp.cs section
old1 = "|   |   |   |   |   Unknown). Consumed by MarkerRingView.\n|   |   |   |   +-- POIPool.cs"
# We need actual tree chars. Let's find the exact substring.
idx = content.find("Unknown). Consumed by MarkerRingView.")
if idx < 0:
    print("ERROR: could not find StatusRamp section")
    exit(1)

# Get the exact text around this point
old1_real = content[idx:idx+300]
print("DEBUG found text:", repr(old1_real[:80]))
