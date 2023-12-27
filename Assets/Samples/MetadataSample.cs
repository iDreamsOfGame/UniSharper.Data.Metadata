using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniSharper.Data.Metadata.Samples
{
    public class MetadataSample : MonoBehaviour
    {
        [SerializeField]
        private RectTransform content;

        [SerializeField]
        private MetadataEntityPropertyItem propertyItemTemplate;
        
        private MetadataManager MetadataManager => MetadataManager.Instance;

        private void Awake()
        {
            propertyItemTemplate.Visible = false;
            
            var binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlayerPath.GetAssetPath($"Metadata/Data/MetadataEntityDBConfig{FileExtensions.DatabaseFile}{FileExtensions.UnityBinaryFile}"));
            MetadataManager.Initialize(binAsset.bytes);
        
            // Load DB data of Metadata
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlayerPath.GetAssetPath($"Metadata/Data/GenericTypeSampleMetadata{FileExtensions.DatabaseFile}{FileExtensions.UnityBinaryFile}"));
            MetadataManager.LoadEntityDatabase<GenericTypeSampleMetadata>(binAsset.bytes);
            
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlayerPath.GetAssetPath($"Metadata/Data/UnityTypeSampleMetadata{FileExtensions.DatabaseFile}{FileExtensions.UnityBinaryFile}"));
            MetadataManager.LoadEntityDatabase<UnityTypeSampleMetadata>(binAsset.bytes);
        }

        private void Start()
        {
            AddGenericTypeSampleMetadataItems();
            AddUnityTypeSampleMetadataItems();
        }
        
        private void OnDestroy()
        {
            MetadataManager.Dispose();
        }

        private void AddGenericTypeSampleMetadataItems()
        {
            AddTitleItem("Generic Type");
            
            var metadata = MetadataManager.GetEntity<GenericTypeSampleMetadata>(1L);

            AddPropertyItem(nameof(metadata.StringSample), metadata.StringSample);
            AddPropertyItem(nameof(metadata.BooleanSample), metadata.BooleanSample);
            AddPropertyItem(nameof(metadata.ByteSample), metadata.ByteSample);
            AddPropertyItem(nameof(metadata.SByteSample), metadata.SByteSample);
            AddPropertyItem(nameof(metadata.Int16Sample), metadata.Int16Sample);
            AddPropertyItem(nameof(metadata.UInt16Sample), metadata.UInt16Sample);
            AddPropertyItem(nameof(metadata.Int32Sample), metadata.Int32Sample);
            AddPropertyItem(nameof(metadata.UInt32Sample), metadata.UInt32Sample);
            AddPropertyItem(nameof(metadata.Int64Sample), metadata.Int64Sample);
            AddPropertyItem(nameof(metadata.UInt64Sample), metadata.UInt64Sample);
            AddPropertyItem(nameof(metadata.SingleSample), metadata.SingleSample);
            AddPropertyItem(nameof(metadata.DoubleSample), metadata.DoubleSample);
            AddPropertyItem(nameof(metadata.DecimalSample), metadata.DecimalSample);
            AddPropertyItem(nameof(metadata.EnumSample), metadata.EnumSample);
            
            // Array
            AddPropertyItem(nameof(metadata.StringArraySample), ToString(metadata.StringArraySample));
            AddPropertyItem(nameof(metadata.BooleanArraySample), ToString(metadata.BooleanArraySample));
            AddPropertyItem(nameof(metadata.ByteArraySample), ToString(metadata.ByteArraySample));
            AddPropertyItem(nameof(metadata.SByteArraySample), ToString(metadata.SByteArraySample));
            AddPropertyItem(nameof(metadata.Int16ArraySample), ToString(metadata.Int16ArraySample));
            AddPropertyItem(nameof(metadata.UInt16ArraySample), ToString(metadata.UInt16ArraySample));
            AddPropertyItem(nameof(metadata.Int32ArraySample), ToString(metadata.Int32ArraySample));
            AddPropertyItem(nameof(metadata.UInt32ArraySample), ToString(metadata.UInt32ArraySample));
            AddPropertyItem(nameof(metadata.Int64ArraySample), ToString(metadata.Int64ArraySample));
            AddPropertyItem(nameof(metadata.UInt64ArraySample), ToString(metadata.UInt64ArraySample));
            AddPropertyItem(nameof(metadata.SingleArraySample), ToString(metadata.SingleArraySample));
            AddPropertyItem(nameof(metadata.DoubleArraySample), ToString(metadata.DoubleArraySample));
            AddPropertyItem(nameof(metadata.DecimalArraySample), ToString(metadata.DecimalArraySample));
        }

        private void AddUnityTypeSampleMetadataItems()
        {
            AddTitleItem("Unity Type");
            
            var metadata = MetadataManager.GetEntity<UnityTypeSampleMetadata>(1L);
            
            AddPropertyItem(nameof(metadata.Vector2Sample), metadata.Vector2Sample);
            AddPropertyItem(nameof(metadata.Vector2IntSample), metadata.Vector2IntSample);
            AddPropertyItem(nameof(metadata.Vector3Sample), metadata.Vector3Sample);
            AddPropertyItem(nameof(metadata.Vector3IntSample), metadata.Vector3IntSample);
            AddPropertyItem(nameof(metadata.Vector4Sample), metadata.Vector4Sample);
            AddPropertyItem(nameof(metadata.RangeIntSample), $"({metadata.RangeIntSample.start}, {metadata.RangeIntSample.length})");
            AddPropertyItem(nameof(metadata.QuaternionSample), metadata.QuaternionSample);
            AddPropertyItem(nameof(metadata.RectSample), metadata.RectSample);
            AddPropertyItem(nameof(metadata.RectIntSample), metadata.RectIntSample);
            AddPropertyItem(nameof(metadata.ColorSample), metadata.ColorSample);
            AddPropertyItem(nameof(metadata.Color32Sample), metadata.Color32Sample);

            // Array
            AddPropertyItem(nameof(metadata.Vector2ArraySample), ToString(metadata.Vector2ArraySample));
            AddPropertyItem(nameof(metadata.Vector2IntArraySample), ToString(metadata.Vector2IntArraySample));
            AddPropertyItem(nameof(metadata.Vector3ArraySample), ToString(metadata.Vector3ArraySample));
            AddPropertyItem(nameof(metadata.Vector3IntArraySample), ToString(metadata.Vector3IntArraySample));
            AddPropertyItem(nameof(metadata.Vector4ArraySample), ToString(metadata.Vector4ArraySample));
            AddPropertyItem(nameof(metadata.RangeIntArraySample), ToRangeIntArrayString(metadata.RangeIntArraySample));
            AddPropertyItem(nameof(metadata.QuaternionArraySample), ToString(metadata.QuaternionArraySample));
            AddPropertyItem(nameof(metadata.RectArraySample), ToString(metadata.RectArraySample));
            AddPropertyItem(nameof(metadata.RectIntArraySample), ToString(metadata.RectIntArraySample));
            AddPropertyItem(nameof(metadata.ColorArraySample), ToString(metadata.ColorArraySample));
            AddPropertyItem(nameof(metadata.Color32ArraySample), ToString(metadata.Color32ArraySample));
        }

        private void AddTitleItem(string title)
        {
            var instance = Instantiate(propertyItemTemplate, content);
            if (!instance)
                return;
            
            instance.LabelText = title;
            instance.Bold = true;
            instance.Visible = true;
        }

        private void AddPropertyItem(string key, object value)
        {
            if (!propertyItemTemplate)
                return;
            
            var instance = Instantiate(propertyItemTemplate, content);
            if (!instance)
                return;

            instance.LabelText = $"{key}: {value}";
            instance.Visible = true;
        }

        private static string ToRangeIntArrayString(RangeInt[] array)
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in array)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append("|");
                
                stringBuilder.Append($"({item.start}, {item.length})");
            }
            return stringBuilder.ToString();
        }
        
        private static string ToString<T>(IEnumerable<T> array)
        {
            var values = array.Select(item => item.ToString()).ToList();
            return string.Join(", ", values);
        }
    }
}