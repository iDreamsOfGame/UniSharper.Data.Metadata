using System.Collections.Generic;
using System.Linq;
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
            
            var binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/MetadataEntityDBConfig.db.bytes");
            MetadataManager.Initialize(binAsset.bytes);
        
            // Load DB data of ExampleMetadata
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/SampleMetadata.db.bytes");
            MetadataManager.LoadEntityDatabase<SampleMetadata>(binAsset.bytes);
        }

        private void Start()
        {
            var metadata = MetadataManager.GetEntity<SampleMetadata>(1L);

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
        
        private void OnDestroy()
        {
            MetadataManager.Dispose();
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
        
        private static string ToString<T>(IEnumerable<T> array)
        {
            var values = array.Select(item => item.ToString()).ToList();
            return string.Join(", ", values);
        }
    }
}