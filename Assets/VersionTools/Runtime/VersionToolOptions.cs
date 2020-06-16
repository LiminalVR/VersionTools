using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildTools
{
    [CreateAssetMenu]
    public class VersionToolOptions : ScriptableObject
	{
		#region Variables
		#region Fields
		[SerializeField] private bool _appendVersionNumber = true;

		[SerializeField] private string[] _appendToFilesWithEndings = new string[] 
		{ 
			"apk",
			"exe"
			// TODO: Determine more defaults
		};
		#endregion

		#region Properties
		public bool AppendVersionNumber => _appendVersionNumber;
		public string[] FileEndings => _appendToFilesWithEndings;
		#endregion
		#endregion
	}
}