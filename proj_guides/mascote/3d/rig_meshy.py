# Builds the Lab's 3D mascots from the developer's Meshy models (proj_guides/mascote/3d/meshy/*.glb).
# Run inside Blender 5.2 (blender-mcp execute_blender_code):  exec(open(r"...\rig_meshy.py").read()); build("mouse")
# Each model: import, reduce to a phone-sized mesh, (explorer) paint its colour from the picture, a skeleton, weights,
# the front arm as its own mesh, blink lids and mouth morphs, the seven clips of the mascot contract (_3.7 section 1),
# and a GLB next to this file. Landmarks are pixels of an 800 x 800 front orthographic render (400 px = 1 unit,
# x = (px - 400) / 400, z = 1.95 - py / 400) of the reduced model standing at x = 0 on z = 0.
import bpy, bmesh, math, os
import numpy as np
from mathutils import Vector, Quaternion
from mathutils.bvhtree import BVHTree

HERE = r"C:\Users\franc\Desktop\TileStories\proj_guides\mascote\3d"
PICS = r"C:\Users\franc\Desktop\TileStories\proj_guides\mascote"

SPEC = {
    "azu": dict(src="azu_meshy.glb", weld=False, tris=24000, prefix="AzuM", face="body", weights="rules",
                scale=1 / 1.2625,  # the slab is one tile
                bones={"root": ((392, 800), (392, 760), None), "body": ((392, 690), (392, 437), "root"),
                       "head": ((392, 185), (392, 50), "body"),
                       "arm.R": ((150, 470), (60, 580), "body"), "arm.L": ((630, 440), (735, 350), "body"),
                       "leg.R": ((300, 690), (290, 785), "root"), "leg.L": ((470, 690), (480, 785), "root")},
                eyes=[(290, 380, 59, 68), (497, 376, 59, 68)], mouth=(395, 437, 30),
                think=(0.35, -0.25, 0.9)),  # the slab is too wide for a hand at the chin: the hand goes up by its corner
    "mouse": dict(src="mouse_meshy.glb", weld=True, tris=24000, prefix="Mouse", face="head", weights="heat", arm_reach=(0.09, 0.14), gestures="fused",
                  scale=1 / 1.2625,
                  bones={"root": ((375, 800), (375, 760), None), "body": ((375, 690), (375, 420), "root"),
                         "head": ((375, 420), (375, 60), "body"),
                         "arm.R": ((280, 445), (235, 545), "body"), "arm.L": ((480, 448), (525, 590), "body"),
                         "leg.R": ((330, 690), (325, 775), "root"), "leg.L": ((420, 690), (430, 775), "root"),
                         "tail1": ((470, 700, "back"), (560, 725), "body"), "tail2": ((560, 725), (640, 680), "tail1"),
                         "tail3": ((640, 680), (665, 555), "tail2")},
                  eyes=[(293, 258, 32, 36), (452, 258, 32, 36)], mouth=(372, 338, 26)),
    "explorer": dict(src="explorer_meshy.glb", weld=True, tris=22000, prefix="Explorer", face="head", weights="heat", arm_reach=(0.09, 0.14), gestures="fused",
                     scale=1 / 1.2625, paint="explorer.png",
                     bones={"root": ((400, 800), (400, 760), None), "body": ((400, 690), (400, 420), "root"),
                            "head": ((400, 420), (400, 30), "body"),
                            "arm.R": ((255, 480), (205, 610), "body"), "arm.L": ((545, 480), (590, 590), "body"),
                            "leg.R": ((335, 690), (335, 775), "root"), "leg.L": ((465, 690), (465, 775), "root")},
                     eyes=[(320, 280, 31, 33), (478, 280, 31, 33)], mouth=(397, 345, 40)),
}


def P(px, py):
    return (px - 400) / 400.0, 1.95 - py / 400.0


def deselect():
    for o in bpy.context.view_layer.objects:
        o.select_set(False)


def activate(o):
    deselect()
    bpy.context.view_layer.objects.active = o
    o.select_set(True)


# ---------- 1. import and reduce ----------
def prep(name):
    s = SPEC[name]
    col = bpy.data.collections.new(s["prefix"])
    bpy.context.scene.collection.children.link(col)
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(HERE, "meshy", s["src"]))
    o = [x for x in bpy.data.objects if x not in before and x.type == 'MESH'][0]
    for c in list(o.users_collection):
        c.objects.unlink(o)
    col.objects.link(o)
    o.name = name + "_low"
    o.location = (0, 0, 0)
    bpy.context.view_layer.update()
    zmin = min((o.matrix_world @ Vector(c)).z for c in o.bound_box)
    o.location.z = -zmin
    activate(o)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if s["weld"]:  # glTF splits vertices along the texture seams; welding lets the reduction keep the shape whole
        bm = bmesh.new(); bm.from_mesh(o.data)
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
        bm.to_mesh(o.data); bm.free()
    m = o.modifiers.new("dec", 'DECIMATE')
    m.ratio = s["tris"] / len(o.data.polygons)
    m.use_collapse_triangulate = True
    bpy.ops.object.modifier_apply(modifier="dec")
    bpy.ops.object.shade_smooth()
    if s.get("paint"):
        paint(o, os.path.join(PICS, s["paint"]))
    return o


# ---------- 2. the explorer's colour, painted from the picture (Meshy gave the shape only) ----------
def paint(o, picture, N=1024):
    activate(o)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=1.15, island_margin=0.004, area_weight=0.0, correct_aspect=True, scale_to_bounds=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    im = bpy.data.images.load(picture, check_existing=True)
    W, H = im.size
    rgb = np.array(im.pixels[:], np.float32).reshape(H, W, im.channels)[::-1][:, :, :3]
    ink = rgb.min(axis=2) < 0.9
    ink[618:, :] = False  # the floor shadow
    src = rgb.copy(); filled = ink.copy()
    for _ in range(40):  # the drawing's edge colours spread outward: an edge miss never samples the white page
        if filled.all(): break
        acc = np.zeros_like(src); cnt = np.zeros((H, W))
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            f = np.roll(filled, (dy, dx), (0, 1)); acc += np.roll(src, (dy, dx), (0, 1)) * f[..., None]; cnt += f
        new = (~filled) & (cnt > 0); src[new] = acc[new] / cnt[new][:, None]; filled |= new
    hair = np.array([226, 203, 172]) / 255.; navy = np.array([33, 45, 84]) / 255.
    me = o.data; me.calc_loop_triangles()
    co = np.array([v.co for v in me.vertices]); nv = np.array([v.normal for v in me.vertices])
    zt = co[:, 2].max(); k = zt / 621.0; cx = 251.5; top = 21  # the picture's figure: 621 px from hat to soles
    uvl = me.uv_layers.active.data
    tex = np.zeros((N, N, 3), np.float32); got = np.zeros((N, N), bool)

    def samp(px, py):
        px = np.clip(px, 0, W - 1); py = np.clip(py, 0, H - 1)
        x0 = np.floor(px).astype(int); y0 = np.floor(py).astype(int); x1 = np.minimum(x0 + 1, W - 1); y1 = np.minimum(y0 + 1, H - 1)
        fx = (px - x0)[:, None]; fy = (py - y0)[:, None]
        return src[y0, x0] * (1 - fx) * (1 - fy) + src[y0, x1] * fx * (1 - fy) + src[y1, x0] * (1 - fx) * fy + src[y1, x1] * fx * fy

    for t in me.loop_triangles:
        uv = np.array([uvl[l].uv for l in t.loops]) * N
        x0, y0 = np.floor(uv.min(0)).astype(int); x1, y1 = np.ceil(uv.max(0)).astype(int)
        x0 = max(x0, 0); y0 = max(y0, 0); x1 = min(x1, N - 1); y1 = min(y1, N - 1)
        if x1 < x0 or y1 < y0: continue
        gx, gy = np.meshgrid(np.arange(x0, x1 + 1) + .5, np.arange(y0, y1 + 1) + .5)
        (ax, ay), (bx, by), (qx, qy) = uv
        d = (by - qy) * (ax - qx) + (qx - bx) * (ay - qy)
        if abs(d) < 1e-12: continue
        l1 = ((by - qy) * (gx - qx) + (qx - bx) * (gy - qy)) / d; l2 = ((qy - ay) * (gx - qx) + (ax - qx) * (gy - qy)) / d; l3 = 1 - l1 - l2
        m = (l1 >= -.02) & (l2 >= -.02) & (l3 >= -.02)
        if not m.any(): continue
        L = np.stack([l1[m], l2[m], l3[m]], 1); vi = list(t.vertices)
        p = L @ co[vi]; n = L @ nv[vi]; n /= np.linalg.norm(n, axis=1)[:, None] + 1e-9
        px = cx + p[:, 0] / k; py = top + (zt - p[:, 2]) / k
        c = samp(px, py)
        head = (py > 150) & (py < 350) & (np.abs(px - cx) < 190)
        tors = (py >= 350) & (py < 585) & (np.abs(px - cx) < 150)
        hat = py <= 150
        wb = np.where(head, np.clip((n[:, 1] + 0.3) / 0.35, 0, 1), np.clip((n[:, 1] - 0.05) / 0.35, 0, 1))  # facing away
        bc = c.copy(); bc[head] = hair; bc[hat] = navy
        if tors.any():  # the back of the coat: the coat's colour from beside the waistcoat
            dx = px[tors] - cx; bc[tors] = samp(cx + np.sign(dx) * (72 + np.abs(dx) * 0.42), py[tors])
        c = c * (1 - wb[:, None]) + bc * wb[:, None]
        up = np.where(py < 215, np.clip((n[:, 2] - 0.3) / 0.3, 0, 1), 0)[:, None]  # the hat's top
        c = c * (1 - up) + navy * up
        under = np.clip((-n[:, 2] - 0.5) / 0.4, 0, 1)[:, None]; c = c * (1 - 0.3 * under)
        yy = gy[m].astype(int); xx = gx[m].astype(int); tex[yy, xx] = c; got[yy, xx] = True
    for _ in range(8):  # pad the islands
        acc = np.zeros_like(tex); cnt = np.zeros((N, N))
        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            f = np.roll(got, (dy, dx), (0, 1)); acc += np.roll(tex, (dy, dx), (0, 1)) * f[..., None]; cnt += f
        new = (~got) & (cnt > 0); tex[new] = acc[new] / cnt[new][:, None]; got |= new
    img = bpy.data.images.new("explorer_color", N, N)
    img.pixels[:] = np.concatenate([tex, np.ones((N, N, 1), np.float32)], 2).ravel()
    img.filepath_raw = os.path.join(HERE, "explorer_color.png"); img.file_format = 'PNG'; img.save()
    mat = bpy.data.materials.new("Explorer"); mat.use_nodes = True
    bs = next(nd for nd in mat.node_tree.nodes if nd.type == "BSDF_PRINCIPLED")
    ti = mat.node_tree.nodes.new("ShaderNodeTexImage"); ti.image = img
    mat.node_tree.links.new(ti.outputs[0], bs.inputs["Base Color"]); bs.inputs["Roughness"].default_value = 0.62
    me.materials.clear(); me.materials.append(mat)


# ---------- helpers: the surface, its colour ----------
class Surf:
    def __init__(self, o):
        self.o = o
        self.bvh = BVHTree.FromObject(o, bpy.context.evaluated_depsgraph_get())
        self.me = o.data
        img = None
        for mat in o.data.materials:
            for nd in mat.node_tree.nodes:
                if nd.type == 'TEX_IMAGE' and any(l.to_socket.name == "Base Color" for l in nd.outputs[0].links):
                    img = nd.image
        self.img = img
        if img:
            w, h = img.size
            self.pix = np.array(img.pixels[:], np.float32).reshape(h, w, img.channels)

    def front(self, x, z):
        hit = self.bvh.ray_cast(Vector((x, -5, z)), Vector((0, 1, 0)))
        return hit

    def back(self, x, z):
        return self.bvh.ray_cast(Vector((x, 5, z)), Vector((0, -1, 0)))

    def mid_y(self, x, z, mode="mid"):
        f = self.front(x, z); b = self.back(x, z)
        if mode == "back" and b[0]:
            return b[0].y - 0.04
        if f[0] and b[0]:
            return (f[0].y + b[0].y) / 2
        return 0.0

    def color(self, x, z):  # the texture's colour where the front ray lands (linear, for a material)
        loc, nrm, idx, dist = self.front(x, z)
        if loc is None or self.img is None:
            return (0.8, 0.8, 0.8)
        poly = self.me.polygons[idx]
        vs = [self.me.vertices[i].co for i in poly.vertices]
        uvs = [self.me.uv_layers.active.data[li].uv for li in poly.loop_indices]
        a, b, c = vs[0], vs[1], vs[2]
        v0, v1, v2 = b - a, c - a, loc - a
        d00, d01, d11, d20, d21 = v0.dot(v0), v0.dot(v1), v1.dot(v1), v2.dot(v0), v2.dot(v1)
        den = d00 * d11 - d01 * d01 or 1e-9
        v = (d11 * d20 - d01 * d21) / den; w = (d00 * d21 - d01 * d20) / den; u = 1 - v - w
        uv = uvs[0] * u + uvs[1] * v + uvs[2] * w
        h, wd = self.pix.shape[:2]
        px = self.pix[min(h - 1, max(0, int(uv.y % 1 * h))), min(wd - 1, max(0, int(uv.x % 1 * wd)))][:3]
        lin = lambda s: s / 12.92 if s <= 0.04045 else ((s + 0.055) / 1.055) ** 2.4
        return tuple(lin(float(s)) for s in px) if not self.img.is_float else tuple(float(s) for s in px)


# ---------- 3. the skeleton and the weights ----------
def skeleton(name, o, sf):
    s = SPEC[name]
    ad = bpy.data.armatures.new(s["prefix"] + "_Rig")
    arm = bpy.data.objects.new(s["prefix"] + "_Rig", ad)
    o.users_collection[0].objects.link(arm)
    activate(arm)
    bpy.ops.object.mode_set(mode='EDIT')
    for bn, (h, t, parent) in s["bones"].items():
        eb = ad.edit_bones.new(bn)
        hx, hz = P(h[0], h[1]); tx, tz = P(t[0], t[1])
        hm = h[2] if len(h) > 2 else "mid"
        eb.head = (hx, sf.mid_y(hx, hz, hm), hz)
        eb.tail = (tx, sf.mid_y(tx, tz), tz)
        if bn == "root":
            eb.head.y = eb.tail.y = 0
        if bn in ("body", "head"):
            eb.tail.y = eb.head.y
        eb.roll = 0
    for bn, (h, t, parent) in s["bones"].items():
        if parent:
            ad.edit_bones[bn].parent = ad.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    for bn in ad.bones:
        bn.use_deform = bn.name != "root"
    return arm


def weigh(name, o, arm):
    s = SPEC[name]
    if s["weights"] == "heat":
        deselect(); o.select_set(True); arm.select_set(True); bpy.context.view_layer.objects.active = arm
        bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    else:  # the tile: rigid parts (a ceramic slab must not bend)
        deselect(); o.select_set(True); arm.select_set(True); bpy.context.view_layer.objects.active = arm
        bpy.ops.object.parent_set(type='ARMATURE_NAME')
        g = {vg.name: vg for vg in o.vertex_groups}
        x0, z0 = P(145, 690); x1, z1 = P(640, 185)
        for v in o.data.vertices:
            x, z = v.co.x, v.co.z
            if z > 1.508: b = "head"  # the slab, measured: z 0.212 to 1.504
            elif x < -0.665 and z > z0 - 0.02: b = "arm.R"  # the slab's edges, measured: -0.65 and 0.605
            elif x > 0.616 and z > z0 - 0.02: b = "arm.L"
            elif z < 0.205: b = "leg.L" if x > -0.04 else "leg.R"
            else: b = "body"
            g[b].add([v.index], 1.0, 'REPLACE')
    if s.get("arm_reach"):  # heat weights reach into the coat; an arm keeps only what lies near its bone
        r0, r1 = s["arm_reach"]
        gb = o.vertex_groups["body"]
        for bn in ("arm.L", "arm.R"):
            g = o.vertex_groups[bn]; b = arm.data.bones[bn]
            a, c = b.head_local, b.tail_local; ab = c - a
            for v in o.data.vertices:
                w = next((ge.weight for ge in v.groups if ge.group == g.index), 0.0)
                if w <= 0: continue
                t = max(0.0, min(1.0, (v.co - a).dot(ab) / ab.dot(ab)))
                d = (v.co - (a + ab * t)).length
                k = 1.0 if d <= r0 else 0.0 if d >= r1 else 1 - (d - r0) / (r1 - r0)
                if k < 1:
                    g.add([v.index], w * k, 'REPLACE'); gb.add([v.index], w * (1 - k), 'ADD')
    # every vertex must follow some bone
    vg_ids = {vg.index for vg in o.vertex_groups if vg.name != "root"}
    loose = [v.index for v in o.data.vertices if not any(ge.group in vg_ids and ge.weight > 0.01 for ge in v.groups)]
    if loose:
        o.vertex_groups["body"].add(loose, 1.0, 'REPLACE')
    return len(loose)


def split_front_arm(name, o):
    """The front arm (the character's left, x+) becomes its own mesh on the same skeleton (drawn in front of the card).
    The cut runs where both sides carry the same weights, so the seam never opens."""
    s = SPEC[name]
    gi = o.vertex_groups["arm.L"].index
    wl = np.zeros(len(o.data.vertices))
    for v in o.data.vertices:
        for ge in v.groups:
            if ge.group == gi: wl[v.index] = ge.weight
    sel = np.array([all(wl[i] >= 0.5 for i in p.vertices) for p in o.data.polygons])
    front = o.copy(); front.data = o.data.copy()
    for c in o.users_collection: c.objects.link(front)
    for ob, drop in ((o, sel), (front, ~sel)):  # each keeps its own faces; the vertex groups come along
        bm = bmesh.new(); bm.from_mesh(ob.data); bm.faces.ensure_lookup_table()
        bmesh.ops.delete(bm, geom=[f for f in bm.faces if drop[f.index]], context='FACES')
        bm.to_mesh(ob.data); bm.free()
    o.name = s["prefix"] + "_Body"; front.name = s["prefix"] + "_ArmFront"
    return front, int(sel.sum())


# ---------- 4. the face: blink lids and the mouth's morphs, on the surface ----------
def mat_rgb(nm, rgb, rough=0.55):
    m = bpy.data.materials.get(nm) or bpy.data.materials.new(nm)
    m.use_nodes = True
    bs = next(nd for nd in m.node_tree.nodes if nd.type == "BSDF_PRINCIPLED")
    bs.inputs["Base Color"].default_value = (*rgb, 1); bs.inputs["Roughness"].default_value = rough
    return m


def on_surface(sf, x, z, lift):
    hit = sf.front(x, z)
    y = hit[0].y if hit[0] else 0.0
    return Vector((x, y - lift, z))


def face(name, sf, arm):
    s = SPEC[name]; pre = s["prefix"]; col = arm.users_collection[0]
    lift = 0.006
    # eyes: each lid is rows x columns; at rest every row sits on the eye's top arc (no area), Blink spreads them over the eye
    R, C = 12, 18  # the last band of rows is the lash line
    verts_b, verts_k, faces, fmat = [], [], [], []
    lid_rgb = None
    for (cx, cy, rx, ry) in s["eyes"]:
        ex, ez = P(cx, cy); erx = rx / 400 * 1.1; erz = ry / 400 * 1.1
        if lid_rgb is None:
            lid_rgb = sf.color(ex, ez + erz + 0.025)
        base = len(verts_b)
        for r in range(R):
            f = r / (R - 1)
            for c in range(C):
                u = -1 + 2 * c / (C - 1); h = math.sqrt(max(0.0, 1 - u * u))
                x = ex + u * erx
                zt = ez + erz * h; zb = ez - erz * h * 0.95
                verts_b.append(on_surface(sf, x, zt, lift))
                verts_k.append(on_surface(sf, x, zt + (zb - zt) * f, lift))
        for r in range(R - 1):
            for c in range(C - 1):
                a = base + r * C + c
                faces.append((a, a + 1, a + C + 1, a + C)); fmat.append(1 if r == R - 2 else 0)
    eyes = mesh_with_keys(pre + "_Eyes", verts_b, faces, fmat, {"Blink": verts_k},
                          [mat_rgb(pre + "_Lid", lid_rgb, 0.85), mat_rgb(pre + "_Lash", (0.02, 0.02, 0.035), 0.7)], col)
    # mouth: a fan; at rest a flat line (no area), the keys open it into ovals
    mx, mz, hw = s["mouth"][0], s["mouth"][1], s["mouth"][2] / 400
    mx, mz = P(mx, mz)
    K = 24
    def ring(ox, oz, rx, rz):
        return [on_surface(sf, mx + ox + rx * math.cos(2 * math.pi * i / K), mz + oz + rz * math.sin(2 * math.pi * i / K), lift)
                for i in range(K)] + [on_surface(sf, mx + ox, mz + oz, lift)]
    rest = ring(0, 0, 0.8 * hw, 0)
    keys = {"Open": ring(0, -0.15 * hw, 0.85 * hw, 0.7 * hw), "Wide": ring(0, -0.05 * hw, 1.05 * hw, 0.36 * hw),
            "Round": ring(0, -0.12 * hw, 0.48 * hw, 0.55 * hw)}  # each covers the painted smile
    mf = [(K, i, (i + 1) % K) for i in range(K)]
    mouth = mesh_with_keys(pre + "_Mouth", rest, mf, [0] * K, keys, [mat_rgb(pre + "_MouthIn", (0.07, 0.012, 0.018), 0.45)], col)
    for ob in (eyes, mouth):
        ob.parent = arm
        vg = ob.vertex_groups.new(name=s["face"]); vg.add(list(range(len(ob.data.vertices))), 1.0, 'REPLACE')
        md = ob.modifiers.new("Armature", 'ARMATURE'); md.object = arm
    return eyes, mouth


def mesh_with_keys(nm, basis, faces, fmat, keys, mats, col):
    me = bpy.data.meshes.new(nm)
    me.from_pydata([tuple(v) for v in basis], [], faces)
    for m in mats: me.materials.append(m)
    for p, i in zip(me.polygons, fmat): p.material_index = i
    me.shade_flat() if hasattr(me, "shade_flat") else None
    ob = bpy.data.objects.new(nm, me); col.objects.link(ob)
    ob.shape_key_add(name="Basis")
    for k, vs in keys.items():
        sk = ob.shape_key_add(name=k)
        for i, v in enumerate(vs): sk.data[i].co = v
        sk.value = 0.0  # Blender 5.2 starts new keys at 1
    return ob


# ---------- 5. the clips ----------
def qlocal(arm, bn, R):
    M = arm.data.bones[bn].matrix_local.to_3x3().to_quaternion()
    return M.inverted() @ R @ M


def rest_dir(arm, bn):
    b = arm.data.bones[bn]
    return (b.tail_local - b.head_local).normalized()


def aim(arm, bn, d):
    return rest_dir(arm, bn).rotation_difference(Vector(d).normalized())


def rot(axis, deg):
    return Quaternion(Vector(axis), math.radians(deg))


X, Y, Z = (1, 0, 0), (0, 1, 0), (0, 0, 1)


def clips(name, arm):
    s = SPEC[name]
    bones = [b.name for b in arm.data.bones]
    dl = rest_dir(arm, "arm.L"); dr = rest_dir(arm, "arm.R")
    if dl.z > -0.2:  # an arm modelled raised (Azu waves in Meshy's pose): it hangs like the other arm
        dl = Vector((-dr.x, dr.y, dr.z))
    DOWN_L = aim(arm, "arm.L", dl)
    fused = s.get("gestures") == "fused"
    G_TALK = (0.55, -0.6, -0.4) if fused else (0.6, -0.75, 0.05)
    G_THINK = (0.25, -0.6, -0.75) if fused else s.get("think", (-0.25, -0.75, 0.6))
    G_POINT = (0.75, -0.45, -0.45) if fused else (0.85, -0.45, 0.28)
    G_WAVE, SWING = ((0.8, -0.35, -0.2), 12) if fused else ((0.5, -0.3, 0.81), 15)
    I = Quaternion()
    tail = [b for b in bones if b.startswith("tail")]

    def blend(a, b, w):
        return a.slerp(b, max(0.0, min(1.0, w)))

    def pose_idle(t):
        sn = math.sin(2 * math.pi * t)
        p = {"body": rot(Y, 1.5 * sn), "head": rot(Y, -2.5 * sn) @ rot(X, 1.0 * math.sin(4 * math.pi * t)),
             "arm.L": rot(Y, 3 * sn) @ DOWN_L, "arm.R": rot(Y, 3 * sn)}
        for i, b in enumerate(tail): p[b] = rot(Z, (10 + 4 * i) * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.012 * (1 - math.cos(2 * math.pi * t)) / 2

    def pose_talk(t):
        sn = math.sin(2 * math.pi * t); s2 = math.sin(4 * math.pi * t)
        g = aim(arm, "arm.L", G_TALK)
        p = {"body": rot(X, 2 * sn), "head": rot(X, 4 * s2) @ rot(Y, 2 * sn),
             "arm.L": blend(DOWN_L, g, 0.55 + 0.3 * sn), "arm.R": rot(X, -6 * sn)}
        for i, b in enumerate(tail): p[b] = rot(Z, 8 * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.015 * abs(s2)

    def pose_listen(t):
        sn = math.sin(2 * math.pi * t)
        p = {"body": rot(X, 3), "head": rot(Y, 10 + 1.5 * sn), "arm.L": DOWN_L, "arm.R": I}
        for i, b in enumerate(tail): p[b] = rot(Z, 6 * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.0

    def pose_think(t):
        sn = math.sin(2 * math.pi * t)
        p = {"body": rot(Y, -2), "head": rot(Y, -8 + 2 * sn) @ rot(X, -5),
             "arm.L": rot(Y, -3 * sn) @ aim(arm, "arm.L", G_THINK), "arm.R": I}
        for i, b in enumerate(tail): p[b] = rot(Z, 14 * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.0

    def pose_point(t):
        e = min(1.0, t / 0.4); e = e * e * (3 - 2 * e)
        p = {"body": rot(Y, 4 * e), "head": rot(Y, 3 * e), "arm.L": blend(DOWN_L, aim(arm, "arm.L", G_POINT), e), "arm.R": I}
        for b in tail: p[b] = rot(Z, 10 * e)
        return p, 0.0

    def pose_walk(t):
        sn = math.sin(2 * math.pi * t)
        p = {"body": rot(Y, 3 * sn), "head": rot(Y, -2 * sn), "leg.L": rot(X, -22 * sn), "leg.R": rot(X, 22 * sn),
             "arm.L": rot(X, 15 * sn) @ DOWN_L, "arm.R": rot(X, -15 * sn)}
        for i, b in enumerate(tail): p[b] = rot(Z, 16 * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.035 * abs(sn)

    def pose_wave(t):
        sn = math.sin(2 * math.pi * t)
        p = {"body": rot(Y, 2 * sn), "head": rot(Y, 4 * sn),
             "arm.L": rot(Y, SWING * sn) @ aim(arm, "arm.L", G_WAVE), "arm.R": I}
        for i, b in enumerate(tail): p[b] = rot(Z, 12 * math.sin(2 * math.pi * t - 0.6 * i))
        return p, 0.0

    # the wave turns about the shoulder: rot(Y) applied after the aim, both in armature space
    defs = [("idle", pose_idle, 48), ("talk", pose_talk, 24), ("listen", pose_listen, 48), ("think", pose_think, 48),
            ("point", pose_point, 24), ("walk", pose_walk, 24), ("wave", pose_wave, 24)]
    arm.animation_data_create()
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
    made = []
    for cn, fn, length in defs:
        old = bpy.data.actions.get(s["prefix"] + "_" + cn)
        if old: bpy.data.actions.remove(old)
        act = bpy.data.actions.new(cn)
        act.use_fake_user = True
        arm.animation_data.action = act
        step = 4 if length > 24 else 3
        for fr in list(range(0, length, step)) + [length]:
            t = fr / length
            pose, up = fn(t)
            if s["face"] == "body":  # the tile's face is on its body: the body takes the head's turns (the hat keeps its own)
                pose["body"] = pose.get("head", I) @ pose.get("body", I)
            for pb in arm.pose.bones:
                R = pose.get(pb.name, I)
                pb.rotation_quaternion = qlocal(arm, pb.name, R) if pb.name != "root" else I
                pb.location = (0, up, 0) if pb.name == "root" else (0, 0, 0)
                pb.keyframe_insert("rotation_quaternion", frame=fr + 1)
                pb.keyframe_insert("location", frame=fr + 1)
        made.append(act)
    arm.animation_data.action = bpy.data.actions["idle"] if "idle" in bpy.data.actions else made[0]
    return made


# ---------- 6. export ----------
def shrink_textures(objs, size=1024):
    done = set()
    for o in objs:
        for m in o.data.materials:
            if not m or not m.use_nodes: continue
            for nd in m.node_tree.nodes:
                if nd.type == 'TEX_IMAGE' and nd.image and nd.image.name not in done:
                    im = nd.image; done.add(im.name)
                    lim = size if any(l.to_socket.name in ("Base Color", "Color") and l.to_node.type in ("BSDF_PRINCIPLED", "NORMAL_MAP") for l in nd.outputs[0].links) else size // 2
                    if im.size[0] > lim: im.scale(lim, lim)


def export(name, arm):
    s = SPEC[name]
    objs = [arm] + [c for c in arm.children]
    shrink_textures([o for o in objs if o.type == 'MESH'])
    deselect()
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = arm
    if not arm.get("scaled"):
        # the size is baked into the bones and the meshes (every node at scale 1): three.js frames a skinned mesh by its
        # geometry's box, which a scaled parent node would put off by the scale
        arm.scale = (s["scale"],) * 3
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        arm["scaled"] = True
    props = bpy.ops.export_scene.gltf.get_rna_type().properties.keys()
    kw = dict(filepath=os.path.join(HERE, name + ".glb"), export_format='GLB', use_selection=True,
              export_animation_mode='ACTIONS', export_morph=True, export_skins=True, export_apply=False)
    for k, v in (("export_image_format", 'JPEG'), ("export_jpeg_quality", 82), ("export_image_quality", 82),
                 ("export_yup", True), ("export_texcoords", True), ("export_normals", True),
                 ("export_force_sampling", True), ("export_anim_slide_to_zero", True)):
        if k in props: kw[k] = v
    bpy.ops.export_scene.gltf(**kw)
    return kw["filepath"]


def build(name, do_export=True):
    o = prep(name)
    sf = Surf(o)
    arm = skeleton(name, o, sf)
    loose = weigh(name, o, arm)
    front, nf = split_front_arm(name, o)
    eyes, mouth = face(name, sf, arm)
    acts = clips(name, arm)
    out = export(name, arm) if do_export else None
    print(name, "loose", loose, "front faces", nf, "clips", [a.name for a in acts], "->", out)
    return arm


# ---------- 7. the Lab's picker pictures (thumbs/<id>.png, 192 px, from each exported GLB; run in a fresh file) ----------
def thumbs(models=(("azu", "azu.glb"), ("tile", "azu_tile.glb"), ("mouse", "mouse.glb"), ("explorer", "explorer.glb"))):
    sc = bpy.context.scene
    eng = [i.identifier for i in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items]
    sc.render.engine = 'BLENDER_EEVEE' if 'BLENDER_EEVEE' in eng else 'BLENDER_EEVEE_NEXT'
    sc.render.film_transparent = True; sc.render.resolution_x = sc.render.resolution_y = 192
    sc.render.image_settings.file_format = 'PNG'; sc.render.image_settings.color_mode = 'RGBA'
    sc.view_settings.view_transform = 'Standard'
    w = bpy.data.worlds.new("W"); sc.world = w; w.use_nodes = True
    bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND'); bg.inputs[0].default_value = (1, 1, 1, 1); bg.inputs[1].default_value = 0.9
    ld = bpy.data.lights.new("Sun", 'SUN'); ld.energy = 2.2; lo = bpy.data.objects.new("Sun", ld); sc.collection.objects.link(lo); lo.rotation_euler = (0.7, 0.2, -0.5)
    cd = bpy.data.cameras.new("Cam"); cd.type = 'ORTHO'; cam = bpy.data.objects.new("Cam", cd); sc.collection.objects.link(cam); sc.camera = cam
    os.makedirs(os.path.join(HERE, "thumbs"), exist_ok=True)
    for mid, f in models:
        for o in list(bpy.data.objects):
            if o.name not in ("Sun", "Cam"): bpy.data.objects.remove(o, do_unlink=True)
        bpy.ops.import_scene.gltf(filepath=os.path.join(HERE, f))
        sc.frame_set(1); bpy.context.view_layer.update()
        dg = bpy.context.evaluated_depsgraph_get(); pts = []
        for o in bpy.data.objects:  # the importer's "Icosphere" is its bone-display shape, not the model
            if o.type != 'MESH' or o.name.startswith("Icosphere"): continue
            e = o.evaluated_get(dg); me = e.to_mesh(); pts += [e.matrix_world @ v.co for v in me.vertices]; e.to_mesh_clear()
        mn = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
        mx = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
        c = (mn + mx) / 2; yaw = math.radians(18)
        cam.rotation_euler = (math.radians(90), 0, yaw); cam.location = c + Vector((math.sin(yaw) * 10, -math.cos(yaw) * 10, 0))
        cd.ortho_scale = max(mx.x - mn.x, mx.z - mn.z) * 1.1
        sc.render.filepath = os.path.join(HERE, "thumbs", mid + ".png"); bpy.ops.render.render(write_still=True)
