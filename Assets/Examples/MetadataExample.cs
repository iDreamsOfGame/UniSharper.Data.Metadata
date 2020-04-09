using UniSharper.Data.Metadata;
using UniSharper.Data.Metadata.Example.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UniSharper.Data.Metadata.Example
{
    public class MetadataExample : MonoBehaviour
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
            GameObject go = new GameObject();
            go.name = "Text";
            go.layer = LayerMask.NameToLayer("UI");
            Text textFiled = go.AddComponent<Text>();
            textFiled.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            textFiled.text = text;
            textFiled.alignment = TextAnchor.MiddleCenter;
            go.transform.SetParent(canvas.transform, false);
        }

        private void Awake()
        {
            TextAsset binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/MetadataEntityDBConfig.db.bytes");
            MetadataManager.Initialize(binAsset.bytes);

            // Load DB data of EmapleMetadata
            binAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Metadata/Data/ExampleMetadata.db.bytes");
            MetadataManager.LoadEntityDatabase<ExampleMetadata>(binAsset.bytes);

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
            ExampleMetadata metadata = MetadataManager.GetEntity<ExampleMetadata>(1L);
            AddText(metadata.TestString);
            AddText(metadata.TestEnum.ToString());
            Debug.Log(MetadataManager.GetEntity<ExampleMetadata>(2L));

            TestMetadata testMetadata = MetadataManager.GetEntity<TestMetadata>(2L);
            AddText(testMetadata.Name);

            ExampleMetadata[] exampleMetadatas = MetadataManager.GetAllEntities<ExampleMetadata>();
            AddText(string.Format("The count of entity 'ExampleMetadata': {0}", exampleMetadatas.Length));

            TestMetadata[] testMetadataList = MetadataManager.GetEntities<TestMetadata>("Age", 30);

            Debug.Log(testMetadataList.Length);
        }

        #endregion Methods
    }
}