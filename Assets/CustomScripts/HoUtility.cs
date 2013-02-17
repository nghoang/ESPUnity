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
