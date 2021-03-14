using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UniSharper.Data.Metadata.Samples
{
    public class MetadataSample : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private Canvas canvas = null;

        #endregion Fields

        #region Properties

        private MetadataManager MetadataManager => MetadataManager.Instance;

        #endregion Properties

        #region Methods

        private void AddText(string text)
        {
            var go = new GameObject
            {
                name = "Text",
                layer = LayerMask.NameToLayer("UI")
            };
            var textFiled = go.AddComponent<Text>();
            textFiled.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            textFiled.text = text;
            textFiled.alignment = TextAnchor.MiddleCenter;
            go.transform.SetParent(canvas.transform, false);
        }

        private void Awake()
        {
            var binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/MetadataEntityDBConfig.db.bytes");
            MetadataManager.Initialize(binAsset.bytes);

            // Load DB data of EmapleMetadata
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/SampleMetadata.db.bytes");
            MetadataManager.LoadEntityDatabase<SampleMetadata>(binAsset.bytes);

            // Load DB data of TestMetadata
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/TestMetadata.db.bytes");
            MetadataManager.LoadEntityDatabase<TestMetadata>(binAsset.bytes);
        }

        private void OnDestroy()
        {
            MetadataManager.Dispose();
        }

        private void Start()
        {
            var metadata = MetadataManager.GetEntity<SampleMetadata>(1L);
            AddText(metadata.TestString);
            AddText(metadata.TestEnum.ToString());
            Debug.Log(MetadataManager.GetEntity<SampleMetadata>(2L));

            var testMetadata = MetadataManager.GetEntity<TestMetadata>(2L);
            AddText(testMetadata.Name);

            var exampleMetadatas = MetadataManager.GetAllEntities<SampleMetadata>();
            AddText(string.Format("The count of entity 'ExampleMetadata': {0}", exampleMetadatas.Length));

            var testMetadataList = MetadataManager.GetEntities<TestMetadata>("Age", 30);

            Debug.Log(testMetadataList.Length);
        }

        #endregion Methods
    }
}