using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FreeImageAPI;

public class FbxPlugin
{
	private FbxPluginScene fbxPluginScene;
	private FbxPluginOptions fbxPluginOptions;

	[DllImport("FbxPlugin")]
	private static extern bool ImportScene (ref FbxPluginOptions options);
	
	[DllImport("FbxPlugin")]
	private static extern bool ExportScene (ref FbxPluginOptions options);
	
	public void ImportModel (FbxPluginOptions options)
	{
		fbxPluginOptions = options;
		Debug.Log ("Import Success:" + ImportScene (ref fbxPluginOptions));
		fbxPluginScene = fbxPluginOptions.scene;
	}
	
	public GameObject DisplayScene ()
	{
		return DisplayScene (false);
	}
	
	public GameObject DisplayScene (bool normalCaculateByUnity)
	{
		string name = fbxPluginOptions.filename;
		if (name.Contains ("/"))
			name = name.Substring (name.LastIndexOf ("/") + 1);
		if (name.Contains ("\\"))
			name = name.Substring (name.LastIndexOf ("\\") + 1);
		name = name.Substring (0, name.LastIndexOf ("."));
		GameObject go = new GameObject (name);
		DisplayScene (go.transform, normalCaculateByUnity);
		return go;
	}
	
	public void DisplayScene (Transform parent)
	{
		DisplayScene (parent, false);
	}
	
	public void DisplayScene (Transform parent, bool normalCaculateByUnity)
	{
		for (int i = 0; i < fbxPluginScene.nodeCount; i++) {
			DisplayNode (fbxPluginScene.GetNode (i), parent, normalCaculateByUnity);
		}
	}
	
	private void DisplayNode (FbxPluginNode node, Transform parent, bool normalCaculateByUnity)
	{
		GameObject go = new GameObject (node.name);
		
		if (node.mesh.ToInt32 () != 0) {
			go.AddComponent<MeshRenderer> ();
			go.AddComponent<MeshFilter> ();
			
			FbxPluginMesh mesh = node.GetMesh ();
			
			Mesh m = new Mesh ();
			
			if (mesh.vertexCount != 0) {
				Vector3[] vertices = new Vector3[mesh.vertexCount];
				for (int i = 0; i < mesh.vertexCount; i++) {
					FbxPluginVector4 vertex = mesh.GetVertex (i);
					vertices [i] = new Vector3 (-vertex.x, vertex.y, vertex.z);
				}
				m.vertices = vertices;
			}
			
			if (mesh.uvCount != 0) {
				Vector2[] uvs = new Vector2[mesh.uvCount];
				for (int i = 0; i < mesh.uvCount; i++) {
					FbxPluginVector4 uv = mesh.GetUv (i);
					uvs [i] = new Vector2 (uv.x, uv.y);
				}
				m.uv = uvs;
			}
			
			if (mesh.uv2Count != 0) {
				Vector2[] uvs2 = new Vector2[mesh.uv2Count];
				for (int i = 0; i < mesh.uv2Count; i++) {
					FbxPluginVector4 uv2 = mesh.GetUv2 (i);
					uvs2 [i] = new Vector2 (uv2.x, uv2.y);
				}
				m.uv2 = uvs2;
			}
			
			if (mesh.normalCount != 0) {
				Vector3[] normals = new Vector3[mesh.normalCount];
				for (int i = 0; i < mesh.normalCount; i++) {
					FbxPluginVector4 normal = mesh.GetNormal (i);
					normals [i] = new Vector3 (-normal.x, normal.y, normal.z);
				}
				m.normals = normals;
			}
			
			if (mesh.tangentCount != 0) {
				Vector4[] tangents = new Vector4[mesh.tangentCount];
				for (int i = 0; i < mesh.tangentCount; i++) {
					FbxPluginVector4 tangent = mesh.GetTangent (i);
					tangents [i] = new Vector4 (-tangent.x, tangent.y, tangent.z, tangent.w);
				}
				m.tangents = tangents;
			}
			
			if (mesh.trianglelistCount != 0) {
				m.subMeshCount = mesh.trianglelistCount;
				for (int i=0; i<mesh.trianglelistCount; i++) {
					FbxPluginTrianglelist trianglelist = mesh.GetTrianglelist (i);
					if (trianglelist.triangleCount != 0) {
						int[] triangles = new int[trianglelist.triangleCount];
						for (int j=0; j<trianglelist.triangleCount; j+=3) {
							triangles [j] = trianglelist.GetTriangle (j + 2);
							triangles [j + 1] = trianglelist.GetTriangle (j + 1);
							triangles [j + 2] = trianglelist.GetTriangle (j);
						}
						m.SetTriangles (triangles, i);
					}
				}
			}
			
			if (mesh.materialCount != 0) {
				Material[] materials = new Material[mesh.materialCount];
				for (int i = 0; i < mesh.materialCount; i++) {
					FbxPluginMaterial material = mesh.GetMaterial (i);
					if (material.normalmapTexture == "")
						materials [i] = new Material (Shader.Find ("Diffuse"));
					else
						materials [i] = new Material (Shader.Find ("Bumped Diffuse"));
					materials [i].name = material.name;
					materials [i].color = new Color (material.diffuse.x, material.diffuse.y, material.diffuse.z);
					if (material.diffuseTexture != "") {
						Texture2D t2d = LoadTexture (fbxPluginOptions.filename, material.diffuseTexture);
						materials [i].mainTexture = t2d;
					}
					if (material.normalmapTexture != "") {
						Texture2D t2d = LoadTexture (fbxPluginOptions.filename, material.normalmapTexture);
						if (t2d)
							materials [i].SetTexture ("_BumpMap", ConvertNormal (t2d));
					}
				}
				go.renderer.sharedMaterials = materials;
			}
			{
				Material[] materials = new Material[m.subMeshCount];
				int i = 0;
				foreach (Material mat in go.renderer.sharedMaterials) 
					materials [i++] = mat ? mat : new Material (Shader.Find ("Diffuse"));
				for (; i<m.subMeshCount; i++)
					materials [i] = new Material (Shader.Find ("Diffuse"));
				go.renderer.sharedMaterials = materials;
			}
			
			if (normalCaculateByUnity || m.normals.Length == 0)
				m.RecalculateNormals ();
			m.RecalculateBounds ();
			go.GetComponent<MeshFilter> ().sharedMesh = m;
		}

		for (int i = 0; i < node.nodeCount; i++) {
			DisplayNode (node.GetNode (i), go.transform, normalCaculateByUnity);
		}
		
		go.transform.localPosition = new Vector3 (-node.position.x, node.position.y, node.position.z);
		go.transform.localEulerAngles = ConvertRotation (node.rotation);
		go.transform.localScale = new Vector3 (node.scale.x, node.scale.y, node.scale.z);
		go.transform.parent = parent;
	}
	
	private Texture2D LoadTexture (string modelFilename, string textureFilename)
	{
		string texturePath;
		if (File.Exists (textureFilename)) {
			texturePath = textureFilename;
		} else if (File.Exists (modelFilename.Substring (0, modelFilename.LastIndexOf ("/")) + "/" + textureFilename)) {
			texturePath = modelFilename.Substring (0, modelFilename.LastIndexOf ("/")) + "/" + textureFilename;
		} else {
			texturePath = modelFilename.Substring (0, modelFilename.LastIndexOf ("."));
			texturePath += ".fbm/";
			if (textureFilename.Contains ("/")) {
				texturePath += textureFilename.Substring (textureFilename.LastIndexOf ("/") + 1);
			} else if (textureFilename.Contains ("\\")) {
				texturePath += textureFilename.Substring (textureFilename.LastIndexOf ("\\") + 1);
			} else {
				texturePath += textureFilename;
			}
			if (!File.Exists (texturePath)) {
				return null;
			}	
		}
		if (!File.Exists (texturePath.Substring (0, modelFilename.LastIndexOf (".")) + ".png")) {
			FIBITMAP dib = FreeImage.LoadEx (texturePath);
			FreeImage.SaveEx (dib, texturePath.Substring (0, texturePath.LastIndexOf (".")) + ".png");
		}
		FIBITMAP fib = FreeImage.LoadEx (texturePath.Substring (0, texturePath.LastIndexOf (".")) + ".png");
		Texture2D t2d = new Texture2D ((int)FreeImage.GetWidth (fib), (int)FreeImage.GetHeight (fib));
		FileStream fs = new FileStream (texturePath.Substring (0, texturePath.LastIndexOf (".")) + ".png", FileMode.Open);
		byte[] bytes = new byte[fs.Length];
		fs.Read (bytes, 0, (int)fs.Length);
		fs.Close ();
		t2d.LoadImage (bytes);
		return t2d;
	}
	
	private Texture2D ConvertNormal (Texture2D t2d)
	{
		Texture2D temp = new Texture2D (t2d.width, t2d.height);
		Color[] pixels = t2d.GetPixels ();
		Color[] tempC = new Color[pixels.Length];
		for (int i=0; i<pixels.Length; i++) {
			tempC [i] = new Color (0f, pixels [i].g, 0f, pixels [i].r);
		}
		temp.SetPixels (tempC);
		temp.Apply ();
		return temp;
	}
	
	private Vector3 ConvertRotation (FbxPluginVector4 vector)
	{
		Quaternion qx = Quaternion.AngleAxis (vector.x, Vector3.right);
		Quaternion qy = Quaternion.AngleAxis (-vector.y, Vector3.up);
		Quaternion qz = Quaternion.AngleAxis (-vector.z, Vector3.forward);
		Quaternion qq = qz * qy * qx;
		return qq.eulerAngles;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginVector4
{
	public float x;
	public float y;
	public float z;
	public float w;
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginTrianglelist
{
	public int triangleCount;
	public IntPtr triangles;

	public int GetTriangle (int index)
	{
		IntPtr ptr = (IntPtr)(triangles.ToInt32 () + Marshal.SizeOf (typeof(int)) * index);
		return Marshal.ReadInt32 (ptr);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginMaterial
{
	public string name;
	public FbxPluginVector4 diffuse;
	public string diffuseTexture;
	public string normalmapTexture;
};

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginMesh
{
	public int id;
	public int vertexCount;
	public IntPtr vertices;
	public int uvCount;
	public IntPtr uvs;
	public int uv2Count;
	public IntPtr uvs2;
	public int normalCount;
	public IntPtr normals;
	public int tangentCount;
	public IntPtr tangents;
	public int trianglelistCount;
	public IntPtr trianglelists;
	public int materialCount;
	public IntPtr materials;

	public FbxPluginVector4 GetVertex (int index)
	{
		IntPtr ptr = (IntPtr)(vertices.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginVector4)) * index);
		return (FbxPluginVector4)Marshal.PtrToStructure (ptr, typeof(FbxPluginVector4));
	}

	public FbxPluginVector4 GetUv (int index)
	{
		IntPtr ptr = (IntPtr)(uvs.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginVector4)) * index);
		return (FbxPluginVector4)Marshal.PtrToStructure (ptr, typeof(FbxPluginVector4));
	}

	public FbxPluginVector4 GetUv2 (int index)
	{
		IntPtr ptr = (IntPtr)(uvs2.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginVector4)) * index);
		return (FbxPluginVector4)Marshal.PtrToStructure (ptr, typeof(FbxPluginVector4));
	}

	public FbxPluginVector4 GetNormal (int index)
	{
		IntPtr ptr = (IntPtr)(normals.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginVector4)) * index);
		return (FbxPluginVector4)Marshal.PtrToStructure (ptr, typeof(FbxPluginVector4));
	}

	public FbxPluginVector4 GetTangent (int index)
	{
		IntPtr ptr = (IntPtr)(tangents.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginVector4)) * index);
		return (FbxPluginVector4)Marshal.PtrToStructure (ptr, typeof(FbxPluginVector4));
	}

	public FbxPluginTrianglelist GetTrianglelist (int index)
	{
		IntPtr ptr = (IntPtr)(trianglelists.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginTrianglelist)) * index);
		return (FbxPluginTrianglelist)Marshal.PtrToStructure (ptr, typeof(FbxPluginTrianglelist));
	}

	public FbxPluginMaterial GetMaterial (int index)
	{
		IntPtr ptr = (IntPtr)(materials.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginMaterial)) * index);
		return (FbxPluginMaterial)Marshal.PtrToStructure (ptr, typeof(FbxPluginMaterial));
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginNode
{
	public string name;
	public FbxPluginVector4 position;
	public FbxPluginVector4 rotation;
	public FbxPluginVector4 scale;
	public IntPtr mesh;
	public int nodeCount;
	public IntPtr nodes;

	public FbxPluginMesh GetMesh ()
	{
		return (FbxPluginMesh)Marshal.PtrToStructure (mesh, typeof(FbxPluginMesh));
	}

	public FbxPluginNode GetNode (int index)
	{
		IntPtr ptr = (IntPtr)(nodes.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginNode)) * index);
		return (FbxPluginNode)Marshal.PtrToStructure (ptr, typeof(FbxPluginNode));
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginScene
{
	public int nodeCount;
	public IntPtr nodes;

	public FbxPluginNode GetNode (int index)
	{
		IntPtr ptr = (IntPtr)(nodes.ToInt32 () + Marshal.SizeOf (typeof(FbxPluginNode)) * index);
		return (FbxPluginNode)Marshal.PtrToStructure (ptr, typeof(FbxPluginNode));
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct FbxPluginOptions
{
	//Option: 0 - None, 1 - Import, 2 - Compute/Generate(Normal/Tangent only)
	public int normalsOption;
	public int tangentsOption;
	public int materialsOption;
	public FbxPluginScene scene;
	public string filename;
	public string password;
}