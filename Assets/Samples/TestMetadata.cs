﻿// NOTE: This script generated by editor automatically. No need to edit manually!

using UnityEngine.Scripting;
using UniSharper.Data.Metadata;

namespace UniSharper.Data.Metadata.Samples
{
	/// <summary>
	/// TestMetadata
	/// Note: Generated by Editor automatically. Please don't edit this file manually!
	/// </summary>
	public partial class TestMetadata : MetadataEntity
	{

		/// <summary>
		/// ID
		/// </summary>
		public long ID
		{
			get;
			set;
		}

		/// <summary>
		/// 名称
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// 年龄
		/// </summary>
		public int Age
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMetadata"/> class.
		/// </summary>
		[Preserve]
		public TestMetadata()
			: base()
		{
		}
	}
}
