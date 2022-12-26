using UnityEngine;
using UnityEngine.UI;

namespace UniSharper.Data.Metadata.Samples
{
    public class MetadataEntityPropertyItem : MonoBehaviour
    {
        [SerializeField]
        private RectTransform cachedRectTransform;
        
        [SerializeField]
        private Text label;

        public RectTransform CachedRectTransform => cachedRectTransform;

        public bool Visible
        {
            get => gameObject.activeInHierarchy;
            set => gameObject.SetActive(value);
        }

        public string LabelText
        {
            set => label.text = value;
        }

        public bool Bold
        {
            set => label.fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}