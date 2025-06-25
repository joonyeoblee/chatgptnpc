using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogueExtractor
{
	public static string ExtractAllDialoguesAsString(string replyMessage)
	{
		var matches = Regex.Matches(replyMessage, "\"(.*?)\"");
		List<string> quotes = new List<string>();

		foreach (Match match in matches)
		{
			quotes.Add(match.Groups[1].Value);
		}

		return string.Join(" ", quotes); // 공백으로 연결
	}
}
