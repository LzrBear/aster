using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// ASCII FBX 6.0 → DirectX .x converter (one mesh, flat normals)

var fbxPath = args.Length > 0 ? args[0]
    : @"C:\Users\Sameer Saini\Desktop\aster\3D Model\3D ModelContent\Models\p1_wedge.fbx";
var xPath = args.Length > 1 ? args[1]
    : @"C:\Users\Sameer Saini\Desktop\aster\3D Model\3D ModelContent\Models\p1_wedge.x";

Console.WriteLine($"Reading {fbxPath}");
string raw = File.ReadAllText(fbxPath);

static float[] ParseFloats(string src)
{
    var result = new List<float>();
    foreach (Match m in Regex.Matches(src, @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?"))
        if (float.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
            result.Add(f);
    return result.ToArray();
}

static int[] ParseInts(string src)
{
    var result = new List<int>();
    foreach (Match m in Regex.Matches(src, @"-?[0-9]+"))
        if (int.TryParse(m.Value, out int n))
            result.Add(n);
    return result.ToArray();
}

static string? ExtractBlock(string src, string keyword, int searchFrom = 0)
{
    int kIdx = src.IndexOf(keyword, searchFrom, StringComparison.Ordinal);
    if (kIdx < 0) return null;
    int open = src.IndexOf('{', kIdx);
    if (open < 0) return null;
    int depth = 1, i = open + 1;
    while (i < src.Length && depth > 0)
    {
        if (src[i] == '{') depth++;
        else if (src[i] == '}') depth--;
        i++;
    }
    return src.Substring(open + 1, i - open - 2);
}

static string? ExtractField(string block, string field)
{
    int idx = block.IndexOf(field + ":", StringComparison.Ordinal);
    if (idx < 0) return null;
    int start = idx + field.Length + 1;
    // Collect until a line that (after trimming whitespace) does NOT start with comma or digit/sign
    // i.e. collect the value line plus all continuation lines
    var sb2 = new StringBuilder();
    int pos = start;
    while (pos < block.Length)
    {
        int lineEnd = block.IndexOf('\n', pos);
        if (lineEnd < 0) lineEnd = block.Length;
        string line = block.Substring(pos, lineEnd - pos);
        string trimmed = line.TrimStart();
        // First line always included; subsequent lines only if they are continuations
        if (pos != start)
        {
            // Continuation: starts with comma, digit, sign, or dot
            if (trimmed.Length == 0 || (!trimmed.StartsWith(",") && !char.IsDigit(trimmed[0]) && trimmed[0] != '-' && trimmed[0] != '+' && trimmed[0] != '.'))
                break;
        }
        sb2.Append(line);
        pos = lineEnd + 1;
    }
    return sb2.ToString().Trim();
}

string? meshBlock = ExtractBlock(raw, "\"Mesh\"");
if (meshBlock == null) { Console.Error.WriteLine("No Mesh block found"); Environment.Exit(1); return; }

string? vertSrc = ExtractField(meshBlock, "Vertices");
if (vertSrc == null) { Console.Error.WriteLine("No Vertices field"); Environment.Exit(1); return; }
float[] vf = ParseFloats(vertSrc);
int vCount = vf.Length / 3;
Console.WriteLine($"  Vertices: {vCount}");

string? pidxSrc = ExtractField(meshBlock, "PolygonVertexIndex");
if (pidxSrc == null) { Console.Error.WriteLine("No PolygonVertexIndex field"); Environment.Exit(1); return; }
int[] rawIdx = ParseInts(pidxSrc);

var faces = new List<int[]>();
var face = new List<int>();
foreach (int idx in rawIdx)
{
    if (idx < 0) { face.Add(-idx - 1); faces.Add(face.ToArray()); face = new List<int>(); }
    else face.Add(idx);
}
if (face.Count > 0) faces.Add(face.ToArray());

var tris = new List<int[]>();
foreach (var f in faces)
    for (int i = 1; i < f.Length - 1; i++)
        tris.Add(new[] { f[0], f[i], f[i + 1] });
Console.WriteLine($"  Polygons: {faces.Count}, Triangles: {tris.Count}");

// UVs
float[]? uvs = null;
int[]? uvIdx = null;
string? uvBlock = ExtractBlock(meshBlock, "LayerElementUV");
if (uvBlock != null)
{
    string? us = ExtractField(uvBlock, "UV");
    if (us != null) uvs = ParseFloats(us);
    string? ui = ExtractField(uvBlock, "UVIndex");
    if (ui != null) uvIdx = ParseInts(ui);
}
Console.WriteLine($"  UVs: {(uvs != null ? uvs.Length / 2 : 0)}");

var sb = new StringBuilder();
sb.AppendLine("xof 0303txt 0032");
sb.AppendLine();
sb.AppendLine("Frame Root {");
// 180° rotation around Z to flip the model nose direction
sb.AppendLine("  FrameTransformMatrix {");
sb.AppendLine("    -1.000000, 0.000000, 0.000000, 0.000000,");
sb.AppendLine("    0.000000, -1.000000, 0.000000, 0.000000,");
sb.AppendLine("    0.000000, 0.000000, 1.000000, 0.000000,");
sb.AppendLine("    0.000000, 0.000000, 0.000000, 1.000000;;");
sb.AppendLine("  }");
sb.AppendLine("  Mesh {");
sb.AppendLine($"    {vCount};");
for (int i = 0; i < vCount; i++)
{
    char sep = (i < vCount - 1) ? ',' : ';';
    sb.AppendLine(FormattableString.Invariant($"    {vf[i*3]:F6};{vf[i*3+1]:F6};{vf[i*3+2]:F6};{sep}"));
}
sb.AppendLine($"    {tris.Count};");
for (int i = 0; i < tris.Count; i++)
{
    var t = tris[i];
    char sep = (i < tris.Count - 1) ? ',' : ';';
    sb.AppendLine(FormattableString.Invariant($"    3;{t[0]},{t[1]},{t[2]};{sep}"));
}

// Flat normals
sb.AppendLine("    MeshNormals {");
sb.AppendLine($"      {tris.Count};");
for (int i = 0; i < tris.Count; i++)
{
    var t = tris[i];
    float ax = vf[t[0]*3], ay = vf[t[0]*3+1], az = vf[t[0]*3+2];
    float bx = vf[t[1]*3], by = vf[t[1]*3+1], bz = vf[t[1]*3+2];
    float cx = vf[t[2]*3], cy = vf[t[2]*3+1], cz = vf[t[2]*3+2];
    float ux = bx-ax, uy = by-ay, uz = bz-az;
    float vx = cx-ax, vy = cy-ay, vz = cz-az;
    float nx = uy*vz - uz*vy, ny = uz*vx - ux*vz, nz = ux*vy - uy*vx;
    float len = MathF.Sqrt(nx*nx + ny*ny + nz*nz);
    if (len > 0) { nx /= len; ny /= len; nz /= len; }
    char sep = (i < tris.Count - 1) ? ',' : ';';
    sb.AppendLine(FormattableString.Invariant($"      {nx:F6};{ny:F6};{nz:F6};{sep}"));
}
sb.AppendLine($"      {tris.Count};");
for (int i = 0; i < tris.Count; i++)
{
    char sep = (i < tris.Count - 1) ? ',' : ';';
    sb.AppendLine($"      3;{i},{i},{i};{sep}");
}
sb.AppendLine("    }");

if (uvs != null && uvIdx != null && uvIdx.Length > 0)
{
    var uvPerVertex = new (float u, float v, int count)[vCount];
    int pvIdx = 0;
    foreach (var f2 in faces)
    {
        for (int k = 0; k < f2.Length; k++)
        {
            if (pvIdx < uvIdx.Length)
            {
                int ui2 = uvIdx[pvIdx];
                if (ui2 >= 0 && ui2*2+1 < uvs.Length && f2[k] < vCount)
                {
                    uvPerVertex[f2[k]].u += uvs[ui2*2];
                    uvPerVertex[f2[k]].v += uvs[ui2*2+1];
                    uvPerVertex[f2[k]].count++;
                }
            }
            pvIdx++;
        }
    }
    sb.AppendLine("    MeshTextureCoords {");
    sb.AppendLine($"      {vCount};");
    for (int i = 0; i < vCount; i++)
    {
        float u = uvPerVertex[i].count > 0 ? uvPerVertex[i].u / uvPerVertex[i].count : 0;
        float v2 = uvPerVertex[i].count > 0 ? uvPerVertex[i].v / uvPerVertex[i].count : 0;
        char sep = (i < vCount - 1) ? ',' : ';';
        sb.AppendLine(FormattableString.Invariant($"      {u:F6};{v2:F6};{sep}"));
    }
    sb.AppendLine("    }");
}

sb.AppendLine("    MeshMaterialList {");
sb.AppendLine("      1;");
sb.AppendLine($"      {tris.Count};");
sb.AppendLine("      " + string.Join(",", new string[tris.Count].AsSpan().ToArray().Select(_ => "0")) + ";");
sb.AppendLine("      Material {");
sb.AppendLine("        1.000000;1.000000;1.000000;1.000000;;");
sb.AppendLine("        0.000000;");
sb.AppendLine("        0.000000;0.000000;0.000000;;");
sb.AppendLine("        0.000000;0.000000;0.000000;;");
sb.AppendLine("        TextureFilename { \"../Textures/wedge_p1_diff_v1.tga\"; }");
sb.AppendLine("      }");
sb.AppendLine("    }");
sb.AppendLine("  }");
sb.AppendLine("}");

File.WriteAllText(xPath, sb.ToString());
Console.WriteLine($"Written: {xPath}");
