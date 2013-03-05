using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class HoUtility : MonoBehaviour {
	
	public static List<string> SimpleRegex(string pattern, string content, int group, RegexOptions options)
    {
        Regex exp = new Regex(pattern, options);
        MatchCollection MatchList = exp.Matches(content);
        List<string> res = new List<string>();
        foreach (Match m in MatchList)
        {
            res.Add(m.Groups[group].Value);
        }
        return res;
    }
	
	public static Vector3 GetCenterOfGroup(GameObject go)
	{
		float minX=float.MaxValue;
		float minY=float.MaxValue;
		float minZ=float.MaxValue;
		float maxX=float.MaxValue*(-1);
		float maxY=float.MaxValue*(-1);
		float maxZ=float.MaxValue*(-1);
		foreach (Transform t in go.transform)
		{
			if (t.renderer != null)
			{
				if (t.renderer.bounds.center.x < minX)
					minX = t.renderer.bounds.center.x;
				if (t.renderer.bounds.center.y < minY)
					minY = t.renderer.bounds.center.y;
				if (t.renderer.bounds.center.z < minZ)
					minZ = t.renderer.bounds.center.z;
				
				if (t.renderer.bounds.center.x > maxX)
					maxX = t.renderer.bounds.center.x;
				if (t.renderer.bounds.center.y > maxY)
					maxY = t.renderer.bounds.center.y;
				if (t.renderer.bounds.center.z > maxZ)
					maxZ = t.renderer.bounds.center.z;
			}
		}
		return new Vector3((minX + maxX)/2,(minY + maxY)/2,(minZ + maxZ)/2);
	}

    public static string SimpleRegexSingle(string pattern, string content, int group, RegexOptions options)
    {
        List<string> res = SimpleRegex(pattern, content, group, options);
        if ((res.Count > 0))
        {
            return res[0];
        }
        else
        {
            return "";
        }
    }

    public static List<string> SimpleRegex(string pattern, string content, int group)
    {
        return SimpleRegex(pattern, content, group, RegexOptions.Singleline);
    }

    public static string SimpleRegexSingle(string pattern, string content, int group)
    {
        return SimpleRegexSingle(pattern, content, group, RegexOptions.Singleline);
    }

	static public void WriteLog(string mes)
    {
        WriteFile("log.txt", "\n" + System.DateTime.Now + " >> " + mes, true);
    }

    static public void WriteLog(string mes, int count)
    {
        string content = ReadFileString("log.txt");
        string[] lines = content.Split('\n');
       
        string newContent = "";
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            newContent = lines[i] + "\n" + newContent;
            count--;
            if (count < 0)
                break;
        }
        newContent += "\n" + mes;
        WriteFile("log.txt", newContent, false);
    }
	
	static public void WriteFile(string fn, string content, bool append)
    {
        System.IO.StreamWriter file = new System.IO.StreamWriter(fn, append);
        file.Write(content);
        file.Close();
    }
	
	public static string ByteToString(byte[] ar)
	{
		return ASCIIEncoding.ASCII.GetString(ar);
	}
	
	public static string ReadFileString(string path)
    {
        if (IsFileExist(path) == false)
            return "";
        string content = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
        return content;
    }
	
	public static bool IsFileExist(string p)
    {
        return File.Exists(p);
    }

}
